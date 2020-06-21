using System.IO;

namespace AdditionalLibrary
{
    public static class FileStorage
    {
        public static void SetupStorage(string fileStoragePath)
        {
            if (Directory.Exists(fileStoragePath))
            {
                DirectoryInfo fileStorageDirectory = new DirectoryInfo(fileStoragePath);
                foreach (var file in fileStorageDirectory.GetFiles()) file.Delete();
            }
            else Directory.CreateDirectory(fileStoragePath);
        }
    }
}
