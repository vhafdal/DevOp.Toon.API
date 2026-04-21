using DevOp.Toon;
using DevOp.Toon.API;
using DevOp.Toon.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DevOp.Toon.API.Tests;

public sealed class ToonServiceCollectionExtensionsTests
{
    [Fact]
    public void AddToon_UsesCompactColumnarEncodeDefaults()
    {
        var services = new ServiceCollection();

        services.AddControllers().AddToon();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<ToonServiceOptions>().Encode;

        Assert.True(options.IgnoreNullOrEmpty);
        Assert.Equal(ToonDelimiter.COMMA, options.Delimiter);
        Assert.Equal(1, options.Indent);
        Assert.True(options.ExcludeEmptyArrays);
        Assert.Equal(ToonKeyFolding.Off, options.KeyFolding);
        Assert.Equal(ToonObjectArrayLayout.Columnar, options.ObjectArrayLayout);
    }

    [Fact]
    public void AddToon_AllowsCallerToOverrideDefaults()
    {
        var services = new ServiceCollection();

        services.AddControllers().AddToon(options =>
        {
            options.Encode.IgnoreNullOrEmpty = false;
            options.Encode.Indent = 2;
        });

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<ToonServiceOptions>().Encode;

        Assert.False(options.IgnoreNullOrEmpty);
        Assert.Equal(2, options.Indent);
    }

    [Fact]
    public void AddToon_DoesNotDuplicateFormatters()
    {
        var services = new ServiceCollection();

        services.AddControllers().AddToon().AddToon();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<MvcOptions>>().Value;

        Assert.Single(options.InputFormatters.OfType<ToonInputFormatter>());
        Assert.Single(options.OutputFormatters.OfType<ToonOutputFormatter>());
    }

    [Fact]
    public void AddToon_AddsToonMimeTypesToResponseCompressionOptions()
    {
        var services = new ServiceCollection();

        services.AddControllers().AddToon();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ResponseCompressionOptions>>().Value;

        Assert.Contains(ToonMediaTypes.Application, options.MimeTypes);
        Assert.Contains(ToonMediaTypes.Text, options.MimeTypes);
    }

    [Fact]
    public void AddToon_PreservesDefaultResponseCompressionMimeTypes()
    {
        var services = new ServiceCollection();

        services.AddControllers().AddToon();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ResponseCompressionOptions>>().Value;

        Assert.Contains("application/json", options.MimeTypes);
        Assert.Contains(ToonMediaTypes.Application, options.MimeTypes);
        Assert.Contains(ToonMediaTypes.Text, options.MimeTypes);
    }

    [Fact]
    public void AddToon_PreservesResponseCompressionMimeTypesConfiguredAfterToon()
    {
        var services = new ServiceCollection();

        services.AddControllers().AddToon();
        services.AddResponseCompression(options =>
        {
            options.MimeTypes = ["application/json"];
        });

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ResponseCompressionOptions>>().Value;

        Assert.Contains("application/json", options.MimeTypes);
        Assert.Contains(ToonMediaTypes.Application, options.MimeTypes);
        Assert.Contains(ToonMediaTypes.Text, options.MimeTypes);
    }

    [Fact]
    public void AddToon_RejectsNullConfigureCallback()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>("configure", () => services.AddToon(configure: null!));
    }
}
