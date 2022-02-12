using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CoDLuaDecompiler.Decompiler.LuaFile.Havok.Debug;
using CoDLuaDecompiler.Decompiler.LuaFile.Structures.LuaFunction.Structures;

namespace CoDLuaDecompiler.Decompiler.LuaFile.Havok
{
    public class HavokDebugFile
    {
        public List<FunctionDebugInfo> DebugInfo { get; set; } = new List<FunctionDebugInfo>();
        public BinaryReader Reader { get; set; }

        public HavokDebugFile(BinaryReader reader)
        {
            Reader = reader;

            ReadDebugFile();
        }
        
        public HavokDebugFile()
        {
        }

        private void ReadDebugFile()
        {
            ReadMainFunction();

            int id = 1;
            while (Reader.BaseStream.Position != Reader.BaseStream.Length)
            {
                var func = ReadFunction();
                func.Id = id++;
                DebugInfo.Add(func);
            }
        }

        private void ReadMainFunction()
        {
            var debugInfo = new FunctionDebugInfo();
            debugInfo.Id = 0;
            Reader.ReadInt32();
            int instructionCount = Reader.ReadInt32();
            Int64 constantCount = Reader.ReadInt64();
            // unk null
            Reader.ReadInt64();
            Int64 fileNameLength = Reader.ReadInt64();
            debugInfo.Filename = Encoding.ASCII.GetString(Reader.ReadBytes((int) (fileNameLength - 1)));
            Reader.ReadByte();
            Int64 chunkNameLength = Reader.ReadInt64();
            debugInfo.ChunkName = Encoding.ASCII.GetString(Reader.ReadBytes((int) (chunkNameLength - 1)));
            Reader.ReadByte();

            for (int i = 0; i < instructionCount; i++)
            {
                debugInfo.InstructionLocations.Add(Reader.ReadInt32());
            }
            
            for (int i = 0; i < constantCount; i++)
            {
                debugInfo.VariableNames.Add(ReadVariableData());
            }

            debugInfo.VariableNames = debugInfo.VariableNames.Where(v => !v.Name.StartsWith("(")).ToList();
            
            DebugInfo.Add(debugInfo);
        }

        private FunctionDebugInfo ReadFunction()
        {
            var debugInfo = new FunctionDebugInfo();
            // Is always 1
            int funcStart = Reader.ReadInt32();
            int instructionCount = Reader.ReadInt32();
            int variableNameCount = Reader.ReadInt32();
            int upvalueNameCount = Reader.ReadInt32();
            debugInfo.FunctionStart = Reader.ReadInt32();
            debugInfo.FunctionEnd = Reader.ReadInt32();
            // Unk null
            Int64 nullPtr = Reader.ReadInt64();
            Int64 funcNameLength = Reader.ReadInt64();

            if (funcNameLength > 0)
            {
                string funcName = Encoding.ASCII.GetString(Reader.ReadBytes((int) (funcNameLength - 1)));
                Reader.ReadByte();
                debugInfo.ChunkName = funcName;
            }
            
            for (int i = 0; i < instructionCount; i++)
            {
                debugInfo.InstructionLocations.Add(Reader.ReadInt32());
            }
            
            for (int i = 0; i < variableNameCount; i++)
            {
                debugInfo.VariableNames.Add(ReadVariableData());
            }

            debugInfo.VariableNames = debugInfo.VariableNames.Where(v => !v.Name.StartsWith("(")).ToList();
            
            for (int i = 0; i < upvalueNameCount; i++)
            {
                Int64 upvalueStrLength = Reader.ReadInt64();
                string funcName = Encoding.ASCII.GetString(Reader.ReadBytes((int) (upvalueStrLength - 1)));
                Reader.ReadByte();
                debugInfo.UpvalueNames.Add(funcName);
            }

            return debugInfo;
        }

        private Local ReadVariableData()
        {
            var debugConstant = new Local();
            Int64 stringLength = Reader.ReadInt64();
            debugConstant.Name = Encoding.ASCII.GetString(Reader.ReadBytes((int) (stringLength - 1)));
            Reader.ReadByte();
            debugConstant.Start = Reader.ReadInt32();
            debugConstant.End = Reader.ReadInt32();
            return debugConstant;
        }
    }
}