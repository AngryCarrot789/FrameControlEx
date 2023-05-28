namespace FrameControlEx.Core.Settings.ColourTheme {
    /// <summary>
    /// Represents a 32-bit ARGB colour structure
    /// </summary>
    public readonly struct Colour {
        private readonly uint color;

        /// <summary>
        /// The actual ARGB colour
        /// </summary>
        // ReSharper disable once ConvertToAutoProperty
        public uint ColourValue => this.color;

        /// <summary>Gets the alpha component of the color.</summary>
        /// <value />
        /// <remarks />
        public byte Alpha => (byte) (this.color >> 24 & byte.MaxValue);

        /// <summary>Gets the red component of the color.</summary>
        /// <value />
        /// <remarks />
        public byte Red => (byte) (this.color >> 16 & byte.MaxValue);

        /// <summary>Gets the green component of the color.</summary>
        /// <value />
        /// <remarks />
        public byte Green => (byte) (this.color >> 8 & byte.MaxValue);

        /// <summary>Gets the blue component of the color.</summary>
        /// <value />
        /// <remarks />
        public byte Blue => (byte) (this.color & byte.MaxValue);

        public Colour(uint color) {
            this.color = color;
        }

        public Colour(byte a, byte r, byte g, byte b) {
            this.color = a | ((uint) r << 8) | ((uint) g << 16) | ((uint) b << 24);
        }
    }
}