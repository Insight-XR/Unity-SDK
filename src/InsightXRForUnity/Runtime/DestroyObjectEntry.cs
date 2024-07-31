using System.IO;

namespace InsightDesk
{
    public class DestroyObjectEntry
    {
        public uint instanceId;

        public static void Write(InsightBuffer buffer, uint instanceId)
        {
            buffer.Write(instanceId);
        }

        public DestroyObjectEntry(BinaryReader binaryReader)
        {
            instanceId = binaryReader.ReadUInt32();
        }
    }
}