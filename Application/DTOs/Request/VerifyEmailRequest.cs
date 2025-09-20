using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Request
{
    public class VerifyEmailRequest
    {
        public string Email { get; set; } = String.Empty;
        public string VerificationToken { get; set; } = String.Empty;
    }
}
