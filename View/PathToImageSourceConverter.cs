using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Globalization;
using System.IO;

namespace MrMeeseeks.NonogramSolver.View
{
    public class PathToImageSourceConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string path && File.Exists(path))
            {
                return new Bitmap(path);
            }
            
            return new ImageDrawing {Rect = new Rect(new Size(20, 20))}.ImageSource!;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return "";
        }
    }
}