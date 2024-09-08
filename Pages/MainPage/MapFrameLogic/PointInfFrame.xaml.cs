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
            PointInfSingle Single_PointInf = new PointInfSingle(_mapFrame.PointInf);
            PointInfTemp Temp_PointInf = new PointInfTemp(Single_PointInf);
            refreshPage();
            
        }

        public class PageFolder
        {
            public string Name { get; set; }
        }

        public class PageFolderViewModel
        {
            public ObservableCollection<PageFolder> NavigationFolders { get; set; }
            public PageFolderViewModel()
            {
                NavigationFolders = new ObservableCollection<PageFolder>
                {
                    new  PageFolder { Name = "BasePoint" }
                };
            }

            // 添加新导航项
            public void AddNavigationItem(string folderName)
            {
                NavigationFolders.Add(new PageFolder { Name = folderName });
            }

            // 点击某个导航项时返回到该项并移除后续项
            public void NavigateToFolder(PageFolder selectedFolder)
            {
                int index = NavigationFolders.IndexOf(selectedFolder);
                if (index < NavigationFolders.Count - 1)
                {
                    // 移除后续的所有项
                    while (NavigationFolders.Count > index + 1)
                    {
                        NavigationFolders.RemoveAt(NavigationFolders.Count - 1);
                    }
                }
                // 执行对应的页面导航逻辑
                // 刷新本页面即可
            }
        }

        // 本组点信息
        PointInfSingle Single_PointInf;
        PointInfTemp Temp_PointInf;
        // 最重要的部分
        private void refreshPage()
        {
            //根据布局创建页面 
            if (_mapFrame.pointInfSelect() != -1)
            {
                //开始查找点信息和索引
                Single_PointInf.GetValue(_mapFrame.pointInfSelect());
                //一次即可得到点所有的信息 现在开始展示

            }
            else 
            {
                //初始化点
                Single_PointInf.clear();

            }
        }


        // 如果指定了初始化方式
        // 首先要获取点信息类型列表 在AssetsGet中
        // 其次根据点信息列表初始化控件
        // 如果没有指定 那么就是空白页面和增加表项按钮

        private void SavePoint_Click(object sender, RoutedEventArgs e)
        {
            Temp_PointInf.merge();
            // 保存数据到服务器
            // 保存数据到本地
        }
        private void DeletePoint_Click(object sender, RoutedEventArgs e)
        {
            Temp_PointInf.clear();
            _mapFrame.PointInf.RemovePoint(_mapFrame.pointInfSelect());
            // 从数据库中删除
            // 从服务器中删除
            // 从本地中删除
        }
        private void AbortPoint_Click(object sender, RoutedEventArgs e)
        {
            Temp_PointInf.clear();
            // 退出不保存
        }
    }
}
