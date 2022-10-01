using System.Net;
using System.Text;

namespace HttpServer
{
    public class HttpServer
    {
        private HttpListener listener;
        private byte[] buffer;

        public void Start()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8888/");
            listener.Start();
            Console.WriteLine("Ожидание подключений...");

            while (listener.IsListening)
            {
                Listen();
                Program.ExecuteCommand(this);
            }
        }

        private async Task Listen()
        {
            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                HttpListenerRequest request = context.Request;

                string? path = context.Request.Url?.LocalPath;
                // получаем объект ответа
                HttpListenerResponse response = context.Response;

                switch (path)
                {
                    case "/google":
                        buffer = LoadFile(context, "google.html");
                        break;
                    case "/style.css":
                        buffer = LoadFile(context, "style.css");
                        break;
                    case "/google.png":
                        buffer = LoadFile(context, "google.png");
                        break;
                    default:
                        buffer = NotFound(context);
                        ShowOutput(response);
                        Stop();
                        break;
                }
                ShowOutput(response);
            }
        }

        private void ShowOutput(HttpListenerResponse response)
        {
            // получаем поток ответа и пишем в него ответ
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            // закрываем поток
            output.Close();
        }

        static byte[] NotFound(HttpListenerContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            string err = "404 - not found";
            return Encoding.UTF8.GetBytes(err);
        }

        static byte[] LoadFile(HttpListenerContext context, string path)
        {
            return File.ReadAllBytes("/Users/user/Documents/ITIS/Info/HttpServer/" + path);
        }

        public void Stop()
        {
            // останавливаем прослушивание подключений
            listener.Stop();
            Console.WriteLine("Сервер завершил работу.");
        }
    }
}