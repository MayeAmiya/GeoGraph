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
using static GeoGraph.Network.Update;
using System.Runtime.CompilerServices;
using Windows.Devices.Enumeration;
using Microsoft.UI.Xaml.Documents;
using System.Reflection;
using WinRT;
using Microsoft.UI.Xaml.Shapes;

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

        }

        // 本组点信息
        public static PointInf Basic_PointInf;
        public static Update Update_PointInf;
        public static PointInfTemp Temp_PointInf;
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
            else if (Basic_PointInf.basicInfo.ContainsKey(Index))
            {
                Now = Basic_PointInf.basicInfo[Index];
            }
            else 
            { 
                Now = null;
            }
            return Now;
        }

        public void refreshPage(int pageIndex)
        {
            //根据布局创建页面 每次更改信息要同步到布局器中
            Information.Children.Clear();
            // 本次得到的是点信息直接对应的页面 所以先拿出一个Page 然后对这Page的List展示元素
            // 根据List的索引值取出元素
            // 依次从Temp Update Basic中获取信息 然后布局
            Property PageNow = find(pageIndex);
            List<int> itemList = PageNow.Object as List<int>;
            // 如果是空页面 那么就需要在这里初始化它 而且这个pageIndex

            StackPanel verticalPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Microsoft.UI.Xaml.Thickness(5)
            };

            StackPanel horizontalPanelName = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Microsoft.UI.Xaml.Thickness(5)
            };

            TextBlock textBlockName = new TextBlock
            {
                Text = "Name : "
                ,VerticalAlignment = VerticalAlignment.Center
            };

            horizontalPanelName.Children.Add(textBlockName);

            if (PageNow.Name == null)
            {
                TextBox textBoxName = new TextBox
                {
                    Text = PageNow.Name,
                    VerticalAlignment = VerticalAlignment.Center
                };

                textBoxName.TextChanged += (sender, e) =>
                {
                    // 更新 newProperty.Name 为 TextBox 的新值
                    PageNow.Object = textBoxName.Text;
                };

                horizontalPanelName.Children.Add(textBoxName);
            }
            else
            {
                TextBlock textBoxName = new TextBlock
                {
                    Text = PageNow.Name,
                    VerticalAlignment = VerticalAlignment.Center
                };
                horizontalPanelName.Children.Add(textBoxName);
            }

            TextBlock textBlockIndex = new TextBlock
            {
                Text = $"Index : {PageNow.Index}",
                VerticalAlignment = VerticalAlignment.Center
            };

            StackPanel horizontalPanelIndex = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Microsoft.UI.Xaml.Thickness(5)
            };

            horizontalPanelIndex.Children.Add(textBlockIndex);

            // 竖直排列 作为主体

            verticalPanel.Children.Add(horizontalPanelName);
            verticalPanel.Children.Add(horizontalPanelIndex);
            Information.Children.Add(verticalPanel);

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
                            // 首先这个Page会存入Temp 然后把这个item的原有的Index替换为新的Index
                            // 更新 newProperty.Name 为 TextBox 的新值
                            ItemNow.Object = editableTextBox.Text;
                        };

                    // 创建水平布局容器
                    StackPanel horizontalPanel = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Margin = new Microsoft.UI.Xaml.Thickness(5),
                            VerticalAlignment = VerticalAlignment.Center
                        };

                        // 将文本框添加到水平布局容器中
                        horizontalPanel.Children.Add(readOnlyTextBox);
                        horizontalPanel.Children.Add(editableTextBox);
                        // 添加删除按钮
                        Button DeleteButton = new Button
                        {
                            Content = "Delete -",
                            Margin = new Microsoft.UI.Xaml.Thickness(5),
                            VerticalAlignment = VerticalAlignment.Center
                        };

                        DeleteButton.Click += (sender, e) =>
                        {
                            //选择项目 删掉这行
                            Information.Children.Remove(horizontalPanel);
                        };

                        horizontalPanel.Children.Add(DeleteButton);
                        // 将水平布局容器添加到页面的某个容器控件中（例如 StackPanel 或 Grid）
                        Information.Children.Add(horizontalPanel);

                    }
                    if (ItemNow.Type == "Image")
                        {
                            // 说明是一个图片 这里是图片名 我们需要下载图片并且在asset中找到他 暂且不做
                            string content = ItemNow.Object as string;
                            // 创建一个图像显示 缩放至合适大小 宽度为控件宽度
                            string ImagePath = Assets.FindImage(content);
                            Image image = new Image
                            {
                                Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(content)),
                                Stretch = Microsoft.UI.Xaml.Media.Stretch.Uniform,
                                Width = 150,
                                Height = 150
                            };
                            StackPanel horizontalPanel = new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                Margin = new Microsoft.UI.Xaml.Thickness(5)
                            };
                            horizontalPanel.Children.Add(image);

                            Button DeleteButton = new Button
                            {
                                Content = "Delete -",
                                Margin = new Microsoft.UI.Xaml.Thickness(5),

                            };

                            DeleteButton.Click += (sender, e) =>
                            {
                                //选择项目 删掉这行
                                Information.Children.Remove(horizontalPanel);
                            };

                            horizontalPanel.Children.Add(DeleteButton);
                            Information.Children.Add(horizontalPanel);
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
                            Selected.Object = selectedEnum.Object; // 将选中的枚举赋值给 Selected
                        }
                    };


                    StackPanel horizontalPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Microsoft.UI.Xaml.Thickness(5),
                    };

                    // 创建不可修改的文本框
                    TextBlock readOnlyTextBox = new TextBlock
                    {
                        Text = $"{ItemNow.Name} :",
                        Margin = new Microsoft.UI.Xaml.Thickness(5)
                    };

                    horizontalPanel.Children.Add(readOnlyTextBox);

                    Button DeleteButton = new Button
                    {
                        Content = "Delete",
                        Margin = new Microsoft.UI.Xaml.Thickness(5),
                        HorizontalAlignment = HorizontalAlignment.Right
                    };

                    horizontalPanel.Children.Add(comboBox);

                    DeleteButton.Click += (sender, e) =>
                    {
                        //选择项目 删掉这行
                        // 结果是当前页面标记更新 送入缓存区 然后切换到然后切换到缓存区页面 再删掉缓存区的索引
                        Information.Children.Remove(horizontalPanel);

                    };

                    horizontalPanel.Children.Add(DeleteButton);

                    Information.Children.Add(horizontalPanel);
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
                            Temp_PointInf.newPage(ItemNow.Index);
                            Information.Children.Clear();
                            refreshPage(ItemNow.Index);
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

                        // 创建水平布局容器
                        StackPanel horizontalPanel = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };

                        // 将文本框添加到水平布局容器中
                        horizontalPanel.Children.Add(NameBlock);
                        horizontalPanel.Children.Add(button);

                        Button DeleteButton = new Button
                        {
                            Content = "Delete -",
                            Margin = new Microsoft.UI.Xaml.Thickness(5),
                            VerticalAlignment = VerticalAlignment.Center
                        };

                        DeleteButton.Click += (sender, e) =>
                        {
                            //选择项目 删掉这行
                            Information.Children.Remove(horizontalPanel);
                        };

                        horizontalPanel.Children.Add(DeleteButton);
                        // 将水平布局容器添加到页面的某个容器控件中（例如 StackPanel 或 Grid）
                        Information.Children.Add(horizontalPanel);
                    }
                else
                    {
                        Console.WriteLine("Error: Unknown Type");
                    }
                }
            Item_Add_Button(PageNow);
        }

        public void PaneClear()
        {
            Information.Children.Clear();
        }

        // 如果指定了初始化方式
        // 首先要获取点信息类型列表 在AssetsGet中
        // 其次根据点信息列表初始化控件
        // 如果没有指定 那么就是空白页面和增加表项按钮

        // 添加 元素添加按钮
        private void Item_Add_Button(Property PageNow)
        {
            //这里应该是个flyout
            //选择项目 展开选择行


            Property newProperty = new Property(null, -1, null, null);


            var NaviPivot = new Pivot();
            NaviPivot.SelectionChanged += (sender, e) => AddItem_Click(sender, e, newProperty);

            var expander = new Expander
            {
                Header = "Add Item",
                Content = NaviPivot,
                Margin = new Microsoft.UI.Xaml.Thickness(5),
                VerticalAlignment = VerticalAlignment.Center

            };

            StackPanel verticalPanelString = new StackPanel();
            StackPanel verticalPanelPage = new StackPanel();
            StackPanel verticalPanelEnum = new StackPanel();


            TextBox textBox = new TextBox
            {
                Text = newProperty.Object as string,
                VerticalAlignment = VerticalAlignment.Center
            };

            textBox.TextChanged += (sender, e) =>
            {
                // 更新 newProperty.Name 为 TextBox 的新值
                newProperty.Object = textBox.Text;
            };

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
            verticalPanelString.Children.Add(textBox);

            Button Save_button = new Button
            {
                Content = "Save",
                VerticalAlignment = VerticalAlignment.Center
            };

            Save_button.Click += (sender, e) =>
            {
                if (newProperty.Name != null)
                {
                    newProperty.IndexUpdate();
                    System.Diagnostics.Debug.WriteLine(newProperty.Index);

                    if (PageNow.Object is List<int> tempList)
                    {
                        tempList.Add(newProperty.Index);
                    }

                    Temp_PointInf.Temp_basicInfo.Add(newProperty.Index, newProperty);
                    Information.Children.Remove(expander);
                    refreshPage(PageNow.Index);
                }
            };

            verticalPanelString.Children.Add(Save_button);

            var PivotItemString = new PivotItem
            {
                Header = "String_Item",
                Content = verticalPanelString
            };

            // 创建字符串类型数据塞入缓存区 并加入Enum的Object 并创建一个textbox指向这个数据 最后添加一个新增按钮
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
                    verticalPanelEnum.Children.RemoveAt(verticalPanelEnum.Children.Count - 1);

                    Button clickedButton = sender as Button;
                    verticalPanelEnum.Children.Remove(clickedButton);

                    Property eNumTemp = new Property("NoName", -1, "EnumString", "NULL");
                    eNumTemp.IndexUpdate();
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

                    horizontalPanel.Children.Add(textBox);
                    horizontalPanel.Children.Add(DeleteEnumItem);

                    DeleteEnumItem.Click += (sender, e) =>
                    {
                        //选择项目 删掉这行
                        verticalPanelEnum.Children.Remove(horizontalPanel);
                        Temp_PointInf.Temp_basicInfo.Remove(eNumTemp.Index);
                    };

                    verticalPanelEnum.Children.Add(horizontalPanel);

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
                        newProperty.IndexUpdate();

                        // 枚举选择项
                        Property eNumSelectTemp = new Property("NoName", -1, "String", "No Set");
                        // 从枚举列表中获取一个作为初始值
                        eNumSelectTemp.IndexUpdate();
                        // 真正的枚举项
                        Property TrueeNum = new Property(newProperty.Name, -1, "Enum", new List<int>()
                        { eNumSelectTemp.Index, newProperty.Index});

                        TrueeNum.IndexUpdate();

                        if (PageNow.Object is List<int> tempList)
                        {
                            tempList.Add(TrueeNum.Index);
                        }

                        Temp_PointInf.Temp_basicInfo.Add(eNumSelectTemp.Index, eNumSelectTemp);
                        Temp_PointInf.Temp_basicInfo.Add(TrueeNum.Index, TrueeNum);

                        Temp_PointInf.Temp_basicInfo.Add(newProperty.Index, newProperty);
                        Information.Children.Remove(expander);
                        refreshPage(PageNow.Index);
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

            // 一个确认按钮 是否创建新页面 确定了则创建新页面加入原有的Page中
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
                    newProperty.IndexUpdate();
                    PageNow.Object.As<List<int>>().Add(newProperty.Index);
                    Temp_PointInf.Temp_basicInfo.Add(newProperty.Index, newProperty);
                    Information.Children.Remove(expander);
                    refreshPage(PageNow.Index);
                }
            };

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
            verticalPanelPage.Children.Add(buttonPage);

            var PivotItemPage = new PivotItem
            {
                Header = "Page",
                Content = verticalPanelPage
            };

            NaviPivot.Items.Add(PivotItemString);
            NaviPivot.Items.Add(PivotItemEnum);
            NaviPivot.Items.Add(PivotItemPage);

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

            StackPanel horizontalPanelName = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Microsoft.UI.Xaml.Thickness(5)
            };

            horizontalPanelName.Children.Add(textBlockName);
            horizontalPanelName.Children.Add(textBoxName);

            TextBlock textBlockIndex = new TextBlock
            {
                Text = $"Index : {newProperty.Index}",
                VerticalAlignment = VerticalAlignment.Center
            };


            StackPanel horizontalPanelIndex = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Microsoft.UI.Xaml.Thickness(5)
            };

            horizontalPanelIndex.Children.Add(textBlockIndex);



            StackPanel horizontalPanelType = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Microsoft.UI.Xaml.Thickness(5)
            };

            TextBlock textBlockType = new TextBlock
            {
                Text = "Type  : ",
                VerticalAlignment = VerticalAlignment.Center

            };

            horizontalPanelType.Children.Add(textBlockType);

            TextBlock textBoxType = new TextBlock
            {
                Text = newProperty.Type,
                VerticalAlignment = VerticalAlignment.Center
            };

            horizontalPanelType.Children.Add(textBoxType);
            

            // 竖直排列 作为主体

            verticalPanel.Children.Add(horizontalPanelName);
            verticalPanel.Children.Add(horizontalPanelIndex);
            verticalPanel.Children.Add(horizontalPanelType);

            return verticalPanel;
        }

        private void AddItem_Click(object sender, SelectionChangedEventArgs e, Property newProperty)
        {
            var pivot = sender as Pivot;
            if (pivot != null)
            {
                // 获取当前选中的项
                var selectedItem = pivot.SelectedItem as PivotItem;

                if (selectedItem != null)
                {
                    // 根据 Header 修改 myTextBlock 的值
                    switch (selectedItem.Header.ToString())
                    {
                        case "String_Item":
                            newProperty.Type = "String";
                            newProperty.Object = "New String";
                            break;
                        case "Enum":
                            newProperty.Type = "Enum";
                            newProperty.Object = new List<int>();
                            break;
                        case "Page":
                            newProperty.Type = "Page";
                            newProperty.Object = new List<int>();
                            // 确认按钮 创建新页面并跳转到新页面 同时旧页面保存到缓存区
                            break;
                    }
                }
            }
        }

        private Button Item_Update_Button(StackPanel horizontalPanel)
        {
            Button UpdateButton = new Button
            {
                Content = "Update",
                Margin = new Microsoft.UI.Xaml.Thickness(5),
            };
            void UpdateButtonClick(object sender, RoutedEventArgs e)
            {
                //选择项目 更新这行
            }
            UpdateButton.Click += UpdateButtonClick;
            Information.Children.Add(UpdateButton);
            return UpdateButton;
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

        private void SavePoint_Click(object sender, RoutedEventArgs e)
        {
            Update_PointInf.merge(Temp_PointInf);
            // 保存数据到更新请求列表
        }

        private void AbrotPoint_Clock(object sender, RoutedEventArgs e)
        {
            Temp_PointInf.clear();
            // 退出不保存 放弃本点更改
        }

    }
}
