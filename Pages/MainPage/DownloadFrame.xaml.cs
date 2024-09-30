using GeoGraph.Network;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GeoGraph.Pages.MainPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DownloadFrame : Page
    {
        public DownloadFrame()
        {
            this.InitializeComponent();
        }

        public void diff_display()
        {
            clear_display();
            foreach (BasePoint temp in PointInf.basePoints)
            {
                if (temp.deleted)
                {
                    continue;
                }
                if(temp.updated)
                {
                    continue;
                }
                show(temp);
            }
            foreach(BasePoint temp in UpdatePoints.Update_basePoints)
            {
                if (temp.deleted)
                {
                    continue;
                }
                show(temp);
            }
        }

        public void show(BasePoint temp)
        {
            // ����Ҫ��ʾɾ���ĵ�
            // ÿ���㶼����һ���� ����stackpanel

            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Vertical;
            TextBlock textBlockPoint = new TextBlock();

            textBlockPoint.Text = "Point Index: " + temp.pointInfCode;


            TextBlock textBlockInf = new TextBlock();
            textBlockInf.Text = "Loc.X: " + temp.location.X + " Loc.Y: " + temp.location.Y;

            stackPanel.Children.Add(textBlockPoint);
            stackPanel.Children.Add(textBlockInf);

            Grid grid = find_diff(temp.pointInfCode);

            if (grid != null)
                stackPanel.Children.Add(grid);

            AllContent.Children.Add(stackPanel);

            Border border = new Border();
            border.Height = 2; // ���÷ָ��ߵĸ߶�
            border.Background = new SolidColorBrush(Colors.Gray); // ���÷ָ��ߵ���ɫ
            border.Margin = new Thickness(0, 10, 0, 10);

            AllContent.Children.Add(border);
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

        public Grid find_diff(int Index)
        {
            
            {
                // ����Ҫ��ʾ���µĵ�
                // ÿ���㶼����һ���� ����stackpanel
                }
                var temp = find(Index);
                StackPanel _stackPanel = new StackPanel();
                TextBlock textBlockType = new TextBlock();
                if (temp == null)
                {
                    return null;
                }
                switch (temp.Type)
                {
                    case "String":
                        // �����Ƿ����
                        textBlockType.Text = "Type -> String";
                        _stackPanel.Children.Add(textBlockType);

                         {
                             TextBlock textBlock = new TextBlock();
                             textBlock.Text = "Item Name : " + temp.Name + " Value : " + temp.Object as string;
                             _stackPanel.Children.Add(textBlock);
                         }
                        
                        break;

                    case "Enum":
                        textBlockType.Text = "Type -> Enum";
                        _stackPanel.Children.Add(textBlockType);
                        List<int> tuple = temp.Object as List<int>;
                        {
                            TextBlock textBlock = new TextBlock();
                            textBlock.Text = "Added Enum Name : " + temp.Name + " Value : " + UpdatePoints.Update_basicInfo[tuple[0]] as string;
                            _stackPanel.Children.Add(textBlock);
                        }
                        
                        break;

                    case "Page":
                        // Page Ҫ�ݹ�����
                        textBlockType.Text = "Type -> Page";
                        _stackPanel.Children.Add(textBlockType);

                        {   
                            TextBlock textBlock = new TextBlock();
                             textBlock.Text = "Created Page : " + temp.Name + " Created date : " + temp.date;
                            _stackPanel.Children.Add(textBlock);
                        }
                        

                        List<int> newpage = temp.Object as List<int>;
                        if (newpage == null)
                        {
                            return null;
                        }
                        foreach (int items in newpage)
                        {
                            Grid grids = find_diff(items);

                            if (grids != null)
                                _stackPanel.Children.Add(grids);
                        }
                        break;
                }

                Grid grid = new Grid();

                // ������
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) }); // ���̶����
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // �Ҳ�����Ӧ���

                Grid.SetColumn(_stackPanel, 1);
                grid.Children.Add(_stackPanel);

                return grid;
            }


        public void clear_display()
        {
            AllContent.Children.Clear();
        }

        public async void Save_Click(Object sender, RoutedEventArgs e)
        {

            //����
            string basePointsJson = JsonConvert.SerializeObject(PointInf.basePoints);
            string BpropertiesJson = JsonConvert.SerializeObject(PointInf.basicInfo);
            string UpropertiesJson = JsonConvert.SerializeObject(UpdatePoints.Update_basicInfo);
            //string����Ϊjson
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(basePointsJson);
            stringBuilder.Append(BpropertiesJson);
            stringBuilder.Append(UpropertiesJson);
            string json = stringBuilder.ToString();
            //���浽����
            // ѡ�񱣴�λ��
            string fold = await PickFolderButton_Click();
            if(fold != "Operation cancelled.")
                File.WriteAllText(fold+"\\Save.json", json);
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
