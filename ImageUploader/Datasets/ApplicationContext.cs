using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ImageUploader.Models;

namespace ImageUploader.Datasets
{
  public class ApplicationContext: IdentityDbContext<User>
  {
    public DbSet<FilesModel> FilesModels { get; set; }

    public ApplicationContext(DbContextOptions<ApplicationContext> options)
    : base(options)
    {
    }
  }
}
