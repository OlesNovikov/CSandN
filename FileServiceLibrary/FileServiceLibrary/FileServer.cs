using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace FileServiceLibrary
{
    public class FileServer
    {
        private readonly string FILE_STORAGE = Directory.GetCurrentDirectory() + "\\File storage\\";

        private HttpListener httpListener;
        private Dictionary<int, string> DictionaryOfFiles;
        private Thread httpListenThread;
        private int maxFileID = 0;
        
        public FileServer(string prefix)
        {
            httpListener = new HttpListener();
            httpListener.Prefixes.Add(prefix);
            DictionaryOfFiles = new Dictionary<int, string>();
            httpListenThread = new Thread(StartListen);
            httpListenThread.IsBackground = true;
            httpListenThread.Start();
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

        }

        private void HandleHEADMethod(HttpListenerContext listenerContext)
        {

        }

        private void HandleDELETEMethod(HttpListenerContext listenerContext)
        {
            const int ERROR_CODE = 404;
            const int SUCCESS_CODE = 200;

            int removeID = int.Parse(listenerContext.Request.Url.LocalPath.Substring(1));

            using (listenerContext.Response.OutputStream)
            {
                if (FileExist(removeID))
                {
                    listenerContext.Response.StatusCode = SUCCESS_CODE;
                    Console.WriteLine(DateTime.Now.ToShortTimeString() + " File removed from storage: " + DictionaryOfFiles[removeID] + removeID.ToString());
                    string removePath = FILE_STORAGE + DictionaryOfFiles[removeID];
                    File.Delete(removePath);
                    DictionaryOfFiles.Remove(removeID);
                }
                else listenerContext.Response.StatusCode = ERROR_CODE;
            }
        }

        private bool FileExist(int fileID)
        {
            if (DictionaryOfFiles.ContainsKey(fileID)) return true;
            else return false;
        }

        private void SaveFileInStorage(string fileName, byte[] fileContent)
        {
            string fileSavePath = FILE_STORAGE + fileName;
            using (FileStream fileStream = new FileStream(fileSavePath, FileMode.Create))
            {
                fileStream.Write(fileContent, 0, fileContent.Length);
            }
            DictionaryOfFiles.Add(maxFileID, fileName);
            Console.WriteLine(DateTime.Now.ToShortTimeString() + " New file added to storage: " + fileName + " [" + maxFileID.ToString() + "]");
        }
    }
}