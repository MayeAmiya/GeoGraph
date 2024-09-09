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
        public static string _MapName;
        public static int MapInfWidth;
        public static int MapInfHeight;
        public static string newImagePath;

        public static bool AssetsReady = false;

        public static PointInf _Basic_PointInf;
        public static PointInfUpdate _Update_PointInf;
        public static PointInfTemp _Temp_PointInf;

        private static NetworkClient _client;

        public Assets()
        {
            _client = MainWindow._NetworkClient;
        }

        // 获取地图信息
        public static async void ParseMapInfAsync()
        {
            string json = await _client.SendMessageAsync(_MapName + "MapInf");
            // 解析 JSON 字符串
            var jsonDocument = JsonDocument.Parse(json);
            // 获取地图信息
            var mapInf = jsonDocument.RootElement;

            // 获取地图宽度
            MapInfWidth = mapInf.GetProperty("Width").GetInt32();
            // 获取地图高度
            MapInfHeight = mapInf.GetProperty("Height").GetInt32();
            // 获取地图图片路径
            newImagePath = mapInf.GetProperty("ImagePath").GetString();
            // 设置资源准备完毕
            AssetsReady = true;
            // 初始化点信息
            PointInf _Basic_PointInf = new PointInf(_MapName);
            PointInfUpdate _Update_PointInf = new PointInfUpdate(_Basic_PointInf);
            PointInfTemp _Temp_PointInf = new PointInfTemp(_Basic_PointInf);
            Download(newImagePath);
        }

        public static async void Download(string url)
        {
            return;
        }

        public static async Task<string> FindImage(string ImageName)
        {
            Download(ImageName);
            string ImagePath = null; //!!!
            return ImagePath;
        }
    }

}
