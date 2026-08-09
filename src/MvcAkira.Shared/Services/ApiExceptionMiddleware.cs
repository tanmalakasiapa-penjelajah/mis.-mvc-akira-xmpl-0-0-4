using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MvcAkira.Shared.Services;

/// <summary>
/// Mengubah ApiException menjadi HTTP status yang sesuai (bukan 500).
/// ApiException lain diteruskan agar BecomeDeveloperExceptionPage menanganinya.
/// </summary>
public class ApiExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionMiddleware> _logger;

    public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException ex)
        {
            _logger.LogInformation("ApiException status={Status} code={Code} msg={Msg}",
                ex.StatusCode, ex.Code, ex.Message);
            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsync(JsonSerializer.Serialize(
                new { error = ex.Code, message = ex.Message }, JsonOpts));
        }
    }
}