using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Diagnostics;
using GeoGraph.Pages.Login;
using Microsoft.UI.Xaml.Media.Imaging;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GeoGraph
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>

    public sealed partial class MainWindow : Window
    {
        public static GeoGraph.Network.NetworkClient _NetworkClient;
        public static GeoGraph.Network.Connect _Connect;
        public static GeoGraph.Network.Assets _Assets;

        public MainWindow()
        {
            this.InitializeComponent();
            // 设置主页面指向
            App.MainFrame = this.MainFrame;

            _NetworkClient = new GeoGraph.Network.NetworkClient();
            // 初始化资源
            _Assets = new GeoGraph.Network.Assets();
            // 导航到初始化页面

            string absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
            Network.Assets.absolutePath = absolutePath;
            System.Diagnostics.Debug.WriteLine($"当前资源目录: {absolutePath}");

            // 检查目录是否存在，如果不存在则创建它
            if (!Directory.Exists(absolutePath))
            {
                Directory.CreateDirectory(absolutePath);
                System.Diagnostics.Debug.WriteLine($"资源目录: {absolutePath} 已创建");
            }
            MainWindow.NavigateTo(typeof(GeoGraph.Pages.Login.LoginPage));
        }

        public static void NavigateTo(System.Type page)
        {
            // 记下页面
            App.MainFrame.Navigate(page);
        }


    }
}
