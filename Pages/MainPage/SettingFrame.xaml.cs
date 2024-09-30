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
using Microsoft.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using WinRT.Interop;
using Microsoft.UI.Xaml.Shapes;
using Newtonsoft.Json.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GeoGraph.Pages.MainPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// 这里调整一些设置以及用户权限组信息和与相应设置
    /// </summary>
    public sealed partial class SettingFrame : Page
    {
        public SettingFrame()
        {
            this.InitializeComponent();

            if (!string.IsNullOrEmpty(Assets.userImage) && File.Exists(Assets.userImage))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.UriSource = new Uri(Assets.userImage); // 设置图像路径

                userImage.Source = bitmapImage; // 将图像源设置为BitmapImage
            }
            else
            {
                // 文件不存在，可以设置一个默认图像或处理错误
                userImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/StoreLogo.png")); // 示例默认图像
            }
        }

        private async void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                var picker = new Windows.Storage.Pickers.FileOpenPicker();

                IntPtr hwnd = WindowNative.GetWindowHandle(App.GetWindow());
                InitializeWithWindow.Initialize(picker, hwnd);

                picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;

                picker.FileTypeFilter.Add(".png");

                // 显示文件选择器
                Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();

                if (file != null)
                {
                    // 用户选择了图片
                    string path = System.IO.Path.Combine(Assets.absolutePath, "user");
                    string fullpath = System.IO.Path.Combine(path, Network.Connect._username+".png");

                    path = System.IO.Path.Combine(Assets.absolutePath, "user");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    File.Copy(file.Path, fullpath, true);
                    await NetworkClient.Upload("user", Network.Connect._username, true, file.Path);

                    Assets.userImage = file.Path;
                    userImage.Source = new BitmapImage(new Uri(Assets.userImage));
                }
                else
                {
                    // 用户取消了文件选择
                    Debug.WriteLine("No image selected.");
                }
            }
            catch (Exception ex)
            {
                // 输出异常信息
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void ReLoginButton(object sender, RoutedEventArgs e)
        {
            // 退出登录
            // 清除信息 退出到LoginPage
            MainWindow.NavigateTo(typeof(GeoGraph.Pages.Login.LoginPage));
            Master.reset();
            Assets.reset();
        }

        private void ReSetButton(object sender, RoutedEventArgs e)
        {
            // 原地生成一个TextBox 输入后确认启用并重新登录
            
            TextBox passwordBox1 = new TextBox();
            passwordBox1.PlaceholderText = "Please input new password";
            TextBox passwordBox2 = new TextBox();
            passwordBox2.PlaceholderText = "Please input new password again";
            Button passwordbutton = new Button();
            passwordbutton.Content = "Confirm";
            Button passwordresetcancel = new Button();
            passwordresetcancel.Content = "Cancel";

            StackPanel reset = new StackPanel();
            reset.Orientation = Orientation.Vertical;
            reset.Children.Add(passwordBox1);
            reset.Children.Add(passwordBox2);

            StackPanel resetbuttons = new StackPanel();
            resetbuttons.Orientation = Orientation.Horizontal;
            resetbuttons.Children.Add(passwordbutton);
            resetbuttons.Children.Add(passwordresetcancel);
            reset.Children.Add(resetbuttons);

            passwordbutton.Click += async (sender,e) => 
            {
                if (passwordBox1.Text == passwordBox2.Text)
                {
                    // 重置密码
                    // 退出登录
                    string _ret = await Network.Connect.AttemptReSetAsync(passwordBox1.Text);
                    if(_ret == "success")
                    {
                        MainWindow.NavigateTo(typeof(GeoGraph.Pages.Login.LoginPage));
                        Master.reset();
                        Assets.reset();
                    }

                }
                else
                {
                    passwordBox1.Text = "Not Same inputs";
                    passwordBox2.Text = "please try again";
                }
            };

            var parent = (sender as Button).Parent as Panel;

            // 获取按钮的索引
            int index = parent.Children.IndexOf(sender as Button);

            // 在按钮位置插入 TextBox
            parent.Children.Insert(index, reset);
            UserPane.Children.Remove(sender as Button);

            passwordresetcancel.Click += (sender, e) =>
            {
                UserPane.Children.Remove(reset);
                Button newButton = new Button
                {
                    Content = "ReSet Password",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(10, 10, 10, 10)
                };

                // 绑定 Click 事件
                newButton.Click += ReSetButton;

                // 插入到父容器的指定位置
                parent.Children.Insert(index, newButton);
            };
        }

        private void ExitButton(object sender, RoutedEventArgs e)
        {
            // 退出登录
            App.Current.Exit();
        }

        public async void CommandInterFace()
        {
            await User.RequestUserListAsync();
            if (Network.Connect._userRank >8)
            {
                // 初始化管理界面
                // 读取用户列表->用户数据库 ->展示列表 选择列表填表
                // StackPanel中显示选项 调整UserRank UserPermission
                // 更高等级的用户不可管理
                // 现在Connect中有userlist了

                // "UsersList"
                UsersList.Items.Clear();
                foreach (User tempuser in Network.User.userlist)
                {
                    ListViewItem userItem = new ListViewItem();
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = "Name : "+ tempuser.username+  " Rank = " + tempuser.userRank;
                    userItem.Content = textBlock;
                    userItem.Tapped += (sender, e) =>
                    {
                        UserControl.Children.Clear();
                        // 展示列表
                        RadioButtons radioButtons = new RadioButtons();
                        radioButtons.Items.Add(new RadioButton { Content = "Read" });
                        radioButtons.Items.Add(new RadioButton { Content = "Write" });
                        radioButtons.Items.Add(new RadioButton { Content = "Delete" });

                        switch (tempuser.permission)
                        {
                            case "Read":
                                radioButtons.SelectedIndex = 0;
                                break;
                            case "Write":
                                radioButtons.SelectedIndex = 1;
                                break;
                            case "Delete":
                                radioButtons.SelectedIndex = 2;
                                break;
                        }

                        radioButtons.SelectionChanged += (sender, e) =>
                        {
                            var selectedRadioButton = (RadioButton)radioButtons.SelectedItem;
                            if (selectedRadioButton != null)
                            {
                                tempuser.permission = selectedRadioButton.Content.ToString();
                            }
                        };

                        TextBlock textBlock1 = new TextBlock()
                        {
                            Text = "Rank : "
                        };
                        TextBox textBox = new TextBox()
                        {
                            Text = tempuser.userRank.ToString()
                        };
                        textBox.TextChanged += (sender, e) =>
                        {
                            tempuser.userRank = Convert.ToInt32(textBox.Text);
                        };
                       
                        StackPanel newstack = new StackPanel();
                        
                        newstack.Children.Add(textBlock1);
                        newstack.Children.Add(textBox);

                        UserControl.Children.Add(newstack);

                        if (Network.Connect._userRank <= tempuser.userRank)
                        {
                            radioButtons.IsEnabled = false;
                            textBox.IsEnabled = false;
                        }
                        UserControl.Children.Add(radioButtons);

                        Button button = new Button()
                        {
                            Content = "Update Selected User"
                        };
                        button.Click += async (sender, e) =>
                        {
                            UpdateUser.AddUserList(tempuser);
                            await UpdateUser.Update();
                            await User.RequestUserListAsync();
                            CommandInterFace();
                        };
                        UserControl.Children.Add(button);

                    };

                    UsersList.Items.Add(userItem);
                }
            }
        }
    }
}
