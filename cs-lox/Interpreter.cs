using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using cslox.UtilityTypes;
using jlox;
using Environment = clox.Environment;

namespace cslox
{
    public class Interpreter: Expr.IVisitor<object>, Stmt.IVisitor<Unit>
    {
        private readonly Environment globals = new Environment();
        
        /// <summary>
        /// Maps each expression that uses a variable that contains:
        /// 1) its scope depth
        ///     i.e. the number of environments between the scope where the variable is used
        ///     and the one where it was initialized.
        /// 2) the index of the variable in the variable array in the scope
        /// </summary>
        private readonly Dictionary<Expr, (int depth, int index)> locals = new Dictionary<Expr, (int depth, int index)>();
        private readonly Dictionary<string, int> globalIndicies = new Dictionary<string, int>();
        
        private Environment environment;

        public Interpreter()
        {
            environment = globals;
            globals.Define("clock", Clock.Impl);
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
                cslox.RuntimeError(e);
            }
        }

        public object ExecuteBlock(List<Stmt> statements, Environment environment)
        {
            var previousEnv = this.environment;
            try
            {
                this.environment = environment;
                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                this.environment = previousEnv;
            }
            return null;
        }

        public void Resolve(Expr expr, int depth, int index)
        {
            locals[expr] = (depth, index);
        }
        
        #region Statement Visitor

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
        public Unit VisitVarDeclStmt(Stmt.VarDecl stmt)
        {
            int index;
            if (stmt.initializer != null)
            {
                var value = Evaluate(stmt.initializer);
                index = environment.Define(stmt.name, value);
            }
            else
            {
                index = environment.Define(stmt.name);
            }
            if (environment.Enclosing == null)
            {
                globalIndicies[stmt.name.lexeme] = index;
            }
            return Unit.Default;
        }

        public Unit VisitFunctionDeclStmt(Stmt.FunctionDecl stmt)
        {
            var function = new LoxFunction(stmt.name.lexeme, stmt.parameters, stmt.body, environment);
            environment.Define(stmt.name, function);
            return Unit.Default;
        }

        public Unit VisitBlockStmt(Stmt.Block stmt)
        {
            _ = ExecuteBlock(stmt.statements, new Environment(environment));
            return Unit.Default;
        }

        public Unit VisitIfStmt(Stmt.If stmt)
        {
            if (IsTruthy(Evaluate(stmt.condition)))
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
                catch (Break)
                {
                    break;
                }
            }
            return Unit.Default;
        }

        public Unit VisitBreakStmt(Stmt.Break stmt)
        {
            throw new Break();
        }

        public Unit VisitReturnStmt(Stmt.Return stmt)
        {
            object value = null;
            if (stmt.value != null)
            {
                value = Evaluate(stmt.value);
            }
            throw new Return(value);
        }

        #endregion //Statement Visitor

        #region Expression Visitor
        
        // variable assignment
        public object VisitAssignExpr(Expr.Assign expr)
        {
            var value = Evaluate(expr.value);
            if (locals.TryGetValue(expr, out var location))
            {
                environment.AssignAt(location.depth, location.index, expr.name, value);
            }
            else
            {
                var index = globalIndicies[expr.name.lexeme];
                globals.Assign(expr.name, index, value);   
            }
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
            return LookUpVariable(expr.name, expr);
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            var callee = Evaluate(expr.callee);
            var arguments = expr.arguments.Select(Evaluate);
            if (!(callee is ILoxCallable function))
            {
                throw new RuntimeError(expr.closingParen, $"{Stringify(callee)} is not a function or class");
            }
            if (expr.arguments.Count != function.Arity)
            {
                throw new RuntimeError(expr.closingParen,
                    $"Incorrect number of arguments for {Stringify(callee)}: expected {function.Arity}, but got {expr.arguments.Count}");
            }
            return function.Call(this, arguments);
        }

        public object VisitLambdaExpr(Expr.Lambda expr)
        {
            return new LoxFunction("lambda", expr.parameters, expr.body, environment);
        }

        #endregion // Expression Visitor
        
        #region Helpers
        
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


        private object LookUpVariable(Token name, Expr expr)
        {
            if (locals.TryGetValue(expr, out var position))
            {
                return environment.GetAt(name, position.depth, position.index);
            }
            if (!globalIndicies.TryGetValue(name.lexeme, out var index))
            {
                index = -1;
            }
            return globals.Get(name, index);
        }

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
        
        #endregion // Helpers
    }
    
}