namespace FrameControlEx.Core.Settings.ColourTheme {
    public interface IColourSelector {
        Colour SelectARGB(Colour def = default);
    }
}