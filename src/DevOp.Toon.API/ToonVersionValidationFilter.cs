using DevOp.Toon.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace DevOp.Toon.API;

internal sealed class ToonVersionValidationFilter : IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var request = context.HttpContext.Request;
        if (request.Headers.TryGetValue("X-Toon-Version", out var values))
        {
            if (values.Count != 1 || values[0]!.Contains(',')) { Fail(context, 400, "Specify exactly one body protocol version."); return; }
            if (!ToonProtocolVersions.TryParse(values[0], out var version))
            {
                var wellFormed = System.Text.RegularExpressions.Regex.IsMatch(values[0] ?? "", @"^(?:[0-9]+\.[0-9]+|custom-[a-z0-9.-]+)$");
                Fail(context, wellFormed ? 415 : 400, "Invalid or unsupported TOON body protocol version."); return;
            }
            var configured = context.HttpContext.RequestServices.GetService<ToonServiceOptions>()?.Decode.ProtocolVersion ?? ToonProtocolVersion.Auto;
            if (configured != ToonProtocolVersion.Auto && configured != version) { Fail(context, 400, "Body protocol version conflicts with configured decoder."); return; }
        }
        if (request.Headers.Keys.Any(key => key.StartsWith("X-Toon-Option-", StringComparison.OrdinalIgnoreCase)))
        {
            try { ToonOutputFormatter.PrepareEncodeOptions(context.HttpContext); }
            catch (ArgumentException error)
            {
                Fail(context, error is ToonOutputFormatter.NotSupportedVersionException ? 406 : 400, error.Message); return;
            }
        }
        await next();
    }

    private static void Fail(ResourceExecutingContext context, int status, string detail)
    {
        context.Result = new JsonResult(new ProblemDetails { Status = status, Title = "Invalid TOON version options", Detail = detail })
        {
            StatusCode = status,
            ContentType = "application/problem+json"
        };
    }
}
