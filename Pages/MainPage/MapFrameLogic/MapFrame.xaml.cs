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
using Microsoft.UI.Xaml.Shapes;
using GeoGraph.Network;
using Microsoft.UI.Xaml.Media.Imaging;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GeoGraph.Pages.MainPage.MapFrameLogic
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 这里实现一个地图 要求能够加载SVG/PNG等格式的图片 等比放大 并且根据比例确定点的位置
    public sealed partial class MapFrame : Page
    {
        public MapFrame()
        {
            this.InitializeComponent();

            this.InitializeMap();

            this.RightPaneFrame.Navigate(typeof(GeoGraph.Pages.MainPage.MapFrameLogic.PointInfFrame));

            pointInfFramePage = this.RightPaneFrame.Content as GeoGraph.Pages.MainPage.MapFrameLogic.PointInfFrame;
            pointInfFramePage.GetMapFrame(this);
        }
        // 基本点信息

        // 该考虑点预载信息了 这部分在初始化时自动完成
        GeoGraph.Pages.MainPage.MapFrameLogic.PointInfFrame pointInfFramePage;
        private List<BasePoint> originalPositions;
        // 需要一个字典将pointInf与点的信息对应起来 

        // 上一个拖动的点和当前选择的点
        private static Ellipse _selectedPoint;
        private Point _lastPointerPosition;

        private static bool _isDragging;
        private static double _currentScale = 1.0;

        private const double ScaleStep = 0.1;
        private const double MaxScale = 5.00;
        private const double MinScale = 0.05;


        private void InitializeMap()
        {
            // 初始化Canvas大小
            MainCanvas.Width = Assets._MapInfo.Width;
            MainCanvas.Height = Assets._MapInfo.Height;
            // 初始化Map图像
            BitmapImage bitmapImage = new BitmapImage(new Uri(Assets._MapInfo.ImagePath));
            Image.Source = bitmapImage;
            // 同时在Assets初始化PointInf和Update的数据 根据地图类型直接引用
            originalPositions = Assets._Basic_PointInf.basePoints;
        }


        private Ellipse CreateEllipse(BasePoint position)
        {
            // 建立新点
            Ellipse ellipse = new Ellipse
            {
                Fill = new SolidColorBrush(Microsoft.UI.Colors.Red),
                Width = 10,
                Height = 10,
                Tag = position
            };
            // 设置位置
            Canvas.SetLeft(ellipse, position.location.X* _currentScale);
            Canvas.SetTop(ellipse, position.location.Y* _currentScale);
            // 添加点击事件
            ellipse.Tapped += OnEllipseTapped;
            // 添加到画布 
            MainCanvas.Children.Add(ellipse);
            return ellipse;
        }

        // 移动画面
        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _isDragging = false;
            MainCanvas.ReleasePointerCapture(e.Pointer);
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isDragging)
            {
                var currentPosition = e.GetCurrentPoint(MainCanvas).Position;
                var deltaX = currentPosition.X - _lastPointerPosition.X;
                var deltaY = currentPosition.Y - _lastPointerPosition.Y;

                // 更新 Canvas 的位置
                CanvasTranslateTransform.X += deltaX;
                CanvasTranslateTransform.Y += deltaY;

                _lastPointerPosition = currentPosition;
            }
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // 获取鼠标点击的指针属性
            var pointerProperties = e.GetCurrentPoint(null).Properties;
            // 右键拖动地图 左键选择，创建，取消

            //如果是左键 那么取消选择点
            if (!pointerProperties.IsLeftButtonPressed)
            {
                //如果是右键那就是拖动逻辑
                MainSplitView.IsPaneOpen = false;
                _lastPointerPosition = e.GetCurrentPoint(MainCanvas).Position;
                _isDragging = true;
                MainCanvas.CapturePointer(e.Pointer);
                
            }
            
            else
            {
                if(sender is Ellipse)
                {
                    _selectedPoint = sender as Ellipse;
                    OnEllipseTapped(_selectedPoint, null);
                    pointInfFramePage.refreshPage(pointInfSelect().pointInfCode);
                }
                else
                {
                    // 如果此时没有选中点 则创建点
                    // 释放原有选择点
                    releasePoint();

                    //读取此时鼠标点击的位置 注意这个位置相对画布实际的位置
                    BasePoint temp = new BasePoint(e.GetCurrentPoint(MainCanvas).Position, -1)
                    {
                        //这个Position要作变换
                        isTemp = true
                    };

                    _selectedPoint = CreateEllipse(temp);
                    OnEllipseTapped(_selectedPoint, null);
                    PointInfFrame.Temp_PointInf.newPoint(temp);
                    pointInfFramePage.refreshPage(temp.pointInfCode);
                }
            }

        }

        private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var delta = e.GetCurrentPoint(MainCanvas).Properties.MouseWheelDelta;
            if (delta > 0)
            {
                _currentScale += ScaleStep;
            }
            else if (delta < 0)
            {
                _currentScale -= ScaleStep;
            }

            // Ensure scale is within bounds
            _currentScale = Math.Max(MinScale, Math.Min(MaxScale, _currentScale));

            // Apply scale transform
            CanvasScaleTransform.ScaleX = _currentScale;
            CanvasScaleTransform.ScaleY = _currentScale;

        }

        // 选中点 这里不好 创建点应用点创建工具 而不是鼠标右键点击
        private void OnEllipseTapped(object sender, TappedRoutedEventArgs e)
        {
            // 本来设了创建工具选择还没上 这里默认都是创建点了
            Ellipse tappedEllipse = sender as Ellipse;

            // 如果已经有选中的点，将其恢复为未选中状态
            if (_selectedPoint != null)
            {
                if(_selectedPoint != tappedEllipse)
                {
                    // 清空暂时点数据
                    pointInfFramePage.PaneClear();
                    PointInfFrame.Temp_PointInf.clear();
                    // 如果是暂时点 那么从画布中删除
                    if (((BasePoint)_selectedPoint.Tag).isTemp)
                    {
                        MainCanvas.Children.Remove(_selectedPoint);
                        _selectedPoint = null;
                    }
                    //如果是永久点 那么取消选择
                    else
                    {
                        PointInfFrame.Temp_PointInf.clear();
                        _selectedPoint.Fill = new SolidColorBrush(Microsoft.UI.Colors.Red);
                        _selectedPoint = null;
                    }
                }
            }

            _selectedPoint = tappedEllipse;
            _selectedPoint.Fill = new SolidColorBrush(Microsoft.UI.Colors.Blue);
            // C#的class 一般都是引用 所以这里直接强转类型然后改就行

            MainSplitView.IsPaneOpen = true;
        }

        public static BasePoint pointInfSelect()
        {
            if(_selectedPoint != null)
                return ((BasePoint)_selectedPoint.Tag);
            else
                return null;
        }

        public void paneExp()
        {
            if(MainSplitView.OpenPaneLength == 400)
            {
                MainSplitView.OpenPaneLength = 800;
            }
            else
            {
                MainSplitView.OpenPaneLength = 400;
            }

        }

        public void releasePoint()
        {
            if (_selectedPoint != null)
            {
                //如果是暂时点 那么从画布中删除
                if (((BasePoint)_selectedPoint.Tag).isTemp)
                {
                    MainCanvas.Children.Remove(_selectedPoint);
                }
                //如果是永久点 那么取消选择
                else
                {
                    _selectedPoint.Fill = new SolidColorBrush(Microsoft.UI.Colors.Red);
                }
                _selectedPoint = null;
            }
        }

        public void rightPaneControl()
        {
            if (MainSplitView.IsPaneOpen == true)
            {
                MainSplitView.IsPaneOpen = false;
            }
            else
            {
                MainSplitView.IsPaneOpen = true;
            }
        }

        public void savePoint()
        {
            ((BasePoint)_selectedPoint.Tag).isTemp = false;
        }
    }
}

