using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("Roles")]
    public class Role : IdentityRole
    {
        public string? Description { get; set; }
    }
}
