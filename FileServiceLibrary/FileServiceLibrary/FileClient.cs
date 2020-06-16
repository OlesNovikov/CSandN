using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileServiceLibrary
{
    public class FileClient
    {
        private const string SERVER_URI = "http://localhost:8888/";
        private const int MB = 1024 * 1024;
        private const int MAX_FILE_SIZE = 5 * MB;
        private const int MAX_TOTAL_SIZE = 3 * MAX_FILE_SIZE;

        private Dictionary<int, string> DictionaryOfFiles;
        private List<string> ListOfFilesExtensions = new List<string>() { ".txt", ".docx", ".png", ".jpg", ".jpeg", ".pdf" };
        private int TotalSize = 0;

        public FileClient()
        {
            DictionaryOfFiles = new Dictionary<int, string>();
        }

        public bool SizeFits(int fileSize)
        {
            int totalSize = TotalSize;
            if ((fileSize <= MAX_FILE_SIZE) && ((TotalSize += fileSize) <= MAX_TOTAL_SIZE)) return true;
            else return false;
        }

        public bool ExtensionExists(string extension)
        {
            if (ListOfFilesExtensions.Exists(x => x.Contains(extension))) return true;
            else return false;
        }

        private string UniqueFileName(string fileName)
        {
            string randomFileName = Path.GetRandomFileName();
            string uniqueFileName = randomFileName.Substring(0, 8) + "_" + fileName;
            return uniqueFileName;
        }

        private MultipartFormDataContent MIMEEncodedContent(string filePath)
        {
            MultipartFormDataContent encodedContent = new MultipartFormDataContent();
            ByteArrayContent byteArrayContent;
            byte[] byteArray;

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                int fileSize = (int)fileStream.Length;
                byteArray = new byte[fileSize];
                fileStream.Read(byteArray, 0, fileSize);
            }
            byteArrayContent = new ByteArrayContent(byteArray);
            encodedContent.Add(byteArrayContent);
            return encodedContent;
        }

        public async Task<int> LoadFileToService(string filePath)
        {
            const int ERROR_CODE = -200;
            try
            {
                string fileName = Path.GetFileName(filePath);
                string fileExtension = Path.GetExtension(filePath);

                using (HttpClient client = new HttpClient())
                {
                    HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, SERVER_URI + UniqueFileName(fileName));
                    httpRequestMessage.Content = MIMEEncodedContent(filePath);
                    httpRequestMessage.Headers.Add("FileName", fileName);

                    HttpResponseMessage httpResponseMessage = await client.SendAsync(httpRequestMessage);
                    string fileID = await httpResponseMessage.Content.ReadAsStringAsync();
                    return int.Parse(fileID);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("public async Task<int> LoadFileToService(string filePath)" + exception.Message);
                return ERROR_CODE;
            }
        }
    }
}