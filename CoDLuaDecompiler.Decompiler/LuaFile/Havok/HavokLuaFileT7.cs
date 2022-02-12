using System.IO;

namespace CoDLuaDecompiler.Decompiler.LuaFile.Havok
{
    public class HavokLuaFileT7 : HavokLuaFile
    {
        public HavokLuaFileT7(BinaryReader reader) : base(reader){}
        public HavokLuaFileT7(BinaryReader reader, BinaryReader debugReader) : base(reader)
        {
            DebugFile = new HavokDebugFile(debugReader);
        }
    }
}