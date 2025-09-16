using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class RegisterUserDTO
    {
        [Required]
        public string? UserName { get; set; } = String.Empty;

        [Required, EmailAddress]
        public string? Email { get; set; } = String.Empty;
        [Required]
        public string? Password { get; set; } = String.Empty;
        [Required, Compare(nameof(Password))]
        public string? ConfirmPassword { get; set; } = String.Empty;
    }
}
