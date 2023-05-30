using FrameControlEx.Core.RBC;

namespace FrameControlEx.Core.FrameControl {
    public interface IRBESerialisable {
        void WriteToRBE(RBEDictionary data);

        void ReadFromRBE(RBEDictionary data);
    }
}