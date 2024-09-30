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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Text;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GeoGraph.Pages.MainPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchFrame : Page
    {
        public SearchFrame()
        {
            this.InitializeComponent();
        }

        public List<int> found;
        public HashSet<int> Selectedtion;

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                // 处理回车事件
                search(searchbar.Text);
                List<int> found = new List<int>();
            }
        }

        private void searchView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 处理选择事件
            // 选择了一个结果
            // 我们要把这个结果显示在地图上
            // 选择的结果是一个properties
            // 我们要把这个properties显示在地图上
            // 我们要把这个properties显示在右侧面板上
        }

        private Property find(int Index)
        {
            Property Now;

            if (UpdatePoints.Update_basicInfo.ContainsKey(Index))
            {
                Now = UpdatePoints.Update_basicInfo[Index];
                return Now;
            }
            else if (PointInf.basicInfo.ContainsKey(Index))
            {
                Now = PointInf.basicInfo[Index];
                return Now;
            }
            else
            {
                Now = null;
                return Now;
            }
        }
        private static string findmaster(int index)
        {
            foreach(KeyValuePair<int,Property> temp in UpdatePoints.Update_basicInfo)
            {
                Property get = temp.Value;

                if(get.Object is List<int> getting)
                {
                    if (getting.Contains(index))
                    {
                        if (get.Name != null&&get.Name!="null")
                        {
                            return findmaster(get.Index) + get.Name + "=>";
                        }
                        else
                        {
                            return findmaster(get.Index) + "NoName" + "=>";
                        }
                        
                    }
                }
            }

            foreach (KeyValuePair<int, Property> temp in PointInf.basicInfo)
            {
                Property get = temp.Value;

                if (get.Object is List<int> getting)
                {
                    if (getting.Contains(index))
                    {
                        if (get.Name != null && get.Name != "null")
                        {
                            return findmaster(get.Index) + get.Name + "=>";
                        }
                        else
                        {
                            return findmaster(get.Index) + "NoName" + "=>";
                        }

                    }
                }
            }
            return "Start=>";
            
        }
        public async void search(string input)
        {
            // 输入字符串 查找下一个
            // 一定是找properties 所以我们把input输入到sql中 然后返回index 直接定位到指定index就行
            saveButton.IsEnabled = false;
            searchView.Items.Clear();
            found = new List<int>();
            Selectedtion = new HashSet<int>();
            found = await NetworkClient.Search(input);
            // 找到了就显示
            if (found.Count > 0)
            {
                saveButton.IsEnabled = true;
            }
            foreach(int index in found)
            {
                Property temp = find(index);
                if (temp != null)
                {
                    ListViewItem item = new ListViewItem();

                    TextBlock textBlock = new TextBlock();
                    if (temp.Type == "String" || temp.Type == "EnumItem")
                    {
                        textBlock.Text = "Item Name : " + temp.Name + " Value : " + temp.Object as string;
                    }
                    else
                    {
                        string tempstring = "";
                        if(temp.Object is List<int> tL)
                        {
                            foreach (int i in tL)
                                tempstring += " " + find(i).Name;
                        }
                        textBlock.Text = "Item Name : " + temp.Name + " Value : " + tempstring;
                    }


                    TextBlock source = new TextBlock();
                    source.Text = findmaster(temp.Index)+temp.Name;
                    ToggleButton newtoggle = new ToggleButton();
                    newtoggle.Content = "Select";
                    newtoggle.Click += (sender, e) =>
                    {
                        if(newtoggle.IsChecked == true)
                        {
                            newtoggle.Content = "Selected";
                            Selectedtion.Add(index);
                        }
                        else
                        {
                            newtoggle.Content = "Select";
                            Selectedtion.Remove(index);
                        }

                        if(Selectedtion.Count > 0)
                        {
                            saveButton.IsEnabled = true;
                        }
                        else
                        {
                            saveButton.IsEnabled = false;
                        }
                    };
                    StackPanel tempS = new StackPanel();
                    tempS.Children.Add(newtoggle);
                    tempS.Children.Add(textBlock);
                    tempS.Children.Add(source);

                    item.Content = tempS;
                    searchView.Items.Add(item);
                }
                // 显示这个点
                // 显示这个点的信息
                // 显示这个点的属性
            }
        }
        public async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach(int index in Selectedtion)
            {
                Property temp = find(index);
                if (temp != null)
                {
                    if (temp.Type == "String" || temp.Type == "EnumItem")
                    {
                        stringBuilder.Append("Item Name : " + temp.Name + " Value : " + temp.Object as string + "\n");
                    }
                    else
                    {
                        string tempstring = "";
                        if (temp.Object is List<int> tL)
                        {
                            foreach (int i in tL)
                                tempstring += " " + find(i).Name;
                        }
                        stringBuilder.Append("Item Name : " + temp.Name + " Value : " + tempstring + "\n");
                    }
                }
            }

            string fold = await PickFolderButton_Click();
            if (fold != "Operation cancelled.")
                File.WriteAllText(fold + "\\Search.json", stringBuilder.ToString());

        }

        private async Task<string> PickFolderButton_Click()
        {


            // Create a folder picker
            FolderPicker openPicker = new Windows.Storage.Pickers.FolderPicker();

            // See the sample code below for how to make the window accessible from the App class.
            var window = App.m_window;

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // Initialize the folder picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // Set options for your folder picker
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add("*");

            // Open the picker for the user to pick a folder
            StorageFolder folder = await openPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                return folder.Path;
            }
            else
            {
                return "Operation cancelled.";
            }

        }
    }
}
