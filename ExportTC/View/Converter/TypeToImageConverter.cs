using ExportTC.Constants;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ExportTC.Converters
{
    public class TypeToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string elementType)
            {
                if (elementType == ElementConstants.DETAIL)
                
                {
                    return "pack://application:,,,/Resources/ic_sw_cool_prt.png";
                }
                else if (elementType == ElementConstants.ASSEMBLY)
                {
                    return "pack://application:,,,/Resources/ic_sldasm.png";
                }
                else if (elementType == ElementConstants.PDF)
                {
                    return "pack://application:,,,/Resources/ic_pdf.png";
                }
            }

            // Если тип неизвестен, возвращаем картинку по умолчанию
            return "pack://application:,,,/Resources/ic_default.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
