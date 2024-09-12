using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GeoGraph.Pages.MainPage.MapChooseFrame;

namespace GeoGraph.Network
{
    // 这里存储本次登录所作过更改的点信息 用于更新 更新时搜索PointInf标记Updated
    // 搜索时优先搜索PointInfUpdate 找不到再搜索PointInf
    public class Update
    {
        private PointInf _pointinf;

        public Update(PointInf pointinf)
        {
            _pointinf = pointinf;

            Update_basePoints = new List<BasePoint>();
            Update_basicInfo = new Dictionary<int, Property>();
            Update_EnumInfo = new Dictionary<string, Property>();
            Update_PageInfo = new Dictionary<string, Property>();
        }


        public Dictionary<int, Property> Update_basicInfo; // 以数据库结构存储 自己找到自己的位置
        public Dictionary<string, Property> Update_EnumInfo;
        public Dictionary<string, Property> Update_PageInfo;

        List<BasePoint> Update_basePoints;

        public void clear()
        {
            Update_basicInfo.Clear();
            Update_EnumInfo.Clear();
            Update_PageInfo.Clear();
        }

        public void UPDATE()
        {
            // 上传所有的更改
            foreach (var item in Update_basicInfo)
            {
                //不应该在这里处理
            }
            // 并清空更改
            clear();
        }
        // 原点标记删除 更新区间记录
        public void RemovePoint(BasePoint deleted)
        {
            // 更新点 删除点
            deleted.deleted = true;
            deleted.updated = true;
            Update_basePoints.Add(deleted);
        }

        // 合并更改的点信息 合并到已更改列表
        // 好了 已更改列表更改的对象是不同于原表的
        public void merge(PointInfTemp temp)
        {
            foreach (var item in temp.Temp_basicInfo)
            {
                if (_pointinf.basicInfo.ContainsKey(item.Key))
                {
                    // 标记更改
                    var existingProperty = _pointinf.basicInfo[item.Key];
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

        public static void Upload(string RequestObject)
        {
            return;
        }

        public static void UpdateMap(MapInfo update)
        {
            return;
        }
    }
}
