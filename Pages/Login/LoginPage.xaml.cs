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
using Windows.System;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GeoGraph.Pages.Login
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
            // 后端未完成 MainWindow._Connect = new GeoGraph.Network.Connect();
        }

        // 这里加入图形验证码窗口
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            // 网络未完成 string ret = await MainWindow._Connect.AttemptLoginAsync(username, password);
            // 在这里添加登录逻辑，例如验证用户名和密码
            string ret = "success";

            ContentDialog loginDialog;
            if (ret!=null)
            {
                switch (ret)
                {
                    case "success":
                        // 跳转页面
                        System.Diagnostics.Debug.WriteLine("Login Success");
                        MainWindow.NavigateTo(typeof(GeoGraph.Pages.MainPage.Master));
                        // MainWindow.NavigateTo(typeof(GeoGraph.Pages.Login.WelcomePage));
                        return;
                    case "usernotexist":
                        loginDialog = new ContentDialog
                        {
                            Title = "Login Failed",
                            Content = "User Not Exist"
                        };
                        break;
                    case "passworderror":
                        loginDialog = new ContentDialog
                        {
                            Title = "Login Failed",
                            Content = "Password Error"
                        };
                        break;
                    case "alreadylogin":
                        loginDialog = new ContentDialog
                        {
                            Title = "Login Failed",
                            Content = "Already Login"
                        };
                        break;
                    default:
                        loginDialog = new ContentDialog
                        {
                            Title = "Login Failed",
                            Content = "Unknown Error"
                        };
                        break;
                }
            }
            else
            {
                // 登录失败，显示错误消息
                loginDialog = new ContentDialog
                {
                    Title = "Login Failed",
                    Content = "Invalid username or password.\nOr check your Network",
                };
            }

            loginDialog.CloseButtonText = "OK";
            loginDialog.XamlRoot = this.XamlRoot;
            await loginDialog.ShowAsync();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            string ret = await MainWindow._Connect.AttemptRegisterAsync(username, password);
            // 在这里添加登录逻辑，例如验证用户名和密码
            ContentDialog registerDialog;
            if (ret != null)
            {
                switch (ret)
                {
                    case "success":
                        registerDialog = new ContentDialog
                        {
                            Title = "Register Success",
                            Content = "Register Success"
                        };
                        break;
                    case "userexist":
                        registerDialog = new ContentDialog
                        {
                            Title = "Register Failed",
                            Content = "User Exist"
                        };
                        break;
                    default:
                        registerDialog = new ContentDialog
                        {
                            Title = "Register Failed",
                            Content = "Unknown Error"
                        };
                        break;
                }
            }
            else
            {
                // 登录失败，显示错误消息
                registerDialog = new ContentDialog
                {
                    Title = "Login Failed",
                    Content = "Invalid username or password.\nOr check your Network",
                };
            }
            registerDialog.CloseButtonText = "OK";
            registerDialog.XamlRoot = this.XamlRoot;
            await registerDialog.ShowAsync();
        }

        private void UsernameTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                // 将焦点移动到 PasswordBox，方便用户直接输入密码
                PasswordBox.Focus(FocusState.Programmatic);
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                // 处理登录操作
                LoginButton_Click(sender, e);
            }
        }
    }
}
