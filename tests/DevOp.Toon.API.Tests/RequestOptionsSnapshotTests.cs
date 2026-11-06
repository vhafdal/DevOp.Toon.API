using System.Text;
using DevOp.Toon.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace DevOp.Toon.API.Tests;

public sealed class RequestOptionsSnapshotTests
{
    [Fact]
    public async Task Write_OverridesChangeAfterValidation_UsesUpdatedOptionsWithoutChangingOtherRequests()
    {
        using var services = global::DevOp.Toon.API.ToonServiceCollectionExtensions.AddToon(new ServiceCollection()).BuildServiceProvider();
        var first = Context(services, "custom-legacy");
        var filter = (IAsyncResourceFilter)Activator.CreateInstance(typeof(ToonOutputFormatter).Assembly.GetType("DevOp.Toon.API.ToonVersionValidationFilter")!, true)!;
        var action = new ActionContext(first, new RouteData(), new ActionDescriptor());
        var filters = Array.Empty<IFilterMetadata>();
        var configured = services.GetRequiredService<ToonServiceOptions>();
        await filter.OnResourceExecutionAsync(new ResourceExecutingContext(action, filters, new List<IValueProviderFactory>()), async () =>
        {
            first.Request.Headers["X-Toon-Option-ProtocolVersion"] = "4.4";
            await Write(first);
            return new ResourceExecutedContext(action, filters);
        });
        Assert.Equal("4.4", first.Response.Headers["X-Toon-Version"].ToString());
        Assert.Equal(ToonProtocolVersion.V4_4, configured.Encode.ProtocolVersion);
        var second = Context(services, "4.4");
        await Write(second);
        Assert.Equal("4.4", second.Response.Headers["X-Toon-Version"].ToString());
    }

    [Theory]
    [InlineData("add")]
    [InlineData("remove")]
    [InlineData("replace")]
    [InlineData("mutate-array")]
    public async Task Write_OptionHeadersChangeAfterValidation_RevalidatesCurrentValues(string change)
    {
        using var services = global::DevOp.Toon.API.ToonServiceCollectionExtensions.AddToon(new ServiceCollection()).BuildServiceProvider();
        var context = Context(services, "4.4");
        const string key = "X-Toon-Option-ByteArrayFormat";
        var values = new[] { "Base64String", "Base64String" };
        if (change == "remove") context.Request.Headers[key] = "NumericArray";
        else if (change == "mutate-array") context.Request.Headers[key] = new StringValues(values);
        else if (change == "replace") context.Request.Headers[key] = "Base64String";
        var filter = (IAsyncResourceFilter)Activator.CreateInstance(typeof(ToonOutputFormatter).Assembly.GetType("DevOp.Toon.API.ToonVersionValidationFilter")!, true)!;
        var action = new ActionContext(context, new RouteData(), new ActionDescriptor());
        var filters = Array.Empty<IFilterMetadata>();
        await filter.OnResourceExecutionAsync(new ResourceExecutingContext(action, filters, new List<IValueProviderFactory>()), async () =>
        {
            if (change == "remove") context.Request.Headers.Remove(key);
            else if (change == "mutate-array") values[1] = "NumericArray";
            else context.Request.Headers[key] = "NumericArray";
            await new ToonOutputFormatter().WriteResponseBodyAsync(new OutputFormatterWriteContext(context,
                (_, _) => TextWriter.Null, typeof(object), new { Data = new byte[] { 1, 2 } }), Encoding.UTF8);
            return new ResourceExecutedContext(action, filters);
        });
        Assert.Equal(change == "remove" ? "Data: AQI=" : "Data[2]: 1,2", Encoding.UTF8.GetString(((MemoryStream)context.Response.Body).ToArray()));
    }

    private static DefaultHttpContext Context(IServiceProvider services, string version)
    {
        var context = new DefaultHttpContext { RequestServices = services };
        context.Response.Body = new MemoryStream();
        context.Request.Headers["X-Toon-Option-ProtocolVersion"] = version;
        return context;
    }

    private static Task Write(HttpContext context) => new ToonOutputFormatter().WriteResponseBodyAsync(
        new OutputFormatterWriteContext(context, (_, _) => TextWriter.Null, typeof(object), new { Id = 42 }), Encoding.UTF8);
}
