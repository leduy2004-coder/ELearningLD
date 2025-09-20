using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.Exceptions
{
    public class ErrorCode
    {
        public int Code { get; }
        public string Message { get; }
        public HttpStatusCode StatusCode { get; }

        private ErrorCode(int code, string message, HttpStatusCode statusCode)
        {
            Code = code;
            Message = message;
            StatusCode = statusCode;
        }

        // Định nghĩa các error codes
        public static readonly ErrorCode UNCATEGORIZED_EXCEPTION =
            new(9999, "Uncategorized error", HttpStatusCode.InternalServerError);

        public static readonly ErrorCode INVALID_USERNAME =
            new(1001, "FullName existed", HttpStatusCode.BadRequest);

        public static readonly ErrorCode USER_EXISTED =
            new(1002, "User existed", HttpStatusCode.BadRequest);

        public static readonly ErrorCode PASSWORD_WRONG =
            new(1003, "Incorrect current password", HttpStatusCode.BadRequest);

        public static readonly ErrorCode PASSWORD_NOT_SAME =
            new(1004, "New password and confirmation password do not match", HttpStatusCode.BadRequest);

        public static readonly ErrorCode INVALID_PASSWORD =
            new(1005, "Password must be at least {min} characters", HttpStatusCode.BadRequest);

        public static readonly ErrorCode USER_NOT_EXISTED =
            new(1006, "User not existed", HttpStatusCode.NotFound);

        public static readonly ErrorCode UNAUTHENTICATED =
            new(1007, "Unauthenticated", HttpStatusCode.Unauthorized);

        public static readonly ErrorCode UNAUTHORIZED =
            new(1008, "You do not have permission", HttpStatusCode.Forbidden);

        public static readonly ErrorCode TOKEN_INVALID =
            new(1009, "Invalid token", HttpStatusCode.BadRequest);

        public static readonly ErrorCode RE_TOKEN_EXPIRED =
            new(1010, "Expired refresh token", HttpStatusCode.BadRequest);

        public static readonly ErrorCode TOKEN_EXPIRED =
            new(1011, "Expired refresh token", HttpStatusCode.BadRequest);

        public static readonly ErrorCode REVOKE_FAIL =
            new(1012, "Revoke failed", HttpStatusCode.BadRequest);

        public static readonly ErrorCode EMAIL_NOT_VERIFIED =
            new(1013, "Email not verified", HttpStatusCode.InternalServerError);

        public static readonly ErrorCode EMAIL_SEND_FAILED =
            new(1014, "Failed to send email", HttpStatusCode.InternalServerError);
    }
}
