using System.Collections.Generic;
using System.IO;
using CoDLuaDecompiler.Decompiler.Analyzers;
using CoDLuaDecompiler.Decompiler.InstructionConverters;
using CoDLuaDecompiler.Decompiler.LuaFile.Structures.FileHeader;
using CoDLuaDecompiler.Decompiler.LuaFile.Structures.LuaConstant;
using CoDLuaDecompiler.Decompiler.LuaFile.Structures.LuaConstant.Havok;
using CoDLuaDecompiler.Decompiler.LuaFile.Structures.LuaFunction;
using CoDLuaDecompiler.Decompiler.LuaFile.Structures.LuaFunction.Havok;

namespace CoDLuaDecompiler.Decompiler.LuaFile.Havok
{
    public abstract class HavokLuaFile : LuaFile
    {
        public HavokDebugFile DebugFile { get; set; }
        public override IInstructionConverter InstructionConverter => new HavokInstructionConverter();
        public override IAnalyzerList AnalyzerList => new HavokAnalyzerList();
        public override IAnalyzerList FileAnalyzerList => new HavokFileAnalyzerList();
        public HavokLuaFile(BinaryReader reader) : base(reader)
        {
        }

        protected override IFileHeader ReadHeader()
        {
            var header = new HavokFileHeader()
            {
                Magic = Reader.ReadChars(4).ToString(),
                Version = Reader.ReadByte(),
                CompilerVersion = Reader.ReadByte(),
                Endianness = Reader.ReadByte(),
                SizeOfInt = Reader.ReadByte(),
                SizeOfSizeT = Reader.ReadByte(),
                SizeOfInstruction = Reader.ReadByte(),
                SizeOfLuaNumber = Reader.ReadByte(),
                IntegralFlag = Reader.ReadByte(),
                GameByte = Reader.ReadByte(),
            };
            
            Reader.ReadByte();
            header.ConstantTypeCount = Reader.ReadInt32();

            return header;
        }

        protected override IList<IHavokLuaConstant> ReadConstants()
        {
            var constants = new List<IHavokLuaConstant>();

            for (var i = 0; i < ((HavokFileHeader)Header).ConstantTypeCount; i++)
            {
                var type = (HavokConstantType)Reader.ReadInt32();
                var length = Reader.ReadInt32();
                var constant = Reader.ReadBytes(length);
            }

            return constants;
        }

        protected override ILuaFunction ReadFunctions()
        {
            return new HavokLuaFunctionT7(this, Reader);
        }
    }
}