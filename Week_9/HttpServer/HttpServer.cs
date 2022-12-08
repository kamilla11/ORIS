using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using HttpServer.Attributes;

namespace HttpServer
{
    public class HttpServer
    {
        private readonly HttpListener _httpListener;
        private ServerSettings _serverSettings;
        private DBSettings _dbSettings;

        public HttpServer()
        {
            _httpListener = new HttpListener();
            _dbSettings = new DBSettings();
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

                    var dbSettingsFile = File.ReadAllBytes($"{DBSettings.SettingsPath}");
                    _dbSettings = JsonSerializer.Deserialize<DBSettings>(dbSettingsFile);

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
                var _httpContext = await _httpListener.GetContextAsync();
                HttpListenerRequest request = _httpContext.Request;

                // получаем объект ответа
                HttpListenerResponse response = _httpContext.Response;

                byte[] buffer;
                var responseProvider = new ResponseProvider(_httpContext);

                if (!responseProvider.FilesHandler(_serverSettings, out buffer) &&
                    !MethodHandler(_httpContext, out buffer))
                {
                    buffer = responseProvider.NotFound();
                }

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

        private bool MethodHandler(HttpListenerContext _httpContext, out byte[] buffer)
        {
            bool isSaveAccountMethod = false;
            buffer = null;
            // объект запроса
            HttpListenerRequest request = _httpContext.Request;

            // объект ответа
            HttpListenerResponse response = _httpContext.Response;

            if (_httpContext.Request.Url.Segments.Length < 2) return false;

            string controllerName = _httpContext.Request.Url.Segments[1].Replace("/", "");
            string methodName = _httpContext.Request.Url.Segments[2].Replace("/", "");

            string[] strParams = { };
            object[] queryParams = { };

            Type controller;
            if (!TryGetController(out controller, controllerName)) return false;

            MethodInfo method;
            if (!TryGetMethod(out method, controller, methodName, _httpContext, out queryParams)) return false;


            if (_httpContext.Request.Url.Segments.Length > 3 && queryParams.Length == 0)
            {
                strParams = _httpContext.Request.Url
                    .Segments
                    .Skip(3)
                    .Select(s => s.Replace("/", ""))
                    .ToArray();

                queryParams = method.GetParameters()
                    .Select((p, i) => Convert.ChangeType(strParams[i], p.ParameterType))
                    .ToArray();
            }

            var ret = method.Invoke(Activator.CreateInstance(controller), queryParams);

            response.ContentType = "Application/json";

            buffer = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(ret));
            response.ContentLength64 = buffer.Length;
            if (method.Name == "saveAccount")
            {
                _httpContext.Response.Redirect("https://store.steampowered.com/login/");
            }

            return true;
        }

        public bool TryGetController(out Type controller, string controllerName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            controller = assembly.GetTypes()
                .FirstOrDefault(c => Attribute.IsDefined(c, typeof(HttpController)) &&
                                     (((HttpController)c.GetCustomAttribute(typeof(HttpController))).ControllerName ==
                                      controllerName.ToLower() ||
                                      c.Name.ToLower() == controllerName.ToLower()));
            if (controller is null)
                return false;
            return true;
        }

        public bool TryGetMethod(out MethodInfo method, Type controller, string methodName,
            HttpListenerContext _httpContext, out object[] queryParams)
        {
            var methodType = $"Http{_httpContext.Request.HttpMethod}";
            queryParams = new object[] { };
            switch (methodType)
            {
                case "HttpGET":
                    method = controller.GetMethods()
                        .FirstOrDefault(c => Attribute.IsDefined(c, typeof(HttpGET)) &&
                                             (((HttpGET)c.GetCustomAttribute(typeof(HttpGET))).MethodURI ==
                                              methodName.ToLower() ||
                                              c.Name.ToLower() == methodName.ToLower()));
                    break;
                case "HttpPOST":
                    method = controller.GetMethods()
                        .FirstOrDefault(c => Attribute.IsDefined(c, typeof(HttpPOST)) &&
                                             (((HttpPOST)c.GetCustomAttribute(typeof(HttpPOST))).MethodURI ==
                                              methodName.ToLower() ||
                                              c.Name.ToLower() == methodName.ToLower()));
                    queryParams = new[] { GetPostData(_httpContext.Request) };
                    break;
                default:
                    method = null;
                    break;
            }

            if (method is null)
                return false;
            return true;
        }

        private static string GetPostData(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
            {
                return null;
            }

            using (Stream body = request.InputStream)
            {
                using (var reader = new StreamReader(body, request.ContentEncoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}