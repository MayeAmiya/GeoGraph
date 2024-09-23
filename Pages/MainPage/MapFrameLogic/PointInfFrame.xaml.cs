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

using System.Runtime.CompilerServices;
using Windows.Devices.Enumeration;
using Microsoft.UI.Xaml.Documents;
using System.Reflection;
using WinRT;
using Microsoft.UI.Xaml.Shapes;
using Windows.Storage;

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

        public PointInfFrame()
        {
            this.InitializeComponent();

            // 从地图框架中获取点信息 初始化页面
            
            Basic_PointInf = Assets._Basic_PointInf;
            Update_PointInf = Assets._Update_PointInf;
            Temp_PointInf = Assets._Temp_PointInf;

            Breadcrumbs.Clear();

        }
        GeoGraph.Pages.MainPage.MapFrameLogic.MapFrame _mapFrameLogic;
        // 本组点信息
        public static PointInf Basic_PointInf;
        public static UpdatePoints Update_PointInf;
        public static PointInfTemp Temp_PointInf;

        public Property PageNow;

        public readonly struct Crumb
        {
            public Crumb(String label, int pageIndex)
            {
                Label = label;
                PageIndex = pageIndex;
            }
            public string Label { get; }
            public int PageIndex { get; }
            public override string ToString() => Label;
        }

        ObservableCollection<object> Breadcrumbs =
                        new ObservableCollection<object>();


        private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
        {
            if (args.Index < Breadcrumbs.Count - 1)
            {
             
                var crumb = (Crumb)args.Item;
                setPage(crumb.PageIndex);

                while (Breadcrumbs.Count > args.Index + 1)
                {
                    Breadcrumbs.RemoveAt(Breadcrumbs.Count - 1);
                }

                refreshPage();
            }
        }



        // 最重要的部分
        private Property find(int Index)
        {
            Property Now;
            if (Temp_PointInf.Temp_basicInfo.ContainsKey(Index))
            {
                Now = Temp_PointInf.Temp_basicInfo[Index];
                return Now;
            }
            else if (UpdatePoints.Update_basicInfo.ContainsKey(Index))
            {
                Now = UpdatePoints.Update_basicInfo[Index];
                return Now;
            }
            else if (PointInf.basicInfo.ContainsKey(Index))
            {
                Now = PointInf.basicInfo[Index];
                return Now;
            }
            else 
            { 
                Now = null;
                return Now;
            }
        }

        public void setPage(int Index)
        {
            // 设置页面

            PageNow = find(Index);
            Temp_PointInf.Temp_Page = PageNow;

        }

        public void refreshPage()
        {
            //根据布局创建页面 每次更改信息要同步到布局器中
            Information.Children.Clear();
            // 本次得到的是点信息直接对应的页面 所以先拿出一个Page 然后对这Page的List展示元素
            // 根据List的索引值取出元素
            // 依次从Temp Update Basic中获取信息 然后布局
            List<int> itemList = PageNow.Object as List<int>;
            // 如果是空页面 那么就需要在这里初始化它 而且这个pageIndex

            StackPanel verticalPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Microsoft.UI.Xaml.Thickness(5)
            };
            {
                TextBlock textBlockName = new TextBlock
                {
                    Text = "Name : "
    ,
                    VerticalAlignment = VerticalAlignment.Center
                };

                if (PageNow.Name == null)
                {
                    // 没有Name那它肯定在temp中
                    TextBox textBoxName = new TextBox
                    {
                        Text = PageNow.Name,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    textBoxName.TextChanged += (sender, e) =>
                    {
                        // 更新 newProperty.Name 为 TextBox 的新值
                        PageNow.Name = textBoxName.Text;

                        if (Breadcrumbs.Count > 0)
                        {
                            Breadcrumbs.RemoveAt(Breadcrumbs.Count - 1);
                            Breadcrumbs.Add(new Crumb(PageNow.Name, PageNow.Index));
                        }
                    };

                    {
                        Grid grid = taskforce141(textBlockName, textBoxName, null);
                        Information.Children.Add(grid);
                    }

                }
                else
                {
                    TextBox textBoxName = new TextBox
                    {
                        Text = PageNow.Name,
                        VerticalAlignment = VerticalAlignment.Center,
                        IsReadOnly = true
                    };

                    {
                        Grid grid = taskforce141(textBlockName, textBoxName, null);
                        Information.Children.Add(grid);
                    }

                }

                TextBlock textBlockIndex = new TextBlock
                {
                    Text = "Index : ",
                    VerticalAlignment = VerticalAlignment.Center
                };

                TextBox textIndex = new TextBox
                {
                    Text = $"{PageNow.Index}",
                    VerticalAlignment = VerticalAlignment.Center,
                    IsReadOnly = true
                };

                {
                    Grid grid = taskforce141(textBlockIndex, textIndex, null);
                    verticalPanel.Children.Add(grid);
                }

                Information.Children.Add(verticalPanel);
            }

            if(itemList != null)
                foreach (int index in itemList)
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
                            TextBlock readOnlyTextBox = new TextBlock
                            {
                                Text = $"{ItemNow.Name} : ",
                                Margin = new Microsoft.UI.Xaml.Thickness(5),
                                VerticalAlignment = VerticalAlignment.Center
                            };
                            // 创建可编辑的文本框
                            TextBox editableTextBox = new TextBox
                            {
                                Text = $"{content}",
                                Margin = new Microsoft.UI.Xaml.Thickness(5),
                                VerticalAlignment = VerticalAlignment.Center
                            };

                            editableTextBox.TextChanged += (sender, e) =>
                            {
                                // 复制一份到Temp中 然后修改Temp值
                                var NewTemp = Temp_PointInf.newItem(ItemNow);
                                // 首先这个Page会存入Temp 然后把这个item的原有的Index替换为新的Index
                                // 更新 newProperty.Name 为 TextBox 的新值
                                NewTemp.Object = editableTextBox.Text;
                            };

                            // 添加删除按钮
                            Button DeleteButton = new Button
                            {
                                Content = "Delete",
                                Margin = new Microsoft.UI.Xaml.Thickness(5),
                                VerticalAlignment = VerticalAlignment.Center
                            };

                            {
                                Grid grid = taskforce141(readOnlyTextBox, editableTextBox, DeleteButton);

                                DeleteButton.Click += (sender, e) =>
                                {
                                    //选择项目 删掉这行
                                    Information.Children.Remove(grid);
                                    Temp_PointInf.deleteItem(ItemNow);
                                };
                                Information.Children.Add(grid);
                            }

                        }
                    }
                    else if (ItemNow.Type == "Enum")
                    {
                        // 说明是一个枚举
                        // 用ListView吧！
                        var tuple = ItemNow.Object as List<int>;

                        Property Selected = find(tuple[0]);
                        Property EnumItem = find(tuple[1]);

                        var list = EnumItem.Object as List<int>;

                        ComboBox comboBox = new ComboBox
                        {
                            Margin = new Microsoft.UI.Xaml.Thickness(5),
                            Width = 200 // 设置适当的宽度
                        };

                        // 添加枚举项到 ComboBox
                        foreach (int i in list)
                        {
                            Property enumNow = find(i);

                            // 创建 ComboBoxItem 作为每个选项
                            ComboBoxItem comboBoxItem = new ComboBoxItem
                            {
                                Content = enumNow.Object as string,
                                Tag = enumNow.Index // 保留索引信息
                            };

                            // 添加 ComboBoxItem 到 ComboBox
                            comboBox.Items.Add(comboBoxItem);
                        }

                        ComboBoxItem comboBoxItemNow = new ComboBoxItem
                        {
                            Content = Selected.Object
                        };
                        comboBox.Items.Add(comboBoxItemNow);
                        comboBox.SelectedItem = comboBoxItemNow; // 初始值

                        // 处理 ComboBox 选择变化事件
                        comboBox.SelectionChanged += (sender, e) =>
                        {
                            if (comboBox.SelectedItem is ComboBoxItem selectedItem)
                            {
                                int tempTag = (int)selectedItem.Tag;
                                Property selectedEnum = find(tempTag);
                                var NewTemp = Temp_PointInf.newItem(Selected);
                                NewTemp.Object = selectedEnum.Object; // 将选中的枚举赋值给 Selected
                            }
                        };

                        // 创建不可修改的文本框
                        TextBlock readOnlyTextBox = new TextBlock
                        {
                            Text = $"{ItemNow.Name} :",
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };


                        Button DeleteButton = new Button
                        {
                            Content = "Delete",
                            Margin = new Microsoft.UI.Xaml.Thickness(5),
                            HorizontalAlignment = HorizontalAlignment.Right
                        };

                        {
                        Grid grid = new Grid();

                        // 定义三列，分别用于 TextBox, TextBlock, Button
                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }); // 第1列
                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) }); // 第2列
                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }); // 第3列

                        Grid.SetColumn(readOnlyTextBox, 0);
                        grid.Children.Add(readOnlyTextBox);

                        Grid.SetColumn(comboBox, 1);
                        grid.Children.Add(comboBox);

                        Grid.SetColumn(DeleteButton, 2);
                        grid.Children.Add(DeleteButton);

                        Information.Children.Add(grid);

                        DeleteButton.Click += (sender, e) =>
                        {
                            //选择项目 删掉这行
                            // 结果是当前页面标记更新 送入缓存区 然后切换到然后切换到缓存区页面 再删掉缓存区的索引
                            Information.Children.Remove(grid);
                            // 删掉该枚举 就是删除这个枚举的索引 标记为Deleted
                            Temp_PointInf.deleteItem(ItemNow);
                        };
                    }
                    }
                    else if (ItemNow.Type == "Page")
                    {
                        // 说明是一个页面

                        // 添加一个按钮 跳转到下一个页面
                        Button button = new Button
                        {
                            Content = "Check IN",
                            Margin = new Microsoft.UI.Xaml.Thickness(5),
                            VerticalAlignment = VerticalAlignment.Center
                        };

                        void ButtonClick(object sender, RoutedEventArgs e)
                        {
                            Information.Children.Clear();
                            Breadcrumbs.Add(new Crumb(ItemNow.Name, ItemNow.Index));
                            setPage(ItemNow.Index);
                            refreshPage();
                        }

                        button.Click += ButtonClick;

                        // 创建不可修改的文本框
                        TextBlock NameBlock = new TextBlock
                        {
                            Text = $"Page Name: {ItemNow.Name}",
                            Margin = new Microsoft.UI.Xaml.Thickness(5),
                            Width = 150,
                            VerticalAlignment = VerticalAlignment.Center
                        };

                        Button DeleteButton = new Button
                        {
                            Content = "Delete",
                            Margin = new Microsoft.UI.Xaml.Thickness(5),
                            VerticalAlignment = VerticalAlignment.Center
                        };

                        Grid grid = new Grid();

                        // 定义三列，分别用于 TextBox, TextBlock, Button
                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }); // 第1列
                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) }); // 第2列
                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }); // 第3列

                        Grid.SetColumn(NameBlock, 0);
                        grid.Children.Add(NameBlock);

                        Grid.SetColumn(button, 1);
                        grid.Children.Add(button);

                        Grid.SetColumn(DeleteButton, 2);
                        grid.Children.Add(DeleteButton);
                        

                        DeleteButton.Click += (sender, e) =>
                        {
                            //选择项目 删掉这行
                            Information.Children.Remove(grid);
                            Temp_PointInf.deleteItem(ItemNow);
                        };

                        Information.Children.Add(grid);
                    }
                    else
                    {
                        Console.WriteLine("Error: Unknown Type");
                    }
                }
            Item_Add_Button(PageNow);
        }

        // 如果指定了初始化方式
        // 首先要获取点信息类型列表 在AssetsGet中
        // 其次根据点信息列表初始化控件
        // 如果没有指定 那么就是空白页面和增加表项按钮

        // 添加 元素添加按钮
        private void Item_Add_Button(Property PageNow)
        {
            // 新元素
            

            // 扩展边导航栏
            var NaviPivot = new Pivot();

            var expander = new Expander
            {
                Header = "Add One Item",
                Content = NaviPivot,
                Margin = new Microsoft.UI.Xaml.Thickness(5),
                VerticalAlignment = VerticalAlignment.Center
            };

            StackPanel verticalPanelString = new StackPanel();
            {
                Property newProperty = new Property(null, -1, null, null);

                verticalPanelString.Children.Add(createProerty(newProperty));
                // 添加分隔线到 StackPanel
                verticalPanelString.Children.Add(new Line
                {
                    X1 = 0,
                    X2 = 1,
                    Y1 = 0,
                    Y2 = 0,
                    Stroke = new SolidColorBrush(Microsoft.UI.Colors.Gray),
                    StrokeThickness = 1,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                });
                // 添加元素项

                

                TextBox textBox = new TextBox
                {
                    Text = newProperty.Object as string,
                    Margin = new Microsoft.UI.Xaml.Thickness(5),
                    VerticalAlignment = VerticalAlignment.Center
                };

                verticalPanelString.Children.Add(textBox);

                Button Save_button = new Button
                {
                    Content = "Save",
                    VerticalAlignment = VerticalAlignment.Center
                };

                Save_button.Click += (sender, e) =>
                {
                    System.Diagnostics.Debug.WriteLine(newProperty.Name);
                    if (newProperty.Name != null)
                    {
                        // 赋值
                        newProperty.Object = textBox.Text;
                        // 更新索引
                        newProperty.IndexUpdate();

                        // 写入元素到页面索引表
                        if (PageNow.Object is List<int> tempList)
                        {
                            newProperty.Type = "String";
                            System.Diagnostics.Debug.WriteLine("write in PageNow");

                            // 注册元素
                            Temp_PointInf.Temp_basicInfo.Add(newProperty.Index, newProperty);

                            tempList.Add(newProperty.Index);
                        }

                        // 移除添加按钮
                        Information.Children.Remove(expander);
                        // 刷新页面
                        refreshPage();
                        System.Diagnostics.Debug.WriteLine("sAVED AND REF");
                    }
                };

                verticalPanelString.Children.Add(Save_button);

                var PivotItemString = new PivotItem
                {
                    Header = "String_Item",
                    Content = verticalPanelString
                };

                NaviPivot.Items.Add(PivotItemString);
            }

            StackPanel verticalPanelEnum = new StackPanel();
            {

                Property newProperty = new Property(null, -1, null, new List<int>() );

                Button buttonEnum()
                {
                    Button buttonenum = new Button
                    {
                        Content = "Add Enum",
                        Margin = new Microsoft.UI.Xaml.Thickness(5),
                        VerticalAlignment = VerticalAlignment.Center

                    };

                    buttonenum.Click += (sender, e) =>
                    {
                        // 删除掉最后一个元素 就是添加按钮
                        verticalPanelEnum.Children.RemoveAt(verticalPanelEnum.Children.Count - 1);

                        Button clickedButton = sender as Button;
                        verticalPanelEnum.Children.Remove(clickedButton);
                        // 新元素 元素类型元素项 无名 空值
                        Property eNumTemp = new Property(null, -1, "EnumItem", "NULL");

                        // 更新索引
                        eNumTemp.IndexUpdate();

                        // 注册元素
                        Temp_PointInf.Temp_basicInfo.Add(eNumTemp.Index, eNumTemp);

                        if (newProperty.Object is List<int> tempEnum)
                        {
                            tempEnum.Add(eNumTemp.Index);
                        };

                        TextBox textBox = new TextBox
                        {
                            Text = eNumTemp.Object as string,
                            Margin = new Microsoft.UI.Xaml.Thickness(5),
                        };

                        textBox.TextChanged += (sender, e) =>
                        {
                            // 更新 newProperty.Name 为 TextBox 的新值
                            eNumTemp.Object = textBox.Text;
                        };

                        // 还有删除按钮
                        Button DeleteEnumItem = new Button
                        {
                            Content = "Delete",
                            Margin = new Microsoft.UI.Xaml.Thickness(5),
                        };

                        StackPanel horizontalPanel = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };

                        {
                            Grid grid = taskforce141(null,textBox,DeleteEnumItem);

                            verticalPanelEnum.Children.Add(grid);

                            DeleteEnumItem.Click += (sender, e) =>
                            {
                                //选择项目 删掉这行
                                verticalPanelEnum.Children.Remove(grid);
                                Temp_PointInf.Temp_basicInfo.Remove(eNumTemp.Index);
                            };
                        }

                        verticalPanelEnum.Children.Add(buttonEnum());
                        verticalPanelEnum.Children.Add(CreateEnumSave());
                    };

                    return buttonenum;
                }

                // 也是还没写 向布局中添加文本框并创建字符串类型数据塞入缓存区

                verticalPanelEnum.Children.Add(createProerty(newProperty));

                verticalPanelEnum.Children.Add(new Line
                {
                    X1 = 0,
                    X2 = 1,
                    Y1 = 0,
                    Y2 = 0,
                    Stroke = new SolidColorBrush(Microsoft.UI.Colors.Gray),
                    StrokeThickness = 1,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                });

                verticalPanelEnum.Children.Add(buttonEnum());


                Button CreateEnumSave()
                {
                    Button EnumSave_button = new Button
                    {
                        Content = "Save",
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    EnumSave_button.Click += (sender, e) =>
                    {
                        if (newProperty.Name != null)
                        {
                            newProperty.Type = "EnumList";

                            newProperty.IndexUpdate();

                            // 枚举选择项
                            Property eNumSelectTemp = new Property(null, -1, "EnumItem", "No Set");
                            eNumSelectTemp.IndexUpdate();

                            // 真正的枚举项
                            Property TrueeNum = new Property(
                                newProperty.Name, -1, "Enum", 
                                new List<int>(){ eNumSelectTemp.Index, newProperty.Index});

                            TrueeNum.IndexUpdate();

                            // 放到页面索引表中
                            if (PageNow.Object is List<int> tempList)
                            {
                                tempList.Add(TrueeNum.Index);
                            }
                            // 注册枚举和枚举选择项以及枚举
                            Temp_PointInf.Temp_basicInfo.Add(eNumSelectTemp.Index, eNumSelectTemp);
                            Temp_PointInf.Temp_basicInfo.Add(TrueeNum.Index, TrueeNum);
                            Temp_PointInf.Temp_basicInfo.Add(newProperty.Index, newProperty);

                            Information.Children.Remove(expander);
                            refreshPage();
                        }
                    };
                    return EnumSave_button;
                }

                verticalPanelEnum.Children.Add(CreateEnumSave());

                var PivotItemEnum = new PivotItem
                {
                    Header = "Enum",
                    Content = verticalPanelEnum
                };
                NaviPivot.Items.Add(PivotItemEnum);

            }

            StackPanel verticalPanelPage = new StackPanel();
            {
                Property newProperty = new Property(null, -1, null, new List<int>() );

                verticalPanelPage.Children.Add(createProerty(newProperty));

                verticalPanelPage.Children.Add(new Line
                {
                    X1 = 0,
                    X2 = 1,
                    Y1 = 0,
                    Y2 = 0,
                    Stroke = new SolidColorBrush(Microsoft.UI.Colors.Gray),
                    StrokeThickness = 1,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                });

                Button buttonPage = new Button
                {
                    Content = "Add Page",
                    Margin = new Microsoft.UI.Xaml.Thickness(5),
                    VerticalAlignment = VerticalAlignment.Center
                };

                buttonPage.Click += (sender, e) =>
                {
                    if (newProperty.Name != null)
                    {
                        newProperty.Type = "Page";
                        newProperty.IndexUpdate();

                        if (PageNow.Object is List<int> tempList)
                        {
                            tempList.Add(newProperty.Index);
                        }

                        Temp_PointInf.Temp_basicInfo.Add(newProperty.Index, newProperty);
                        Information.Children.Remove(expander);
                        refreshPage();
                    }
                };

                verticalPanelPage.Children.Add(buttonPage);

                var PivotItemPage = new PivotItem
                {
                    Header = "Page",
                    Content = verticalPanelPage
                };

                NaviPivot.Items.Add(PivotItemPage);
            }

            Information.Children.Add(expander);
        }


        private StackPanel createProerty(Property newProperty)
        {
            StackPanel verticalPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Microsoft.UI.Xaml.Thickness(5)
            };

            TextBlock textBlockName = new TextBlock
            {
                Text = "Name : ",
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBox textBoxName = new TextBox
            {
                Text = newProperty.Name,
                VerticalAlignment = VerticalAlignment.Center
            };

            textBoxName.TextChanged += (sender, e) =>
            {
                // 更新 newProperty.Name 为 TextBox 的新值
                newProperty.Name = textBoxName.Text;
            };

            TextBlock textBlockIndex = new TextBlock
            {
                Text = "Index : ",
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock textIndex = new TextBlock
            {
                Text = $"{newProperty.Index}",
                VerticalAlignment = VerticalAlignment.Center
            };

            {
                Grid grid = taskforce141(textBlockName,textBoxName,null);

                verticalPanel.Children.Add(grid);
            }

            return verticalPanel;
        }



        private void PageView(Property EnumNow, ListViewItem listViewItem)
        {
            // 这里展示页面信息
            var enumValue = EnumNow.date;

            // 显示一个 Flyout 提示信息
            var flyout = new Flyout
            {
                Content = new TextBlock { Text = $"{enumValue}" }
            };

            // 在当前元素处显示 Flyout
            flyout.ShowAt(listViewItem);
        }

        public void PaneClear()
        {
            PointInfFrame.Temp_PointInf.clear();
            Information.Children.Clear();
        }
        public void PaneStart()
        {
            Breadcrumbs.Clear();
            Breadcrumbs.Add(new Crumb(PageNow.Name, PageNow.Index));
        }


        // 底部四个按钮

        private void SavePoint_Click(object sender, RoutedEventArgs e)
        {
            if(pointInfSelect().isTemp == false)
            {
                // 永久点的保存则为更新
                pointInfSelect().updated = true;
            }
            else
            {
                // 暂时点的保存则转为永久点
                pointInfSelect().isTemp = false;
            }

            // 暂时点确认 合并到更新列表 清空缓存区
            Temp_PointInf.Temp_Point = pointInfSelect();
            UpdatePoints.merge(Temp_PointInf);
            Temp_PointInf.clear();
            // 保存数据到更新请求列表
        }

        private void AbrotPoint_Clock(object sender, RoutedEventArgs e)
        {
            Temp_PointInf.clear();
            _mapFrameLogic.releasePoint();
            _mapFrameLogic.rightPaneControl();
            // 退出不保存 放弃本点更改 释放点 释放侧面板
        }

        private void ExpPoint_Clock(object sender, RoutedEventArgs e)
        {
            _mapFrameLogic.paneExp();
            // 扩展面板
        }

        private void DeletePoint_Clock(object sender, RoutedEventArgs e)
        {
            // 地图上删除点 确认点删除
            if (pointInfSelect().isTemp == true)
            {
                // 不是永久点 释放
                _mapFrameLogic.RemovePoint();
            }
            else
            {
                // 是永久点 删除
                _mapFrameLogic.RemovePoint();
                pointInfSelect().deleted = true;
                Temp_PointInf.Temp_Point = pointInfSelect();
                UpdatePoints.remove(Temp_PointInf);
            }
        }

        public void GetMapFrame(GeoGraph.Pages.MainPage.MapFrameLogic.MapFrame mapFrameLogic)
        {
            _mapFrameLogic = mapFrameLogic;
        }


        // 用于创建Item 的 Grid
        private Grid taskforce141(TextBlock textBlock, TextBox textBox, Button button)
        {
            // 创建一个新的 Grid
            Grid grid = new Grid();

            // 定义三列，分别用于 TextBox, TextBlock, Button
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }); // 第1列
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) }); // 第2列
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }); // 第3列

            // 自动识别并添加 TextBox 到 Grid 的第2列
            if (textBlock != null)
            {
                Grid.SetColumn(textBlock, 0);
                grid.Children.Add(textBlock);
            }

            // 自动识别并添加 TextBlock 到 Grid 的第1列
            if (textBox != null)
            {
                Grid.SetColumn(textBox, 1);
                grid.Children.Add(textBox);
            }

            // 自动识别并添加 Button 到 Grid 的第3列
            if (button != null)
            {
                Grid.SetColumn(button, 2);
                grid.Children.Add(button);
            }

            return grid;
        }
    }
}
