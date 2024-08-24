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
        public static System.Type GlobalFrame;

        public MainWindow()
        {
            this.InitializeComponent();
            //初始化网络客户端
            _NetworkClient = new GeoGraph.Network.NetworkClient();
            App.MainFrame = MainFrame;
            GlobalFrame = typeof(MainWindow);
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // 导航到另一个页面
            NavigateTo(typeof(GeoGraph.Pages.Login.LoginPage));
        }

        public static void NavigateTo(System.Type page)
        {
            // 记下页面
            GlobalFrame = page;
            App.MainFrame.Navigate(GlobalFrame);
        }


    }
}
