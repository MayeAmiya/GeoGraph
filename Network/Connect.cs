using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GeoGraph.Network
{
    public class Connect
    {
        private string _username;
        private string _password;
        private static NetworkClient _server;
        private static string _connectcode;
        public Connect()
        {
            _connectcode = null;
            // 引用一个NetworkClient对象
            NetworkClient _server = MainWindow._NetworkClient;
        }

        public async Task<string> AttemptLoginAsync(string username, string password)
        {
            try
            {
                if (_server == null) 
                    return null;
                // 密码哈希处理后再发送 在后端比对 不需要解密
                // 这里组成JSON

                // 创建登录组的hashcode
                HashCode hashCode = new HashCode();
                hashCode.Add(username);
                hashCode.Add(password);
                int combinedHash = hashCode.ToHashCode();

                var loginData = new
                {
                    command = "login", 
                    user = username,
                    hash = combinedHash
                };

                string jsonString = JsonSerializer.Serialize(loginData);

                // 发送JSON
                _connectcode = await _server.SendMessageAsync("login " + jsonString);

                // 在这里处理登录结果
                return _connectcode;
            }
            catch (Exception ex)
            {
                // 处理异常情况
                Console.WriteLine($"Error during login: {ex.Message}");
                return null;
            }
        }

        public async Task<string> AttemptRegisterAsync(string username, string password)
        {
            try
            {
                if (_server == null) 
                    return null;
                // 密码哈希处理后再发送 在后端比对 不需要解密
                // 这里组成JSON

                // 创建注册组的hashcode
                HashCode hashCode = new HashCode();
                hashCode.Add(username);
                hashCode.Add(password);
                int combinedHash = hashCode.ToHashCode();

                var registerData = new
                {
                    command = "register", 
                    user = username,
                    hash = combinedHash
                };

                string jsonString = JsonSerializer.Serialize(registerData);

                // 发送JSON
                _connectcode = await _server.SendMessageAsync(jsonString);

                // 在这里处理注册结果
                return _connectcode;
            }
            catch (Exception ex)
            {
                // 处理异常情况
                Console.WriteLine($"Error during register: {ex.Message}");
                return null;
            }
        }
    }
}
