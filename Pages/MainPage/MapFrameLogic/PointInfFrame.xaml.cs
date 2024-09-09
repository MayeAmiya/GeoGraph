using GeoGraph.Network;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

using static GeoGraph.Pages.MainPage.MapFrameLogic.MapFrame;
using static GeoGraph.Pages.MainPage.MapFrameLogic.PointInfFrame;

using static GeoGraph.Network.PointInf;
using static GeoGraph.Network.PointInfUpdate;
using System.Runtime.CompilerServices;
using Windows.Devices.Enumeration;
using Microsoft.UI.Xaml.Documents;
using System.Reflection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GeoGraph.Pages.MainPage.MapFrameLogic
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public sealed partial class PointInfFrame : Page
    {
        private MapFrame _mapFrame;
        public PointInfFrame(MapFrame _mapFrame)
        {
            this.InitializeComponent();
            this._mapFrame = _mapFrame;
            Update_PointInf.bindTemp(Temp_PointInf);
            // 从地图框架中获取点信息 初始化页面
            refreshPage(_mapFrame.pointInfSelect().pointInfCode);
            Basic_PointInf = Assets._Basic_PointInf;
            Update_PointInf = Assets._Update_PointInf;
            Temp_PointInf = Assets._Temp_PointInf;
        }

        // 本组点信息
        static PointInf Basic_PointInf;
        static PointInfUpdate Update_PointInf;
        static PointInfTemp Temp_PointInf;
        // 最重要的部分
        private Property find(int Index)
        {
            Property Now;
            if (Temp_PointInf.Temp_basicInfo.ContainsKey(Index))
            {
                Now = Temp_PointInf.Temp_basicInfo[Index];
            }
            else if (Update_PointInf.Update_basicInfo.ContainsKey(Index))
            {
                Now = Update_PointInf.Update_basicInfo[Index];
            }
            else
            {
                Now = Basic_PointInf.basicInfo[Index];
            }
            return Now;
        }

        private async void refreshPage(int pageIndex)
        {
            //根据布局创建页面 每次更改信息要同步到布局器中

            // 开始查找点信息和索引 如果已经存在则不需要更新
            Basic_PointInf.GetValue(pageIndex);
            // 一次即可得到点所有需要的信息 现在开始展示
            // 本次得到的是点信息直接对应的页面 所以先拿出一个Page 然后对这Page的List展示元素
            // 根据List的索引值取出元素
            // 依次从Temp Update Basic中获取信息 然后布局
            Property PageNow = find(pageIndex);
            List<int> itemList = PageNow.Object as List<int>;
            foreach(int index in itemList)
            {
                //如果不是页面或枚举的话
                Property ItemNow = find(index);
                //根据ItemNow的类型进行布局
                if (ItemNow.Type != "Enum" && ItemNow.Type != "Page")
                {
                    //说明类型是字符串！这里是字符串处理
                    if (ItemNow.Type == "String")
                        {
                            // 说明是一个字符串
                            string content = ItemNow.Object as string;
                            // 字符串一般是一个Name:Value的形式
                            // 创建一对TextBox

                            // 创建不可修改的文本框
                            TextBox readOnlyTextBox = new TextBox
                            {
                                Text = $"Read-Only: {ItemNow.Name}",
                                Margin = new Microsoft.UI.Xaml.Thickness(5),
                                Width = 150,
                                IsReadOnly = true
                            };
                            // 创建可编辑的文本框
                            TextBox editableTextBox = new TextBox
                            {
                                Text = $"Editable: {content}",
                                Margin = new Microsoft.UI.Xaml.Thickness(5),
                                Width = 150
                            };

                            // 创建水平布局容器
                            StackPanel horizontalPanel = new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                Margin = new Microsoft.UI.Xaml.Thickness(5)
                            };

                            // 将文本框添加到水平布局容器中
                            horizontalPanel.Children.Add(readOnlyTextBox);
                            horizontalPanel.Children.Add(editableTextBox);

                            // 将水平布局容器添加到页面的某个容器控件中（例如 StackPanel 或 Grid）
                            Information.Children.Add(horizontalPanel);
                        }
                    if (ItemNow.Type == "Image")
                        {
                            // 说明是一个图片 这里是图片名 我们需要下载图片并且在asset中找到他 暂且不做
                            string content = ItemNow.Object as string;
                            // 创建一个图像显示 缩放至合适大小 宽度为控件宽度
                            string ImagePath = await Assets.FindImage(content);
                            Image image = new Image
                            {
                                Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(content)),
                                Stretch = Microsoft.UI.Xaml.Media.Stretch.Uniform,
                                Width = 150,
                                Height = 150
                            };
                        }
                }
                else if (ItemNow.Type == "Enum")
                {
                    // 说明是一个枚举
                    // 用ListView吧！
                }
                else if (ItemNow.Type == "Page")
                {
                    // 说明是一个页面

                    // 添加一个按钮 跳转到下一个页面
                    Button button = new Button
                    {
                        Content = "Check IN",
                        Margin = new Microsoft.UI.Xaml.Thickness(5),
                    };
                    void ButtonClick(object sender, RoutedEventArgs e)
                    {
                        nextPage(ItemNow.Index);
                    }

                    button.Click += ButtonClick;

                    // 创建不可修改的文本框
                    TextBox readOnlyTextBox = new TextBox
                    {
                        Text = $"Read-Only: {ItemNow.Name}",
                        Margin = new Microsoft.UI.Xaml.Thickness(5),
                        Width = 150,
                        IsReadOnly = true
                    };

                    // 创建水平布局容器
                    StackPanel horizontalPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Microsoft.UI.Xaml.Thickness(5)
                    };

                    // 将文本框添加到水平布局容器中
                    horizontalPanel.Children.Add(readOnlyTextBox);
                    horizontalPanel.Children.Add(button);

                    // 将水平布局容器添加到页面的某个容器控件中（例如 StackPanel 或 Grid）
                    Information.Children.Add(horizontalPanel);
                }
                else
                {
                    Console.WriteLine("Error: Unknown Type");
                }
            }
            // 添加添加按钮
            Button AddButton = new Button
            {
                Content = "Add +",
                Margin = new Microsoft.UI.Xaml.Thickness(5),
            };
            void AddButtonClick(object sender, RoutedEventArgs e)
            {
                //选择项目
            }
            AddButton.Click += AddButtonClick;
            Information.Children.Add(AddButton);

        }

        private void nextPage(int Index)
        {
            // 跳转到下一个页面
            Temp_PointInf.newPage(Index);
            refreshPage(Index);
        }
        // 如果指定了初始化方式
        // 首先要获取点信息类型列表 在AssetsGet中
        // 其次根据点信息列表初始化控件
        // 如果没有指定 那么就是空白页面和增加表项按钮

        private void SavePoint_Click(object sender, RoutedEventArgs e)
        {
            Update_PointInf.merge();
            // 保存数据到更新请求列表
        }
        private void DeletePoint_Click(object sender, RoutedEventArgs e)
        {
            Temp_PointInf.clear();
            Update_PointInf.RemovePoint(_mapFrame.pointInfSelect());
            // 添加到删除请求列表 并且打上删除标签
        }
        private void AbortPoint_Click(object sender, RoutedEventArgs e)
        {
            Temp_PointInf.clear();
            // 退出不保存 放弃本点更改
        }
    }
}
