using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace cslox
{
    public class Scanner
    {
        private readonly string source;
        private readonly int sourceLength;
        private readonly List<Token> tokens = new List<Token>();

        private int start = 0;
        private int current = 0;
        private int line = 1;

        private bool isAtEnd() => current == sourceLength;
        
        private static readonly IReadOnlyDictionary<char, TokenType> singleCharacterToTokenType = new Dictionary<char, TokenType>
        {
            {'(', TokenType.LEFT_PAREN},
            {')', TokenType.RIGHT_PAREN},
            {'{', TokenType.LEFT_BRACE},
            {'}', TokenType.RIGHT_BRACE},
            {',', TokenType.COMMA},
            {'.', TokenType.DOT},
            {'-', TokenType.MINUS},
            {'+', TokenType.PLUS},
            {';', TokenType.SEMICOLON},
            {'*', TokenType.STAR},
            {'?', TokenType.QUESTION},
            {':', TokenType.COLON}
        };
        
        private static readonly IReadOnlyDictionary<string, TokenType> keywords = new Dictionary<string, TokenType>()
        {
            {"and", TokenType.AND}, 
            {"class", TokenType.CLASS}, 
            {"else", TokenType.ELSE}, 
            {"false", TokenType.FALSE}, 
            {"for", TokenType.FOR}, 
            {"fun", TokenType.FUN}, 
            {"if", TokenType.IF}, 
            {"nil", TokenType.NIL}, 
            {"or", TokenType.OR}, 
            {"print", TokenType.PRINT}, 
            {"return", TokenType.RETURN}, 
            {"super", TokenType.SUPER}, 
            {"this", TokenType.THIS}, 
            {"true", TokenType.TRUE}, 
            {"var", TokenType.VAR}, 
            {"while", TokenType.WHILE}
        };

        private static readonly IReadOnlyList<TokenType> literalTypes = new[]
        {
            TokenType.NUMBER,
            TokenType.STRING
        };
        
        public Scanner(string source)
        {
            this.source = source;
            sourceLength = source.Length;
        }

        public List<Token> ScanTokens()
        {
            while (!isAtEnd())
            {
                start = current;
                var tokenType = ScanToken();
                if (tokenType == null)
                {
                    continue;
                }
                var lexeme = GetLexeme(source, start, current);
                if (literalTypes.Contains(tokenType.Value))
                {
                    var literal = GetLiteral(source, start, current, tokenType.Value);
                    tokens.Add(new Token(tokenType.Value, lexeme, literal, line));
                }
                else
                {
                    tokens.Add(new Token(tokenType.Value, lexeme, null, line));
                }
            }
            tokens.Add(new Token(TokenType.EOF, "", null, line));
            return tokens;
        }
        
        private TokenType? ScanToken()
        {
            var c = source[current];
            if (singleCharacterToTokenType.TryGetValue(c, out var type))
            {
                current++;
                return type;
            }
            bool Equals(int current, char expected)
            {
                var next = source[current];
                return next == expected;
            }
            bool String(ref int current)
            {
                while (!isAtEnd() && !Equals(current, '"'))
                {
                    if (Equals(current, '\n'))
                    {
                        line++;
                    }
                    current++;
                }
                if (!isAtEnd() && Equals(current, '"'))
                {
                    current++;
                    return true;
                }
                jlox.Error(line, "Unterminated string");
                return false;
            }
            void Number(ref int current)
            {
                while (!isAtEnd() && IsDigit(source[current]))
                {
                    current++;
                }
                if (!isAtEnd() && source[current] == '.' && IsDigit(source[current + 1]))
                {
                    current++;
                    while (IsDigit(source[current]))
                    {
                        current++;
                    }
                }
            }
            TokenType Identifier(int start, ref int current)
            {
                while (!isAtEnd() && IsAlphaNumeric(source[current]))
                {
                    current++;
                }
                var text = source.Substring(start, current - start);
                return 
                    keywords.TryGetValue(text, out var value) ? 
                    value : 
                    TokenType.IDENTIFIER;
            }
            current++;
            switch (c)
            {
                case '!':
                    if (Equals(current, '='))
                    {
                        current++;
                        return TokenType.BANG_EQUAL;
                    }
                    else
                    {
                        return TokenType.BANG;
                    }
                case '=':
                    if (Equals(current, '='))
                    {
                        current++;
                        return TokenType.EQUAL_EQUAL;
                    }
                    else
                    {
                        return TokenType.EQUAL;
                    }
                case '<':
                    if (Equals(current, '='))
                    {
                        current++;
                        return TokenType.LESS_EQUAL;
                    }
                    else
                    {
                        return TokenType.LESS;
                    }
                case '>':
                {
                    if (Equals(current, '='))
                    {
                        current++;
                        return TokenType.GREATER_EQUAL;
                    }
                    else
                    {
                        return TokenType.GREATER;
                    }
                }
                case '/':
                    if (Equals(current, '/'))
                    {
                        // A comment goes until the end of the line.
                        do
                        {
                            current++;
                        } while (!isAtEnd() && !Equals(current, '\n'));
                    }
                    else if (Equals(current, '*'))
                    {
                        var foundCommentEnd= false;
                        while (!foundCommentEnd)
                        {
                            do
                            {
                                if (Equals(current, '\n'))
                                {
                                    line++;
                                }
                                current++;
                            } while (!isAtEnd() && !Equals(current, '*'));
                            current++;
                            foundCommentEnd = Equals(current, '/');
                            current++;
                        }
                    }
                    else
                    {
                        return TokenType.SLASH;
                    }
                    break;
                case ' ':
                case '\r':
                case '\t':
                    break;
                case '\n':
                    line++;
                    break;
                case '"':
                    if (String(ref current))
                    {
                        return TokenType.STRING;
                    }
                    else
                    {
                        return null;
                    }
                default:
                    if (IsDigit(c))
                    {
                        Number(ref current);
                        return TokenType.NUMBER;
                    }
                    else if (IsAlpha(c))
                    {
                        return Identifier(start, ref current);
                    }
                    else
                    {
                        jlox.Error(line, $"Unexpected Character: {c}");
                        break;
                    }
            }
            return null;
        }
        
        private static string GetLexeme(string source, int start, int current) =>
            source.Substring(start, current - start);

        private static object GetLiteral(string source, int start, int current, TokenType type)
        {
            switch (type)
            {
                case TokenType.NUMBER:
                    return double.Parse(source.Substring(start, current - start));
                case TokenType.STRING:
                    return source.Substring(++start, --current - start);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private static bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                   (c == '_');
        }

        private static bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }
    }
}