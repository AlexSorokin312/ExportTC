using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ExportTC.Converters
{
    public class ProductStatusToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var status = value.ToString().ToLower();
            if (status == "new")
            {

            }
            string imagePath = status switch
            {
                "checkedin" => "pack://application:,,,/Resources/checkedin.png",
                "checkedout" => "pack://application:,,,/Resources/checkedout.png",
                "state" => "pack://application:,,,/Resources/state.png",
                "frozen" => "/Resources/frozen.png",
                "released" => "/Resources/released.png",
                "blank" => "/Resources/blank.png",
                "new" => "pack://application:,,,/Resources/new.png",
                _ => "/Resources/default.png"
            };

            return new BitmapImage(new Uri(imagePath, UriKind.Relative));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
