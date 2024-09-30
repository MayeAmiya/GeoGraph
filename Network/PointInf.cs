using GeoGraph.Pages.MainPage.MapFrameLogic;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Protection.PlayReady;
using static System.Runtime.InteropServices.JavaScript.JSType;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace GeoGraph.Network
{
    // 设置属性类型
    public class Property
    {
        public string Name { get; set; }
        public int Index { get; set; }
        // Type当有约束 String ; Int Double Enum Bool ; List
        public string Type { get; set; }
        public object Object { get; set; }
        public string date { get; set;}

        // 通过检查updated和deleted来判断是否需要更新 事后只需根据索引号上传信息即可
        public bool deleted = false;
        public bool updated = false;

        public Property(string name, int index,string type, object obj)
        {
            Name = name;
            Index = index;
            Type = type;
            Object = obj;
            date = DateTime.Now.ToString();
            IndexUpdate();
        }

        public Property(Property property)
        {
            Name = property.Name;
            Index = property.Index;
            Type = property.Type;
            Object = property.Object;
            date = DateTime.Now.ToString();
        }
        
        public void dateUpdate()
        {
            date = DateTime.Now.ToString();
        }

        public override int GetHashCode()
        {
            // 使用质数 17 和 31 开始（这些质数常用于哈希计算）
            int hash = 17;

            // 加上每个字段的哈希码
            hash = hash * 31 + (Name != null ? Name.GetHashCode() : 0);
            hash = hash * 31 + (Type != null ? Type.GetHashCode() : 0);
            hash = hash * 31 + (date != null ? date.GetHashCode() : 0);

            return hash;
        }

        public void IndexUpdate()
        {
            this.dateUpdate();
            this.Index = Guid.NewGuid().GetHashCode()^this.GetHashCode();
        }
    }

    // 设置基点类型
    public class BasePoint
    {
        public Point location;
        public int pointInfCode;
        public string date;
        public bool isTemp = false;
        public bool deleted = false;
        public bool updated = false;

        public BasePoint(Point location, Int32 PointInfCode)
        {
            this.location = location;
            //是的 理应根据时间和位置生成hashcode
            this.pointInfCode = PointInfCode;
            // DateTime.Now.TimeOfDay.GetHashCode() ^ location.X.GetHashCode() ^ location.Y.GetHashCode();
            this.isTemp = false;
            this.deleted = false;
            this.updated = false;
            date = DateTime.Now.ToString();
        }

        public void dateUpdate()
        {
            date = DateTime.Now.ToString();
        }
    }

    // 点的总集 所有点信息汇总于此
    public class PointInf
    {

        // 在选择地图后初始化 所以这里应当在Assets中初始化
        public PointInf()
        {

            basePoints = new List<BasePoint>();

            basicInfo = new Dictionary<int, Property>();

            requse();
        }
        // 点信息列表
        public static List<BasePoint> basePoints;
        // 属性列表
        public static Dictionary<int, Property> basicInfo;
        // 从服务器获取点信息 主表

        public static async void requse()
        {
            await ParsePointInfAsync();
            await ParsePropertyInfAsync();
        }

        public class ResponseDataP
        {
            public string response { get; set; }
            public List<PointData> content { get; set; }
        }

        public class PointData
        {
            public double X { get; set; }
            public double Y { get; set; }
            public int PointInfCode { get; set; }
        }

        public static async Task<bool> ParsePointInfAsync()
        {
            //这时候要考虑服务器送的数据格式
            //是一个Json 包含一大堆数据 有普通数据类型 也可能是列表
            //对于字符串 整数 浮点数 可以认为是单个量
            //而对于列表和枚举 就需要递归处理

            //这里我们假设服务器返回的是一个Json字符串
            var requsetsData = new
            {
                command = "Points",
                token = Connect._token,
                map = Map._MapInfo.MapCode
            };

            string Data = JsonSerializer.Serialize(requsetsData);
            string MainPoints = await NetworkClient.SendMessageAsync(Data);

            var responseDataP = JsonSerializer.Deserialize<ResponseDataP>(MainPoints);
            //Point组成是Primary主键 有name 
            //这个Point组成是两个double 是x和y 还有一个页面 是属性 真正的点信息 但是我们的Json本次
            //不需要传输页面 预留出来 之后通过索引值查询从表
            //现在我们解析这个Json
            if (responseDataP != null && responseDataP.content != null)
            {
                foreach (var point in responseDataP.content)
                {
                    var x = point.X;
                    var y = point.Y;
                    // 坐标
                    var location = new Windows.Foundation.Point(x, y);
                    // 页面索引值
                    var PointInfCode = point.PointInfCode;

                    PointInf.basePoints.Add(new BasePoint(location, PointInfCode));
                }
            }
            return true;
        }

        // 从服务器获取属性信息 从表

        public class ResponseData
        {
            public string response { get; set; }
            public Dictionary<int, JProperty> content { get; set; }
        }

        public class JProperty
        {
            public string Name { get; set; }
            public int Index { get; set; }
            // Type当有约束 String ; Int Double Enum Bool ; List
            public string Type { get; set; }
            public object Object { get; set; }
            public string date { get; set; }

            // 通过检查updated和deleted来判断是否需要更新 事后只需根据索引号上传信息即可
            public bool deleted = false;
            public bool updated = false;

            public List<int> getList()
            {
                JArray obj = Object as JArray;
                List<int> list = obj.ToObject<List<int>>();
                return list;
            }

        }

        public static async Task<bool> ParsePropertyInfAsync()
        {
            string requestInfo;
            //我们需要再次考虑服务器的数据类型 我们可以假定服务器的每个值都是固定的 而服务器在搜索后返回我们需要的宽域内容
            // 发送序号 服务器进行查询处理 发送大批数据

            var requestData = new
            {
                command = "Properties",
                token = Connect._token,
                map = Map._MapInfo.MapCode
            };
            string Data = JsonSerializer.Serialize(requestData);

            //再次考虑服务器送的数据格式
            string propertyInfo = await NetworkClient.SendMessageAsync(Data);

            var responseData = JsonConvert.DeserializeObject<ResponseData>(propertyInfo);

            if (responseData != null && responseData.content != null)
            {
                foreach (var kvp in responseData.content)
                {
                    int index = kvp.Key;
                    JProperty property = kvp.Value;


                    //如果是基本数据结构 那么Object就是数据

                    if (property.Type != "Enum" && property.Type != "EnumList" && property.Type != "Page")
                    {
                        Property finder = new Property(property.Name, property.Index, property.Type, property.Object);
                        finder.Index = property.Index;
                        basicInfo.Add(index, finder);

                    }
                    else
                    {
                        Property finder = new Property(property.Name, property.Index, property.Type, property.getList());
                        finder.Index = property.Index;
                        basicInfo.Add(index, finder);
                    }

                }

                //这里我们假设服务器返回的是一个Json字符串 先检验！
            }
            return true;
        }

        /*
         * 我们在这里描述一下这个点信息类在本地和在云端的数据结构吧
         * 在本地 点信息由一个点和点信息索引构成
         * 点包含坐标和名字 点信息索引指向一个Page
         * Page包含基本数据类型 string int double bool
         * Page也包含枚举类型 Enum
         * Enum本质上是一个列表 包含基本数据类型的索引值或Page的索引值
         * 基本数据类型的内容包含在一个List<Object>中 而Page的内容包含在一个List<Page>中
         * Page本身是一个列表 包含上述信息类型的索引值
         * 也就是说 在本地 是BasePoint  ->    Page  ->   List<Object>  + List<Page> -> List<Object> + List<Page> + ...
         *                    [PointInfCode]   [ObejctCode]                   [PageCode]
         * 这样的一个递归的结构 我们要创建缓存集 将同类型的Page和Enum归于特定缓存集 加速查找
         * 访问时通过访问索引来构造具体页面
         * 
         * 那么在云端呢 我们也是差不多的结构 但是我们通过约束来限制访问
         * BasePoint  ： String ， Int ， Double ， Bool ， Enum ， Page
         * 每一个项都有一个外键绑定指向一个BasePoint或一个Page 而Page外键绑定指向BasePoint或Page 并且要检查避免循环依赖
         */
        // 获取值

    }

    // 这里存储本次更改的点信息 作用于单个点
    public class PointInfTemp
    {
        public PointInf _pointinf;
        public Property Temp_Page;
        public BasePoint Temp_Point;

        public PointInfTemp()
        {
            _pointinf = Assets._Basic_PointInf;
            Temp_basicInfo = new Dictionary<int, Property>();

        }
        // 用于UpdatePointInf的更新
        public Dictionary<int, Property> GET_Temp_basicInfo()
        {
            return Temp_basicInfo;
        }

        public Dictionary<int, Property> Temp_basicInfo;

        // 清空本次更改信息
        public void clear()
        {
            Temp_basicInfo.Clear();
            Temp_Page = null;
            Temp_Point = null;
        }

        // 删除点信息
        public void remove(int index)
        {
            // 如果这个点信息是原有的呢！
            if(!Temp_basicInfo.ContainsKey(index))
            {
                // 如果是原有的点信息 那么只需要打上删除标记 并且加入到本次更改列表
                var existingProperty = PointInf.basicInfo[index];
                existingProperty.deleted = true;
                Temp_basicInfo.Add(index, existingProperty);
            }
            else
            {
                // 删除项 删除页面 删除枚举
                var existingProperty = Temp_basicInfo[index];
                existingProperty.deleted = true;
                Temp_basicInfo[index] = existingProperty;
                // 如果要删除点 需要给对应点打上删除标记并上传到本次更改列表
            }
        }

        // 增加点信息
        public void newPoint(BasePoint temp)
        {
            Temp_Point = temp;
            // newPage

            Property new_Page = new Property
            (
                null,
                -1,
                "Page",
                new List<int>()
            );

            new_Page.IndexUpdate();

            temp.pointInfCode = new_Page.Index;
            Temp_basicInfo.Add(new_Page.Index, new_Page);

        }

        public Property newItem(Property temp_item)
        {
            // 加入本次更改信息列表
            if(!Temp_basicInfo.ContainsKey(temp_item.Index))
            {
                Property newTemp = new Property(temp_item);
                Temp_basicInfo.Add(newTemp.Index, newTemp);
                return newTemp;
            }
            return temp_item;
        }

        public Property deleteItem(Property temp_item)
        {
            // 删除本次更改信息列表
            if(!Temp_basicInfo.ContainsKey(temp_item.Index))
            {
                temp_item.deleted = true;
                Temp_basicInfo.Add(temp_item.Index, temp_item);
            }
            return temp_item;
        }
    }
}
