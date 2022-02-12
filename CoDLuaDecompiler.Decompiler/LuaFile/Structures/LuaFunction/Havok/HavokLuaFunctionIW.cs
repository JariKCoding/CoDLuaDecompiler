using System.Collections.Generic;
using System.IO;
using System.Text;
using CoDLuaDecompiler.Decompiler.LuaFile.Havok;
using CoDLuaDecompiler.Decompiler.LuaFile.Havok.Debug;
using CoDLuaDecompiler.Decompiler.LuaFile.Structures.LuaFunction.Structures;

namespace CoDLuaDecompiler.Decompiler.LuaFile.Structures.LuaFunction.Havok
{
    public class HavokLuaFunctionIW : HavokLuaFunction
    {
        public HavokLuaFunctionIW(ILuaFile luaFile, BinaryReader reader) : base(luaFile, reader)
        {
        }
        
        protected override FunctionFooter ReadFunctionFooter()
        {
            var unk1 = Reader.ReadInt32();
            if (unk1 == 0)
            {
                return new FunctionFooter
                {
                    Unknown1 = unk1,
                    Unknown2 = Reader.ReadSingle(),
                    SubFunctionCount = Reader.ReadInt32(),
                };
            }

            if (((HavokLuaFile) LuaFile).DebugFile == null)
            {
                ((HavokLuaFile) LuaFile).DebugFile = new HavokDebugFile();
            }
            
            var debugInfo = new FunctionDebugInfo();
            debugInfo.Id = LuaFile.FunctionIdCounter++;
            // Functionpos 238 is the root lua function
            if (FunctionPos != 238)
            {
                debugInfo.FunctionStart = Reader.ReadInt32();
                debugInfo.FunctionEnd = Reader.ReadInt32();
            }
            Reader.ReadInt64();
            if (FunctionPos == 238)
            {
                var fileNameLength = Reader.ReadInt64();
                debugInfo.Filename = Encoding.ASCII.GetString(Reader.ReadBytes((int) (fileNameLength - 1)));
                Reader.ReadByte();
            }
            var chunkNameLength = Reader.ReadInt64();
            if (chunkNameLength > 0)
            {
                debugInfo.ChunkName = Encoding.ASCII.GetString(Reader.ReadBytes((int) (chunkNameLength - 1)));
                Reader.ReadByte();
            }
            
            var instructionCount = Reader.ReadInt64();
            for (int i = 0; i < instructionCount; i++)
            {
                debugInfo.InstructionLocations.Add(Reader.ReadInt32());
            }

            var constantCount = Reader.ReadInt32();
            for (int i = 0; i < constantCount; i++)
            {
                var debugConstant = new Local();
                var stringLength = Reader.ReadInt64();
                debugConstant.Name = Encoding.ASCII.GetString(Reader.ReadBytes((int) (stringLength - 1)));
                Reader.ReadByte();
                debugConstant.Start = Reader.ReadInt32();
                debugConstant.End = Reader.ReadInt32();
                debugInfo.VariableNames.Add(debugConstant);
            }

            if (FunctionPos != 238)
            {
                var upvalueNameCount = Reader.ReadInt32();
                for (int i = 0; i < upvalueNameCount; i++)
                {
                    var upvalueStrLength = Reader.ReadInt64();
                    string upvalueName = Encoding.ASCII.GetString(Reader.ReadBytes((int) (upvalueStrLength - 1)));
                    Reader.ReadByte();
                    debugInfo.UpvalueNames.Add(upvalueName);
                }
            }
            else
            {
                Reader.ReadInt32();
            }
            
            ((HavokLuaFile) LuaFile).DebugFile.DebugInfo.Add(debugInfo);
            
            return new FunctionFooter
            {
                SubFunctionCount = Reader.ReadInt32(),
            };
        }
        
        protected override List<ILuaFunction> ReadChildFunctions()
        {
            var childFunctions = new List<ILuaFunction>();

            for (var i = 0; i < Footer.SubFunctionCount; i++)
            {
                childFunctions.Add(new HavokLuaFunctionIW(LuaFile, Reader));
            }

            return childFunctions;
        }
    }
}