using Application.DTOs.Response;
using Application.Exceptions;
using System.Net;
using System.Text.Json;

namespace WebAPI.Exceptions
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            catch (Exception exception)
            {
                _logger.LogError(exception, "An unexpected error occurred");
                await HandleExceptionAsync(context, exception);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            ApiResponse response;
            HttpStatusCode statusCode;

            switch (exception)
            {
                case AppException appEx:
                    response = new ApiResponse
                    {
                        Code = appEx.ErrorCode.Code,
                        Message = appEx.ErrorCode.Message
                    };
                    statusCode = appEx.ErrorCode.StatusCode;
                    break;

                case UnauthorizedAccessException:
                    var unauthorizedError = ErrorCode.UNAUTHORIZED;
                    response = new ApiResponse
                    {
                        Code = unauthorizedError.Code,
                        Message = unauthorizedError.Message
                    };
                    statusCode = HttpStatusCode.Forbidden;
                    break;

                default:
                    var generalError = ErrorCode.UNCATEGORIZED_EXCEPTION;
                    response = new ApiResponse
                    {
                        Code = generalError.Code,
                        Message = generalError.Message
                    };
                    statusCode = HttpStatusCode.InternalServerError;
                    break;
            }

            context.Response.StatusCode = (int)statusCode;

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

}
