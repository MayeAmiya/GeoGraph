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
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.AccessControl;
using GeoGraph.Network;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GeoGraph.Pages.MainPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Master : Page
    {

        public Master()
        {
            this.InitializeComponent();
            System.Diagnostics.Debug.WriteLine("MasterHere");
            App.m_mainFrame = MasterPageFrame;
            MasterPageFrame.Navigate(typeof(EmptyFrame));
        }

        private void nvSample_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItemContainer != null)
            {
                var itemTag = args.InvokedItemContainer.Tag.ToString();
                NavigateToPage(itemTag);
            }
        }

        private static Dictionary<string, Page> _pageInstances = new Dictionary<string, Page>();

        public static void reset()
        {
            _pageInstances.Clear();
        }

        private static void NavigateToPage(string itemTag)
        {
            if (!_pageInstances.TryGetValue(itemTag, out Page pageInstance))
            {
                switch (itemTag)
                {
                    case "MapPage":
                        if (Map._MapInfo?.MapName != null)
                            pageInstance = new GeoGraph.Pages.MainPage.MapFrameLogic.MapFrame();
                        break;
                    case "SavePage":
                        if (Map._MapInfo?.MapName != null)
                            pageInstance = new SaveInfFrame();
                        break;
                    case "Settings":
                        pageInstance = new SettingFrame();
                        break;
                    case "MapChoosePage":
                        pageInstance = new MapChooseFrame();
                        break;
                    case "DownloadPage":
                        if (Map._MapInfo?.MapName != null)
                            pageInstance = new DownloadFrame();
                        break;
                    case "SearchPage":
                        if (Map._MapInfo?.MapName != null)
                            pageInstance = new SearchFrame();
                        break;
                }

                if (pageInstance != null)
                    _pageInstances[itemTag] = pageInstance;
            }

            if (pageInstance != null && App.m_mainFrame.Content != pageInstance)
            {
                App.m_mainFrame.Content = pageInstance;  // 直接设置页面实例
                if(itemTag == "SavePage" && Map._MapInfo?.MapName != null)
                {
                    var Save = pageInstance as SaveInfFrame;
                    Save.clear_display();
                    Save.diff_display();
                }

                if (itemTag == "DownloadPage" && Map._MapInfo?.MapName != null)
                {
                    var Download = pageInstance as DownloadFrame;
                    Download.clear_display();
                    Download.diff_display();
                }

                if (itemTag == "Settings")
                {
                    var Setting = pageInstance as SettingFrame;
                    Setting.CommandInterFace();
                }

                if (itemTag == "MapPage" && Map._MapInfo?.MapName != null)
                {
                    var MapPage = pageInstance as GeoGraph.Pages.MainPage.MapFrameLogic.MapFrame;
                }
            }
        }


        private void Refresh_Click()
        {
            // refresh the map
            var MapPage = _pageInstances["MapPage"] as GeoGraph.Pages.MainPage.MapFrameLogic.MapFrame;
        }

        public static void NavigateTo(System.Type page)
        {
            App.m_mainFrame.Navigate(page);
        }
    }
}
