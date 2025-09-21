using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Request.AuthGG
{
    public class GoogleUserInfo
    {
        public string Sub { get; set; }  
        public string Email { get; set; }
        public string Name { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string Picture { get; set; }
        public bool EmailVerified { get; set; }
    }
}
