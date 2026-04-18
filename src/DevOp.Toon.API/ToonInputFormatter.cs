using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using DevOp.Toon;

namespace DevOp.Toon.API;

/// <summary>
/// ASP.NET Core input formatter for TOON request bodies.
/// </summary>
public sealed class ToonInputFormatter : TextInputFormatter
{
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
        using var reader = new StreamReader(context.HttpContext.Request.Body, encoding);
        var text = await reader.ReadToEndAsync();

        try
        {
            var decodeOptions = ResolveDecodeOptions(context.HttpContext.RequestServices, text);
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
        catch (Exception ex)
        {
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
        var method = typeof(ToonDecoder)
            .GetMethods()
            .Single(method =>
                method.Name == nameof(ToonDecoder.Decode)
                && method.IsGenericMethodDefinition
                && method.GetParameters().Length == 2
                && method.GetParameters()[0].ParameterType == typeof(string)
                && method.GetParameters()[1].ParameterType == typeof(ToonDecodeOptions));

        var genericMethod = method.MakeGenericMethod(modelType);
        return genericMethod.Invoke(null, new object?[] { text, options });
    }
}
