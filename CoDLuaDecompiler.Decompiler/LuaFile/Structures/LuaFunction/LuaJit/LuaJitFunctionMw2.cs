using System;
using System.IO;
using System.Text;
using CoDLuaDecompiler.Decompiler.Extensions;
using CoDLuaDecompiler.Decompiler.LuaFile.LuaJit;
using CoDLuaDecompiler.Decompiler.LuaFile.Structures.LuaConstant.LuaJit;
using CoDLuaDecompiler.Decompiler.LuaFile.Structures.LuaOpCodeTable.LuaJit;

namespace CoDLuaDecompiler.Decompiler.LuaFile.Structures.LuaFunction.LuaJit
{
    public class LuaJitFunctionMw2 : LuaJitFunction
    {
        public override ILuaJitOpCodeTable OpCodeTable => new LuaJitOpCodeTableBOCW();
        public LuaJitFunctionMw2(ILuaFile luaFile, BinaryReader reader) : base(luaFile, reader)
        {
        }
        
        protected override ILuaJitConstant ReadComplexConstant()
        {
            var type = Reader.ReadULEB128();

            if (type >= 6)
            {
                var str = Encoding.UTF8.GetString(Reader.ReadBytes((int) (type - 6)));

                //Console.WriteLine("Complex string: " + str);

                return new LuaJitConstant(str);
            }

            if (type == 0)
            {
                var child = ((LuaJitFile)LuaFile).Functions.Pop();
                ChildFunctions.Add(child);
                return new LuaJitConstant((LuaJitFunction) child);
            }

            if (type == 1)
                // Parse table
                return new LuaJitConstant(ReadTable());
            if (type == 2) Console.WriteLine("TYPE 2");
            if (type == 3)
            {
                Console.WriteLine("{0:X}", Reader.BaseStream.Position);
                Console.WriteLine("TYPE 3");
                Console.WriteLine(Reader.ReadBytes(4).ToString());
            }
            if (type == 4)
            {
                var hi = Reader.ReadULEB128();
                var lo = Reader.ReadULEB128();
                var idfk = Reader.ReadByte(); // Seems like hash type? 2 for material, 0 for array index, engine #
                var hash = ((hi << 32) | lo); //& 0xFFFFFFFFFFFFFFF;
                if (Decompiler.HashEntries.ContainsKey(hash))
                    return new LuaJitConstant(Decompiler.HashEntries[hash]);
                return new LuaJitConstant(hash);
            }
            Console.WriteLine("unknown TYPE " + type);

            return null;
        }
        
        protected override ILuaJitConstant ReadTableItem()
        {
            var type = Reader.ReadULEB128();

            if (type > 6)
                return new LuaJitConstant(Encoding.UTF8.GetString(Reader.ReadBytes((int) (type - 6))));
            if (type == 5)
            {
                var hi = Reader.ReadULEB128();
                var lo = Reader.ReadULEB128();
                Reader.ReadByte();
                var hash = ((hi << 32) | lo);// & 0xFFFFFFFFFFFFFFF;
                if (Decompiler.HashEntries.ContainsKey(hash))
                    return new LuaJitConstant(Decompiler.HashEntries[hash]);
                return new LuaJitConstant(hash);
            }
            if (type == 3)
                return new LuaJitConstant(Reader.ReadLEB128());
            if (type == 4)
                return new LuaJitConstant(ReadNumber());
            if (type == 2)
                return new LuaJitConstant(true);
            if (type == 1)
                return new LuaJitConstant(false);
            return new LuaJitConstant();
        }
    }
}