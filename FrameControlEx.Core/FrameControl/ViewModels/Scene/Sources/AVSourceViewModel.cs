using System;
using System.Numerics;
using System.Threading.Tasks;
using FrameControlEx.Core.FrameControl.Models.Scene.Sources.Base;
using FrameControlEx.Core.Utils;
using FrameControlEx.Core.Views.Dialogs.UserInputs;

namespace FrameControlEx.Core.FrameControl.Scene.Sources {
    /// <summary>
    /// Base class for audio visual sources. These contain a position, scale,
    /// scale origin, etc, that represent the standard controls for an input/source
    /// </summary>
    public abstract class AVSourceViewModel : SourceViewModel {
        public new AVSourceModel Model => (AVSourceModel) ((BaseIOViewModel) this).Model;

        public float PosX {
            get => this.Pos.X;
            set => this.Pos = new Vector2(value, this.Pos.Y);
        }

        public float PosY {
            get => this.Pos.Y;
            set => this.Pos = new Vector2(this.Pos.X, value);
        }

        public float ScaleX {
            get => this.Scale.X;
            set => this.Scale = new Vector2(value, this.Scale.Y);
        }

        public float ScaleY {
            get => this.Scale.Y;
            set => this.Scale = new Vector2(this.Scale.X, value);
        }

        public float ScaleOriginX {
            get => this.ScaleOrigin.X;
            set => this.ScaleOrigin = new Vector2(value, this.ScaleOrigin.Y);
        }

        public float ScaleOriginY {
            get => this.ScaleOrigin.Y;
            set => this.ScaleOrigin = new Vector2(this.ScaleOrigin.X, value);
        }

        /// <summary>
        /// The pixel-based offset of this visual source, from the top-left corner of the view port. 0,0 is the very top left
        /// </summary>
        public Vector2 Pos {
            get => this.Model.Pos;
            set {
                this.Model.Pos = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.PosX));
                this.RaisePropertyChanged(nameof(this.PosY));
                this.Model.InvalidateVisual();
            }
        }

        /// <summary>
        /// This visual source's scale translation. By default, this is 1,1 (meaning no scaling is applied)
        /// </summary>
        public Vector2 Scale {
            get => this.Model.Scale;
            set {
                this.Model.Scale = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.ScaleX));
                this.RaisePropertyChanged(nameof(this.ScaleY));
                this.Model.InvalidateVisual();
            }
        }

        /// <summary>
        /// The origin point for scaling. Defaults to 0.5,0.5 (the center of the rendered object). 0,0 is the top-left corner, 1,1 is the bottom right
        /// </summary>
        public Vector2 ScaleOrigin {
            get => this.Model.ScaleOrigin;
            set {
                this.Model.ScaleOrigin = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.ScaleOriginX));
                this.RaisePropertyChanged(nameof(this.ScaleOriginY));
                this.Model.InvalidateVisual();
            }
        }

        public AsyncRelayCommand EditPosCommand { get; }
        public AsyncRelayCommand EditScaleCommand { get; }
        public AsyncRelayCommand EditScaleOriginCommand { get; }
        public RelayCommand ResetCommand { get; }

        protected AVSourceViewModel(AVSourceModel model) : base(model) {
            this.EditPosCommand = new AsyncRelayCommand(this.EditPosAction);
            this.EditScaleCommand = new AsyncRelayCommand(this.EditScaleAction);
            this.EditScaleOriginCommand = new AsyncRelayCommand(this.EditScaleOriginAction);
            this.ResetCommand = new RelayCommand(() => {
                this.Pos = new Vector2(0, 0);
                this.Scale = new Vector2(1, 1);
                this.ScaleOrigin = new Vector2(0.5f, 0.5f);
            });
        }

        public static Rect CalculateOutputRect(Rect vp, Vector2 pos, Vector2 scale, Vector2 origin) {
            // Calculate the scaled size based on the original size and scale factors
            float scaledWidth = vp.Width * scale.X;
            float scaledHeight = vp.Height * scale.Y;

            // Calculate the scaled position based on the original position and scale factors
            float scaledX = pos.X + (vp.Width * (origin.X - 0.5f) * (1 - scale.X));
            float scaledY = pos.Y + (vp.Height * (origin.Y - 0.5f) * (1 - scale.Y));

            // Create the output rect using the scaled position and size
            Rect outputRect = new Rect(scaledX, scaledY, scaledWidth, scaledHeight);

            return outputRect;
        }

        public Rect GetFullRectangle() {
            Vector2 size = this.Model.GetSize();
            Vector2 scaledSize = size * this.Scale;
            Vector2 realPos = this.Pos + (scaledSize * this.ScaleOrigin);
            return new Rect(realPos, scaledSize);
        }

        protected DoubleInputViewModel CreateVec2Editor(string msgA, string msgB, string title, float inputA, float inputB) {
            DoubleInputViewModel input = new DoubleInputViewModel() {
                MessageA = msgA, MessageB = msgB, Title = title,
                InputA = Math.Round(inputA, 4).ToString(),
                InputB = Math.Round(inputB, 4).ToString()
            };

            input.ValidateInputA = InputValidator.FromFunc((x) => float.TryParse(x, out _) ? null : $"Invalid float value: {x}");
            return input;
        }

        public virtual async Task EditPosAction() {
            DoubleInputViewModel input = this.CreateVec2Editor("Pos X:", "Pos Y:", "Input a new position", this.PosX, this.PosY);
            if (await IoC.UserInput.ShowDoubleInputDialogAsync(input)) {
                if (float.TryParse(input.InputA, out float x) && float.TryParse(input.InputB, out float y)) {
                    this.Pos = new Vector2(x, y);
                }
            }
        }

        public virtual async Task EditScaleAction() {
            DoubleInputViewModel input = this.CreateVec2Editor("Scale X:", "Scale Y:", "Input a new scale", this.ScaleX, this.ScaleY);
            if (await IoC.UserInput.ShowDoubleInputDialogAsync(input)) {
                if (float.TryParse(input.InputA, out float x) && float.TryParse(input.InputB, out float y)) {
                    this.Scale = new Vector2(x, y);
                }
            }
        }

        public virtual async Task EditScaleOriginAction() {
            DoubleInputViewModel input = this.CreateVec2Editor("Origin X:", "Origin Y:", "Input a new scale origin", this.ScaleOriginX, this.ScaleOriginY);
            if (await IoC.UserInput.ShowDoubleInputDialogAsync(input)) {
                if (float.TryParse(input.InputA, out float x) && float.TryParse(input.InputB, out float y)) {
                    this.ScaleOrigin = new Vector2(x, y);
                }
            }
        }
    }
}