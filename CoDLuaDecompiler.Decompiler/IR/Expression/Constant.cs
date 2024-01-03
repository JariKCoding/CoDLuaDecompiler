using CoDLuaDecompiler.Decompiler.IR.Identifiers;
using System.Text;
using System.Text.RegularExpressions;

namespace CoDLuaDecompiler.Decompiler.IR.Expression
{
    public class Constant : IExpression
    {
        public Identifiers.ValueType Type { get; set; }
        public double Number { get; set; }
        public string String { get; set; }
        public bool Boolean { get; set; }
        public ulong Hash { get; set; }

        public int Id { get; set; }
        
        public Constant(double num, int id)
        {
            Type = Identifiers.ValueType.Number;
            Number = num;
            Id = id;
        }

        public Constant(string str, int id)
        {
            StringBuilder constStringBuilder = new StringBuilder();
            Type = Identifiers.ValueType.String;
            for (int i = 0; i != str.Length; i++)
            {
                constStringBuilder.Append(str[i] switch
                {
                    '\0' => "\\0",
                    '\a' => "\\a",
                    '\t' => "\\t",
                    '\n' => "\\n",
                    '\v' => "\\v",
                    '\r' => "\\r",
                    '\\' => "\\\\",
                    '"' => "\\\"",
                    _ => str[i],
                });
            }
            String = constStringBuilder.ToString();
            Id = id;
        }

        public Constant(bool b, int id)
        {
            Type = Identifiers.ValueType.Boolean;
            Boolean = b;
            Id = id;
        }

        public Constant(ulong h, int id)
        {
            Type = Identifiers.ValueType.Hash;
            Hash = h;
            Id = id;
        }

        public Constant(Identifiers.ValueType typ, int id)
        {
            Type = typ;
            Id = id;
        }

        /// <summary>
        /// Checks whether this constant uses characters that are only allowed inside double brackets
        /// </summary>
        /// <returns></returns>
        public bool StringHasIllegalCharacter()
        {
            return Type != Identifiers.ValueType.String || String == null || String == "" && String.Contains("<") || String.Contains(">") || String.Contains("\"") || 
                   String.Contains(".") || String.Contains(" ") || Regex.IsMatch(String, @"^\d+") || String == "return";
        }
        
        public override string ToString()
        {
            switch (Type)
            {
                case Identifiers.ValueType.Number:
                    return Number.ToString();
                case Identifiers.ValueType.String:
                    return "\"" + String + "\"";
                case Identifiers.ValueType.Boolean:
                    return Boolean ? "true" : "false";
                case Identifiers.ValueType.Hash:
                    return $"0x{Hash/* & 0xFFFFFFFFFFFFFFF*/:X016}";
                case Identifiers.ValueType.Table:
                    return "{}";
                case Identifiers.ValueType.VarArgs:
                    return "...";
                case Identifiers.ValueType.Nil:
                    return "nil";
            }
            return "";
        }
        
        public override int GetLowestConstantId()
        {
            return Id;
        }
    }
}