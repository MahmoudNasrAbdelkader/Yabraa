using System.Net;

namespace Yabraa.Helpers
{
    public class ResponseVM
    {
        public HttpStatusCode StatusCode { get; set; }
        public object? Data { get; set; }
        public string? ErrorMessageEn { get; set; }        
        public string? ErrorMessageAr { get; set; }


        //public object Error { get; set; }
        //public string OperationMessage { get; set; }

    }
}
