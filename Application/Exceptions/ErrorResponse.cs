using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Exceptions
{
    public class ErrorResponse
    {
        public string Titel { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
    }
}
