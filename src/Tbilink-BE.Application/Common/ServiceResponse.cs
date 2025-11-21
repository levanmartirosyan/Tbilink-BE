namespace Tbilink_BE.Models
{
    public class ServiceResponse<T>
    {
        public T? Data { get; set; }
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string? Message { get; set; }

        private ServiceResponse(
            T? data,
            bool isSuccess,
            int statusCode,
            string? message)
        {
            Data = data;
            IsSuccess = isSuccess;
            StatusCode = statusCode;
            Message = message;
        }

        public static ServiceResponse<T> Success(T? data, string? message, int statusCode = 200)
        {
            return new ServiceResponse<T>(
                data: data,
                isSuccess: true,
                statusCode: statusCode,
                message: message
            );
        }

        public static ServiceResponse<T> Fail(T? data, string message, int statusCode = 400)
        {
            return new ServiceResponse<T>(
                data: data,
                isSuccess: false,
                statusCode: statusCode,
                message:  message 
            );
        }

    }
}
