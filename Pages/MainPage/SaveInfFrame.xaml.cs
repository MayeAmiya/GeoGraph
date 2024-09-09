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
    /// 这个页面主要是最终确认保存信息 这里应当先展示删除内容 再展示添加内容 最后展示修改内容并同时显示对比
    /// 最后提供一个上传确认按钮 对应网络功能里的update
    /// </summary>
    public sealed partial class SaveInfFrame : Page
    {
        public SaveInfFrame()
        {
            this.InitializeComponent();
        }
    }
}
