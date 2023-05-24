using System;
using System.Numerics;
using System.Threading.Tasks;
using FrameControlEx.Core.Views.Dialogs.UserInputs;
using SkiaSharp;

namespace FrameControlEx.Core.FrameControl.Scene.Sources {
    /// <summary>
    /// Base class for audio visual sources. These contain a position, scale,
    /// scale origin, etc, that represent the standard controls for an input/source
    /// </summary>
    public abstract class AVSourceViewModel : SourceViewModel, IAudioSource, IVisualSource {
        private Vector2 pos;
        private Vector2 scale;
        private Vector2 scaleOrigin;

        public float PosX {
            get => this.pos.X;
            set => this.Pos = new Vector2(value, this.pos.Y);
        }

        public float PosY {
            get => this.pos.Y;
            set => this.Pos = new Vector2(this.pos.X, value);
        }

        public float ScaleX {
            get => this.scale.X;
            set => this.Scale = new Vector2(value, this.scale.Y);
        }

        public float ScaleY {
            get => this.scale.Y;
            set => this.Scale = new Vector2(this.scale.X, value);
        }

        public float ScaleOriginX {
            get => this.scaleOrigin.X;
            set => this.ScaleOrigin = new Vector2(value, this.scaleOrigin.Y);
        }

        public float ScaleOriginY {
            get => this.scaleOrigin.Y;
            set => this.ScaleOrigin = new Vector2(this.scaleOrigin.X, value);
        }

        /// <summary>
        /// The pixel-based offset of this visual source, from the top-left corner of the view port. 0,0 is the very top left
        /// </summary>
        public Vector2 Pos {
            get => this.pos;
            set {
                this.RaisePropertyChanged(ref this.pos, value);
                this.RaisePropertyChanged(nameof(this.PosX));
                this.RaisePropertyChanged(nameof(this.PosY));
                this.InvalidateVisual();
            }
        }

        /// <summary>
        /// This visual source's scale translation. By default, this is 1,1 (meaning no scaling is applied)
        /// </summary>
        public Vector2 Scale {
            get => this.scale;
            set {
                this.RaisePropertyChanged(ref this.scale, value);
                this.RaisePropertyChanged(nameof(this.ScaleX));
                this.RaisePropertyChanged(nameof(this.ScaleY));
                this.InvalidateVisual();
            }
        }

        /// <summary>
        /// The origin point for scaling. Defaults to 0.5,0.5 (the center of the rendered object). 0,0 is the top-left corner, 1,1 is the bottom right
        /// </summary>
        public Vector2 ScaleOrigin {
            get => this.scaleOrigin;
            set {
                this.RaisePropertyChanged(ref this.scaleOrigin, value);
                this.RaisePropertyChanged(nameof(this.ScaleOriginX));
                this.RaisePropertyChanged(nameof(this.ScaleOriginY));
                this.InvalidateVisual();
            }
        }

        public AsyncRelayCommand EditPosCommand { get; }
        public AsyncRelayCommand EditScaleCommand { get; }
        public AsyncRelayCommand EditScaleOriginCommand { get; }
        public RelayCommand ResetCommand { get; }

        public event VisualInvalidatedEventHandler OnVisualInvalidated;

        protected AVSourceViewModel() {
            this.pos = new Vector2(0, 0);
            this.scale = new Vector2(1, 1);
            this.scaleOrigin = new Vector2(0.5f, 0.5f);
            this.EditPosCommand = new AsyncRelayCommand(this.EditPosAction);
            this.EditScaleCommand = new AsyncRelayCommand(this.EditScaleAction);
            this.EditScaleOriginCommand = new AsyncRelayCommand(this.EditScaleOriginAction);
            this.ResetCommand = new RelayCommand(() => {
                this.Pos = new Vector2(0, 0);
                this.Scale = new Vector2(1, 1);
                this.ScaleOrigin = new Vector2(0.5f, 0.5f);
            });
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

        /// <summary>
        /// Invokes the visual invalidation events for this instance and its associated deck (if present), causing the visual
        /// source to be rendered (may also result in all sources being rendered, or may do nothing if the UI rendering is tick based)
        /// </summary>
        public void InvalidateVisual() {
            this.OnVisualInvalidated?.Invoke(this);
            this.Deck?.InvalidateVisual();
        }

        /// <summary>
        /// Called just before rendering in order to tick things (e.g. the next gif frame to display)
        /// </summary>
        public virtual void OnTickVisual() {

        }

        public virtual void OnRender(RenderContext context) {

        }

        protected override void LoadThisIntoUserCopy(BaseIOViewModel vm) {
            base.LoadThisIntoUserCopy(vm);
            if (vm is AVSourceViewModel vs) { // syntax looks nicer than force cast
                vs.pos = this.pos;
                vs.scale = this.scale;
                vs.scaleOrigin = this.scaleOrigin;
            }
        }
    }
}