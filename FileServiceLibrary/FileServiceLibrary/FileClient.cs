using AdditionalLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileServiceLibrary
{
    public class FileClient
    {
        private const int SUCCESS_CODE = 200;
        private const int ERROR_CODE = 404;
        private const string SERVER_URI = "http://localhost:8080/";
        private const int MB = 1024 * 1024;
        private const int MAX_FILE_SIZE = 5 * MB;
        private const int MAX_TOTAL_SIZE = 3 * MAX_FILE_SIZE;
        private readonly static string SAVE_FILE_PATH = Directory.GetCurrentDirectory() + "\\File storage\\";

        public Dictionary<int, string> DictionaryOfFiles;
        private List<string> ListOfFilesExtensions = new List<string>() { ".txt", ".docx", ".png", ".jpg", ".jpeg", ".pdf", ".rar" };
        public int TotalSize = 0;

        public FileClient()
        {
            DictionaryOfFiles = new Dictionary<int, string>();
            FileStorage.SetupStorage(SAVE_FILE_PATH);
        }

        public bool SizeFits(int fileSize)
        {
            int totalSize = TotalSize;
            if ((fileSize <= MAX_FILE_SIZE) && ((totalSize += fileSize) <= MAX_TOTAL_SIZE)) return true;
            else return false;
        }

        public bool ExtensionExists(string extension)
        {
            if (ListOfFilesExtensions.Exists(x => x.Contains(extension))) return true;
            else return false;
        }

        private MultipartFormDataContent MIMEEncodedContent(string filePath)
        {
            MultipartFormDataContent encodedContent = new MultipartFormDataContent();
            ByteArrayContent byteArrayContent;
            byte[] byteArray;

            using (FileStream fileStream = File.OpenRead(filePath))
            {
                byteArray = new byte[fileStream.Length];
                fileStream.Read(byteArray, 0, byteArray.Length);
            }
            byteArrayContent = new ByteArrayContent(byteArray);
            encodedContent.Add(byteArrayContent);
            return encodedContent;
        }

        public async Task<int> LoadFileToService(string filePath)
        {
            try
            {
                string fileName = Path.GetFileName(filePath);

                using (HttpClient client = new HttpClient())
                {
                    
                    HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, SERVER_URI);
                    httpRequestMessage.Content = MIMEEncodedContent(filePath);
                    httpRequestMessage.Headers.Add("FileName", fileName); 
                    
                    HttpResponseMessage httpResponseMessage = await client.SendAsync(httpRequestMessage); 
                    if (httpResponseMessage.IsSuccessStatusCode)
                    {
                        string fileID = await httpResponseMessage.Content.ReadAsStringAsync();
                        DictionaryOfFiles.Add(int.Parse(fileID), fileName);
                        return int.Parse(fileID);
                    }
                    else return ERROR_CODE;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("public async Task<int> LoadFileToService(string filePath). " + exception.Message);
                return ERROR_CODE;
            }
        }

        public async Task<int> RemoveFileFromService(int fileID)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, SERVER_URI + fileID);
                HttpResponseMessage httpResponseMessage = await client.SendAsync(httpRequestMessage);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    DictionaryOfFiles.Remove(fileID);
                    return SUCCESS_CODE;
                }
                else return ERROR_CODE;
            }
        }

        public async Task<int> GetFileSize(int fileID)
        {
            int fileSize = 0;
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Head, SERVER_URI + fileID);
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage httpResponseMessage = await client.SendAsync(httpRequestMessage);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var fileSizeHeadersValue = httpResponseMessage.Headers.GetValues("FileSize");
                    fileSize = int.Parse(fileSizeHeadersValue.First());
                    return fileSize;
                }
            }
            return fileSize;
        }

        public async Task<string> GetFileName(int fileID)
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Head, SERVER_URI + fileID);
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage httpResponseMessage = await client.SendAsync(httpRequestMessage);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var fileNameHeadersValue = httpResponseMessage.Headers.GetValues("FileName");
                    string fileName = fileNameHeadersValue.First();
                    return fileName;
                }
                return "";
            }
        }

        public async Task<string> DownloadFileFromService(int fileID)
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, SERVER_URI + fileID);

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage httpResponseMessage = await client.SendAsync(httpRequestMessage);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    string fileName = await GetFileName(fileID);
                    string clientFileName = fileName.Substring(9);
                    string filePath = SAVE_FILE_PATH + clientFileName;

                    byte[] fileContent = await httpResponseMessage.Content.ReadAsByteArrayAsync();

                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        fileStream.Write(fileContent, 0, fileContent.Length);
                    }

                    return clientFileName;
                }
                else return "";
            }
        }
    }
}