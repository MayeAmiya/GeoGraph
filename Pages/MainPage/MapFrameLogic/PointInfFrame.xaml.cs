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
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GeoGraph.Pages.MainPage.MapFrameLogic
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PointInfFrame : Page
    {
        public PointInfFrame()
        {
            this.InitializeComponent();
        }

        // 首先要获取点信息类型列表 在AssetsGet中
        // 其次根据点信息列表初始化控件

        public void CreatePropertyInputs(List<(string Name, Type Type, object Value)> properties)
        {
            PropertyPanel.Children.Clear(); // 清空面板中的现有内容

            foreach (var prop in properties)
            {
                // 创建一个TextBlock作为标签
                var label = new TextBlock
                {
                    Text = prop.Name,
                    Margin = new Thickness(0, 0, 0, 5)
                };
                PropertyPanel.Children.Add(label);

                // 根据属性类型创建对应的输入控件
                if (prop.Type == typeof(string))
                {
                    var textBox = new TextBox
                    {
                        PlaceholderText = $"Enter {prop.Name}",
                        Margin = new Thickness(0, 0, 0, 10)
                    };
                    PropertyPanel.Children.Add(textBox);
                }
                else if (prop.Type == typeof(int))
                {
                    var numberBox = new TextBox
                    {
                        PlaceholderText = $"Enter {prop.Name} (integer)",
                        Margin = new Thickness(0, 0, 0, 10),
                        InputScope = new InputScope
                        {
                            Names = { new InputScopeName(InputScopeNameValue.Number) }
                        }
                    };
                    PropertyPanel.Children.Add(numberBox);
                }
                else if (prop.Type == typeof(double))
                {
                    var numberBox = new TextBox
                    {
                        PlaceholderText = $"Enter {prop.Name} (double)",
                        Margin = new Thickness(0, 0, 0, 10),
                        InputScope = new InputScope
                        {
                            Names = { new InputScopeName(InputScopeNameValue.Number) }
                        }
                    };
                    PropertyPanel.Children.Add(numberBox);
                }
                // 可以为其他类型添加更多的条件
            }
        }
    }
}
