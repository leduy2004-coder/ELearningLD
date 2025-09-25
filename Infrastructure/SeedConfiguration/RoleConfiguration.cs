using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.SeedConfiguration
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasData(
                new Role
                {
                    Id = "d290f1ee-6c54-4b01-90e6-d701748f0851",
                    Name = "Admin",
                    Description = "Administrator with full system access",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = "admin-full-access"
                },
                new Role
                {
                    Id = "c290f1ee-6c54-4b01-90e6-d701748f0852",
                    Name = "Student",
                    Description = "Standard user with restricted access",
                    NormalizedName = "STUDENT",
                    ConcurrencyStamp = "student-restricted-access"
                },
                new Role
                {
                    Id = "b290f1ee-6c54-4b01-90e6-d701748f0853",
                    Name = "Teacher",
                    Description = "User with teaching-related access",
                    NormalizedName = "TEACHER",
                    ConcurrencyStamp = "teacher-teaching-access"
                }
            );
        }
    }
}
