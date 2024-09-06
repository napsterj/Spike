using System.Net;

namespace Spike.Domain.Models.Response
{
    public abstract class BaseResponse
    {
        public string StatusMessage { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string[] ModelStateErrors { get; set; }
    }
}
