using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System.Text;
using DevOp.Toon;

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
        var encodeOptions = ResolveEncodeOptions(context.HttpContext);
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

    private static ToonEncodeOptions ResolveEncodeOptions(HttpContext httpContext)
    {
        var serviceOptions = httpContext.RequestServices.GetService<ToonServiceOptions>();
        var encodeOptions = CloneEncodeOptions(serviceOptions?.Encode ?? ToonEncoder.DefaultOptions);

        ApplyRequestOptionOverrides(encodeOptions, httpContext.Request.Headers);

        return encodeOptions;
    }

    private static ToonEncodeOptions CloneEncodeOptions(ToonEncodeOptions options)
    {
        return new ToonEncodeOptions
        {
            Indent = options.Indent,
            Delimiter = options.Delimiter,
            KeyFolding = options.KeyFolding,
            FlattenDepth = options.FlattenDepth,
            ObjectArrayLayout = options.ObjectArrayLayout,
            ByteArrayFormat = options.ByteArrayFormat,
            IgnoreNullOrEmpty = options.IgnoreNullOrEmpty,
            ExcludeEmptyArrays = options.ExcludeEmptyArrays
        };
    }

    private static void ApplyRequestOptionOverrides(ToonEncodeOptions encodeOptions, IHeaderDictionary headers)
    {
        foreach (var header in headers)
        {
            if (!header.Key.StartsWith(OptionHeaderPrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var optionName = header.Key[OptionHeaderPrefix.Length..];
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
