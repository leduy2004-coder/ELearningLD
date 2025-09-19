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
                    Description = "Administrator with full access",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = "Administrator with full access",
                },
                new Role
                {
                    Id = "c290f1ee-6c54-4b01-90e6-d701748f0852",
                    Name = "User",
                    Description = "Regular user with limited access",
                    NormalizedName = "USER",
                    ConcurrencyStamp = "Regular user with limited access",
                }
            );
        }
    }
}
