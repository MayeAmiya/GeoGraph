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
using Windows.Media.Protection.PlayReady;

namespace GeoGraph.Network
{
    //设置属性类型
    public struct Property
    {
        public string Name { get; set; }
        public int Index { get; set; }
        // Type当有约束 String ; Int Double Enum Bool ; List
        public string Type { get; set; }
        public object Object { get; set; }

        // 通过检查updated和deleted来判断是否需要更新 事后只需根据索引号上传信息即可
        public bool deleted = false;
        public bool updated = false;

        public Property(string name, int index,string type, object obj)
        {
            Name = name;
            Index = index;
            Type = type;
            Object = obj;
        }

    }

    public class BasePoint
    {
        public Point location;
        public int pointInfCode;
        public bool isTemp = false;
        public bool deleted = false;
        public bool updated = false;

        public BasePoint()
        {
            // 可以选择是否初始化默认值
            this.location = new Point(); // 默认值
            this.pointInfCode = 0; // 默认值
            this.isTemp = false; // 默认值
        }

        public BasePoint(Point location, Int32 PointInfCode)
        {
            this.location = location;
            //是的 理应根据时间和位置生成hashcode
            this.pointInfCode = PointInfCode;
            // DateTime.Now.TimeOfDay.GetHashCode() ^ location.X.GetHashCode() ^ location.Y.GetHashCode();
        }
    }

    public class PointInf
    {
        private static NetworkClient _client;
        // 在选择地图后初始化 所以这里应当在Assets中初始化
        public PointInf()
        {
            _client = MainWindow._NetworkClient;

            basePoints = new List<BasePoint>();
            ParsePointInfAsync();

            basicInfo = new Dictionary<int, Property>();

        }
        // 点信息列表
        List<BasePoint> basePoints;
        // 属性列表
        public Dictionary<int, Property> basicInfo;

        // 从服务器获取点信息 主表
        public async void ParsePointInfAsync()
        {
            //这时候要考虑服务器送的数据格式
            //是一个Json 包含一大堆数据 有普通数据类型 也可能是列表
            //对于字符串 整数 浮点数 可以认为是单个量
            //而对于列表和枚举 就需要递归处理

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
                }
                basicInfo.Add(index, new Property(name, index, type, obj));
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

        public async void RemovePoint(int pointindex)
        {
            var basePointToRemove = basePoints.FirstOrDefault(bp => bp.pointInfCode == pointindex);
            //也许我们可以把更改集中于本地
            basePointToRemove.deleted = true;
        }
    }

    internal class PointInfSingle
    {
       
        private PointInf _pointinf;

        public PointInfSingle(PointInf pointinf)
        {
            _pointinf = pointinf;
            Single_basicInfo = new Dictionary<int, Property>();
        }
        public Dictionary<int, Property> Single_basicInfo; // 以数据库结构存储 自己找到自己的位置

        public void clear()
        {
            Single_basicInfo.Clear();
        }

        public async void GetValue(int index)
        {
            // 这里传入的是PageInfCode 我们要从PointInf中获取
            // 这个类每次更换点都会刷新 所以独立出来
            // PageInfCode是一个索引值 通过这个值我们可以找到对应的Page

            // Dict不允许重复索引
            if(_pointinf.basicInfo.ContainsKey(index) && !Single_basicInfo.ContainsKey(index))
                Single_basicInfo.Add(index,_pointinf.basicInfo[index]);
            else
            {
                if(await _pointinf.ParsePropertyInfAsync(index))
                    GetValue(index);
                return;
            }


            // Page 和 Enum 中的Object是一个List<int> 里面存的是索引值
            // 通过索引值我们可以找到对应的基本数据
            ListHandle(index);
        }

        public async void ListHandle(int index)
        {
            foreach (var item in (Single_basicInfo[index].Object as List<int>))
            {
                // 如果是基本数据类型 那么直接添加 如果不是 那么递归
                if (_pointinf.basicInfo.ContainsKey(item) && !Single_basicInfo.ContainsKey(item))
                    if (_pointinf.basicInfo[item].Type == "Enum" || _pointinf.basicInfo[item].Type == "Page")
                        if (_pointinf.basicInfo[item].Object is int Index)
                            ListHandle(Index);
                        else
                            Single_basicInfo.Add(item, _pointinf.basicInfo[item]);
                else
                {
                    if(_pointinf.basicInfo[item].Object is int Index)
                        if(await _pointinf.ParsePropertyInfAsync(Index))
                                ListHandle(Index);
                    }
            }
        }

    }

    internal class PointInfTemp
    {
        private PointInfSingle _pointinfsingle;
        private Property Temp_Page;
        public PointInfTemp(PointInfSingle pointinfsingle)
        {
            _pointinfsingle = _pointinfsingle;
            Temp_basicInfo = new Dictionary<int, Property>();
        }
        private Dictionary<int, Property> Temp_basicInfo;

        public void clear()
        {
            Temp_basicInfo.Clear();
        }

        public void Add_Temp_PointInf(string name, string type, string info)
        {
            int hashcode = name.GetHashCode()^type.GetHashCode()^info.GetHashCode()+DateTime.Now.TimeOfDay.GetHashCode();
            new Property(name, hashcode, type, info);
            Temp_basicInfo.Add(hashcode, new Property(name, hashcode, type, info));

            var tempPage = Temp_Page.Object as List<int>; // 尝试将 object 转换为 List<int>
            tempPage.Add(hashcode); // 添加 hashcode 到列表
            
        }
        public void merge()
        {
            foreach (var item in Temp_basicInfo)
            {
                if(!_pointinfsingle.Single_basicInfo.ContainsKey(item.Key))
                    _pointinfsingle.Single_basicInfo.Add(item.Key, item.Value);
                else
                {
                    var existingProperty = _pointinfsingle.Single_basicInfo[item.Key];
                    // 确保 Object 是 List<int> 类型
                    if (existingProperty.Object is List<int> existingList && item.Value.Object is List<int> newList)
                    {
                        // 创建新列表的深拷贝
                        existingProperty.Object = new List<int>(newList);
                        // 更新字典中的对象
                        _pointinfsingle.Single_basicInfo[item.Key] = existingProperty;
                    }
                }
                // 如果同key 则覆盖 这里需要校验 显然有且只有上文复制页面的时候才会出现同key
            }
        }
        public void remove(int index)
        {
            Temp_basicInfo.Remove(index);
        }

        // 创建新页面时在这里创建副本保存未保存项目 合并时直接添加

        public void newPage(int pageindex)
        {
            //复制一个page
            if(!Temp_basicInfo.ContainsKey(pageindex))
            {            
                Temp_Page = new Property
                {
                    Name = _pointinfsingle.Single_basicInfo[pageindex].Name,
                    Index = _pointinfsingle.Single_basicInfo[pageindex].Index,
                    Type = _pointinfsingle.Single_basicInfo[pageindex].Type,
                    Object = new List<int> ((List<int>)_pointinfsingle.Single_basicInfo[pageindex].Object)
                };
                    Temp_basicInfo.Add(Temp_Page.Index, Temp_Page);
            }
            else
            {
                // 如果存在 有且只有可能是已经保存了的页面
                Temp_Page = Temp_basicInfo[pageindex];
            }
        }
    }
}
