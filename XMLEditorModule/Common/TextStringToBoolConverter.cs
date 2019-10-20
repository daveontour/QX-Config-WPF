using System;
using System.Windows.Data;


namespace WXE.Internal.Tools.ConfigEditor.Common {
    public class TextStringToBoolConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {

            if (value == null) {
                return false;
            }

            if ((string)value == ""){
                return false;
            }

            if (((string)value).ToLower() == "true") {
                return true;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return value.ToString();
        }
    }

   
}
