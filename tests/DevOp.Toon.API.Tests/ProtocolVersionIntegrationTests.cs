using System.Net;
using System.Net.Http.Headers;
using System.Text;
using DevOp.Toon.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace DevOp.Toon.API.Tests;

public sealed class ProtocolVersionIntegrationTests
{
    [Fact]
    public async Task Post_ExplicitVersionWithComment_AcceptsBodyAndEmitsMetadata()
    {
        using var server = Server();
        using var client = server.CreateClient();
        using var request = Post("# exported data\nId: 42\nName: Ada", "4.4");
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("4.4", Assert.Single(response.Headers.GetValues("X-Toon-Version")));
        var text = await response.Content.ReadAsStringAsync();
        var value = ToonDecoder.Decode<ProtocolEchoModel>(text, new ToonDecodeOptions { ProtocolVersion = ToonProtocolVersion.V4_4, Indent = 1 });
        Assert.Equal(42, value!.Id);
        Assert.Equal("Ada", value.Name);
    }

    [Fact]
    public async Task Post_AmbiguousCommentWithoutVersion_ReturnsBadRequest()
    {
        using var server = Server();
        using var client = server.CreateClient();
        using var request = Post("# exported data\nId: 42", null);
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("nonsense", HttpStatusCode.BadRequest)]
    [InlineData("Auto", HttpStatusCode.BadRequest)]
    [InlineData("9.9", HttpStatusCode.UnsupportedMediaType)]
    [InlineData("custom-9.9", HttpStatusCode.UnsupportedMediaType)]
    public async Task Post_InvalidBodyVersion_ReturnsProtocolSpecificStatus(string version, HttpStatusCode expected)
    {
        using var server = Server();
        using var client = server.CreateClient();
        using var request = Post("Id: 42", version);
        using var response = await client.SendAsync(request);
        Assert.Equal(expected, response.StatusCode);
    }

    [Theory]
    [InlineData("4.4", "4.4")]
    [InlineData("4.4", "4.0")]
    public async Task Post_MultipleBodyVersions_ReturnsBadRequest(string first, string second)
    {
        using var server = Server();
        using var client = server.CreateClient();
        using var request = Post("Id: 42", null);
        request.Headers.TryAddWithoutValidation("X-Toon-Version", new[] { first, second });
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_BodyVersionConflictsWithConfiguredDecoder_ReturnsBadRequest()
    {
        using var server = Server(ToonProtocolVersion.V4_1);
        using var client = server.CreateClient();
        using var request = Post("Id: 42", "4.4");
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("custom-legacy", "4.4", ToonProtocolVersion.V4_4)]
    [InlineData("4.4", "custom-legacy", ToonProtocolVersion.CustomLegacy)]
    [InlineData("4.0", "custom-4.4", ToonProtocolVersion.CustomV4_4)]
    public async Task Post_RequestAndResponseVersionsDiffer_UsesIndependentSelections(string bodyVersion, string responseVersion, ToonProtocolVersion decoder)
    {
        using var server = Server();
        using var client = server.CreateClient();
        using var request = Post("Id: 42\nName: Ada", bodyVersion);
        request.Headers.TryAddWithoutValidation("X-Toon-Option-ProtocolVersion", responseVersion);
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(responseVersion, Assert.Single(response.Headers.GetValues("X-Toon-Version")));
        var model = ToonDecoder.Decode<ProtocolEchoModel>(await response.Content.ReadAsStringAsync(), new ToonDecodeOptions { ProtocolVersion = decoder, Indent = 1 });
        Assert.Equal(42, model!.Id);
        Assert.Equal("Ada", model.Name);
    }

    [Theory]
    [InlineData("invalid", HttpStatusCode.BadRequest)]
    [InlineData("9.9", HttpStatusCode.NotAcceptable)]
    public async Task Get_InvalidResponseVersion_ReturnsErrorBeforeWritingModel(string version, HttpStatusCode expected)
    {
        using var server = Server();
        using var client = server.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/protocol/echo");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/toon"));
        request.Headers.TryAddWithoutValidation("X-Toon-Option-ProtocolVersion", version);
        using var response = await client.SendAsync(request);
        Assert.Equal(expected, response.StatusCode);
        Assert.False(response.Headers.Contains("X-Toon-Version"));
        Assert.DoesNotContain("Id: 42", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("X-Toon-Version", "9.9", 415)]
    [InlineData("X-Toon-Version", "invalid", 400)]
    [InlineData("X-Toon-Option-ProtocolVersion", "9.9", 406)]
    [InlineData("X-Toon-Option-ProtocolVersion", "invalid", 400)]
    public async Task Post_UnsupportedOrMalformedVersion_DoesNotReadRequestBody(string header, string value, int expected)
    {
        using var server = Server();
        using var body = new ReadProbeStream();
        var context = await server.SendAsync(http =>
        {
            http.Request.Method = "POST";
            http.Request.Path = "/protocol/echo";
            http.Request.ContentType = "application/toon";
            http.Request.ContentLength = 12;
            http.Request.Headers.Accept = "application/toon";
            http.Request.Headers[header] = value;
            http.Request.Body = body;
        });
        Assert.Equal(expected, context.Response.StatusCode);
        Assert.False(body.ReadAttempted);
    }

