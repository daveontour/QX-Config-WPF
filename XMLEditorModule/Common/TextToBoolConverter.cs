using System;
using System.Windows.Data;


namespace WXE.Internal.Tools.ConfigEditor.Common {
    public class TextToBoolConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string )
            {
                return ((string)value).Length > 0;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

   
}
