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
            //初始化网络客户端
            // 后端未完成 _NetworkClient = new GeoGraph.Network.NetworkClient();
            App.MainFrame = this.MainFrame;
            _Assets = new GeoGraph.Network.Assets();
            MainWindow.NavigateTo(typeof(GeoGraph.Pages.Login.WaitingPage));
        }

        public static void NavigateTo(System.Type page)
        {
            // 记下页面
            App.MainFrame.Navigate(page);
        }


    }
}
