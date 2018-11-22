using System;

namespace ImageUploader.Models
{
  public class FilesModel
  {
    public string FilesModelId { get; set; }

    public string UserId { get; set; }

    public string FileName { get; set; }

    public string FilePath { get; set; }

    public DateTime DateAdded { get; set; }
  }
}
