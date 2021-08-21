using System.Collections.Generic;
using System.Linq;

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
           statement      → exprStmt | 
                          | printStmt 
                          | block
                          | ifStmt
                          | whileStmt
                          | forStmt
                          | breakStmt;
           exprStmt       → expression ";" ;
           printStmt      → "print" expression ";" ; 
           ifStmt         → "if" "("expression ")" statement ("else" statement)?;
           whileStmt      → "while" "(" expression ")" statement;
           forStmt        → "for "(" ( varDecl | exprStmt) (expression)? ";" (expression)? ")" statement;
           breakStatement → "break"";" 
           block          → "{" declaration* "}"; 
           expression     → comma;
           comma          → assignment (, assignment)*;
           assignment     → IDENTIFIER "=" assignment | ternary;
           ternary        → logic_or | (ternary "?" ternary ":" ternary)
           logic_or       → logic_and (or logic_and )*;
           logic_and      → equality (and equality)*:
           equality       → comparison ( ( "!=" | "==" ) comparison )* ;
           comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
           term           → factor ( ( "-" | "+" ) factor )* ;
           factor         → unary ( ( "/" | "*" ) unary )* ;
           unary          → ( "!" | "-" ) unary
                          | primary ;
           primary        →   "true" | "false" | "nil"
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
            if (Match(TokenType.IF))
            {
                return IfStatement();
            }
            if (Match(TokenType.WHILE))
            {
                return WhileStatement();
            }
            if (Match(TokenType.FOR))
            {
                return ForStatement();
            }
            if (Match(TokenType.LEFT_BRACE))
            {
                return Block();
            }
            if (Match(TokenType.BREAK))
            {
                return BreakStatement();
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

        //  ifStmt         → "if" "("expression ")" statement ("else" statement)?;

        private Stmt IfStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expected '(' after 'if'");
            var condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expected ')' after 'if' condition");
            var thenBranch = Statement();
            Stmt elseBranch = null;
            if (Match(TokenType.ELSE))
            {
                elseBranch = Statement();
            }
            return new Stmt.If(condition, thenBranch, elseBranch);
        }
        
        //  whileStmt         → "while" "("expression ")" statement;
        private Stmt WhileStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expected '(' after 'while'");
            var condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expected ')' after 'while' condition");
            var body = Statement();
            return new Stmt.While(condition, body);
        }
        
        // forStmt        → "for "(" ( varDecl | exprStmt) (expression)? ";" (expression)? ")" statement ;
        private Stmt ForStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expected '(' after 'for'");
            Stmt initializer;
            if (Match(TokenType.SEMICOLON))
            {
                initializer = null;
            }
            else if (Match(TokenType.VAR))
            {
                initializer = VarDeclaration();
            }
            else
            {
                initializer = ExpressionStatement();
            }
            Expr condition = null;
            if (!Match(TokenType.SEMICOLON))
            {
                condition = Expression();
            }

            Expr increment = null;
            if(!Check(TokenType.RIGHT_PAREN))
            {
                increment = Expression();
            }
            Consume(TokenType.RIGHT_PAREN, "Expected ')' after 'for' expression");
            var body = Statement();
            if (increment != null)
            {
                body = new Stmt.Block(new List<Stmt>(){body, new Stmt.Expression(increment)});
            }
            if (condition == null)
            {
                condition = new Expr.Literal(true);
            }
            body = new Stmt.While(condition, body);
            if (initializer != null)
            {
                body = new Stmt.Block(new List<Stmt>(){initializer, body});
            }
            return body;
        }

        private Stmt BreakStatement()
        {
            Consume(TokenType.SEMICOLON, "Expected ';' after 'break';");
            return new Stmt.Break();   
        }
        
        // exprStmt       → expression ";" ;
        private Stmt ExpressionStatement()
        {
            var expr = Expression();
            if (Match(TokenType.SEMICOLON))
            {
                return new Stmt.Expression(expr);
            }
            return new Stmt.Print(expr);
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

        // comma          → assignment (, assignment)* ;
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
            var expr = LogicOr();
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

        // logic_or       → logic_and (or logic_and )*;
        private Expr LogicOr()
        {
            var expr = LogicAnd();
            while (Match(TokenType.OR))
            {
                expr = new Expr.Logic(expr, Previous(), LogicAnd());
            }
            return expr;
        }

        // logic_and      → equality (and equality)*:
        private Expr LogicAnd()
        {
            var expr = Equality();
            while (Match(TokenType.AND))
            {
                expr = new Expr.Logic(expr, Previous(), Equality());
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