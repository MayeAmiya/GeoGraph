using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Media.Protection.PlayReady;
using static GeoGraph.Pages.MainPage.MapChooseFrame;

namespace GeoGraph.Network
{
    // 这里存储本次登录所作过更改的点信息 用于更新 更新时搜索PointInf标记Updated
    // 搜索时优先搜索PointInfUpdate 找不到再搜索PointInf
    public class UpdatePoints
    {
        private static PointInf _pointinf;
        private static NetworkClient _client;
        public UpdatePoints()
        {
            _pointinf = Assets._Basic_PointInf;

            _client = MainWindow._NetworkClient;

            Update_basePoints = new List<BasePoint>();
            Update_basicInfo = new Dictionary<int, Property>();
            Update_PageInfo = new Dictionary<string,Property>();
            Update_EnumInfo = new Dictionary<string,Property>();
        }


        public static Dictionary<int, Property> Update_basicInfo; // 以数据库结构存储 自己找到自己的位置
        public static Dictionary<string, Property> Update_PageInfo; 
        public static Dictionary<string, Property> Update_EnumInfo; 

        public static List<BasePoint> Update_basePoints;

        public static void UPDATE()
        {
            // 上传所有的更改
            // 总结为json

            var propertiesList = new List<object>();

            foreach (var items in Update_basicInfo)
            {
                var item = items.Value;
                var newProperty = new
                {
                    Name = item.Name,
                    Index = item.Index,
                    Type = item.Type,
                    Object = item.Object,
                    Updated = item.updated,
                    Deleted = item.deleted,
                };

                propertiesList.Add(newProperty);
            }

            string jsonP = JsonConvert.SerializeObject(propertiesList);

            _client.SendMessageAsync("updatePoints" + jsonP);

            // 上传基点

            var pointsList = new List<object>();

            foreach (var item in Update_basePoints)
            {
                var newPoint = new
                {
                    location = item.location,
                    deleted = item.deleted,
                    updated = item.updated,
                };

                pointsList.Add(newPoint);
            }

            string jsonM = JsonConvert.SerializeObject(propertiesList);

            _client.SendMessageAsync("updatePoints" + jsonM);
        }

        // 原点标记删除 更新区间记录
        public static void RemovePoint(BasePoint deleted)
        {
            // 更新点 删除点
            deleted.deleted = true;
            deleted.updated = true;
            Update_basePoints.Add(deleted);
        }

        // 合并更改的点信息 合并到已更改列表
        // 好了 已更改列表更改的对象是不同于原表的
        public static void merge(PointInfTemp temp)
        {
            foreach (var item in temp.Temp_basicInfo)
            {
                if (PointInf.basicInfo.ContainsKey(item.Key))
                {
                    // 标记更改
                    var existingProperty = PointInf.basicInfo[item.Key];
                    existingProperty.updated = true;
                }
                // 如果同key 则覆盖 这里需要校验 显然有且只有上文复制页面的时候才会出现同key
                if (!Update_basicInfo.ContainsKey(item.Key))
                    Update_basicInfo.Add(item.Key, item.Value);
                else
                {
                    // 同Key 这里直接覆写了 不过为什么不删了呢
                    var existingProperty = Update_basicInfo[item.Key];
                    // 确保 Object 是 List<int> 类型
                    if (existingProperty.Object is List<int> existingList && item.Value.Object is List<int> newList)
                    {
                        // 创建新列表的深拷贝
                        existingProperty.Object = new List<int>(newList);
                        existingProperty.dateUpdate();
                        // 更新字典中的对象
                        Update_basicInfo[item.Key] = existingProperty;
                    }
                    // 然后还要更新一下
                }
            }
            // 上传基点
            Update_basePoints.Add(temp.Temp_Point);
        }

        public static void remove(PointInfTemp temp)
        {
            temp.Temp_Point.deleted = true;
            Update_basePoints.Remove(temp.Temp_Point);
        }

        public static void reset()
        {
            Update_basePoints = null;
            Update_basicInfo = null;
        }
    }

    public class UpdateMap
    {
        private NetworkClient _client;
        public UpdateMap()
        {
            _client = MainWindow._NetworkClient;
        }

        public static List<MapInfo> update_mapinfos;

        public static void AddMapList(MapInfo temp)
        {
            update_mapinfos.Add(temp);
        }

        public static void Update()
        {
            // 上传地图的同时要上传图片
        }

        public static void reset()
        {
            update_mapinfos = null;
        }
    }

    public class UpdateUser
    {
        private static NetworkClient _client;

        public UpdateUser()
        {
            _client = MainWindow._NetworkClient;
        }
        public static List<User> update_users;

        public static void AddUserList(User temp)
        {
            update_users.Add(temp);
        }

        public static void Update()
        { 

            string jsonP = JsonConvert.SerializeObject(update_users);

            _client.SendMessageAsync("updatePoints" + jsonP);
        }

        public static void reset()
        {
            update_users = null;
        }
    }
}