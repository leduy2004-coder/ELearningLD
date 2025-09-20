using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Request
{
    public class ResendVerificationRequest
    {
        public string Email { get; set; } = String.Empty;
    }
}
