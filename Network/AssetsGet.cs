using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using System.Reflection;
using System.Text.Json;

namespace GeoGraph.Network
{
    public class AssetsGet
    {
        public static int MapInfWidth;
        public static int MapInfHeight;
        public static string newImagePath;

        public static bool AssetsReady = false;
        public static List<(string Name, Type Type)> properties = new List<(string Name, Type)>();

        public static class PointInf { };

        private static NetworkClient _client;

        public AssetsGet()
        {
            _client = MainWindow._NetworkClient;
            ParsePropertiesAsync();
            ParsePointInfAsync();
        }

        // 创建点信息字典
        private static Dictionary<int, dynamic> PointInfs = new Dictionary<int, dynamic>();

        // 从服务器获取属性列表
        public static async void ParsePropertiesAsync()
        {
            string json = await _client.SendMessageAsync("Properties");
            // 解析 JSON 字符串
            var jsonDocument = JsonDocument.Parse(json);
            // 遍历 JSON 数组并比对添加属性
            foreach (var element in jsonDocument.RootElement.EnumerateArray())
            {
                var name = element.GetProperty("Name").GetString();
                var typeName = element.GetProperty("Type").GetString();

                // 根据类型名称解析 Type 对象
                Type type = Type.GetType(typeName);

                if (type != null)
                {
                    AssetsGet.properties.Add((name, type));
                }
                else
                {
                    Console.WriteLine($"无法解析类型: {typeName}");
                }
            }
        }

        // 从服务器获取点信息
        public static async void ParsePointInfAsync()
        {
            string json = await _client.SendMessageAsync("PointInf");
            // 解析 JSON 字符串
            var jsonDocument = JsonDocument.Parse(json);
            // 遍历 JSON 数组并比对添加属性
            foreach (var element in jsonDocument.RootElement.EnumerateArray())
            {
                // 获取 Key 值
                var key = element.GetProperty("Key").GetInt32();
                // 初始化字典与点信息对象
                InitializeAndAddPointInf(key, AssetsGet.properties);
                // 引用 点信息 对象
                var pointInf = AssetsGet.PointInfs[key];
                // 建立 点信息 对象
                var pointInfDict = (IDictionary<string, object>)pointInf;
                // 逐个添加属性
                foreach (var property in AssetsGet.properties)
                {
                    var name = property.Name;
                    var value = element.GetProperty(name).GetString();
                    pointInfDict[name] = value;
                }
            }
        }

        // 初始化点信息
        private static void InitializeAndAddPointInf(int _key, List<(string Name, Type Type)> _properties)
        {
            // 使用 ExpandoObject 动态创建对象
            dynamic _pointInf = new ExpandoObject();
            var _pointInfDict = (IDictionary<string, object>)_pointInf;

            foreach (var prop in _properties)
            {
                // 设置默认值
                object _defaultValue = prop.Type.IsValueType ? Activator.CreateInstance(prop.Type) : null;
                _pointInfDict[prop.Name] = _defaultValue;
            }

            // 添加到字典中
            PointInfs[_key] = _pointInf;
        }
        
        // 创建点信息 不应该在这里！
        public static void createPointInf(int key)
        {
            // 要求key值以唯一确定
            InitializeAndAddPointInf(key, AssetsGet.properties);
            // 然后你要通过这个属性列表创建对应的视图 然后填充在点中 最后读入这里
            var pointInf = PointInfs[key];
            // 建立 点信息 对象
            var pointInfDict = (IDictionary<string, object>)pointInf;
            // 逐个添加属性
            foreach (var property in AssetsGet.properties)
            {
                var name = property.Name;
                //是的 这里为空 这个值应该是从视图中读取的 也许可以是个初始化好的Pointinf对象 直接赋给它
                var value = "null";
                pointInfDict[name] = value;
            }
        }
    }

}
