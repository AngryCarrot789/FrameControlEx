using System;
using System.Globalization;
using System.Windows.Data;
using FrameControlEx.Core.FrameControl.ViewModels.Scene;
using FrameControlEx.Core.Utils;

namespace FrameControlEx.FrameControl.Views {
    public class OutputFilterMultiConverter : IMultiValueConverter {
        public static OutputFilterMultiConverter Instance { get; } = new OutputFilterMultiConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values.Length != 2) {
                throw new Exception($"Expected 2 values, not {values.Length}");
            }

            OutputViewModel vm = (OutputViewModel) values[0];
            Predicate<OutputViewModel> func = (Predicate<OutputViewModel>) values[1];
            return (func == null || func(vm)).Box();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}