using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

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
        public struct MapInfo
        {
            public string MapName;
            public int Width;
            public int Height;
            public string ImagePath;
            public string MapInf;
        }

        List<MapInfo> mapinfos;
        MapInfo MapChoosed;
        public MapChooseFrame()
        {
            this.InitializeComponent();

            this. GetMapInfo();
        }

        public void GetMapInfo()
        {
            mapinfos = GeoGraph.Network.Assets.MapList;
        }

        //布置巨大LISTVIEW 向MapListView中填充地图信息
        private void InitializeMapListView()
        {
            // 清空原有的地图项
            MapListView.Items.Clear();

            // 添加地图项
            foreach (var map in mapinfos)
            {
                // 创建地图项
                TextBlock textBlockName = new TextBlock
                {
                    Text = map.MapName,
                    FontSize = 16, // 设置字体大小
                    Margin = new Microsoft.UI.Xaml.Thickness(5)
                };
                TextBlock textBlockSize = new TextBlock
                {
                    Text = map.Width.ToString() +" "+map.Height.ToString(),
                    FontSize = 16, // 设置字体大小
                    Margin = new Microsoft.UI.Xaml.Thickness(5)
                };

                StackPanel stackPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical, // 垂直排列
                    Margin = new Microsoft.UI.Xaml.Thickness(5)
                };

                // 将 TextBlock 添加到 StackPanel 中
                stackPanel.Children.Add(textBlockName);
                stackPanel.Children.Add(textBlockSize);

                ListViewItem item = new ListViewItem()
                {
                    Content = stackPanel
                };
                // 添加到地图列表
                MapListView.Items.Add(item);
            }
            // 再添加一个添加事件
            Button addButton = new Button
            {
                Content = "Add",
                Margin = new Microsoft.UI.Xaml.Thickness(5)
            };

            // 为按钮的 Click 事件添加处理程序
            addButton.Click += AddButton_Click;

            // 将按钮添加到 ListView 的某个容器中，比如 StackPanel
            StackPanel buttonContainer = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Microsoft.UI.Xaml.Thickness(5)
            };
            buttonContainer.Children.Add(addButton);

            ListViewItem itemAdd = new ListViewItem
            {
                Content = buttonContainer
            };

            MapListView.Items.Add(itemAdd);
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // 创建文件选择器
            Windows.Storage.Pickers.FileOpenPicker openPicker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
            };
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            // 选择文件
            Windows.Storage.StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                // 获取地图信息
                string mapName = file.DisplayName;
                // 图片移动到Assets中 !!!
                string mapImage = file.Path;

                // 获取图片的宽度和高度
                var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
                var pixelWidth = decoder.PixelWidth;
                var pixelHeight = decoder.PixelHeight;

                // 添加地图信息到列表
                mapinfos.Add(new MapInfo
                {
                    MapName = mapName,
                    Width = (int)pixelWidth,
                    Height = (int)pixelHeight,
                    ImagePath = mapImage,
                    MapInf = ""
                });

                // 刷新地图列表显示
                InitializeMapListView();
            }
        }

        private void MapListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 获取选中的地图项
            if (MapListView.SelectedItem is ListViewItem selectedItem)
            {
                // 显示划出面板
                InfoPanelSplitView.IsPaneOpen = true;

                // 设置图片和地图信息（可以根据选择的地图进行动态加载）

            }
        }
    }
}
