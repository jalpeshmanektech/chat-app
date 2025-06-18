using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Web.Data
{
     public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
     {
          public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
              : base(options)
          {
          }
          public DbSet<Message> Messages { get; set; }

          protected override void OnModelCreating(ModelBuilder builder)
          {
               base.OnModelCreating(builder);

               // Fix for MySQL: set max length for Identity columns
               builder.Entity<IdentityRole>(entity => {
                    entity.Property(m => m.Name).HasMaxLength(256);
                    entity.Property(m => m.NormalizedName).HasMaxLength(256);
               });
               builder.Entity<IdentityUser>(entity => {
                    entity.Property(m => m.UserName).HasMaxLength(256);
                    entity.Property(m => m.NormalizedUserName).HasMaxLength(256);
                    entity.Property(m => m.Email).HasMaxLength(256);
                    entity.Property(m => m.NormalizedEmail).HasMaxLength(256);
               });
          }
     }
}
