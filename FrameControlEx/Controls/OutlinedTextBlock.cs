using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows;

namespace FrameControlEx.Controls {

    // https://stackoverflow.com/questions/93650/apply-stroke-to-a-textblock-in-wpf

    [ContentProperty("Text")]
    public class OutlinedTextBlock : FrameworkElement {
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register("Fill", typeof(Brush), typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register("Stroke", typeof(Brush), typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender, StrokePropertyChangedCallback));
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness", typeof(double), typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender, StrokePropertyChangedCallback));
        public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily, OnFormattedTextUpdated) {AffectsRender = true, AffectsMeasure = true});
        public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(SystemFonts.MessageFontSize, OnFormattedTextUpdated) {AffectsRender = true, AffectsMeasure = true});
        public static readonly DependencyProperty FontStretchProperty = TextElement.FontStretchProperty.AddOwner(typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(FontStretches.Normal, OnFormattedTextUpdated) {AffectsRender = true, AffectsMeasure = true});
        public static readonly DependencyProperty FontStyleProperty = TextElement.FontStyleProperty.AddOwner(typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(SystemFonts.MessageFontStyle, OnFormattedTextUpdated) {AffectsRender = true, AffectsMeasure = true});
        public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(SystemFonts.MessageFontWeight, OnFormattedTextUpdated) {AffectsRender = true, AffectsMeasure = true});
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(OutlinedTextBlock),new FrameworkPropertyMetadata(OnFormattedTextInvalidated) {AffectsRender = true, AffectsMeasure = true});
        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(OutlinedTextBlock),new FrameworkPropertyMetadata(OnFormattedTextUpdated) {AffectsRender = true, AffectsMeasure = true});
        public static readonly DependencyProperty TextDecorationsProperty = DependencyProperty.Register("TextDecorations", typeof(TextDecorationCollection), typeof(OutlinedTextBlock),new FrameworkPropertyMetadata(OnFormattedTextUpdated) {AffectsRender = true, AffectsMeasure = true});
        public static readonly DependencyProperty TextTrimmingProperty = DependencyProperty.Register("TextTrimming", typeof(TextTrimming), typeof(OutlinedTextBlock),new FrameworkPropertyMetadata(OnFormattedTextUpdated) {AffectsRender = true, AffectsMeasure = true});
        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(OutlinedTextBlock),new FrameworkPropertyMetadata(TextWrapping.NoWrap, OnFormattedTextUpdated) {AffectsRender = true, AffectsMeasure = true});

        private FormattedText formattedText;
        private Geometry textGeometry;
        private Pen pen;

        public Brush Fill {
            get { return (Brush) this.GetValue(FillProperty); }
            set { this.SetValue(FillProperty, value); }
        }

        public FontFamily FontFamily {
            get { return (FontFamily) this.GetValue(FontFamilyProperty); }
            set { this.SetValue(FontFamilyProperty, value); }
        }

        [TypeConverter(typeof(FontSizeConverter))]
        public double FontSize {
            get { return (double) this.GetValue(FontSizeProperty); }
            set { this.SetValue(FontSizeProperty, value); }
        }

        public FontStretch FontStretch {
            get { return (FontStretch) this.GetValue(FontStretchProperty); }
            set { this.SetValue(FontStretchProperty, value); }
        }

        public FontStyle FontStyle {
            get { return (FontStyle) this.GetValue(FontStyleProperty); }
            set { this.SetValue(FontStyleProperty, value); }
        }

        public FontWeight FontWeight {
            get { return (FontWeight) this.GetValue(FontWeightProperty); }
            set { this.SetValue(FontWeightProperty, value); }
        }

        public Brush Stroke {
            get { return (Brush) this.GetValue(StrokeProperty); }
            set { this.SetValue(StrokeProperty, value); }
        }

        public double StrokeThickness {
            get { return (double) this.GetValue(StrokeThicknessProperty); }
            set { this.SetValue(StrokeThicknessProperty, value); }
        }

        public string Text {
            get { return (string) this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        public TextAlignment TextAlignment {
            get { return (TextAlignment) this.GetValue(TextAlignmentProperty); }
            set { this.SetValue(TextAlignmentProperty, value); }
        }

        public TextDecorationCollection TextDecorations {
            get { return (TextDecorationCollection) this.GetValue(TextDecorationsProperty); }
            set { this.SetValue(TextDecorationsProperty, value); }
        }

        public TextTrimming TextTrimming {
            get { return (TextTrimming) this.GetValue(TextTrimmingProperty); }
            set { this.SetValue(TextTrimmingProperty, value); }
        }

        public TextWrapping TextWrapping {
            get { return (TextWrapping) this.GetValue(TextWrappingProperty); }
            set { this.SetValue(TextWrappingProperty, value); }
        }

        public OutlinedTextBlock() {
            this.UpdatePen();
            this.TextDecorations = new TextDecorationCollection();
        }

        private static void StrokePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
            ((OutlinedTextBlock) dependencyObject).UpdatePen();
        }

        private void UpdatePen() {
            this.pen = new Pen(this.Stroke, this.StrokeThickness) {
                DashCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
                LineJoin = PenLineJoin.Round,
                StartLineCap = PenLineCap.Round
            };

            this.InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext) {
            this.EnsureGeometry();

            drawingContext.DrawGeometry(null, this.pen, this.textGeometry);
            drawingContext.DrawGeometry(this.Fill, null, this.textGeometry);
        }

        protected override Size MeasureOverride(Size availableSize) {
            this.EnsureFormattedText();

            // constrain the formatted text according to the available size

            double w = availableSize.Width;
            double h = availableSize.Height;

            // the Math.Min call is important - without this constraint (which seems arbitrary, but is the maximum allowable text width), things blow up when availableSize is infinite in both directions
            // the Math.Max call is to ensure we don't hit zero, which will cause MaxTextHeight to throw
            this.formattedText.MaxTextWidth = Math.Min(3579139, w);
            this.formattedText.MaxTextHeight = Math.Max(0.0001d, h);

            // return the desired size
            return new Size(Math.Ceiling(this.formattedText.Width), Math.Ceiling(this.formattedText.Height));
        }

        protected override Size ArrangeOverride(Size finalSize) {
            this.EnsureFormattedText();

            // update the formatted text with the final size
            this.formattedText.MaxTextWidth = finalSize.Width;
            this.formattedText.MaxTextHeight = Math.Max(0.0001d, finalSize.Height);

            // need to re-generate the geometry now that the dimensions have changed
            this.textGeometry = null;

            return finalSize;
        }

        private static void OnFormattedTextInvalidated(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e) {
            var outlinedTextBlock = (OutlinedTextBlock) dependencyObject;
            outlinedTextBlock.formattedText = null;
            outlinedTextBlock.textGeometry = null;
        }

        private static void OnFormattedTextUpdated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e) {
            var outlinedTextBlock = (OutlinedTextBlock) dependencyObject;
            outlinedTextBlock.UpdateFormattedText();
            outlinedTextBlock.textGeometry = null;
        }

        private void EnsureFormattedText() {
            if (this.formattedText != null) {
                return;
            }

            this.formattedText = new FormattedText(this.Text ?? "",
                CultureInfo.CurrentUICulture, this.FlowDirection,
                new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch), this.FontSize,
                Brushes.Black);

            this.UpdateFormattedText();
        }

        private void UpdateFormattedText() {
            if (this.formattedText == null) {
                return;
            }

            this.formattedText.MaxLineCount = this.TextWrapping == TextWrapping.NoWrap ? 1 : int.MaxValue;
            this.formattedText.TextAlignment = this.TextAlignment;
            this.formattedText.Trimming = this.TextTrimming;

            this.formattedText.SetFontSize(this.FontSize);
            this.formattedText.SetFontStyle(this.FontStyle);
            this.formattedText.SetFontWeight(this.FontWeight);
            this.formattedText.SetFontFamily(this.FontFamily);
            this.formattedText.SetFontStretch(this.FontStretch);
            this.formattedText.SetTextDecorations(this.TextDecorations);
        }

        private void EnsureGeometry() {
            if (this.textGeometry != null) {
                return;
            }

            this.EnsureFormattedText();
            this.textGeometry = this.formattedText.BuildGeometry(new Point(0, 0));
        }
    }
}
