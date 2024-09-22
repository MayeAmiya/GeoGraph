using GeoGraph.Pages.MainPage.MapFrameLogic;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Protection.PlayReady;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        private static NetworkClient _client;
        public static string MapName;
        // 在选择地图后初始化 所以这里应当在Assets中初始化
        public PointInf(string mapname)
        {
            _client = MainWindow._NetworkClient;

            MapName = mapname;
            basePoints = new List<BasePoint>();

            basicInfo = new Dictionary<int, Property>();
            EnumInfo = new Dictionary<string, Property>();
            PageInfo = new Dictionary<string, Property>();
        }
        // 点信息列表
        public List<BasePoint> basePoints;
        // 属性列表
        public Dictionary<int, Property> basicInfo;
        public Dictionary<string, Property> EnumInfo;
        public Dictionary<string, Property> PageInfo;
        // 从服务器获取点信息 主表
        public async void ParsePointInfAsync()
        {
            //这时候要考虑服务器送的数据格式
            //是一个Json 包含一大堆数据 有普通数据类型 也可能是列表
            //对于字符串 整数 浮点数 可以认为是单个量
            //而对于列表和枚举 就需要递归处理
            if(_client == null)
            {
                return;
            }
            //这里我们假设服务器返回的是一个Json字符串
            string MainPoints = await _client.SendMessageAsync("Points");
            //Point组成是Primary主键 有name 
            //这个Point组成是两个double 是x和y 还有一个页面 是属性 真正的点信息 但是我们的Json本次
            //不需要传输页面 预留出来 之后通过索引值查询从表
            //现在我们解析这个Json
            var jsonDocument = JsonDocument.Parse(MainPoints);
            var root = jsonDocument.RootElement;


            //解析Json并向basePoints中添加点
            foreach (var point in root.EnumerateArray())
            {
                var x = point.GetProperty("x").GetDouble();
                var y = point.GetProperty("y").GetDouble();
                // 坐标
                var location = new Windows.Foundation.Point(x, y);
                // 页面索引值
                var PointInfCode = point.GetProperty("PointInfCode").GetInt32();

                basePoints.Add(new BasePoint(location, PointInfCode));
                await ParsePropertyInfAsync(PointInfCode);
            }

        }

        // 从服务器获取属性信息 从表
        public async Task<bool> ParsePropertyInfAsync(int requestIndex)
        {
            string requestInfo;
            //我们需要再次考虑服务器的数据类型 我们可以假定服务器的每个值都是固定的 而服务器在搜索后返回我们需要的宽域内容
            // 发送序号 服务器进行查询处理 发送大批数据
            requestInfo = "Property " + requestIndex.ToString();
            //再次考虑服务器送的数据格式
            string propertyInfo = await _client.SendMessageAsync(requestInfo);
            //这里我们假设服务器返回的是一个Json字符串 先检验！
            if(propertyInfo == "null")
            {
                return false;
            }
            var jsonDocument = JsonDocument.Parse(propertyInfo);
            var root = jsonDocument.RootElement;

            //解析Json并向properties中添加属性
            foreach (var property in root.EnumerateArray())
            {
                var name = property.GetProperty("Name").GetString();
                // 是否是主键也许不重要 待定
                var index = property.GetProperty("Index").GetInt32();

                //如果不是基本数据结构 如string int double bool 那么我们应当认为他是枚举enum 则需要下一步请求
                //如果是页面枚举 那么页面内容则为索引值 通过索引值请求 具体信息再存入从表
                //而对于基本枚举 枚举内容也为索引值 在从表中查找对应基本数据值 如果索引值指向页面 则表示页面名

                //发送目前主体的请求码和请求目标的请求码 服务端返回对应信息

                var type = property.GetProperty("Type").GetString(); // 获取类型名字符串

                var obj_content = property.GetProperty("Object").GetString();
                object obj;
                //如果是基本数据结构 那么Object就是数据
                if (type != "Enum" && type != "Page")
                {
                    obj = obj_content;
                    basicInfo.Add(index, new Property(name, index, type, obj));
                }
                else
                {
                    // 如果是枚举 那么Object就是索引值 那么这是一个列表
                    // 如果是页面 那么也是一个列表
                    obj = new List<int>();
                    foreach (var item in property.GetProperty("Object").EnumerateArray())
                    {
                        (obj as List<int>).Add(item.GetInt32());
                    }

                    Property finder = new Property(name, index, type, obj);
                    if (type == "Enum")
                    {
                        // 枚举类型
                        EnumInfo.Add(name, finder);
                    }
                    else
                    {
                        // 页面类型
                        PageInfo.Add(name, finder);
                    }
                    basicInfo.Add(index, finder);
                }
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
        public async void GetValue(int index)
        {
            // 这里传入的是PageInfCode 我们要从PointInf中获取
            // 这个类每次更换点都会刷新 所以独立出来
            // PageInfCode是一个索引值 通过这个值我们可以找到对应的Page

            // Dict不允许重复索引
            if (!basicInfo.ContainsKey(index))
            {
                if (await ParsePropertyInfAsync(index))
                    GetValue(index);
                return;
            }

            // Page 和 Enum 中的Object是一个List<int> 里面存的是索引值
            // 通过索引值我们可以找到对应的基本数据
        }
    }

    // 这里存储本次更改的点信息 作用于单个点
    public class PointInfTemp
    {
        public PointInf _pointinf;
        public Property Temp_Page;
        public BasePoint Temp_Point;
        public PointInfTemp(PointInf pointinf)
        {
            _pointinf = pointinf;
            Temp_basicInfo = new Dictionary<int, Property>();
            Temp_EnumInfo = new Dictionary<string, Property>();
            Temp_PageInfo = new Dictionary<string, Property>();

        }
        // 用于UpdatePointInf的更新
        public Dictionary<int, Property> GET_Temp_basicInfo()
        {
            return Temp_basicInfo;
        }

        public Dictionary<int, Property> Temp_basicInfo;
        public Dictionary<string, Property> Temp_EnumInfo;
        public Dictionary<string, Property> Temp_PageInfo;

        // 清空本次更改信息
        public void clear()
        {
            Temp_basicInfo.Clear();
            Temp_EnumInfo.Clear();
            Temp_PageInfo.Clear();
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
                var existingProperty = _pointinf.basicInfo[index];
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
                Property newTemp = new Property(temp_item);
                newTemp.deleted = true;
                Temp_basicInfo.Add(newTemp.Index, newTemp);
                return newTemp;
            }
            temp_item.deleted = true;
            return temp_item;
        }
    }
}
