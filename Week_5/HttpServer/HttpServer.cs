using System.Net;
using System.Text;
using System.Text.Json;

namespace HttpServer
{
    public class HttpServer
    {
        private readonly HttpListener _httpListener;
        private ServerSettings _serverSettings;

        public HttpServer()
        {
            _httpListener = new HttpListener();
        }

        public void Start()
        {
            if (!File.Exists(ServerSettings.SettingsPath))
            {
                Console.WriteLine("Не удалось найти файл настроек.");
                Stop();
            }
            else
            {
                if (_httpListener.IsListening) Console.WriteLine("Сервер уже запущен.");
                else
                {
                    var settingsFile = File.ReadAllBytes($"{ServerSettings.SettingsPath}");
                    _serverSettings = JsonSerializer.Deserialize<ServerSettings>(settingsFile);
                    _httpListener.Prefixes.Clear();

                    _httpListener.Prefixes.Add($"http://localhost:{_serverSettings.Port}/");

                    Console.WriteLine("Ожидание подключений...");
                    _httpListener.Start();

                    Console.WriteLine("Сервер запущен.");
                    Run();
                }
            }
        }

        public void Run()
        {
            while (_httpListener.IsListening)
            {
                Listen();
                Program.ExecuteCommand(this);
            }
        }

        private async Task Listen()
        {
            while (true)
            {
                HttpListenerContext listenerContext = await _httpListener.GetContextAsync();
                HttpListenerRequest request = listenerContext.Request;


                // получаем объект ответа
                HttpListenerResponse response = listenerContext.Response;

                byte[] buffer;

                if (Directory.Exists(_serverSettings.Path))
                {
                    var responseProvider = new ResponseProvider(listenerContext);
                    buffer = responseProvider.LoadFile(_serverSettings.Path);
                }
                else
                {
                    string err = $"Directory '{_serverSettings.Path}' not found";
                    buffer = Encoding.UTF8.GetBytes(err);
                }

                // получаем поток ответа и пишем в него ответ
                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                // закрываем поток
                output.Close();
            }
        }


        public void Stop()
        {
            // останавливаем прослушивание подключений
            _httpListener.Stop();
            Console.WriteLine("Сервер завершил работу.");
        }
    }
}