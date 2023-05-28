namespace ColorPicker.Models {
    class HintColorDecorator : IColorStateStorage {
        public ColorState ColorState {
            get => this.storage.HintColorState;
            set => this.storage.HintColorState = value;
        }

        private IHintColorStateStorage storage;

        public HintColorDecorator(IHintColorStateStorage storage) {
            this.storage = storage;
        }
    }
}