namespace FrameControlEx.Core.Settings.ColourTheme {
    /// <summary>
    /// Represents a 32-bit ARGB colour structure
    /// </summary>
    public readonly struct Colour {
        private readonly uint value;

        /// <summary>
        /// The actual ARGB colour
        /// </summary>
        // ReSharper disable once ConvertToAutoProperty
        public uint Value => this.value;

        /// <summary>Gets the alpha component of the color.</summary>
        /// <value />
        /// <remarks />
        public byte Alpha => (byte) (this.value >> 24 & byte.MaxValue);

        /// <summary>Gets the red component of the color.</summary>
        /// <value />
        /// <remarks />
        public byte Red => (byte) (this.value >> 16 & byte.MaxValue);

        /// <summary>Gets the green component of the color.</summary>
        /// <value />
        /// <remarks />
        public byte Green => (byte) (this.value >> 8 & byte.MaxValue);

        /// <summary>Gets the blue component of the color.</summary>
        /// <value />
        /// <remarks />
        public byte Blue => (byte) (this.value & byte.MaxValue);

        public Colour(uint value) {
            this.value = value;
        }

        public Colour(byte a, byte r, byte g, byte b) {
            this.value = a | ((uint) r << 8) | ((uint) g << 16) | ((uint) b << 24);
        }
    }
}