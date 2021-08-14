using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace cslox
{
    public class Interpreter: Expr.IVisitor<object>
    {
        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value;
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.expression);
        }
        
        public object VisitUnaryExpr(Expr.Unary expr)
        {
            var right = Evaluate((expr.right));
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

        public void Interpret(Expr expression)
        {
            try
            {
                object value = Evaluate(expression);
                Console.WriteLine(value);
            }
            catch (RuntimeError e)
            {
                jlox.RuntimeError(e);
            }
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
                return !rightBool;
            }
            return true;
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            var left = Evaluate(expr.left);
            var right = Evaluate(expr.right);
            switch (expr.operatorToken.type)
            {
                case TokenType.PLUS:
                    switch (left)
                    {
                        case string leftString when right is string rightString:
                            return leftString + rightString;
                        case double leftDouble when right is double rightDouble:
                            return leftDouble + rightDouble;
                        default:
                            throw new RuntimeError(expr.operatorToken, "Operands must be two numbers or two strings");
                    }
                case TokenType.MINUS:
                    CheckNumberOperands(expr.operatorToken, left, right);
                    return (double)left - (double)right;
                case TokenType.SLASH:
                    CheckNumberOperands(expr.operatorToken, left, right);
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
                    CheckNumberOperands(expr.operatorToken, left, right);
                    return left.Equals(right);
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
    }
    
}