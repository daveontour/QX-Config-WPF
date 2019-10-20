using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Common {
    public class NodeToVisibilityConverter : IMultiValueConverter {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {

            string nodeType = (string)values[0];
            string epType = (string)values[1];
            string param = (string)values[2];

            Console.WriteLine($"{nodeType}, {epType}, {param}");

            try {
                int conf = ParamConfig.ParamDict[nodeType][param];
                int inQ = conf & ParamConfig.InputVis;
                int outQ = conf & ParamConfig.OutputVis;

                if (epType == "logger" || epType == "monitor" || epType == "altqueue") {
                    epType = "output";
                }

                bool isInput = false;

                if (epType == "input") {
                    isInput = true;
                } else {
                    isInput = false;
                }

                if (isInput && inQ > 0 || !isInput && outQ > 0) {
                    return Visibility.Visible;
                } else {
                    return Visibility.Collapsed;
                };

            } catch (Exception e) {
                Console.WriteLine(e.Message);
                return true;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
