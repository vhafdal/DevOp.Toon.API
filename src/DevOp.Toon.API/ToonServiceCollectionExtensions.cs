using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using DevOp.Toon.Core;
using DevOp.Toon;

namespace DevOp.Toon.API;

/// <summary>
/// Service registration helpers for enabling TOON MVC formatters.
/// </summary>
public static class ToonServiceCollectionExtensions
{
    private static readonly string[] ToonCompressionMimeTypes =
    [
        ToonMediaTypes.Application,
        ToonMediaTypes.Text
    ];

    /// <summary>
    /// Registers TOON services and MVC formatters.
    /// </summary>
    public static IServiceCollection AddToon(this IServiceCollection services, Action<ToonServiceOptions> configure, bool useAsDefaultFormatter = false)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        global::DevOp.Toon.ToonServiceCollectionExtensions.AddToon(services, options =>
        {
            ApplyTransportDefaults(options);
            configure(options);
        });
        services.PostConfigure<ResponseCompressionOptions>(AddToonCompressionMimeTypes);
        services.Configure<MvcOptions>(options => AddToonFormatters(options, useAsDefaultFormatter));
        return services;
    }

    /// <summary>
    /// Registers TOON services and MVC formatters with default configuration.
    /// </summary>
    public static IServiceCollection AddToon(this IServiceCollection services, bool useAsDefaultFormatter = false)
    {
        return AddToon(services, _ => { }, useAsDefaultFormatter);
    }

    /// <summary>
    /// Registers TOON services and MVC formatters for an MVC builder.
    /// </summary>
    public static IMvcBuilder AddToon(this IMvcBuilder builder, Action<ToonServiceOptions> configure, bool useAsDefaultFormatter = false)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddToon(configure, useAsDefaultFormatter);
        return builder;
    }

    /// <summary>
    /// Registers TOON services and MVC formatters for an MVC builder with default configuration.
    /// </summary>
    public static IMvcBuilder AddToon(this IMvcBuilder builder, bool useAsDefaultFormatter = false)
    {
        return AddToon(builder, _ => { }, useAsDefaultFormatter);
    }

    /// <summary>
    /// Registers TOON services and MVC formatters for an MVC core builder.
    /// </summary>
    public static IMvcCoreBuilder AddToon(this IMvcCoreBuilder builder, Action<ToonServiceOptions> configure, bool useAsDefaultFormatter = false)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddToon(configure, useAsDefaultFormatter);
        return builder;
    }

    /// <summary>
    /// Registers TOON services and MVC formatters for an MVC core builder with default configuration.
    /// </summary>
    public static IMvcCoreBuilder AddToon(this IMvcCoreBuilder builder, bool useAsDefaultFormatter = false)
    {
        return AddToon(builder, _ => { }, useAsDefaultFormatter);
    }

    private static void ApplyTransportDefaults(ToonServiceOptions options)
    {
        options.Indent = 1;

        var encode = options.Encode;
        encode.IgnoreNullOrEmpty = true;
        encode.Delimiter = ToonDelimiter.COMMA;
        encode.Indent = 1;
        encode.ExcludeEmptyArrays = true;
        encode.KeyFolding = ToonKeyFolding.Off;
        encode.ObjectArrayLayout = ToonObjectArrayLayout.Columnar;
    }

    private static void AddToonCompressionMimeTypes(ResponseCompressionOptions options)
    {
        var configuredMimeTypes = options.MimeTypes?.ToArray();
        var mimeTypes = configuredMimeTypes is { Length: > 0 }
            ? configuredMimeTypes
            : ResponseCompressionDefaults.MimeTypes;

        options.MimeTypes = mimeTypes
            .Concat(ToonCompressionMimeTypes)
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static void AddToonFormatters(MvcOptions options, bool useAsDefaultFormatter)
    {
        if (options.InputFormatters.OfType<ToonInputFormatter>().Any())
        {
            if (!options.OutputFormatters.OfType<ToonOutputFormatter>().Any())
            {
                AddOutputFormatter(options, useAsDefaultFormatter);
            }

            return;
        }

        if (options.OutputFormatters.OfType<ToonOutputFormatter>().Any())
        {
            AddInputFormatter(options, useAsDefaultFormatter);
            return;
        }

        AddInputFormatter(options, useAsDefaultFormatter);
        AddOutputFormatter(options, useAsDefaultFormatter);
    }

    private static void AddInputFormatter(MvcOptions options, bool useAsDefaultFormatter)
    {
        var formatter = new ToonInputFormatter();

        if (useAsDefaultFormatter)
        {
            options.InputFormatters.Insert(0, formatter);
            return;
        }

        options.InputFormatters.Add(formatter);
    }

    private static void AddOutputFormatter(MvcOptions options, bool useAsDefaultFormatter)
    {
        var formatter = new ToonOutputFormatter();

        if (useAsDefaultFormatter)
        {
            options.OutputFormatters.Insert(0, formatter);
            return;
        }

        options.OutputFormatters.Add(formatter);
    }
}
