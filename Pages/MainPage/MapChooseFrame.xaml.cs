using GeoGraph.Network;
using GeoGraph.Pages.MainPage.MapFrameLogic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.AccessControl;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GeoGraph.Pages.MainPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// 展示地图列表 展示地图图片和信息
    /// 拿到Assets中的地图列表 M
    /// </summary>
    public sealed partial class MapChooseFrame : Page
    {
        public class MapInfo
        {
            public string MapName;
            public int Width;
            public int Height;
            public string ImagePath;
            public string MapInf;
            public int MapCode;

            // struct is deep copy

            public MapInfo(MapInfo temp)
            {
                MapName = temp.MapName;   
                Width = temp.Width;       
                Height = temp.Height;     
                ImagePath = temp.ImagePath;  
                MapInf = temp.MapInf;
                MapCode = temp.MapCode;

            }

            public MapInfo(string MapName, int Width, int Height, string ImagePath, string MapInf, int MapCode)
            {
                this.MapName = MapName;
                this.Width = Width;
                this.Height = Height;
                this.ImagePath = ImagePath;
                this.MapInf = MapInf;
                this.MapCode = MapCode;
            }

        }

        List<MapInfo> mapinfos;

        public MapChooseFrame()
        {
            this.InitializeComponent();

            this.GetMapInfo();

            System.Diagnostics.Debug.WriteLine("MapChooseHere");

            this.InitializeMapListView();
        }

        public void GetMapInfo()
        {
            System.Diagnostics.Debug.WriteLine("mapinfosHere");
            Assets.ParseMapInfAsync();
            mapinfos = GeoGraph.Network.Assets.MapList;
            if(mapinfos is null)
            {
                System.Diagnostics.Debug.WriteLine("mapinfos is null");
                mapinfos = new List<MapInfo>();
            }
        }

        //布置巨大LISTVIEW 向MapListView中填充地图信息
        private void InitializeMapListView()
        {
            // 清空原有的地图项
            MapListView.Items.Clear();
            // 添加地图项
            if (mapinfos != null)
            {
                foreach (var map in mapinfos)
                {
                    // 创建地图项
                    ListViewItem item;
                    StackPanel stackPanel;
                    if (map.MapName is not null)
                    {
                        TextBlock textBlockName = new TextBlock
                        {
                            Text = map.MapName,
                            FontSize = 16, // 设置字体大小
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };
                        TextBlock textBlockSize = new TextBlock
                        {
                            Text = "Size = " + map.Width.ToString() + " x " + map.Height.ToString(),
                            FontSize = 16, // 设置字体大小
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };

                        stackPanel = new StackPanel
                        {
                            Orientation = Orientation.Vertical, // 垂直排列
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };

                        // 将 TextBlock 添加到 StackPanel 中
                        stackPanel.Children.Add(textBlockName);
                        stackPanel.Children.Add(textBlockSize);
                        // 再添加一个选中时返回mapinfo对象的回调
                    }
                    else
                    {
                        TextBlock textBlockName = new TextBlock
                        {
                            Text = "New Map",
                            FontSize = 16, // 设置字体大小
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };

                        TextBlock textBlockEmpty = new TextBlock
                        {
                            Text = "    waiting for input",
                            FontSize = 16, // 设置字体大小
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };

                        stackPanel = new StackPanel
                        {
                            Orientation = Orientation.Vertical, // 垂直排列
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };

                        // 将 TextBlock 添加到 StackPanel 中
                        stackPanel.Children.Add(textBlockName);
                        stackPanel.Children.Add(textBlockEmpty);
                    }

                    item = new ListViewItem()
                    {
                        Content = stackPanel,
                        Tag = map
                    };
                    // 添加到地图列表
                    MapListView.Items.Add(item);
                }
            }
            
            TextBlock BlockNew = new TextBlock
            {
                Text = "NewMap Create",
                FontSize = 16, // 设置字体大小
                Margin = new Microsoft.UI.Xaml.Thickness(5)
            };

            TextBlock BlockEmpty = new TextBlock
            {
                Text = "    check to create new map",
                FontSize = 16, // 设置字体大小
                Margin = new Microsoft.UI.Xaml.Thickness(5)
            };

            StackPanel Panel = new StackPanel
            {
                Orientation = Orientation.Vertical, // 垂直排列
                Margin = new Microsoft.UI.Xaml.Thickness(5)
            };

            Panel.Children.Add(BlockNew);
            Panel.Children.Add(BlockEmpty);

            ListViewItem itemAdd = new ListViewItem
            {
                Content = Panel
            };

            itemAdd.Tapped += AddButton_Click;

            MapListView.Items.Add(itemAdd);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // 添加地图信息到列表

            foreach (var item in mapinfos)
            {
                if (item.MapName == null)
                {
                    return;
                }
            }

            var temp = new MapInfo
            (
                null,
                -1,
                -1,
                null,
                null,
                DateTime.Now.ToString().GetHashCode() ^ "MapInfo".GetHashCode()
            );

            mapinfos.Add(temp);

            // 刷新地图列表显示
            InitializeMapListView();
            // 同时触发selectionchanged事件
            MapListView_SelectionChanged(sender, null);
        }

        private void MapListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 获取选中的地图项
            if (MapListView.SelectedItem is ListViewItem selectedItem)
            {

                MapListViewInf.Children.Clear();
                // 显示划出面板
                InfoPanelSplitView.IsPaneOpen = true;

                System.Diagnostics.Debug.WriteLine("Selected");
                // 现在放置内容
                // 根据mapinfos
                if (selectedItem.Tag is MapInfo selectedMap)
                {
                    System.Diagnostics.Debug.WriteLine("selectedItem.Tag is MapInfo selectedMap");
                    MapInfo mapInfotemp = new MapInfo(selectedMap);

                    Microsoft.UI.Xaml.Controls.Image MapImage = new Microsoft.UI.Xaml.Controls.Image
                    {
                        Stretch = Stretch.Uniform,  // 设置图像缩放方式
                        Margin = new Thickness(10)  // 可选：设置边距
                    };
                    MapListViewInf.Children.Add(MapImage);

                    if (mapInfotemp.MapName is null)
                    {
                        System.Diagnostics.Debug.WriteLine("mapInfotemp.MapName == \"NULL\"");

                        // 添加图像选择

                        //逐对添加文本框
                        TextBlock textName = new TextBlock
                        {
                            Text = $"MapName : ",
                            FontSize = 16, // 设置字体大小
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };

                        TextBox textBlockName = new TextBox
                        {
                            Text = mapInfotemp.MapName,
                            FontSize = 16, // 设置字体大小
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };

                        TextBlock textSize = new TextBlock
                        {
                            Text = $"Size : ",
                            FontSize = 16, // 设置字体大小
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };

                        TextBlock textBlockSize = new TextBlock
                        {
                            Text = $"Not Set",
                            FontSize = 16, // 设置字体大小
                            Margin = new Microsoft.UI.Xaml.Thickness(5),
                        };

                        TextBlock textInf = new TextBlock
                        {
                            Text = $"Description : ",
                            FontSize = 16, // 设置字体大小
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };

                        TextBox textBlockInf = new TextBox
                        {
                            Text = mapInfotemp.MapInf,
                            FontSize = 16, // 设置字体大小
                            Margin = new Microsoft.UI.Xaml.Thickness(5),
                            AcceptsReturn = true, // 允许回车换行
                        };

                        Button imageButton = new Button
                        {
                            Content = "Choose Image",
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };

                        StackPanel stackPanelName = new StackPanel
                        {
                            Orientation = Orientation.Vertical, // 垂直排列
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };
                        StackPanel stackPanelSize = new StackPanel
                        {
                            Orientation = Orientation.Vertical, // 垂直排列
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };
                        StackPanel stackPanelInf = new StackPanel
                        {
                            Orientation = Orientation.Vertical, // 垂直排列
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };

                        imageButton.Click += async (sender, e) =>
                        {
                            try
                            {
                                var picker = new Windows.Storage.Pickers.FileOpenPicker();

                                IntPtr hwnd = WindowNative.GetWindowHandle(App.GetWindow());
                                InitializeWithWindow.Initialize(picker, hwnd);

                                picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;

                                // 只显示图片文件类型
                                picker.FileTypeFilter.Add(".jpg");
                                picker.FileTypeFilter.Add(".jpeg");
                                picker.FileTypeFilter.Add(".png");

                                // 显示文件选择器
                                Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();

                                if (file != null)
                                {
                                    // 用户选择了图片
                                    var bitmapImage = new BitmapImage();
                                    mapInfotemp.ImagePath = file.Path;
                                    bitmapImage.UriSource = new Uri(mapInfotemp.ImagePath);
                                    MapImage.Source = bitmapImage;
                                    while(bitmapImage.PixelWidth == 0)
                                    {
                                        await System.Threading.Tasks.Task.Delay(100);
                                    }

                                    textBlockSize.Text = $"{bitmapImage.PixelWidth} x {bitmapImage.PixelHeight}";

                                    mapInfotemp.Width = bitmapImage.PixelWidth;
                                    mapInfotemp.Height = bitmapImage.PixelHeight;
                                    System.Diagnostics.Debug.WriteLine(bitmapImage.PixelWidth);
                                    MapListViewInf.Children.Remove(imageButton);

                                    // 你可以在这里处理选中的图片文件路径，加载图片等操作
                                }
                                else
                                {
                                    // 用户取消了文件选择
                                    Debug.WriteLine("No image selected.");
                                }
                            }
                            catch (Exception ex)
                            {
                                // 输出异常信息
                                System.Diagnostics.Debug.WriteLine(ex.Message);
                            }
                        };

                        // 添加一个保存按钮 就是把原有的替换掉
                        Button saveButton = new Button
                        {
                            Content = "Save",
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };

                        saveButton.Click += (sender, e) =>
                        {
                            mapInfotemp.MapName = textBlockName.Text;
                            mapInfotemp.MapInf = textBlockInf.Text;
                            if (mapInfotemp.MapName != null)
                            {
                                if(mapInfotemp.ImagePath!= null)
                                {
                                    mapinfos.Add(mapInfotemp);
                                    // 刷新地图列表
                                    mapinfos.Remove(selectedMap);
                                    GeoGraph.Network.Update.UpdateMap(mapInfotemp);
                                    InitializeMapListView();
                                    MapListView_SelectionChanged(selectedItem, null);
                                    MapChoosed(mapInfotemp);
                                }
                            }
                        };


                        // 将 TextBlock 添加到 StackPanel 中

                        
                        stackPanelName.Children.Add(textName);
                        stackPanelName.Children.Add(textBlockName);
                        stackPanelSize.Children.Add(textSize);
                        stackPanelSize.Children.Add(textBlockSize);
                        stackPanelInf.Children.Add(textInf);
                        stackPanelInf.Children.Add(textBlockInf);


                        MapListViewInf.Children.Add(imageButton);
                        MapListViewInf.Children.Add(stackPanelName);
                        MapListViewInf.Children.Add(stackPanelSize);
                        MapListViewInf.Children.Add(stackPanelInf);
                        MapListViewInf.Children.Add(saveButton);
                    }
                    else
                    {
                        // 添加图像 限定大小
                        var bitmapImage = new BitmapImage();
                        bitmapImage.UriSource = new Uri(mapInfotemp.ImagePath);
                        MapImage.Source = bitmapImage;

                        //逐对添加文本框
                        TextBlock textName = new TextBlock
                        {
                            Text = $"MapName : ",
                            FontSize = 16, // 设置字体大小
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };

                        TextBox textBlockName = new TextBox
                        {
                            Text = mapInfotemp.MapName,
                            FontSize = 16, // 设置字体大小
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };

                        TextBlock textSize = new TextBlock
                        {
                            Text = $"Size : ",
                            FontSize = 16, // 设置字体大小
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };

                        TextBlock textBlockSize = new TextBlock
                        {
                            Text = $"{mapInfotemp.Width.ToString() + " x " + mapInfotemp.Height.ToString()}",
                            FontSize = 16, // 设置字体大小
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };

                        TextBlock textInf = new TextBlock
                        {
                            Text = $"Description : ",
                            FontSize = 16, // 设置字体大小
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };

                        TextBox textBlockInf = new TextBox
                        {
                            Text = mapInfotemp.MapInf,
                            FontSize = 16, // 设置字体大小
                            Margin = new Microsoft.UI.Xaml.Thickness(5),
                            AcceptsReturn = true, // 允许回车换行
                        };

                        StackPanel stackPanelImage = new StackPanel
                        {
                            Orientation = Orientation.Vertical, // 垂直排列
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };
                        StackPanel stackPanelName = new StackPanel
                        {
                            Orientation = Orientation.Vertical, // 垂直排列
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };
                        StackPanel stackPanelSize = new StackPanel
                        {
                            Orientation = Orientation.Vertical, // 垂直排列
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };
                        StackPanel stackPanelInf = new StackPanel
                        {
                            Orientation = Orientation.Vertical, // 垂直排列
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };
                        // 将 TextBlock 添加到 StackPanel 中
                        stackPanelName.Children.Add(textName);
                        stackPanelName.Children.Add(textBlockName);
                        stackPanelSize.Children.Add(textSize);
                        stackPanelSize.Children.Add(textBlockSize);
                        stackPanelInf.Children.Add(textInf);
                        stackPanelInf.Children.Add(textBlockInf);

                        Button copyButton = new Button
                        {
                            Content = "Copy",
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };

                        Button chooseButton = new Button
                        {
                            Content = "Choose",
                            Margin = new Microsoft.UI.Xaml.Thickness(5)
                        };

                        copyButton.Click += (sender, e) =>
                        {
                            mapInfotemp.MapName = textBlockName.Text;
                            mapInfotemp.MapInf = textBlockInf.Text;
                            // 保存地图信息
                            // 
                            mapinfos.Add(mapInfotemp);
                            // 刷新地图列表
                            InitializeMapListView();
                        };

                        chooseButton.Click += (sender, e) => MapChoosed(selectedMap);

                        // StackPanel 添加到布局中
                        MapListViewInf.Children.Add(stackPanelImage);
                        MapListViewInf.Children.Add(stackPanelName);
                        MapListViewInf.Children.Add(stackPanelSize);
                        MapListViewInf.Children.Add(stackPanelInf);

                        MapListViewInf.Children.Add(copyButton);
                        MapListViewInf.Children.Add(chooseButton);

                    }
                }
            }
        }


        public void MapChoosed(MapInfo MapChoosedNow)
        {
            Assets.GetPoints(MapChoosedNow);

            // 取得初始点集 建立 初始点集 更新点集 暂存点 
            Master.NavigateTo(typeof(GeoGraph.Pages.MainPage.MapFrameLogic.MapFrame));
        }
    }
}
