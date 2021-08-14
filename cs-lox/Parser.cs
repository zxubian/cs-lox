using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace cslox
{
    public class Parser
    {
        private readonly IReadOnlyList<Token> tokens;

        private int current = 0;

        public Parser(IReadOnlyList<Token> tokens)
        {
            this.tokens = tokens;
        }
        /*
           expression     → equality ;
           equality       → comparison ( ( "!=" | "==" ) comparison )* ;
           comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
           term           → factor ( ( "-" | "+" ) factor )* ;
           factor         → unary ( ( "/" | "*" ) unary )* ;
           unary          → ( "!" | "-" ) unary
                          | primary ;
           primary        → NUMBER | STRING | "true" | "false" | "nil"
                          | "(" expression ")" ; 
         */

        public Expr Parse()
        {
            try
            {
                return Expression();
            }
            catch (ParseError)
            {
                return null;
            }
        }

        // expression     → equality ;
        private Expr Expression() => Equality();

        // equality       → comparison ( ( "!=" | "==" ) comparison )* ;
        private Expr Equality()
        {
            // left
            var expr = Comparison();
            while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                var operatorToken = Previous();
                var right = Comparison();
                expr = new Expr.Binary(expr, operatorToken, right);
            }
            return expr;
        }

       //comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
        private Expr Comparison()
        {
            var term = Term();
            while (Match(TokenType.LESS, TokenType.LESS_EQUAL, TokenType.GREATER, TokenType.GREATER_EQUAL))
            {
                var operatorToken = Previous();
                var right = Term();
                term = new Expr.Binary(term, operatorToken, right);
            }
            return term;
        }

        // term           → factor ( ( "-" | "+" ) factor )* ;
        private Expr Term()
        {
            var factor = Factor();
            while (Match(TokenType.MINUS, TokenType.PLUS))
            {
                var operatorToken = Previous();
                var right = Factor();
                factor = new Expr.Binary(factor, operatorToken, right);
            }
            return factor;
        }

        // factor         → unary ( ( "/" | "*" ) unary )* ;
        private Expr Factor()
        {
            var unary = Unary();
            while (Match(TokenType.SLASH, TokenType.STAR))
            {
                var operatorToken = Previous();
                var right = Unary();
                unary = new Expr.Binary(unary, operatorToken, right);
            }
            return unary;
        }

        //unary          → ( "!" | "-" ) unary | primary ;
        private Expr Unary()
        {
            if (Match(TokenType.BANG, TokenType.MINUS))
            {
                var operatorToken = Previous();
                var right = Unary();
                return new Expr.Unary(operatorToken, right);
            }
            return Primary();
        }

        //primary        → NUMBER | STRING | "true" | "false" | "nil" | "(" expression ")" ; 
        private Expr Primary()
        {
            if (Match(TokenType.FALSE))
            {
                return new Expr.Literal(false);
            }
            if (Match(TokenType.TRUE))
            {
                return new Expr.Literal(true);
            }
            if (Match(TokenType.NIL))
            {
                return new Expr.Literal(null);
            }
            if (Match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Expr.Literal(Previous().literal);
            }
            if (Match(TokenType.LEFT_PAREN))
            {
                var expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expect '\\' after expression.");
                return new Expr.Grouping(expr);
            }
            throw Error(Peek(), "Expect expression.");
        }

        private void Synchronize()
        {
            Advance();
            while (!IsAtEnd())
            {
                if (Previous().type == TokenType.SEMICOLON)
                {
                    return;
                }
                switch (Peek().type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                    case TokenType.VAR:
                    case TokenType.WHILE:
                        return;
                }
                Advance();
            }
        }
        
        
        private bool Match(params TokenType[] types)
        {
            if (types.Any(Check))
            {
                Advance();
                return true;
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

        private Token Consume(TokenType type, string errorMessage)
        {
            if (Check(type))
            {
                return Advance();
            }
            throw Error(Peek(), errorMessage);
        }

        private ParseError Error(Token token, string message)
        {
            jlox.Error(token, message);
            return new ParseError();
        }
    }
}