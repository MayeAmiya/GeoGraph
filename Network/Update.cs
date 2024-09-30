using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Devices.Lights;
using Windows.Media.Protection.PlayReady;
using static GeoGraph.Pages.MainPage.MapChooseFrame;

namespace GeoGraph.Network
{
    // 这里存储本次登录所作过更改的点信息 用于更新 更新时搜索PointInf标记Updated
    // 搜索时优先搜索PointInfUpdate 找不到再搜索PointInf
    public class UpdatePoints
    {
        private static PointInf _pointinf;
        public UpdatePoints()
        {
            _pointinf = Assets._Basic_PointInf;

            Update_basePoints = new List<BasePoint>();
            Update_basicInfo = new Dictionary<int, Property>();
        }


        public static Dictionary<int, Property> Update_basicInfo; // 以数据库结构存储 自己找到自己的位置

        public static List<BasePoint> Update_basePoints;

        public static async void UPDATE()
        {
            // 上传所有的更改
            {
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

                var requestData = new
                {
                    command = "updateProperties",
                    token = Connect._token,
                    properties = propertiesList,
                    map = Map._MapInfo.MapCode
                };

                string jsonP = JsonConvert.SerializeObject(requestData);

                await NetworkClient.SendMessageAsync(jsonP);
            }

            // 上传基点
            {           
                var pointsList = new List<object>();

                foreach (var item in Update_basePoints)
                {
                    var newPoint = new
                    {
                        x = item.location.X,
                        y = item.location.Y,
                        deleted = item.deleted,
                        PointInfCode = item.pointInfCode
                    };

                    pointsList.Add(newPoint);
                }

                var requestDataM = new
                {
                    command = "updatePoints",
                    token = Connect._token,
                    points = pointsList,
                    map = Map._MapInfo.MapCode
                };
                string jsonM = JsonConvert.SerializeObject(requestDataM);

                await NetworkClient.SendMessageAsync(jsonM);
            }
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
                if (Update_basicInfo.ContainsKey(item.Key))
                {
                    Update_basicInfo.Remove(item.Key);
                    Update_basicInfo.Add(item.Key, item.Value);
                }
                else
                {
                    Update_basicInfo.Add(item.Key, item.Value);
                }
            }
            // 上传基点

            var existingPoint = Update_basePoints.FirstOrDefault(point => point.pointInfCode == temp.Temp_Point.pointInfCode);
            if (existingPoint != null)
            {
                Update_basePoints.Remove(existingPoint);
                Update_basePoints.Add(temp.Temp_Point);
            }
            else
            {
                Update_basePoints.Add(temp.Temp_Point);
            }
        }

        public static void reset()
        {
            Update_basePoints = null;
            Update_basicInfo = null;
        }
    }

    public class UpdateMap
    {
        public UpdateMap()
        {
            update_mapinfos = new List<MapInfo>();
        }

        public static List<MapInfo> update_mapinfos;

        public static void AddMapList(MapInfo temp)
        {
            update_mapinfos.Clear();

            update_mapinfos.Add(temp);
            
        }

        public static async Task<bool> Update()
        {
            // 上传地图的同时要上传图片
            var requestData = new
            {
                command = "updateMaps",
                token = Connect._token,
                maps = update_mapinfos
            };

            string json = JsonConvert.SerializeObject(requestData);

           await NetworkClient.SendMessageAsync(json);


            foreach (MapInfo temp in update_mapinfos)
            {
                // 上传图片
                if(!temp.Deleted)
                    await NetworkClient.Upload("map", temp.MapName,true,temp.ImagePath);
            }
            return true;
        }

        public static void reset()
        {
            update_mapinfos = null;
        }
    }

    public class UpdateUser
    {
        public UpdateUser()
        {
            update_users = new List<User>();
        }

        public static List<User> update_users;

        public static void AddUserList(User temp)
        {
            update_users.Clear();
            update_users.Add(temp);
        }

        public static async Task<bool> Update()
        {
            var requestData = new
            {
                command = "updateUsers",
                token = Connect._token,
                users = update_users
            };

            string jsonP = JsonConvert.SerializeObject(requestData);

            await NetworkClient.SendMessageAsync(jsonP);
            return true;
        }

        public static void reset()
        {
            update_users = null;
        }
    }
}