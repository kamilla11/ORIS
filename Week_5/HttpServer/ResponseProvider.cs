using System.Net;
using System.Text;

namespace HttpServer;

public class ResponseProvider
{
    private HttpListenerContext _listenerContext;

    public ResponseProvider(HttpListenerContext listenerContext)
    {
        _listenerContext = listenerContext;
    }

    public byte[] LoadFile(string path)
    {
        var rawUrl = _listenerContext.Request.RawUrl.Replace("%20", " ");
        byte[] localBuffer = null;
        var filePath = path + rawUrl;

        if (Directory.Exists(filePath))
        {
            filePath = filePath + "/index.html";
            if (File.Exists(filePath))
            {
                _listenerContext.Response.Headers.Set("Content-Type", "text/html");
                localBuffer = File.ReadAllBytes(filePath);
            }
        }
        else if (File.Exists(filePath))
        {
            var contentType = Mime.GetMimeType(filePath);
            _listenerContext.Response.Headers.Set("Content-Type", contentType);
            localBuffer = File.ReadAllBytes(filePath);
        }
        else localBuffer = NotFound();

        return localBuffer;
    }

    private byte[] NotFound()
    {
        _listenerContext.Response.Headers.Set("Content-Type", "text/plain");
        _listenerContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
        string err = "404 - not found";
        return Encoding.UTF8.GetBytes(err);
    }
}