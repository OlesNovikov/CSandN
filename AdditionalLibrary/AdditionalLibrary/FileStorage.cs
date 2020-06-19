using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