    [Theory]
    [InlineData("X-Toon-Version")]
    [InlineData("X-Toon-Option-ProtocolVersion")]
    public async Task Post_MultipleVersions_DoesNotReadRequestBody(string header)
    {
        using var server = Server();
        using var body = new ReadProbeStream();
        var context = await server.SendAsync(http =>
        {
            http.Request.Method = "POST";
            http.Request.Path = "/protocol/echo";
            http.Request.ContentType = "application/toon";
            http.Request.ContentLength = 12;
            http.Request.Headers.Accept = "application/toon";
            http.Request.Headers[header] = new StringValues(new[] { "4.4", "4.0" });
            http.Request.Body = body;
        });
        Assert.Equal(400, context.Response.StatusCode);
        Assert.False(body.ReadAttempted);
    }

    [Theory]
    [InlineData("100000000")]
    [InlineData("2147483647")]
    [InlineData("17")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("true")]
    [InlineData("1,2")]
    public async Task Post_InvalidResponseIndent_DoesNotReadRequestBody(string indent)
    {
        using var server = Server();
        using var body = new ReadProbeStream();
        var context = await server.SendAsync(http =>
        {
            http.Request.Method = "POST";
            http.Request.Path = "/protocol/echo";
            http.Request.ContentType = "application/toon";
            http.Request.ContentLength = 12;
            http.Request.Headers.Accept = "application/toon";
            http.Request.Headers["X-Toon-Option-Indent"] = indent;
            http.Request.Body = body;
        });
        Assert.Equal(400, context.Response.StatusCode);
        Assert.False(body.ReadAttempted);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("16")]
    public async Task Get_BoundedResponseIndent_AcceptsWidth(string indent)
    {
        using var server = Server();
        using var client = server.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/protocol/echo");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/toon"));
        request.Headers.TryAddWithoutValidation("X-Toon-Option-Indent", indent);
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("4.4", Assert.Single(response.Headers.GetValues("X-Toon-Version")));
    }

    private static TestServer Server(ToonProtocolVersion decode = ToonProtocolVersion.Auto)
        => new(new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddControllers().AddApplicationPart(typeof(ProtocolEchoController).Assembly);
                global::DevOp.Toon.API.ToonServiceCollectionExtensions.AddToon(services, options => options.Decode.ProtocolVersion = decode, useAsDefaultFormatter: true);
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints => endpoints.MapControllers());
            }));

    private static HttpRequestMessage Post(string text, string? version)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/protocol/echo")
        {
            Content = new StringContent(text, Encoding.UTF8, "application/toon")
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/toon"));
        if (version != null) request.Headers.TryAddWithoutValidation("X-Toon-Version", version);
        return request;
    }

    private sealed class ReadProbeStream : Stream
    {
        internal bool ReadAttempted { get; private set; }
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override int Read(byte[] buffer, int offset, int count) { ReadAttempted = true; throw new InvalidOperationException("Version validation must precede body reads."); }
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        { ReadAttempted = true; return Task.FromException<int>(new InvalidOperationException("Version validation must precede body reads.")); }
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        { ReadAttempted = true; return ValueTask.FromException<int>(new InvalidOperationException("Version validation must precede body reads.")); }
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}

[ApiController]
[Route("protocol/echo")]
public sealed class ProtocolEchoController : ControllerBase
{
    [HttpGet]
    public ProtocolEchoModel Get() => new() { Id = 42, Name = "Ada" };

    [HttpPost]
    public ProtocolEchoModel Post([FromBody] ProtocolEchoModel model) => model;
}

public sealed class ProtocolEchoModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
}
