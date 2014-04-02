using System;
namespace Applenium
{
    public class ResultModel
    {
        public bool Returnresult { get; set; }
        public string Message { get; set; }

        public ResultModel(bool result, string message)
        {
            Returnresult = result;
            Message = message;

        }
    }
}
