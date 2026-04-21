using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;

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

    private static OutputFormatterWriteContext CreateContext()
    {
        return new OutputFormatterWriteContext(
            new DefaultHttpContext(),
            (_, _) => TextWriter.Null,
            typeof(object),
            new { Id = 1 });
    }
}
