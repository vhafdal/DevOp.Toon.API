using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace DevOp.Toon.API.Tests;

public sealed class ToonInputFormatterTests
{
    [Fact]
    public async Task ReadRequestBodyAsync_UnwrapsTypedDecodeExceptions()
    {
        var formatter = new ToonInputFormatter();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().AddToon().BuildServiceProvider()
        };
        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("not valid toon"));

        var modelState = new ModelStateDictionary();
        var metadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(SampleModel));
        var context = new InputFormatterContext(
            httpContext,
            modelName: string.Empty,
            modelState,
            metadata,
            (_, encoding) => new StreamReader(httpContext.Request.Body, encoding));

        var result = await formatter.ReadRequestBodyAsync(context, Encoding.UTF8);

        Assert.True(result.HasError);
        var error = Assert.Single(modelState[string.Empty]!.Errors);
        Assert.IsNotType<System.Reflection.TargetInvocationException>(error.Exception);
    }

    private sealed class SampleModel
    {
        public int Id { get; set; }
    }
}
