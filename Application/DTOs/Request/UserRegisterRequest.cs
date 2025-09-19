using Infrastructure.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.DTOs.Request
{
    public class UserRegisterRequest
    {
        public string FullName { get; set; } = String.Empty;

        [Required]
        [EmailAddress]

        public string Email { get; set; } = String.Empty;

        [Required]
        public string Password { get; set; } = String.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GenderType Gender { get; set; }
    }
}
