using System;

namespace cslox
{
    public class Token
    {
        public readonly TokenType type;
        public readonly string lexeme;
        public Object literal;
        public int line;

        public Token(TokenType type, string lexeme, object literal, int line)
        {
            this.type = type;
            this.lexeme = lexeme;
            this.literal = literal;
            this.line = line;
        }

        public override string ToString()
        {
            return $"{nameof(type)}: {type}, {nameof(lexeme)}: {lexeme}, {nameof(literal)}: {literal}";
        }
    }
}