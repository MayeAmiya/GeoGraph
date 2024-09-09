using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GeoGraph.Pages.MainPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// 这里负责地图的选择 以及用户的登录
    /// 用户组也要放在一个单独的数据库中
    /// 地图组包含地图项以及元素项 元素项包含图形 元素 页面 以及枚举 三个项 图形指向页面 页面可以保存元素 页面 枚举
    /// 数据库采用json库 动态存储
    /// </summary>
    public sealed partial class SettingFrame : Page
    {
        public SettingFrame()
        {
            this.InitializeComponent();
        }
    }
}
