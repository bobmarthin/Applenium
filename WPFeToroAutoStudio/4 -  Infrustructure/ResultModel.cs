using System;
namespace Applenium
{
    public class ResultModel
    {
        public bool Returnresult { get; set; }
        public string Message { get; set; }
        public Exception exc { get; set; }

        public ResultModel(bool result, string message, Exception ex)
        {
            Returnresult = result;
            Message = message;
            exc = ex;

        }
    }
}
