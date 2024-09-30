using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Media.Protection.PlayReady;

namespace GeoGraph.Network
{
    public class Connect
    {
        public static string _username;
        public static string _password;
        public static NetworkClient _client;

        public static string _token;
        public static string _permission;
        public static string _userInfo;
        public static int _userRank;
        public static bool _isLogin;
        public Connect()
        {
            _token = null;
            // 引用一个NetworkClient对象
            _client = MainWindow._NetworkClient;
        }
        // 登录

        public static string ComputeHash(string username, string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(username + password);
                var hashBytes = sha256.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
        public static async Task<string> AttemptLoginAsync(string username, string password)
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

                var loginData = new
                {
                    command = "login", 
                    token = "null",
                    user = username,
                    hash = ComputeHash(username,password)
                };

                string jsonString = JsonSerializer.Serialize(loginData);

                // 发送JSON 但我们可以认为返回的也是个json 所以要解析
                var json = await NetworkClient.SendMessageAsync(jsonString);

                var jsonDocument = JsonDocument.Parse(json);
                var root = jsonDocument.RootElement;

                var response = root.GetProperty("response");

                // 提取数据
                _username = username;
                _permission = response.GetProperty("permission").GetString();
                _userInfo = response.GetProperty("userInfo").GetString();
                _userRank = response.GetProperty("userRank").GetInt32();
                _token = response.GetProperty("token").GetString();

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
        // 注册
        public static async Task<string> AttemptRegisterAsync(string username, string password)
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

                var registerData = new
                {
                    command = "register",
                    token = "null",
                    user = username,
                    hash = ComputeHash(username, password)
                };

                string jsonString = JsonSerializer.Serialize(registerData);

                // 发送JSON
                string json = await NetworkClient.SendMessageAsync(jsonString);

                var jsonDocument = JsonDocument.Parse(json);
                var root = jsonDocument.RootElement;
                string _ret = root.GetProperty("response").GetString();

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
        // 重设密码
        public static async Task<string> AttemptReSetAsync(string password)
        {
            try
            {
                if (_client == null)
                    return null;
                // 密码哈希处理后再发送 在后端比对 不需要解密
                // 这里组成JSON

                // 创建注册组的hashcode
                HashCode hashCode = new HashCode();
                hashCode.Add(_username);
                hashCode.Add(password);

                var resetData = new
                {
                    command = "resetpassword",
                    token = _token,
                    user = _username,
                    hash = ComputeHash(_username, password)
                };

                string jsonString = JsonSerializer.Serialize(resetData);

                // 发送JSON
                string json = await NetworkClient.SendMessageAsync(jsonString);


                var jsonDocument = JsonDocument.Parse(json);
                var root = jsonDocument.RootElement;
                string _ret = root.GetProperty("response").GetString();

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
        
        //验证码
        public static async Task<string> AttemptCaptchaAsync()
        {
            if (_client == null)
                return null;

            var loginData = new
            {
                command = "Captcha",
                token = "null"
            };

            string jsonString = JsonSerializer.Serialize(loginData);

            // 发送JSON 但我们可以认为返回的也是个json 所以要解析
            var json = await NetworkClient.SendMessageAsync(jsonString);

            var jsonDocument = JsonDocument.Parse(json);
            var root = jsonDocument.RootElement;

            var Captcha = root.GetProperty("captcha").GetString();

            await NetworkClient.Download("captcha",Captcha);
            return Captcha;
        }
        // 祝贺
        public static string Greeting()
        {
            if (_isLogin)
                return _userRank + " " + _username;
            else
                return "DisConnected";
        }
    }
}
