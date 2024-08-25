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

namespace GeoGraph.Pages.MainPage
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

            this.InitializeEllipses();

            this.InitializeMap();
        }
        // 基本点信息
        private class BasePoint
        {
            public Point location;
            public int pointInfCode;
            public bool isTemp = false;
        }
        // 动态点信息 在NetWork中

        // 该考虑点预载信息了
        private List<BasePoint> originalPositions;
        // 需要一个字典将pointInf与点的信息对应起来 

        // 上一个拖动的点和当前选择的点
        private Ellipse _selectedPoint;
        private Point _lastPointerPosition;

        private bool _isDragging;
        private double _currentScale = 1.0;

        private const double ScaleStep = 0.1;
        private const double MaxScale = 5.0;
        private const double MinScale = 1.0;

        

        private void InitializeEllipses()
        {
            // 初始化点列表
            originalPositions = new List<BasePoint>();
        }

        private void InitializeMap()
        {
            // 初始化Canvas大小
            MainCanvas.Width = AssetsGet.MapInfWidth;
            MainCanvas.Height = AssetsGet.MapInfHeight;
            // 初始化Map图像
            BitmapImage bitmapImage = new BitmapImage(new Uri(AssetsGet.newImagePath));
            Image.Source = bitmapImage;
        }

        private void CreateEllipse(BasePoint position)
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
            if (_selectedPoint != null)
            {
                //如果是左键 那么取消选择点
                if (pointerProperties.IsLeftButtonPressed)
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
                        _selectedPoint = null;
                    }
                }
                else
                {
                    //如果是右键那就是拖动逻辑
                    _lastPointerPosition = e.GetCurrentPoint(MainCanvas).Position;
                    _isDragging = true;
                    MainCanvas.CapturePointer(e.Pointer);
                }
            }
            // 如果此时没有选中点 则创建点
            else
            {
                //读取此时鼠标点击的位置 注意这个位置相对画布实际的位置
                BasePoint temp = new BasePoint
                {
                    //这个Position要作变换
                    location = e.GetCurrentPoint(MainCanvas).Position,
                    pointInfCode = -1,
                    isTemp = true
                };
                CreateEllipse(temp);
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

        // 选中点
        private void OnEllipseTapped(object sender, TappedRoutedEventArgs e)
        {
            Ellipse tappedEllipse = sender as Ellipse;

            // 如果已经有选中的点，将其恢复为未选中状态
            if (_selectedPoint != null)
            {
                _selectedPoint.Fill = new SolidColorBrush(Microsoft.UI.Colors.Red);
                _selectedPoint = null;
            }

            // 将点击的点设置为选中状态，并且为temp点
            _selectedPoint = tappedEllipse;
            _selectedPoint.Fill = new SolidColorBrush(Microsoft.UI.Colors.Blue);
            // C#的赋值都是引用赋值 所以这里直接强转类型然后改就行
            ((BasePoint)_selectedPoint.Tag).isTemp = true;

        }


    }
}

