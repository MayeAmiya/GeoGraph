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
using Windows.Media.Protection.PlayReady;
using Windows.Devices.Lights;

namespace GeoGraph.Network
{
    public class Assets
    {

        public static bool AssetsReady = false;

        public static PointInf _Basic_PointInf;
        public static PointInfTemp _Temp_PointInf;

        public static UpdatePoints _Update_PointInf;
        public static UpdateMap _Update_Map;
        public static UpdateUser _Update_User;

        public static Map _Map;

        private static NetworkClient _client;

        public Assets()
        {
            _client = MainWindow._NetworkClient;

            _Basic_PointInf = new PointInf();
            _Temp_PointInf = new PointInfTemp();

            _Update_Map = new UpdateMap();
            _Update_PointInf = new UpdatePoints();
            _Update_User = new UpdateUser();

            _Map = new Map();
        }

        public static void reset()
        {
            _Basic_PointInf = null;
            _Update_PointInf = null;
            _Temp_PointInf = null;
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

    public class User
    {
        private static NetworkClient _client;

        public string username;

        public string permission;

        public string userInfo;

        public int userRank;

        public User(string username, string permission, string userInfo, int userRank)
        {
            this.username = username;
            this.permission = permission;
            this.userInfo = userInfo;
            this.userRank = userRank;
            _client = MainWindow._NetworkClient;
        }

        public static List<User> userlist = new List<User>();
        // 用户列表请求

        public static async void RequestUserListAsync()
        {
            // 解析json并生成list
            var resetData = new
            {
                command = "requestUserList"
            };
            string jsonString = JsonSerializer.Serialize(resetData);

            // 发送JSON
            string _ret = await _client.SendMessageAsync(jsonString);

            // 解析json 格式 {"userlist":[{"username":"admin","permission":"admin","userInfo":"admin","userRank":1}]}
            var jsonDocument = JsonDocument.Parse(_ret);
            var userList = jsonDocument.RootElement;
            var userListArray = userList.GetProperty("userlist").EnumerateArray();

            foreach (var user in userListArray)
            {
                string username = user.GetProperty("username").GetString();
                string permission = user.GetProperty("permission").GetString();
                string userInfo = user.GetProperty("userInfo").GetString();
                int userRank = user.GetProperty("userRank").GetInt32();
                userlist.Add(new User(username, permission, userInfo, userRank));
            }
        }

        public static void reset()
        {
            userlist = null;
        }
    }

    public class Map
    {
        private static NetworkClient _client;

        public Map()
        {
            List<MapInfo> MapList = new List<MapInfo>();
            _client = MainWindow._NetworkClient;
        }

        public static MapInfo _MapInfo;

        public static List<MapInfo> MapList;

        public static void GetPoints(MapInfo MapChoosedNow)
        {
            _MapInfo = MapChoosedNow;
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

                Assets.Download(mapImage);
                // 获取地图长宽
                int Width = map.GetProperty("Width").GetInt32();
                int Height = map.GetProperty("Height").GetInt32();

                int mapCode = map.GetProperty("MapCode").GetInt32();
                // 添加入列表
                MapList.Add(new MapInfo(mapName, Width, Height, mapImage, mapInf, mapCode));
            }
        }
    }
}
