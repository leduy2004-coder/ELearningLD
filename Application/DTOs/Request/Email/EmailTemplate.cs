using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Request.Email
{
    public class EmailTemplate
    {
        public string Subject { get; set; }
        public string HtmlBody { get; set; }
        public string TextBody { get; set; }
    }
}
