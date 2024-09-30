using Microsoft.UI.Xaml.Media.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Media.Protection.PlayReady;

namespace GeoGraph.Network
{
    public class NetworkClient
    {
        private static TcpClient _client;
        private static NetworkStream _stream;
 
        private static string _ipAddress;
        private static int _port;
        private static int recoonectTimes;
        private static string _token;
        private static string saveDirectory;

    public NetworkClient()
        {
            _client = null;
            _stream = null;
            _token = Connect._token;
            _ipAddress = null;
            _port = -1;
            saveDirectory = App.saveDirectory;
            recoonectTimes = 0;

            _ = ConnectAsync(App._IP, App._Port);
        }

        private static async Task<bool> ConnectAsync(string ipAddress, int port)
        {
            try
            {
                _ipAddress = ipAddress;
                _port = port;
                //创建一个TcpClient对象
                _client = new TcpClient();
                await _client.ConnectAsync(ipAddress, port);
                //创建一个NetworkStream对象
                _stream = _client.GetStream();
                //检查是否连接成功
                if (!_client.Connected)
                {
                    throw new InvalidOperationException("Failed to connect to the server.");
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection Error: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> ReConnect()
        {
            try
            {
                // 创建一个超时任务
                var timeoutTask = Task.Delay(App._MaxreconnectTime);
                // 创建一个连接任务
                var connectTask = ConnectAsync(_ipAddress, _port);
                // 等待两个任务中任意一个完成
                var completedTask = await Task.WhenAny(connectTask, timeoutTask);


                if (completedTask == connectTask)
                {
                    // 连接成功，重置重连次数
                    recoonectTimes = 0;
                    return true;
                }
                else
                {
                    // 超时，增加重连次数
                    recoonectTimes++;
                    // 如果重连太多，抛出异常
                    if (recoonectTimes >= App._MaxreconnectTimes)
                    {
                        throw new InvalidOperationException("Reconnect times exceed the limit.");
                    }
                    _ = ReConnect();
                    return false;
                }
            }
            catch (Exception ex) {
                recoonectTimes++;
                Console.WriteLine($"Reconnect Error: {ex.Message}");
                return false; 
            }
        }

        public static async Task<string> SendMessageAsync(string message)
        {
            if (_client == null || !_client.Connected)
            {
                await ReConnect();
            }

            try
            {
                using (NetworkStream stream = _client.GetStream())
                using (StreamReader reader = new StreamReader(stream))
                using (StreamWriter writer = new StreamWriter(stream) { AutoFlush = true })
                {    
                    await writer.WriteLineAsync(message); // 如果需要发送换行符，可以这样做
                    System.Diagnostics.Debug.WriteLine("SendMessage" + message);
                    
                    string Response = await reader.ReadLineAsync();
                    System.Diagnostics.Debug.WriteLine("RecvMessage" + Response);
                    if(Response == null)
                    {
                        return null;
                    }
                    return Response.ToString();
                }
                
            }
            catch (Exception ex)
            {
                // 更详细的异常处理
                return $"Error: {ex.Message}\nStackTrace: {ex.StackTrace}";
            }
        }

        public static void Disconnect()
        {
            _stream?.Close();
            _client?.Close();
            _client = null;
            _stream = null;
        }

        public static async Task<string> Download(string type, string filename)
        {
            // 通过地址查找图片
            // 返回文件序列化
            Console.WriteLine("Files Requested");

            var request = new
            {
                command = "download",
                token = Connect._token,
                filename = filename,
                type = type
            };

            string jsonRequest = JsonConvert.SerializeObject(request);

            string Response = await SendMessageAsync(jsonRequest);
            System.Diagnostics.Debug.WriteLine(Response);
            System.Diagnostics.Debug.WriteLine("RecvDownload");

            dynamic jsonObject = JsonConvert.DeserializeObject(Response);
            string exist = jsonObject.filename;
            if(exist == "404notfound")
            {
                return null;
            }
            string base64Data = jsonObject.data;
            if(base64Data == null)
            {
                return null;
            }
            byte[] imageBytes = Convert.FromBase64String(base64Data);
            string path;
            switch (type)
            {
                case "map":
                    path = Path.Combine(Assets.absolutePath, "map");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    break;
                case "captcha":
                    path = Path.Combine(Assets.absolutePath, "captcha");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    else
                    {
                        // 清空文件夹
                        foreach (var file in Directory.GetFiles(path))
                        {
                            File.Delete(file);
                        }
                    }
                    break;
                case "user":
                    path = Path.Combine(Assets.absolutePath, "user");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    break;
                default:
                    path = Assets.absolutePath;
                    break;
            }
            System.Diagnostics.Debug.WriteLine(path);
            string fullPath = Path.Combine(path, filename + ".png");
            File.WriteAllBytes(fullPath, imageBytes);
            return fullPath;
        }

        public static async Task<string> Upload(string type, string filename,bool direct = false, string path = null)
        {
            Console.WriteLine("Files Uploaded");

            byte[] imageBytes;

            if (direct)
            {
                imageBytes = File.ReadAllBytes(path);
            }
            else
            {
                string fullPath = Path.Combine(Assets.absolutePath, filename + ".png");

                imageBytes = File.ReadAllBytes(fullPath);
                
            }

            string base64String = Convert.ToBase64String(imageBytes);

            var request = new
            {
                command = "upload",
                token = Connect._token,
                filename = filename,
                type = type,
                data = base64String
            };

            string jsonRequest = JsonConvert.SerializeObject(request);

            string Response = await SendMessageAsync(jsonRequest);
            System.Diagnostics.Debug.WriteLine("RecvDownload");

            var jsonDocument = JsonDocument.Parse(Response);
            var root = jsonDocument.RootElement;
            string _ret = root.GetProperty("response").GetString();

            return _ret;
        }

        public static async Task<List<int>> Search(string input)
        {
            var request = new
            {
                command = "search",
                token = Connect._token,
                target = input,
                mapcode = Map._MapInfo.MapCode
            };

            string jsonRequest = JsonConvert.SerializeObject(request);

            string Response = await SendMessageAsync(jsonRequest);

            dynamic jsonObject = JsonConvert.DeserializeObject(Response);

            JArray obj = jsonObject.found as JArray;
            List<int> list = obj.ToObject<List<int>>();

            return list;
        }
    }
}

