using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace ImageUploader.Models
{
  public class User : IdentityUser
  {
    public IList<FilesModel> FilesModel { get; set; }

    public User()
    {
      this.FilesModel = new List<FilesModel>();
    }
  }
}
