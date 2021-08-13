using System;
using System.Collections.Generic;

namespace jlox
{
    public class Parser
    {
        private readonly IReadOnlyList<Token> tokens;

        private int current = 0;

        public Parser(IReadOnlyList<Token> tokens)
        {
            this.tokens = tokens;
        }

        private Expr Expression() => Equality();
        
        /*
           expression     → literal
                          | unary
                          | binary
                          | grouping ;
           
           literal        → NUMBER | STRING | "true" | "false" | "nil" ;
           grouping       → "(" expression ")" ;
           unary          → ( "-" | "!" ) expression ;
           binary         → expression operator expression ;
           operator       → "==" | "!=" | "<" | "<=" | ">" | ">="
                          | "+"  | "-"  | "*" | "/" ; 
        */

        
        private Expr Equality()
        {
            // left
            var expr = Comparison();
            while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                var tokenOperator = Previous();
                var right = Comparison();
                expr = new Expr.Binary(expr, tokenOperator, right);
            }
            return expr;
        }

        private bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd())
            {
                return false;
            }
            return Peek().type == type;
        }

        private Token Advance()
        {
            if (!IsAtEnd())
            {
                current++;
            }
            return Previous();
        }

        private bool IsAtEnd() => Peek().type == TokenType.EOF;
        private Token Peek() => tokens[current];
        private Token Previous() => tokens[current-1];


        private Expr Comparison()
        {
            
        }
    }
}