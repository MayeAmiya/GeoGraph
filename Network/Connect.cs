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
        private static string _username;
        private static string _password;
        private static NetworkClient _client;
        //用户名和密码用作第一次登录校验 之后用token
        private static string _token;
        private static string _permission;
        private static string _userInfo;
        private static string _userRank;
        private static bool _isLogin;
        public Connect()
        {
            _token = null;
            // 引用一个NetworkClient对象
            _client = MainWindow._NetworkClient;
        }

        public async Task<string> AttemptLoginAsync(string username, string password)
        {
            try
            {
                if (_client == null) 
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

                // 发送JSON 但我们可以认为返回的也是个json 所以要解析
                _token = await _client.SendMessageAsync("login " + jsonString);

                // 在这里处理登录结果
                return _token;
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
                if (_client == null) 
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
                string _ret = await _client.SendMessageAsync(jsonString);

                // 在这里处理注册结果
                return _ret;
            }
            catch (Exception ex)
            {
                // 处理异常情况
                Console.WriteLine($"Error during register: {ex.Message}");
                return null;
            }
        }

        public static string Greeting()
        {
            if(_isLogin)
                return _userRank + " " + _username;
            else
                return "DisConnected";
        }
    }
}
