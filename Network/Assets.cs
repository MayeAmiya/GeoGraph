using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using System.Reflection;
using System.Text.Json;
using static GeoGraph.Network.PointInf;

namespace GeoGraph.Network
{
    public class Assets
    {
        public static int MapInfWidth;
        public static int MapInfHeight;
        public static string newImagePath;

        public static bool AssetsReady = false;


        private static NetworkClient _client;
        private static PointInf _pointinf;

        public Assets()
        {
            _client = MainWindow._NetworkClient;
            _pointinf = new PointInf();
            
        }

        // 获取地图信息
        public static async void ParseMapInfAsync()
        {
            string json = await _client.SendMessageAsync("MapInf");
            // 解析 JSON 字符串
            var jsonDocument = JsonDocument.Parse(json);
            // 获取地图信息
            var mapInf = jsonDocument.RootElement;
            // 获取地图宽度
            MapInfWidth = mapInf.GetProperty("Width").GetInt32();
            // 获取地图高度
            MapInfHeight = mapInf.GetProperty("Height").GetInt32();
            // 下载地图图片

            // 获取地图图片路径
            newImagePath = mapInf.GetProperty("ImagePath").GetString();
            // 设置资源准备完毕
            AssetsReady = true;
        }
    }

}
