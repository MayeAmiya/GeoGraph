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

namespace GeoGraph.Pages.Login
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WaitingPage : Page
    {
        public WaitingPage()
        {
            this.InitializeComponent();
            System.Diagnostics.Debug.WriteLine("WaitingPage");
        }

        private void Grid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // 导航到另一个页面
            System.Diagnostics.Debug.WriteLine("PointerPressed");
            MainWindow.NavigateTo(typeof(GeoGraph.Pages.Login.LoginPage));
        }
    }
}
