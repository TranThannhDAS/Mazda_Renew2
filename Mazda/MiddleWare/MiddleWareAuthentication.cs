using Mazda;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mazda_Api.MiddleWare
{
    public class MiddleWareAuthentication
    {
        public RequestDelegate _next;
        public MiddleWareAuthentication(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            if (context.Response.StatusCode == 401)
            {
                // Chuỗi JSON bạn muốn trả về
                string jsonMessage = "{\"message\":\"Access Token hết hạn\"}";

                // Thiết lập header Content-Type để báo cho trình duyệt biết đây là JSON
                context.Response.ContentType = "application/json";

                // Thiết lập mã trạng thái là 401 Unauthorized
                context.Response.StatusCode = 401;

                // Gửi nội dung JSON về client
                await context.Response.WriteAsync(jsonMessage);
            }
            else
            {
                // Nếu không phải mã trạng thái 401, chuyển tiếp yêu cầu đến middleware tiếp theo trong pipeline
                await _next(context);
            }
        }

    }
}
