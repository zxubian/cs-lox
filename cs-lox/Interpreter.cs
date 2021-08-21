using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using cslox.UtilityTypes;
using Environment = clox.Environment;

namespace cslox
{
    public class Interpreter: Expr.IVisitor<object>, Stmt.IVisitor<Unit>
    {
        private Environment environment = new Environment();
        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value;
        }

        public object VisitLogicExpr(Expr.Logic expr)
        {
            switch (expr.operatorToken.type)
            {
                case TokenType.OR:
                    return IsTruthy(Evaluate(expr.left)) || IsTruthy(Evaluate(expr.right));
                case TokenType.AND:
                    return IsTruthy(Evaluate(expr.left)) && IsTruthy(Evaluate(expr.right));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.expression);
        }
        
        public object VisitUnaryExpr(Expr.Unary expr)
        {
            var right = Evaluate(expr.right);
            switch (expr.operatorToken.type)
            {
                case TokenType.MINUS:
                    CheckNumberOperand(expr.operatorToken, right);
                    return -(double) right;
                case TokenType.BANG:
                    return !IsTruthy(right);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object VisitTernaryExpr(Expr.Ternary expr)
        {
            switch (expr.firstOperator.type)
            {
                case TokenType.QUESTION when expr.secondOperator.type == TokenType.COLON:
                    var left = Evaluate(expr.left);
                    return IsTruthy(left) ? Evaluate(expr.mid) : Evaluate(expr.right);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // variable evaluation
        public object VisitVariableExpr(Expr.Variable expr)
        {
            return environment.Get(expr.name);
        }

        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeError e)
            {
                jlox.RuntimeError(e);
            }
        }

        private void Execute(Stmt statement)
        {
            statement.Accept(this);
        }
        
        private static string Stringify(object obj)
        {
            switch (obj)
            {
                case null:
                    return "nil";
                case double objDouble:
                {
                    var text = objDouble.ToString();
                    if (text.EndsWith(".0"))
                    {
                        text = text.Substring(0, text.Length - 2);
                    }
                    return text;
                }
                default:
                    return obj.ToString();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object Evaluate(Expr expr) => expr.Accept(this);

        private static bool IsTruthy(object o)
        {
            if (o != null && o is bool rightBool)
            {
                return rightBool;
            }
            if (o == null)
            {
                return false;
            }
            return true;
        }

        // variable assignmnent
        public object VisitAssignExpr(Expr.Assign expr)
        {
            var value = Evaluate(expr.value);
            environment.Assign(expr.name, value);
            return value;
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            var left = Evaluate(expr.left);
            var right = Evaluate(expr.right);
            switch (expr.operatorToken.type)
            {
                case TokenType.PLUS:
                {
                    switch (left)
                    {
                        case string leftString when right is string rightString:
                            return leftString + rightString;
                        case double leftDouble when right is double rightDouble:
                            return leftDouble + rightDouble;
                        default:
                            throw new RuntimeError(expr.operatorToken, "Operands must be two numbers or two strings");
                    }
                }
                case TokenType.MINUS:
                    CheckNumberOperands(expr.operatorToken, left, right);
                    return (double)left - (double)right;
                case TokenType.SLASH:
                    CheckNumberOperands(expr.operatorToken, left, right);
                    CheckNotZero(expr.operatorToken, right);
                    return (double)left / (double)right;
                case TokenType.STAR:
                    CheckNumberOperands(expr.operatorToken, left, right);
                    return (double)left * (double)right;
                case TokenType.GREATER:
                    CheckNumberOperands(expr.operatorToken, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperands(expr.operatorToken, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    CheckNumberOperands(expr.operatorToken, left, right);
                    return (double)left <(double)right;
                case TokenType.LESS_EQUAL:
                    CheckNumberOperands(expr.operatorToken, left, right);
                    return (double)left <= (double)right;
                case TokenType.EQUAL_EQUAL:
                    if (left is double leftDouble1 && right is double rightDouble1)
                    {
                        return Math.Abs(leftDouble1 - rightDouble1) < Double.Epsilon;
                    }
                    if (left == null || right == null)
                    {
                        return left == right;
                    }
                    throw new RuntimeError(expr.operatorToken, "Operands must both be numbers, or at least one should be nil.");
                case TokenType.COMMA:
                    return right;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        
        private static void CheckNumberOperand(Token operatorToken, object operand)
        {
            if (operand is double)
            {
                return;
            }
            throw new RuntimeError(operatorToken, "Operand must be a number");
        }
        private static void CheckNumberOperands(Token operatorToken, object operandLeft, object operandRight)
        {
            if (operandLeft is double && operandRight is double)
            {
                return;
            }
            throw new RuntimeError(operatorToken, "Operand must be a number");
        }
        
        private static void CheckNotZero(Token operatorToken, params object[] operands)
        {
            if (operands.All(operand => Math.Abs((double)operand) > double.Epsilon))
            {
                return;
            }
            var message = operatorToken.type == TokenType.SLASH ? "Division by zero" : "Operand cannot be zero";
            throw new RuntimeError(operatorToken, message);
        }

        public Unit VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.expression);
            return Unit.Default;
        }

        public Unit VisitPrintStmt(Stmt.Print stmt)
        {
            Console.WriteLine(Stringify(Evaluate(stmt.expression)));
            return Unit.Default;
        }

        // variable declaration
        public Unit VisitVarStmt(Stmt.Var stmt)
        {
            if (stmt.initializer != null)
            {
                var value = Evaluate(stmt.initializer);
                environment.Define(stmt.name, value);
            }
            else
            {
                environment.Define(stmt.name);
            }
            return Unit.Default;
        }

        public Unit VisitBlockStmt(Stmt.Block stmt)
        {
            environment = new Environment(environment);
            try
            {
                foreach (var statement in stmt.statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                environment = environment.Enclosing;
            }
            return Unit.Default;
        }

        public Unit VisitIfStmt(Stmt.If stmt)
        {
            if (IsTruthy(stmt.condition))
            {
                Execute(stmt.thenBranch);
            }
            else if(stmt.elseBranch != null)
            {
                Execute(stmt.elseBranch);
            }
            return Unit.Default;
        }

        public Unit VisitWhileStmt(Stmt.While stmt)
        {
            while (IsTruthy(Evaluate(stmt.condition)))
            {
                try
                {
                    Execute(stmt.body);
                }
                catch (BreakException)
                {
                    break;
                }
            }
            return Unit.Default;
        }

        public Unit VisitBreakStmt(Stmt.Break stmt)
        {
            throw new BreakException();
        }
    }
    
}