using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System.Text;
using DevOp.Toon;
using DevOp.Toon.Core;

namespace DevOp.Toon.API;

/// <summary>
/// ASP.NET Core output formatter for TOON response bodies.
/// </summary>
public sealed class ToonOutputFormatter : TextOutputFormatter
{
    private const string OptionHeaderPrefix = "X-Toon-Option-";
    private static readonly IReadOnlyDictionary<string, PropertyInfo> EncodeOptionProperties = typeof(ToonEncodeOptions)
        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Where(property => property.CanRead && property.CanWrite)
        .ToDictionary(property => property.Name, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The <c>text/toon</c> media type.
    /// </summary>
    public const string MediaTypeText = ToonMediaTypes.Text;

    /// <summary>
    /// The <c>application/toon</c> media type.
    /// </summary>
    public const string MediaTypeApplication = ToonMediaTypes.Application;

    /// <summary>
    /// Creates a TOON output formatter with UTF-8 support.
    /// </summary>
    public ToonOutputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeText);
        SupportedMediaTypes.Add(MediaTypeApplication);
        SupportedEncodings.Add(Encoding.UTF8);
    }

    /// <inheritdoc />
    protected override bool CanWriteType(Type? type) => type is not null;

    /// <inheritdoc />
    public override bool CanWriteResult(OutputFormatterCanWriteContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var objectType = context.ObjectType ?? context.Object?.GetType();
        if (!CanWriteType(objectType))
        {
            return false;
        }

        if (IsToonMediaType(context.ContentType) ||
            IsToonMediaType(context.HttpContext.Response.ContentType))
        {
            return true;
        }

        var acceptHeaders = context.HttpContext.Request.GetTypedHeaders().Accept;
        var requestContentType = context.HttpContext.Request.ContentType;

        if (acceptHeaders != null && acceptHeaders.Count > 0)
        {
            return acceptHeaders.Any(header =>
                IsToonMediaType(header.MediaType) ||
                header.MediaType.Equals("*/*", StringComparison.OrdinalIgnoreCase));
        }

        return string.IsNullOrWhiteSpace(requestContentType) || IsToonMediaType(requestContentType);
    }

    /// <inheritdoc />
    public override async Task WriteResponseBodyAsync(
        OutputFormatterWriteContext context,
        Encoding selectedEncoding)
    {
        ToonEncodeOptions encodeOptions;
        try { encodeOptions = ResolveEncodeOptions(context.HttpContext); }
        catch (ArgumentException ex)
        {
            context.HttpContext.Response.StatusCode = ex is NotSupportedVersionException ? 406 : 400;
            await context.HttpContext.Response.WriteAsJsonAsync(new { title = "Invalid TOON version options", detail = ex.Message }, context.HttpContext.RequestAborted);
            return;
        }
        context.HttpContext.Response.Headers["X-Toon-Version"] = ToonProtocolVersions.Format(encodeOptions.ProtocolVersion);
        await ToonEncoder.EncodeToStreamAsync(
            context.Object!,
            context.HttpContext.Response.Body,
            encodeOptions,
            context.HttpContext.RequestAborted).ConfigureAwait(false);
    }

    private static bool IsToonMediaType(StringSegment mediaType)
        => mediaType.Equals(MediaTypeText, StringComparison.OrdinalIgnoreCase) || mediaType.Equals(MediaTypeApplication, StringComparison.OrdinalIgnoreCase);

    private static bool IsToonMediaType(string? mediaType)
        => !string.IsNullOrWhiteSpace(mediaType) &&
           (mediaType.StartsWith(MediaTypeText, StringComparison.OrdinalIgnoreCase) ||
            mediaType.StartsWith(MediaTypeApplication, StringComparison.OrdinalIgnoreCase));

    internal static ToonEncodeOptions ResolveEncodeOptions(HttpContext httpContext)
    {
        if (httpContext.Features.Get<EncodeOptionsSnapshot>() is { } snapshot && snapshot.Matches(httpContext.Request.Headers)) return snapshot.Options;
        var serviceOptions = httpContext.RequestServices.GetService<ToonServiceOptions>();
        var encodeOptions = CloneEncodeOptions(serviceOptions?.Encode ?? ToonEncoder.DefaultOptions);

        ApplyRequestOptionOverrides(encodeOptions, httpContext.Request.Headers);

        encodeOptions.Validate();
        return encodeOptions;
    }

    internal static void PrepareEncodeOptions(HttpContext httpContext)
    {
        var options = ResolveEncodeOptions(httpContext);
        httpContext.Features.Set(new EncodeOptionsSnapshot(options, httpContext.Request.Headers));
    }

    private sealed class EncodeOptionsSnapshot
    {
        private readonly KeyValuePair<string, StringValues>[] overrides;
        internal ToonEncodeOptions Options { get; }

        internal EncodeOptionsSnapshot(ToonEncodeOptions options, IHeaderDictionary headers)
        {
            Options = options;
            var count = 0;
            foreach (var header in headers)
                if (header.Key.StartsWith(OptionHeaderPrefix, StringComparison.OrdinalIgnoreCase)) count++;
            overrides = new KeyValuePair<string, StringValues>[count];
            var index = 0;
            foreach (var header in headers)
            {
                if (!header.Key.StartsWith(OptionHeaderPrefix, StringComparison.OrdinalIgnoreCase)) continue;
                var values = header.Value.Count > 1 ? new StringValues((string?[])header.Value.ToArray().Clone()) : new StringValues(header.Value.Count == 0 ? null : header.Value[0]);
                overrides[index++] = new KeyValuePair<string, StringValues>(header.Key, values);
            }
        }

        internal bool Matches(IHeaderDictionary headers)
        {
            var count = 0;
            foreach (var header in headers)
                if (header.Key.StartsWith(OptionHeaderPrefix, StringComparison.OrdinalIgnoreCase)) count++;
            if (count != overrides.Length) return false;
            foreach (var header in overrides)
                if (!headers.TryGetValue(header.Key, out var current) || !StringValues.Equals(header.Value, current)) return false;
            return true;
        }
    }

    private static ToonEncodeOptions CloneEncodeOptions(ToonEncodeOptions options) => options.Clone();

    internal sealed class NotSupportedVersionException : ArgumentException
    {
        internal NotSupportedVersionException(string message) : base(message) { }
    }

    private static void ApplyRequestOptionOverrides(ToonEncodeOptions encodeOptions, IHeaderDictionary headers)
    {
        if (headers.TryGetValue("X-Toon-Option-ProtocolVersion", out var versions))
        {
            if (versions.Count != 1 || versions[0]!.Contains(',')) throw new ArgumentException("Specify exactly one response protocol version.");
            if (!ToonProtocolVersions.TryParse(versions[0], out var version))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(versions[0] ?? "", @"^(?:[0-9]+\.[0-9]+|custom-[a-z0-9.-]+)$")) throw new ArgumentException("Malformed response protocol version.");
                throw new NotSupportedVersionException("Unsupported response protocol version.");
            }
            encodeOptions.ProtocolVersion = version;
        }
        foreach (var header in headers)
        {
            if (!header.Key.StartsWith(OptionHeaderPrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var optionName = header.Key[OptionHeaderPrefix.Length..];
            if (optionName.Equals("ProtocolVersion", StringComparison.OrdinalIgnoreCase)) continue;
            if (optionName.Equals(nameof(ToonEncodeOptions.Indent), StringComparison.OrdinalIgnoreCase))
            {
                if (header.Value.Count != 1 || !int.TryParse(header.Value[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var indent) || indent is < 1 or > 16)
                    throw new ArgumentException("Response indentation must be one integer between 1 and 16 spaces.");
                encodeOptions.Indent = indent;
                continue;
            }
            if (string.IsNullOrWhiteSpace(optionName) ||
                !EncodeOptionProperties.TryGetValue(optionName, out var property) ||
                !TryConvertHeaderValue(header.Value, property.PropertyType, out var convertedValue))
            {
                continue;
            }

            property.SetValue(encodeOptions, convertedValue);
        }
    }

    private static bool TryConvertHeaderValue(StringValues values, Type targetType, out object? convertedValue)
    {
        convertedValue = null;

        var rawValue = values.Count > 0 ? values[^1] : null;
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return false;
        }

        var effectiveType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (effectiveType.IsEnum)
        {
            if (!Enum.TryParse(effectiveType, rawValue, ignoreCase: true, out var enumValue))
            {
                return false;
            }

            convertedValue = enumValue;
            return true;
        }

        if (effectiveType == typeof(string))
        {
            convertedValue = rawValue;
            return true;
        }

        var converter = TypeDescriptor.GetConverter(effectiveType);
        if (!converter.CanConvertFrom(typeof(string)))
        {
            return false;
        }

        try
        {
            convertedValue = converter.ConvertFrom(null, CultureInfo.InvariantCulture, rawValue);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
