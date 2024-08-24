using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GeoGraph.Network
{
    public class NetworkClient
    {
        private TcpClient _client;
        private NetworkStream _stream;

        private string _ipAddress;
        private int _port;
        private int recoonectTimes;

        public NetworkClient()
        {
            _client = null;
            _stream = null;

            _ipAddress = null;
            _port = -1;

            recoonectTimes = 0;

            _ = ConnectAsync(App._IP, App._Port);
        }

        private async Task<bool> ConnectAsync(string ipAddress, int port)
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
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection Error: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> ReConnect()
        {
            try
            {
                // 创建一个超时任务
                var timeoutTask = Task.Delay(App._MaxreconnectTime);
                // 创建一个连接任务
                var connectTask = this.ConnectAsync(_ipAddress, _port);
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

        public async Task<string> SendMessageAsync(string message)
        {
            if (_client == null || !_client.Connected)
            {
                await ReConnect();
            }

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                await _stream.WriteAsync(data, 0, data.Length);

                // 接收服务器的响应
                data = new byte[1024];
                StringBuilder response = new StringBuilder();
                int bytes;

                do
                {
                    bytes = await _stream.ReadAsync(data, 0, data.Length);
                    response.Append(Encoding.UTF8.GetString(data, 0, bytes));
                }
                while (bytes == data.Length); // 当读取的字节数少于缓冲区大小时，认为数据接收完毕

                return response.ToString();
            }
            catch (Exception ex)
            {
                // 更详细的异常处理
                return $"Error: {ex.Message}\nStackTrace: {ex.StackTrace}";
            }
        }

        public void Disconnect()
        {
            _stream?.Close();
            _client?.Close();
            _client = null;
            _stream = null;
        }
    }
}

