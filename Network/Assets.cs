using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using System.Reflection;
using System.Text.Json;
using static GeoGraph.Network.PointInf;
using static GeoGraph.Pages.MainPage.MapChooseFrame;

namespace GeoGraph.Network
{
    public class Assets
    {
        public static string _MapName;
        public static int MapInfWidth;
        public static int MapInfHeight;
        public static string MapImagePath;

        public static List<MapInfo> MapList;

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
        public static  void ParseMapInfAsync()
        {
            // 设置资源准备完毕
            AssetsReady = true;
            // 初始化点信息
            PointInf _Basic_PointInf = new PointInf(_MapName);
            PointInfUpdate _Update_PointInf = new PointInfUpdate(_Basic_PointInf);
            PointInfTemp _Temp_PointInf = new PointInfTemp(_Basic_PointInf);
            List<MapInfo>MapList = new List<MapInfo>();
        }

        public static async void GetMapList()
        {
            string json = await _client.SendMessageAsync("RequestMapList");
            // 解析 JSON 字符串
            var jsonDocument = JsonDocument.Parse(json);
            // 获取地图信息
            var mapList = jsonDocument.RootElement;
            // 获取地图列表 列表格式为 地图名 地图信息 地图图片名
            var mapListArray = mapList.GetProperty("MapList").EnumerateArray();
            foreach (var map in mapListArray)
            {
                // 获取地图名
                string mapName = map.GetProperty("MapName").GetString();
                // 获取地图信息
                string mapInf = map.GetProperty("MapInf").GetString();
                // 获取地图图片名
                // like xxxxx.jpg
                string mapImage = map.GetProperty("MapImage").GetString();
                Download(mapImage);
                // 获取地图长宽
                int Width = map.GetProperty("Width").GetInt32();
                int Height = map.GetProperty("Height").GetInt32();
                // 添加入列表
                MapList.Add(new MapInfo { MapName = mapName, Width = Width, Height = Height, ImagePath = mapImage, MapInf = mapInf });
            }
        }
        public static async void Download(string RequestObject)
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
