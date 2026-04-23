using System.Text;
using DevOp.Toon;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;

namespace DevOp.Toon.API.Tests;

public sealed class ToonOutputFormatterTests
{
    [Fact]
    public void CanWriteResult_RejectsToonRequestContentTypeWhenAcceptRequestsJson()
    {
        var context = CreateContext();
        context.HttpContext.Request.ContentType = ToonOutputFormatter.MediaTypeApplication;
        context.HttpContext.Request.Headers.Accept = "application/json";

        var formatter = new ToonOutputFormatter();

        Assert.False(formatter.CanWriteResult(context));
    }

    [Fact]
    public void CanWriteResult_AcceptsToonAcceptHeader()
    {
        var context = CreateContext();
        context.HttpContext.Request.Headers.Accept = ToonOutputFormatter.MediaTypeApplication;

        var formatter = new ToonOutputFormatter();

        Assert.True(formatter.CanWriteResult(context));
    }

    [Fact]
    public void CanWriteResult_FallsBackToRequestContentTypeWhenAcceptIsAbsent()
    {
        var context = CreateContext();
        context.HttpContext.Request.ContentType = ToonOutputFormatter.MediaTypeText;

        var formatter = new ToonOutputFormatter();

        Assert.True(formatter.CanWriteResult(context));
    }

    [Fact]
    public void CanWriteResult_AcceptsWildcardAcceptHeader()
    {
        var context = CreateContext();
        context.HttpContext.Request.Headers.Accept = "*/*";

        var formatter = new ToonOutputFormatter();

        Assert.True(formatter.CanWriteResult(context));
    }

    [Fact]
    public void CanWriteResult_AcceptsExplicitFormatterContentType()
    {
        var context = CreateContext();
        context.ContentType = ToonOutputFormatter.MediaTypeApplication;
        context.HttpContext.Request.Headers.Accept = "application/json";

        var formatter = new ToonOutputFormatter();

        Assert.True(formatter.CanWriteResult(context));
    }

    [Fact]
    public async Task WriteResponseBodyAsync_UsesConfiguredByteArrayFormatDefaults()
    {
        var context = CreateContext(new BytePayload { Data = [1, 2, 3] });
        context.HttpContext.RequestServices = new ServiceCollection()
            .AddToon(options => options.Encode.ByteArrayFormat = ToonByteArrayFormat.NumericArray)
            .BuildServiceProvider();

        var formatter = new ToonOutputFormatter();

        await formatter.WriteResponseBodyAsync(context, Encoding.UTF8);

        Assert.Equal("Data[3]: 1,2,3", ReadResponseBody(context.HttpContext));
    }

    [Fact]
    public async Task WriteResponseBodyAsync_AllowsRequestByteArrayFormatOverride()
    {
        var context = CreateContext(new BytePayload { Data = [1, 2, 3] });
        context.HttpContext.RequestServices = new ServiceCollection()
            .AddToon(options => options.Encode.ByteArrayFormat = ToonByteArrayFormat.NumericArray)
            .BuildServiceProvider();
        context.HttpContext.Request.Headers["X-Toon-Option-ByteArrayFormat"] = nameof(ToonByteArrayFormat.Base64String);

        var formatter = new ToonOutputFormatter();

        await formatter.WriteResponseBodyAsync(context, Encoding.UTF8);

        Assert.Equal("Data: AQID", ReadResponseBody(context.HttpContext));
    }

    private static OutputFormatterWriteContext CreateContext(object? value = null)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        return new OutputFormatterWriteContext(
            httpContext,
            (_, _) => TextWriter.Null,
            value?.GetType() ?? typeof(object),
            value ?? new { Id = 1 });
    }

    private static string ReadResponseBody(HttpContext httpContext)
    {
        httpContext.Response.Body.Position = 0;
        using var reader = new StreamReader(httpContext.Response.Body, Encoding.UTF8, leaveOpen: true);
        return reader.ReadToEnd();
    }

    private sealed class BytePayload
    {
        public byte[] Data { get; set; } = [];
    }
}
