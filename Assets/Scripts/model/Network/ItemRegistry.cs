using Unity.Netcode;

namespace model.Network
{
    public class ItemRegistry : NetworkVariableBase
    {
        public override void WriteDelta(FastBufferWriter writer)
        {
            throw new System.NotImplementedException();
        }

        public override void WriteField(FastBufferWriter writer)
        {
            throw new System.NotImplementedException();
        }

        public override void ReadField(FastBufferReader reader)
        {
            throw new System.NotImplementedException();
        }

        public override void ReadDelta(FastBufferReader reader, bool keepDirtyDelta)
        {
            throw new System.NotImplementedException();
        }
    }
}