
namespace Application.Exceptions
{
    public class AppException : Exception
    {
        public ErrorCode ErrorCode { get; }

        public AppException(ErrorCode errorCode) : base(errorCode.Message)
        {
            ErrorCode = errorCode;
        }

        public AppException(ErrorCode errorCode, Exception innerException)
            : base(errorCode.Message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
