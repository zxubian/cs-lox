using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
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
           program        → declaration* EOF ;
           declaration    → varDecl | statement ;
           varDecl        → "var" IDENTIFIER ( "=" expression )? ";" ; 
           statement      → exprStmt | printStmt ;
           statement      → exprStmt | printStmt | block;
           exprStmt       → expression ";" ;
           printStmt      → "print" expression ";" ; 
           block          → "{" declaration* "}"; 
           expression     → comma;
           comma          → assignment (, assignment)*;
           assignment     → IDENTIFIER "=" assignment | ternary;
           ternary        → equality | (ternary "?" ternary ":" ternary)
           equality       → comparison ( ( "!=" | "==" ) comparison )* ;
           comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
           term           → factor ( ( "-" | "+" ) factor )* ;
           factor         → unary ( ( "/" | "*" ) unary )* ;
           unary          → ( "!" | "-" ) unary
                          | primary ;
           primary        →  | "true" | "false" | "nil"
                             | NUMBER | STRING 
                             | "(" expression ")" 
                             | IDENTIFIER ; 
         */

        public List<Stmt> Parse()
        {
            var statements = new List<Stmt>();
            while (!IsAtEnd())
            {
                statements.Add(Declaration());
            }
            return statements;
        }

        // declaration    → varDecl | statement ;
        private Stmt Declaration()
        {
            try
            {
                if (Match(TokenType.VAR))
                {
                    return VarDeclaration();
                }

                return Statement();
            }
            catch (ParseError)
            {
                Synchronize();
                return null;
            }
        }
        
        // varDecl        → "var" IDENTIFIER ( "=" expression )? ";" ; 
        private Stmt VarDeclaration()
        {
            var identifier = Consume(TokenType.IDENTIFIER, "Variable name expected");
            Expr initializer = null;
            if (Match(TokenType.EQUAL))
            {
                initializer = Expression();
            }
            Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
            return new Stmt.Var(identifier, initializer);
        }
        
        // statement      → exprStmt | printStmt ;
        private Stmt Statement()
        {
            if (Match(TokenType.PRINT))
            {
                return PrintStatement();
            }

            if (Match(TokenType.LEFT_BRACE))
            {
                return Block();
            }
            return ExpressionStatement();
        }
        
        // printStmt      → "print" expression ";" ; 
        private Stmt PrintStatement()
        {
            var expression = Expression();
            Consume(TokenType.SEMICOLON, "; expected after statement");
            return new Stmt.Print(expression);
        }

        // exprStmt       → expression ";" ;
        private Stmt ExpressionStatement()
        {
            var expr = Expression();
            Consume(TokenType.SEMICOLON, "; expected after statement");
            return new Stmt.Expression(expr);
        }

        private Stmt Block()
        {
            var statements = new List<Stmt>();
            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                statements.Add(Declaration());
            }
            Consume(TokenType.RIGHT_BRACE, "Expect '}' after block");
            return new Stmt.Block(statements);
        }

        // expression     → comma;
        private Expr Expression() => Comma();

        // comma          → ternary (, ternary)* ;
        private Expr Comma()
        {
            var expr = Assignment();
            while (Match(TokenType.COMMA))
            {
                var operatorToken = Previous();
                var right = Assignment();
                expr = new Expr.Binary(expr, operatorToken, right);
            }
            return expr;
        }

        // assignment     → IDENTIFIER "=" assignment | ternary;
        private Expr Assignment()
        {
            var expr = Ternary();
            if (Match(TokenType.EQUAL))
            {
                var equals = Previous();
                if (expr is Expr.Variable variable)
                {
                    var name = variable.name;
                    return new Expr.Assign(name, Assignment());
                }
                Error(equals, "Invalid assignment target.");
            }
            return expr;
        }

        // ternary        → equality | (ternary ? ternary : ternary)
        private Expr Ternary()
        {
            var expr = Equality();
            if (Match(TokenType.QUESTION))
            {
                var question = Previous();
                var mid = Ternary();
                if (!Match(TokenType.COLON))
                {
                    throw Error(Peek(), "Expect ':' for ternary conditional.");
                }
                var colon = Previous();
                var right = Ternary();
                expr = new Expr.Ternary(expr, question, mid, colon, right);
            }
            return expr;
        }

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
            var expr = Term();
            while (Match(TokenType.LESS, TokenType.LESS_EQUAL, TokenType.GREATER, TokenType.GREATER_EQUAL))
            {
                var operatorToken = Previous();
                var right = Term();
                expr = new Expr.Binary(expr, operatorToken, right);
            }
            return expr;
        }

        // term           → factor ( ( "-" | "+" ) factor )* ;
        private Expr Term()
        {
            var expr = Factor();
            while (Match(TokenType.MINUS, TokenType.PLUS))
            {
                var operatorToken = Previous();
                var right = Factor();
                expr = new Expr.Binary(expr, operatorToken, right);
            }
            return expr;
        }

        // factor         → unary ( ( "/" | "*" ) unary )* ;
        private Expr Factor()
        {
            var expr = Unary();
            while (Match(TokenType.SLASH, TokenType.STAR))
            {
                var operatorToken = Previous();
                var right = Unary();
                expr = new Expr.Binary(expr, operatorToken, right);
            }
            return expr;
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
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Expr.Grouping(expr);
            }
            if (Match(TokenType.IDENTIFIER))
            {
                return new Expr.Variable(Previous());
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