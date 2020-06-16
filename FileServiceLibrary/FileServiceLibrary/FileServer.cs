using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileServiceLibrary
{
    public class FileServer
    {
        private HttpListener httpListener;
        private Dictionary<int, string> DictionaryOfFiles;
        private Thread httpListenThread;
        private int maxFileID = 0;
        
        public FileServer(string prefix)
        {
            httpListener = new HttpListener();
            httpListener.Prefixes.Add(prefix);
            DictionaryOfFiles = new Dictionary<int, string>();
            httpListenThread = new Thread(ListenHttp);
        }

        private void ListenHttp()
        {
            httpListener.Start();
            Console.WriteLine("Server start listen HTTP requests...");

            while (true)
            {
                HttpListenerContext listenerContext = httpListener.GetContext();
                Console.WriteLine(DateTime.Now.ToShortTimeString() + " " + listenerContext.Request.HttpMethod.ToString());
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
            string fileName = listenerContext.Request.Url.LocalPath.Substring(1);
            Stream inputStream = listenerContext.Request.InputStream;
            Encoding contentEncoding = listenerContext.Request.ContentEncoding;

            StreamReader reader = new StreamReader(inputStream, contentEncoding);
            string requestContent = reader.ReadToEnd();
            byte[] fileContent = listenerContext.Request.ContentEncoding.GetBytes(requestContent);
            DictionaryOfFiles.Add(maxFileID, fileContent);
        }

        private void HandleGETMethod(HttpListenerContext listenerContext)
        {

        }

        private void HandleHEADMethod(HttpListenerContext listenerContext)
        {

        }

        private void HandleDELETEMethod(HttpListenerContext listenerContext)
        {

        }
    }
}
