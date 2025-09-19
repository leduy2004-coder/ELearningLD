using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.DTOs.Response
{
    public class ApiResponse<T>
    {
        public int Code { get; set; }
        public string Message { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T Result { get; set; }

        public ApiResponse()
        {
        }

        public ApiResponse(int code, string message, T result = default)
        {
            Code = code;
            Message = message;
            Result = result;
        }

        public static ApiResponse<T> Success(T result = default, string message = "Success")
        {
            return new ApiResponse<T>(1000, message, result);
        }

        public static ApiResponse<T> Error(int code, string message)
        {
            return new ApiResponse<T>(code, message);
        }
    }

    // Non-generic version for backward compatibility
    public class ApiResponse : ApiResponse<object>
    {
    }
}