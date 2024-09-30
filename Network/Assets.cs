using System;
using System.IO;
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
using System.Diagnostics;

namespace GeoGraph.Network
{
    public class Assets
    {

        public static string absolutePath;
        public static bool AssetsReady = false;

        public static PointInf _Basic_PointInf;
        public static PointInfTemp _Temp_PointInf;

        public static UpdatePoints _Update_PointInf;
        public static UpdateMap _Update_Map;
        public static UpdateUser _Update_User;

        public static Map _Map;

        public static string userImage = null;

        public Assets()
        {
            _Map = new Map();

            _Update_Map = new UpdateMap();
            _Update_PointInf = new UpdatePoints();
            _Update_User = new UpdateUser();
        }

        public static void reset()
        {
            _Basic_PointInf = null;
            _Update_PointInf = null;
            _Temp_PointInf = null;
        }

        public static void Points()
        {

            _Basic_PointInf = new PointInf();
            _Temp_PointInf = new PointInfTemp();

        }
    }

    public class User
    {
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
        }

        public static List<User> userlist = new List<User>();
        // 用户列表请求

        public static async Task<bool> RequestUserListAsync()
        {
            // 解析json并生成list
            var resetData = new
            {
                command = "Users",
                token = Connect._token
            };

            string jsonString = JsonSerializer.Serialize(resetData);

            // 发送JSON
            string _ret = await NetworkClient.SendMessageAsync(jsonString);

            var jsonDocument = JsonDocument.Parse(_ret);
            var root = jsonDocument.RootElement;

            // 确认响应类型
            if (root.GetProperty("response").GetString() == "Users")
            {
                userlist = new List<User>();
                var userListArray = root.GetProperty("content").EnumerateArray();

                foreach (var user in userListArray)
                {
                    string _username = user.GetProperty("username").GetString();
                    string _permission = user.GetProperty("permission").GetString();
                    string _userInfo = user.GetProperty("userInfo").GetString();
                    int _userRank = user.GetProperty("userRank").GetInt32();

                    if (!userlist.Any(user => user.username == _username))
                    {
                        userlist.Add(new User(_username, _permission, _userInfo, _userRank));
                    }
                }

            }
            return true;
        }

        public static void reset()
        {
            userlist = null;
        }
    }

    public class MapInfo
    {
        public string MapName;
        public int Width;
        public int Height;
        public string MapInf;
        public int MapCode;
        public int MapRank;
        public bool Changed;
        public bool Deleted;

        public string ImagePath;

        // struct is deep copy

        public MapInfo(MapInfo temp)
        {
            MapName = temp.MapName;
            Width = temp.Width;
            Height = temp.Height;
            MapInf = temp.MapInf;
            MapCode = temp.MapCode;
            MapRank = temp.MapRank;
            ImagePath = null;
        }

        public MapInfo(string MapName, int Width, int Height, string MapInf, int MapCode)
        {
            this.MapName = MapName;
            this.Width = Width;
            this.Height = Height;
            this.MapInf = MapInf;
            this.MapCode = MapCode;
            this.MapRank = 0;
        }

    }

    public class Map
    {
        public static MapInfo _MapInfo;

        public static List<MapInfo> MapList;

        public static void GetPoints(MapInfo MapChoosedNow)
        {
            _MapInfo = MapChoosedNow;
            Assets.Points();
        }

        public static async Task<bool> GetMapList()
        {
            // 发送请求
            var Data = new
            {
                command = "Maps",
                token = Connect._token
            };
            string jsonString = JsonSerializer.Serialize(Data);

            string json = await NetworkClient.SendMessageAsync(jsonString);
            // 解析 JSON 字符串
            var jsonDocument = JsonDocument.Parse(json);
            // 获取地图信息
            var mapList = jsonDocument.RootElement;
            // 获取地图列表 列表格式为 地图名 地图信息 地图图片名
            var mapListArray = mapList.GetProperty("content").EnumerateArray();
            foreach (var map in mapListArray)
            {
                // 获取地图名
                string mapName = map.GetProperty("MapName").GetString();
                // 获取地图信息
                string mapInf = map.GetProperty("MapInf").GetString();


                // 获取地图长宽
                int Width = map.GetProperty("Width").GetInt32();
                int Height = map.GetProperty("Height").GetInt32();

                int mapCode = map.GetProperty("MapCode").GetInt32();
                // 添加入列表

                if (string.IsNullOrEmpty(mapName) )
                {
                    throw new ArgumentNullException("Map name or code cannot be null.");
                }

                if (!MapList.Any(map => map.MapCode == mapCode))
                {
                    MapList.Add(new MapInfo(mapName, Width, Height, mapInf, mapCode));
                }

            }

            return true;
        }

    }
}
