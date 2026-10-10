using System.Net;
using System.Text.Json;

namespace CollegeSchedule.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Необработанное исключение: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            var statusCode = ex switch
            {
                KeyNotFoundException => (int)HttpStatusCode.NotFound,          // 404
                ArgumentException or ArgumentOutOfRangeException
                                     => (int)HttpStatusCode.BadRequest,        // 400
                _ => (int)HttpStatusCode.InternalServerError // 500
            };

            context.Response.StatusCode = statusCode;

            var response = new
            {
                error = ex.Message
            };

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
    }
}