using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FrameControlEx.Core.Utils;

namespace FrameControlEx.Controls {
    [TemplatePart(Name = "PART_TextBlock", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_TextBox", Type = typeof(TextBox))]
    public class NumberDragger : RangeBase {
        private TextBlock PART_TextBlock;
        private TextBox PART_TextBox;
        private bool isEditingDirectly;

        public static readonly DependencyProperty IsVerticalProperty = DependencyProperty.Register("IsVertical", typeof(bool), typeof(NumberDragger), new PropertyMetadata(BoolBox.False));
        public static readonly DependencyPropertyKey IsDraggingPropertyKey = DependencyProperty.RegisterReadOnly("IsDragging", typeof(bool), typeof(NumberDragger), new PropertyMetadata(BoolBox.False));
        public static readonly DependencyProperty ValueMultiplierProperty = DependencyProperty.Register("ValueMultiplier", typeof(double), typeof(NumberDragger), new PropertyMetadata(1d));

        public bool IsVertical {
            get => (bool) this.GetValue(IsVerticalProperty);
            set => this.SetValue(IsVerticalProperty, value.Box());
        }

        public bool IsDragging {
            get => (bool) this.GetValue(IsDraggingPropertyKey.DependencyProperty);
            private set => this.SetValue(IsDraggingPropertyKey, value.Box());
        }

        public double ValueMultiplier {
            get { return (double) this.GetValue(ValueMultiplierProperty); }
            set { this.SetValue(ValueMultiplierProperty, value); }
        }

        private bool isMouseDown;
        private Point lastMouseClick;
        private Point lastMouseMove;
        private double oldValue;

        public NumberDragger() {

        }

        protected override void OnValueChanged(double oldValue, double newValue) {
            base.OnValueChanged(oldValue, newValue);
            if (this.PART_TextBox == null || this.PART_TextBlock == null) {
                return;
            }

            string text = Math.Round(newValue, 2).ToString();
            this.PART_TextBox.Text = text;
            this.PART_TextBox.Text = text;
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.PART_TextBlock = this.GetTemplateChild("PART_TextBlock") as TextBlock ?? throw new Exception("Missing template part: " + nameof(this.PART_TextBlock));
            this.PART_TextBox = this.GetTemplateChild("PART_TextBox") as TextBox ?? throw new Exception("Missing template part: " + nameof(this.PART_TextBox));
            this.PART_TextBox.KeyDown += this.OnTextBoxKeyDown;
            this.PART_TextBox.LostFocus += (sender, args) => {
                this.HideEditor();
            };

            this.HideEditor();
            string text = Math.Round(this.Value, 2).ToString();
            this.PART_TextBox.Text = text;
            this.PART_TextBlock.Text = text;

            this.PART_TextBlock.MouseLeftButtonDown += this.PART_TextBlockOnMouseDown;
            this.PART_TextBlock.MouseMove += this.PART_TextBlockOnMouseMove;
            this.PART_TextBlock.MouseLeftButtonUp += this.PART_TextBlockOnMouseUp;
        }

        private void PART_TextBlockOnMouseDown(object sender, MouseButtonEventArgs e) {
            if (this.IsDragging) {
                return;
            }

            e.Handled = true;
            this.Focus();
            this.CaptureMouse();
            this.isMouseDown = true;
        }

        private void PART_TextBlockOnMouseUp(object sender, MouseButtonEventArgs e) {
            if (this.IsMouseCaptured && this.IsDragging) {
                e.Handled = true;
                this.OnCompleteDrag(false);
            }
            else if (this.isMouseDown) {
                this.EnableEditor();
                this.isMouseDown = false;
            }
        }

        private void PART_TextBlockOnMouseMove(object sender, MouseEventArgs e) {
            if (this.isEditingDirectly)
                return;

            Point pos = e.GetPosition(this);
            if (!this.IsDragging) {
                if (e.LeftButton != MouseButtonState.Pressed) {
                    this.isMouseDown = false;
                    return;
                }

                double change;
                if (this.IsVertical) {
                    change = Math.Abs(pos.Y - this.lastMouseClick.Y);
                }
                else {
                    change = Math.Abs(pos.X - this.lastMouseClick.X);
                }

                if (change < 5d) {
                    return;
                }

                this.IsDragging = true;
                this.lastMouseClick = this.lastMouseMove = e.GetPosition(this);
                this.OnBeginDrag();
            }

            if (e.LeftButton != MouseButtonState.Pressed) {
                this.OnCompleteDrag(true);
                return;
            }

            double delta;
            if (this.IsVertical) {
                delta = Math.Abs(pos.Y - this.lastMouseMove.Y);
                if (delta < 0.5d) {
                    return;
                }
            }
            else {
                delta = Math.Abs(pos.X - this.lastMouseMove.X);
                if (delta < 0.5d) {
                    return;
                }
            }

            if (!this.OnDragDelta(delta)) {
                this.lastMouseMove = pos;
            }
        }

        private void OnTextBoxKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                this.HideEditor();
            }
            else if (e.Key == Key.Enter) {
                if (double.TryParse(((TextBox) sender).Text, out double value)) {
                    this.Value = value;
                }

                this.HideEditor();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if (this.isEditingDirectly)
                return;

            if (e.Key == Key.Enter) {
                this.OnCompleteDrag(e.Key == Key.Enter);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape) {
                this.OnCompleteDrag(true);
                e.Handled = true;
            }
        }

        public void OnBeginDrag() {
            if (this.isEditingDirectly)
                return;

            this.oldValue = this.Value;
            this.PART_TextBox.Visibility = Visibility.Collapsed;
            this.PART_TextBlock.Visibility = Visibility.Visible;
            this.HideEditor();
        }

        public bool OnDragDelta(double delta) {
            if (this.isEditingDirectly)
                return false;

            double change;
            if (Keyboard.IsKeyDown(Key.LeftShift)) {
                change = this.SmallChange * delta;
            }
            else {
                change = this.LargeChange * delta;
            }

            change *= this.ValueMultiplier;
            this.Value += change;
            this.HideEditor();
            return false;
        }

        public void OnCompleteDrag(bool cancel) {
            if (this.isEditingDirectly)
                return;

            if (!this.IsDragging)
                return;
            if (this.IsMouseCaptured)
                this.ReleaseMouseCapture();
            this.ClearValue(IsDraggingPropertyKey);

            if (cancel) {
                this.Value = this.oldValue;
            }

            this.HideEditor();
        }

        public void EnableEditor() {
            this.isEditingDirectly = true;
            this.PART_TextBox.Visibility = Visibility.Visible;
            this.PART_TextBlock.Visibility = Visibility.Collapsed;
        }

        public void HideEditor() {
            this.isEditingDirectly = false;
            this.PART_TextBox.Visibility = Visibility.Collapsed;
            this.PART_TextBlock.Visibility = Visibility.Visible;
        }
    }
}
