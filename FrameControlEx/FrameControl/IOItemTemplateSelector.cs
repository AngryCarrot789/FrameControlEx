using System.Windows;
using System.Windows.Controls;
using FrameControlEx.Core.FrameControl.Scene;

namespace FrameControlEx.MainView {
    public class IOItemTemplateSelector : DataTemplateSelector {
        public DataTemplate SourceTemplate { get; set; }
        public DataTemplate OutputTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            switch (item) {
                case SourceViewModel _: return this.SourceTemplate;
                case OutputViewModel _: return this.OutputTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}