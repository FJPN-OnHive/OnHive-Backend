using OnHive.Core.Library.Enums.Common;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Common
{
    public class Response<T>
    {
        [JsonPropertyName("code")]
        public ResponseCode Code { get; set; } = ResponseCode.OK;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("payload")]
        public T? Payload { get; set; }

        public static Response<T> Create(T payload, ResponseCode code = ResponseCode.OK, string message = "") =>
            new Response<T> { Payload = payload, Code = code, Message = message };

        public static Response<T> Ok(string message = "") =>
            new Response<T> { Payload = default, Code = ResponseCode.OK, Message = message };

        public static Response<T> Ok(T payload, string message = "") =>
            new Response<T> { Payload = payload, Code = ResponseCode.OK, Message = message };

        public static Response<T> Empty(string message = "") =>
            new Response<T> { Payload = default, Code = ResponseCode.Empty, Message = message };

        public static Response<T> Error(T payload, string message) =>
            new Response<T> { Payload = payload, Code = ResponseCode.Error, Message = message };

        public static Response<T> Error(string message) =>
            new Response<T> { Payload = default, Code = ResponseCode.Error, Message = message };

        public static Response<T> Warning(T payload, string message) =>
            new Response<T> { Payload = payload, Code = ResponseCode.Warning, Message = message };

        public static Response<T> Warning(string message) =>
            new Response<T> { Payload = default, Code = ResponseCode.Warning, Message = message };

        public static Response<T> Duplicated(string message) =>
            new Response<T> { Payload = default, Code = ResponseCode.Duplicated, Message = message };

        public static Response<T> EmailNotValidated(string message) =>
            new Response<T> { Payload = default, Code = ResponseCode.EmailNotValidated, Message = message };

        public static Response<T> Unauthorized(string message) =>
            new Response<T> { Payload = default, Code = ResponseCode.Unauthorized, Message = message };

        public static Response<T> Invalid(string message) =>
            new Response<T> { Payload = default, Code = ResponseCode.Invalid, Message = message };
    }
}