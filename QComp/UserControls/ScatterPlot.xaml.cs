using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace QComp.UserControls
{
    public partial class ScatterPlot : UserControl
    {
        private readonly List<double> _horizontal;
        private readonly List<double> _vertical;

        public ScatterPlot(List<double> horizontal, string horizontalName, List<double> vertical, string verticalName, double size)
        {
            InitializeComponent();
            Width = size;
            Height = size;
            HorizontalLabel.Content = horizontalName;
            VerticalLabel.Content = verticalName;
            _horizontal = horizontal;
            _vertical = vertical;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var maxValue = Math.Max(_horizontal.Max(), _vertical.Max());
            maxValue *= 1.1;
            if (maxValue < MainCanvas.ActualWidth)
                maxValue = MainCanvas.ActualWidth;

            MainCanvas.Children.Add(new Label()
            {
                Content = "0",
                Margin = new Thickness(-20, MainCanvas.ActualHeight, 0, 0)
            });

            double scaleX = 1;
            if (maxValue > MainCanvas.ActualWidth)
                scaleX = MainCanvas.ActualWidth / maxValue;
            double scaleY = 1;
            if (maxValue > MainCanvas.ActualHeight)
                scaleY = MainCanvas.ActualHeight / maxValue;

            GenerateHorizontalScale((maxValue / 4) * 1, scaleX);
            GenerateHorizontalScale((maxValue / 4) * 2, scaleX);
            GenerateHorizontalScale((maxValue / 4) * 3, scaleX);
            GenerateVerticalScale((maxValue / 4) * 1, scaleY);
            GenerateVerticalScale((maxValue / 4) * 2, scaleY);
            GenerateVerticalScale((maxValue / 4) * 3, scaleY);
            MainCanvas.Children.Add(new Line()
            {
                X1 = 0,
                Y1 = MainCanvas.ActualHeight,
                X2 = MainCanvas.ActualWidth,
                Y2 = 0,
                Stroke = Brushes.White
            });

            for (int i = 0; i < _horizontal.Count; i++)
            {
                MainCanvas.Children.Add(new Rectangle()
                {
                    Margin = new Thickness(_horizontal[i] * scaleX, MainCanvas.ActualHeight - _vertical[i] * scaleY, 0, 0),
                    Width = 2,
                    Height = 2,
                    Fill = Brushes.Yellow
                });
            }
        }

        private void GenerateHorizontalScale(double value, double scale)
        {
            MainCanvas.Children.Add(new Label()
            {
                Content = Math.Round(value, 0),
                Margin = new Thickness(value * scale, MainCanvas.ActualHeight - 20, 0, 0)
            });

            MainCanvas.Children.Add(new Line()
            {
                X1 = value * scale,
                Y1 = MainCanvas.ActualHeight,
                X2 = value * scale,
                Y2 = 0,
                Stroke = Brushes.DarkGray,
                Opacity = 0.4
            });
        }

        private void GenerateVerticalScale(double value, double scale)
        {
            MainCanvas.Children.Add(new Label()
            {
                Content = Math.Round(value, 0),
                Margin = new Thickness(0, MainCanvas.ActualWidth - value * scale, 0, 0)
            });

            MainCanvas.Children.Add(new Line()
            {
                X1 = 0,
                Y1 = value * scale,
                X2 = MainCanvas.ActualWidth,
                Y2 = value * scale,
                Stroke = Brushes.DarkGray,
                Opacity = 0.4
            });
        }
    }
}
