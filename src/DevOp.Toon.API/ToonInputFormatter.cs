using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using DevOp.Toon;
using DevOp.Toon.Core;

namespace DevOp.Toon.API;

/// <summary>
/// ASP.NET Core input formatter for TOON request bodies.
/// </summary>
public sealed class ToonInputFormatter : TextInputFormatter
{
    private static readonly Encoding StrictUtf8 = new UTF8Encoding(false, true);
    private static readonly Encoding ReplacingUtf8 = new UTF8Encoding(false, false);
    private static readonly MethodInfo DecodeGenericMethod = typeof(ToonDecoder)
        .GetMethods()
        .Single(method =>
            method.Name == nameof(ToonDecoder.Decode)
            && method.IsGenericMethodDefinition
            && method.GetParameters().Length == 2
            && method.GetParameters()[0].ParameterType == typeof(string)
            && method.GetParameters()[1].ParameterType == typeof(ToonDecodeOptions));

    /// <summary>
    /// The <c>text/toon</c> media type.
    /// </summary>
    public const string MediaTypeText = ToonMediaTypes.Text;

    /// <summary>
    /// The <c>application/toon</c> media type.
    /// </summary>
    public const string MediaTypeApplication = ToonMediaTypes.Application;

    /// <summary>
    /// Creates a TOON input formatter with UTF-8 support.
    /// </summary>
    public ToonInputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeText);
        SupportedMediaTypes.Add(MediaTypeApplication);
        SupportedEncodings.Add(Encoding.UTF8);
    }

    /// <inheritdoc />
    protected override bool CanReadType(Type type) => true;

    /// <inheritdoc />
    public override async Task<InputFormatterResult> ReadRequestBodyAsync(
        InputFormatterContext context,
        Encoding encoding)
    {
        try
        {
            var decodeOptions = (context.HttpContext.RequestServices.GetService<ToonServiceOptions>()?.Decode ?? ToonDecoder.DefaultOptions).Clone();
            if (context.HttpContext.Request.Headers.TryGetValue("X-Toon-Version", out var versions))
            {
                if (versions.Count != 1 || versions[0]!.Contains(','))
                {
                    context.HttpContext.Response.StatusCode = 400;
                    throw new ArgumentException("Specify exactly one body protocol version.");
                }
                if (!ToonProtocolVersions.TryParse(versions[0], out var version))
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(versions[0] ?? "", @"^(?:[0-9]+\.[0-9]+|custom-[a-z0-9.-]+)$")) throw new ArgumentException("Malformed TOON body protocol version.");
                    context.ModelState.TryAddModelError(context.ModelName, new UnsupportedContentTypeException("Unsupported TOON body protocol version."), context.Metadata);
                    context.HttpContext.Response.StatusCode = 415;
                    return await InputFormatterResult.FailureAsync();
                }
                if (decodeOptions.ProtocolVersion != ToonProtocolVersion.Auto && decodeOptions.ProtocolVersion != version)
                {
                    context.HttpContext.Response.StatusCode = 400;
                    throw new ArgumentException("Body protocol version conflicts with configured decoder.");
                }
                decodeOptions.ProtocolVersion = version;
            }
            var replaceInvalid = decodeOptions.ProtocolVersion is ToonProtocolVersion.CustomLegacy or ToonProtocolVersion.V3_0
                || !decodeOptions.Strict && decodeOptions.ProtocolVersion is ToonProtocolVersion.V4_2 or ToonProtocolVersion.V4_3;
            using var reader = new StreamReader(context.HttpContext.Request.Body, replaceInvalid ? ReplacingUtf8 : StrictUtf8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);
            var text = await reader.ReadToEndAsync();
            decodeOptions = ToonDecoder.DetectOptions(text, decodeOptions);
            var modelType = context.ModelType;
            object? result;

            if (typeof(ToonNode).IsAssignableFrom(modelType))
            {
                result = ToonDecoder.Decode(text, decodeOptions);
            }
            else
            {
                result = DecodeTyped(text, modelType, decodeOptions);
            }

            return await InputFormatterResult.SuccessAsync(result);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            context.ModelState.TryAddModelError(context.ModelName, ex.InnerException, context.Metadata);
            return await InputFormatterResult.FailureAsync();
        }
        catch (Exception ex)
        {
            if (context.HttpContext.Response.StatusCode == 200) context.HttpContext.Response.StatusCode = 400;
            context.ModelState.TryAddModelError(context.ModelName, ex, context.Metadata);
            return await InputFormatterResult.FailureAsync();
        }
    }

    private static ToonDecodeOptions ResolveDecodeOptions(IServiceProvider services, string text)
    {
        var serviceOptions = services.GetService<ToonServiceOptions>();
        var fallbackOptions = serviceOptions?.Decode ?? ToonDecoder.DefaultOptions;
        return ToonDecoder.DetectOptions(text, fallbackOptions);
    }

    private static object? DecodeTyped(string text, Type modelType, ToonDecodeOptions options)
    {
        var genericMethod = DecodeGenericMethod.MakeGenericMethod(modelType);
        return genericMethod.Invoke(null, new object?[] { text, options });
    }
}
