using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using AdditionalLibrary;

namespace FileServiceLibrary
{
    public class FileServer
    {
        private readonly static string FILE_SERVICE_PATH = "D:\\Oles\\БГУИР\\2 курс\\4 сем\\КСиС\\лабы\\ChatRep\\CSandN\\FileServiceLibrary\\FileServiceLibrary\\bin\\Debug";
        private readonly string FILE_STORAGE = FILE_SERVICE_PATH + "\\File storage\\";

        private const int ERROR_CODE = 404;
        private const int SUCCESS_CODE = 200;

        private HttpListener httpListener;
        private Dictionary<int, string> DictionaryOfFiles;
        public Dictionary<int, int> DictionaryOfSizes;
        private Thread httpListenThread;
        private int maxFileID = 0;
        
        public FileServer(string prefix)
        {
            httpListener = new HttpListener();
            httpListener.Prefixes.Add(prefix);
            DictionaryOfFiles = new Dictionary<int, string>();
            DictionaryOfSizes = new Dictionary<int, int>();
            httpListenThread = new Thread(StartListen);
            httpListenThread.IsBackground = true;
            httpListenThread.Start();

            FileStorage.SetupStorage(FILE_STORAGE);
        }

        private void StartListen()
        {
            httpListener.Start();
            Console.WriteLine("Server start listen HTTP requests...");

            while (true)
            {
                HttpListenerContext listenerContext = httpListener.GetContext();
                Console.WriteLine(DateTime.Now.ToShortTimeString() + " " + listenerContext.Request.HttpMethod.ToString() + " method request");
                IdentifyHttpRequest(listenerContext);
            }
        }

        private void IdentifyHttpRequest(HttpListenerContext listenerContext)
        {
            const string POST = "POST";
            const string GET = "GET";
            const string DELETE = "DELETE";
            const string HEAD = "HEAD";

            string httpMethod = listenerContext.Request.HttpMethod;

            if (httpMethod == POST) HandlePOSTMethod(listenerContext);
            if (httpMethod == GET) HandleGETMethod(listenerContext);
            if (httpMethod == HEAD) HandleHEADMethod(listenerContext);
            if (httpMethod == DELETE) HandleDELETEMethod(listenerContext);
        }

        private void HandlePOSTMethod(HttpListenerContext listenerContext)
        {
            string fileName = listenerContext.Request.Headers.Get("FileName");

            fileName = UniqueFileName(fileName);

            Stream inputStream = listenerContext.Request.InputStream;
            Encoding contentEncoding = listenerContext.Request.ContentEncoding;

            StreamReader reader = new StreamReader(inputStream, contentEncoding);
            string requestContent = reader.ReadToEnd();
            char[] content = requestContent.ToCharArray();

            int length = requestContent.Length - 118;
            byte[] fileContent = listenerContext.Request.ContentEncoding.GetBytes(content, 74, length);

            SaveFileInStorage(fileName, fileContent);

            byte[] buffer = Encoding.ASCII.GetBytes(maxFileID.ToString());
            using (listenerContext.Response.OutputStream)
            {
                listenerContext.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            maxFileID++;
        }

        private void HandleGETMethod(HttpListenerContext listenerContext)
        {
            int getFileID = int.Parse(listenerContext.Request.Url.LocalPath.Substring(1));
            
            using (listenerContext.Response.OutputStream)
            {
                if (FileIDExist(getFileID))
                {
                    listenerContext.Response.StatusCode = SUCCESS_CODE;
                    string fileName = DictionaryOfFiles[getFileID];
                    byte[] getFileContent = FileContent(getFileID, fileName);
                    listenerContext.Response.OutputStream.Write(getFileContent, 0, getFileContent.Length);
                    listenerContext.Response.Headers.Add("FileName", fileName);
                }
                else listenerContext.Response.StatusCode = ERROR_CODE;
            }
        }

        private byte[] FileContent(int getFileID, string fileName)
        {
            string filePath = FILE_STORAGE + fileName;

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                byte[] fileContent = new byte[fileStream.Length];
                fileStream.Read(fileContent, 0, fileContent.Length);
                return fileContent;
            }
        }

        private void HandleHEADMethod(HttpListenerContext listenerContext)
        {
            int fileInformationID = int.Parse(listenerContext.Request.Url.LocalPath.Substring(1));

            using (listenerContext.Response.OutputStream)
            {
                if (FileIDExist(fileInformationID))
                {
                    listenerContext.Response.StatusCode = SUCCESS_CODE;

                    string fileName = DictionaryOfFiles[fileInformationID];
                    string filePath = FILE_STORAGE + fileName;
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                    {
                        int fileSize = (int)fileStream.Length;
                        listenerContext.Response.Headers.Add("FileSize", fileSize.ToString());
                        listenerContext.Response.Headers.Add("FileName", fileName);
                    }
                }
                else listenerContext.Response.StatusCode = ERROR_CODE;
            } 
        }

        private void HandleDELETEMethod(HttpListenerContext listenerContext)
        {
            int removeID = int.Parse(listenerContext.Request.Url.LocalPath.Substring(1));

            using (listenerContext.Response.OutputStream)
            {
                if (FileIDExist(removeID))
                {
                    listenerContext.Response.StatusCode = SUCCESS_CODE;
                    Console.WriteLine(DateTime.Now.ToShortTimeString() + " " + DictionaryOfFiles[removeID] + " [" + removeID.ToString() + "] " + " removed from storage");
                    string removePath = FILE_STORAGE + DictionaryOfFiles[removeID];
                    File.Delete(removePath);

                    DictionaryOfFiles.Remove(removeID);
                    DictionaryOfSizes.Remove(removeID);
                }
                else listenerContext.Response.StatusCode = ERROR_CODE;
            }
        }

        private bool FileIDExist(int fileID)
        {
            if (DictionaryOfFiles.ContainsKey(fileID)) return true;
            else return false;
        }

        private bool FileNameExist(string fileName)
        {
            if (DictionaryOfFiles.ContainsValue(fileName)) return true;
            else return false;
        }

        private string UniqueFileName(string fileName)
        {
            string randomFileName = Path.GetRandomFileName();
            string uniqueFileName = randomFileName.Substring(0, 8) + "_" + fileName;
            return uniqueFileName;
        }

        private void SaveFileInStorage(string fileName, byte[] fileContent)
        {
            string fileSavePath = FILE_STORAGE + fileName;
            int fileSize;

            using (FileStream fileStream = new FileStream(fileSavePath, FileMode.Create))
            {
                fileStream.Write(fileContent, 0, fileContent.Length);
                fileSize = (int)fileStream.Length;
            }
            DictionaryOfFiles.Add(maxFileID, fileName);
            DictionaryOfSizes.Add(maxFileID, fileSize);

            Console.WriteLine(DateTime.Now.ToShortTimeString() + " " + fileName + " [" + maxFileID.ToString() + "] " + "added to storage");
        }
    }
}