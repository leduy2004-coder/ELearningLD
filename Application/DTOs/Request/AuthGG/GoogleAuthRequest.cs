using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Request.AuthGG
{
    public class GoogleAuthRequest
    {
        public string IdToken { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
    }
}
