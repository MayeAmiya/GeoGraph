﻿using System;
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
        public static MapInfo _MapInfo;

        public static List<MapInfo> MapList;

        public static bool AssetsReady = false;

        public static PointInf _Basic_PointInf;
        public static Update _Update_PointInf;
        public static PointInfTemp _Temp_PointInf;

        private static NetworkClient _client;

        public Assets()
        {
            _client = MainWindow._NetworkClient;
        }

        // 获取地图信息
        public static void ParseMapInfAsync()
        {
            // 设置资源准备完毕
            AssetsReady = true;
            // 初始化点信息
            List<MapInfo>MapList = new List<MapInfo>();
            if(MapList is null)
            {
                System.Diagnostics.Debug.WriteLine("MapList is null");
            }
        }

        public static void GetPoints(MapInfo MapChoosedNow)
        {
            _MapInfo = MapChoosedNow;
            _Basic_PointInf = new PointInf(_MapInfo.MapName);
            // 现在没网 就不通过网络初始化了 _Basic_PointInf.ParsePointInfAsync();
            _Update_PointInf = new Update(_Basic_PointInf);
            _Temp_PointInf = new PointInfTemp(_Basic_PointInf);

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

                int mapCode = map.GetProperty("MapCode").GetInt32();
                // 添加入列表
                MapList.Add(new MapInfo (mapName, Width, Height, mapImage, mapInf ,mapCode));
            }
        }

        public static void Download(string RequestObject)
        {
            return;
        }

        public static string FindImage(string ImageName)
        {
            Download(ImageName);
            string ImagePath = null; //!!!
            return ImagePath;
        }
    }

}
