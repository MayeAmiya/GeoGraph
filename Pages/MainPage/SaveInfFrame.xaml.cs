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
using GeoGraph.Network;
using Microsoft.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GeoGraph.Pages.MainPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// 这个页面主要是最终确认保存信息 这里应当先展示删除内容 再展示添加内容 最后展示修改内容并同时显示对比
    /// 最后提供一个上传确认按钮 对应网络功能里的update
    /// </summary>
    public sealed partial class SaveInfFrame : Page
    {
        public SaveInfFrame()
        {
            this.InitializeComponent();
            _Basic_PointInf = Assets._Basic_PointInf;
            _Update_PointInf = Assets._Update_PointInf;
        }

        public static PointInf _Basic_PointInf;
        public static UpdatePoints _Update_PointInf;

        // 这里要得到什么 要得到updatepointinf 和 pointinf 然后遍历updatepointinf 对其中的point先遍历
        // 然后得到pointcode 再陈列
        public void diff_display()
        {
            foreach (BasePoint temp in UpdatePoints.Update_basePoints)
            {
                // 这里要显示删除的点
                // 每个点都创建一个块 塞入stackpanel
                StackPanel stackPanel = new StackPanel();
                stackPanel.Orientation = Orientation.Vertical;
                TextBlock textBlockPoint = new TextBlock();
                if (temp.deleted == true)
                {
                    textBlockPoint.Text = "Deleted Point Index: " + temp.pointInfCode;
                    stackPanel.Background = new SolidColorBrush(Microsoft.UI.Colors.Red);
                }
                else if (temp.updated == true)
                {
                    textBlockPoint.Text = "Updated Point Index: " + temp.pointInfCode;
                    stackPanel.Background = new SolidColorBrush(Microsoft.UI.Colors.Yellow);
                }
                else
                {
                    textBlockPoint.Text = "Created Point Index: " + temp.pointInfCode;
                    stackPanel.Background = new SolidColorBrush(Microsoft.UI.Colors.Green);
                }

                TextBlock textBlockInf = new TextBlock();
                textBlockInf.Text = "Loc.X: " + temp.location.X + " Loc.Y: " + temp.location.Y;

                stackPanel.Children.Add(textBlockPoint);
                stackPanel.Children.Add(textBlockInf);

                Grid grid = find_diff(temp.pointInfCode);

                if (grid != null)
                    stackPanel.Children.Add(grid);

                DiffContent.Children.Add(stackPanel);

                Border border = new Border();
                border.Height = 2; // 设置分隔线的高度
                border.Background = new SolidColorBrush(Colors.Gray); // 设置分隔线的颜色
                border.Margin = new Thickness(0, 10, 0, 10);

                DiffContent.Children.Add(border);
            }
        }

        public Grid find_diff(int Index)
        {
            if(UpdatePoints.Update_basicInfo.ContainsKey(Index))
            {
                // 这里要显示更新的点
                // 每个点都创建一个块 塞入stackpanel
                var temp = UpdatePoints.Update_basicInfo[Index];
                StackPanel _stackPanel = new StackPanel();
                TextBlock textBlockType = new TextBlock();
                switch (temp.Type){
                    case "String":
                        // 查找是否存在
                        textBlockType.Text = "Type -> String";
                        _stackPanel.Children.Add(textBlockType);

                        if (PointInf.basicInfo.ContainsKey(Index))
                        {
                            // 存在则是更改
                            var original = PointInf.basicInfo[Index];
                            _stackPanel.Background = new SolidColorBrush(Microsoft.UI.Colors.Yellow);
                            TextBlock textBlock = new TextBlock();
                            textBlock.Text = "Updated Item Name : " + temp.Name + " Value : " + temp.Object as string;
                            TextBlock textBlockOriginal = new TextBlock();
                            textBlockOriginal.Text = "Original Item Name : " + original.Name + " Value : " + original.Object as string;

                            _stackPanel.Children.Add(textBlock);
                            _stackPanel.Children.Add(textBlockOriginal);
                        }
                        else
                        {
                            // 不存在则是添加
                            _stackPanel.Background = new SolidColorBrush(Microsoft.UI.Colors.Green);
                            TextBlock textBlock = new TextBlock();
                            textBlock.Text = "Added Item Name : " + temp.Name + " Value : " + temp.Object as string;
                            _stackPanel.Children.Add(textBlock);
                        }
                        break;

                    case "Enum":
                        textBlockType.Text = "Type -> Enum";
                        _stackPanel.Children.Add(textBlockType);
                        List<int> tuple = temp.Object as List<int>;
                        if (PointInf.basicInfo.ContainsKey(Index))
                        {
                            // 存在则是更改
                            var original = PointInf.basicInfo[Index];

                            _stackPanel.Background = new SolidColorBrush(Microsoft.UI.Colors.Yellow);

                            TextBlock textBlock = new TextBlock();

                            List<int> oldtuple = PointInf.basicInfo[Index].Object as List<int>;

                            textBlock.Text = "Updated Enum Name : " + temp.Name + " " + UpdatePoints.Update_basicInfo[tuple[0]].Object as string;
                            TextBlock textBlockOriginal = new TextBlock();
                            textBlockOriginal.Text = "Original Enum Name : " + original.Name + " Value : " + PointInf.basicInfo[oldtuple[0]].Object as string;
                            _stackPanel.Children.Add(textBlock);
                            _stackPanel.Children.Add(textBlockOriginal);
                        }
                        else
                        {
                            // 不存在则是添加
                            _stackPanel.Background = new SolidColorBrush(Microsoft.UI.Colors.Green);
                            TextBlock textBlock = new TextBlock();
                            textBlock.Text = "Added Enum Name : " + temp.Name + " Value : " + UpdatePoints.Update_basicInfo[tuple[0]] as string;
                            _stackPanel.Children.Add(textBlock);
                        }
                        break;

                    case "Page":
                        // Page 要递归啦！
                        textBlockType.Text = "Type -> Page";
                        _stackPanel.Children.Add(textBlockType);
                        if (PointInf.basicInfo.ContainsKey(Index))
                        {
                            var original = PointInf.basicInfo[Index];

                            _stackPanel.Background = new SolidColorBrush(Microsoft.UI.Colors.Yellow);
                            TextBlock textBlock = new TextBlock();
                            textBlock.Text = temp.Name + " Last Changed date : " + original.date;
                            _stackPanel.Children.Add(textBlock);
                        }
                        else
                        {
                            _stackPanel.Background = new SolidColorBrush(Microsoft.UI.Colors.Green);
                            TextBlock textBlock = new TextBlock();
                            textBlock.Text = temp.Name + " Created date : " + temp.date;
                            _stackPanel.Children.Add(textBlock);
                        }

                        List<int> newpage = temp.Object as List<int>;
                        if(newpage == null)
                        {
                            return null;
                        }
                        foreach(int items in newpage)
                        {
                            Grid grids= find_diff(items);

                            if (grids !=null)
                                _stackPanel.Children.Add(grids);
                        }
                        break;
                }

                Grid grid = new Grid();

                // 定义列
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) }); // 左侧固定宽度
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // 右侧自适应宽度

                Grid.SetColumn(_stackPanel, 1);
                grid.Children.Add(_stackPanel);

                return grid;
            }
            return null;
        }

        public void clear_display()
        {
            DiffContent.Children.Clear();
        }
        public void Update_Click(object sender, RoutedEventArgs e)
        {
            //调用更新方法的Update
            UpdatePoints.UPDATE();
        }
    }


}
