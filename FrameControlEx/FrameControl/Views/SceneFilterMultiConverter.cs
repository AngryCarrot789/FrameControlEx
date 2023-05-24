using System;
using System.Globalization;
using System.Windows.Data;
using FrameControlEx.Core.FrameControl.Scene;
using FrameControlEx.Core.Utils;

namespace FrameControlEx.FrameControl.Views {
    public class SceneFilterMultiConverter : IMultiValueConverter {
        public static SceneFilterMultiConverter Instance { get; } = new SceneFilterMultiConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values.Length != 2) {
                throw new Exception($"Expected 2 values, not {values.Length}");
            }

            SceneViewModel vm = (SceneViewModel) values[0];
            Predicate<SceneViewModel> func = (Predicate<SceneViewModel>) values[1];
            return (func == null || func(vm)).Box();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}