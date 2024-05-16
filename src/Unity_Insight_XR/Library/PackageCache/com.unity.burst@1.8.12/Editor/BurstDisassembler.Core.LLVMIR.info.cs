namespace Unity.Burst.Editor
{
    internal partial class BurstDisassembler
    {
        internal class LLVMIRInstructionInfo
        {
            internal static bool GetLLVMIRInfo(string instructionName, out string instructionInfo)
            {
                var returnValue = true;

                switch (instructionName)
                {
                    default:
                        instructionInfo = string.Empty;
                        break;
                }

                return returnValue;
            }
        }
    }
}