using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Diagnostics;
using static System.String;

namespace HttpServer
{
    internal class Server
    {
        private static HttpListener _listener;
        // запустить эту ссылку в браузере
        private const string Url = "http://localhost:8000/";
        // указать путь к файлам для сервера
        private const string RootPath = @"D:\PractDotNet\Файлы для HttpServer\";
        // верные логин пароль для авторизации
        private const string Login = "test";
        private const string Password = "1";

        private static void Main()
        {

            _listener = new HttpListener();
            _listener.Prefixes.Add(Url);
            _listener.Start();
            Console.WriteLine($"Я вас жду там - {Url}");

            Process.Start("http://localhost:8000/"); // эта штука запускает сразу страницу, но с ней работает

            var listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();

            _listener.Close();
            Console.ReadKey();
        }

        private static async Task HandleIncomingConnections()
        {
            var runServer = true;

            while (runServer)
            {
                var ctx = await _listener.GetContextAsync();
                var req = ctx.Request;
                var resp = ctx.Response;

                var data = new byte[] { };


                if (req.HttpMethod == "GET" && req.Url.AbsolutePath == "/")
                {
                    using (var reader = new StreamReader(RootPath + "index.html"))
                    {
                        data = Encoding.UTF8.GetBytes(reader.ReadToEnd());
                    }
                    resp.ContentType = "text/html";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = data.LongLength;
                    Console.WriteLine("Ввод логина и пароля.");
                }


                if (req.HttpMethod == "GET" && req.Url.AbsolutePath == "/img")
                {
                    data = File.ReadAllBytes(RootPath + "img.jpg");

                    resp.ContentType = "image/jpg";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = data.LongLength;
                }

                if (req.HttpMethod == "POST" && req.Url.AbsolutePath == "/login")
                {
                    var login = Empty;
                    var password = Empty;
                    using (var reader = new StreamReader(req.InputStream, req.ContentEncoding))
                    {
                        var postBody = reader.ReadToEnd().Split('&');
                        foreach (var str in postBody)
                        {
                            if (str.Contains("login"))
                            {
                                login = str.Replace("login=", Empty);
                            }

                            if (str.Contains("pass"))
                            {
                                password = str.Replace("pass=", Empty);
                            }
                        }
                    }

                    if (Login == login && Password == password)
                    {
                        using (var reader = new StreamReader(RootPath + "success.html"))
                        {
                            data = Encoding.UTF8.GetBytes(reader.ReadToEnd());
                        }
                        resp.ContentType = "text/html";
                        resp.ContentEncoding = Encoding.UTF8;
                        resp.ContentLength64 = data.LongLength;
                        //runServer = false;
                        Console.WriteLine("Успешная авторизация, можете попробовать еще.");
                    }
                    else
                    {
                        using (var reader = new StreamReader(RootPath + "failure.html"))
                        {
                            data = Encoding.UTF8.GetBytes(reader.ReadToEnd());
                        }
                        resp.ContentType = "text/html";
                        resp.ContentEncoding = Encoding.UTF8;
                        resp.ContentLength64 = data.LongLength;
                        Console.WriteLine("Ошибка входа, введите логин и пароль повторно.");
                    }
                }

                if (data.Length.Equals(0))
                {
                    data = Encoding.UTF8.GetBytes("Resources not faund!");
                    resp.ContentType = "text/html";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = data.LongLength;
                    resp.StatusCode = 400;
                }
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();
                // Если вы дочитали до этой строчки. ну конечно я всё делал сам.(но это не точно)
            }
        }
    }
}
