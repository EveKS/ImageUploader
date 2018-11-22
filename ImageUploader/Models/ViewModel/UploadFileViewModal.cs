using Microsoft.AspNetCore.Http;

namespace ImageUploader.Models.ViewModel
{
  public class UploadFileViewModal
  {
    public IFormFileCollection UploadFiles { get; set; }
  }
}
