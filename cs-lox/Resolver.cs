
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Schema;
using cslox.UtilityTypes;

namespace cslox
{
    public class Resolver : Expr.IVisitor<Unit>, Stmt.IVisitor<Unit>
    {
        private readonly Interpreter interpreter;
        private readonly Stack<Dictionary<string, bool>> scopes = new Stack<Dictionary<string, bool>>();

        public Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }
        
        public Unit VisitBlockStmt(Stmt.Block stmt)
        {
            BeginScope();
            Resolve(stmt.statements);
            EndScope();
            return Unit.Default;
        }
        private void BeginScope()
        {
            scopes.Push(new Dictionary<string, bool>());
        }
        private void Resolve(Stmt statement)
        {
            statement.Accept(this);
        }
        public void Resolve(List<Stmt> statements)
        {
            foreach (var statement in statements)
            {
                statement.Accept(this);
            }
        }
        private void EndScope()
        {
            scopes.Pop();
        }
        public Unit VisitVarDeclStmt(Stmt.VarDecl stmt)
        {
            Declare(stmt.name);
            if (stmt.initializer != null)
            {
                Resolve(stmt.initializer);
            }
            Define(stmt.name);
            return Unit.Default;
        }
        private void Declare(Token name)
        {
            if (scopes.Count == 0)
            {
                return;
            }
            var scope = scopes.Peek();
            scope[name.lexeme] = false;
        }
        private void Resolve(Expr expr)
        {
            expr.Accept(this);
        }

        private void Define(Token name)
        {
            if (scopes.Count == 0)
            {
                return;
            }
            scopes.Peek()[name.lexeme] = true;
        }
        
        public Unit VisitVariableExpr(Expr.Variable expr)
        {
            if (scopes.Count > 0)
            {
                if (scopes.Peek().TryGetValue(expr.name.lexeme, out var initialized) && !initialized)
                {
                    jlox.Error(expr.name, "Cannot read local variable in its own initializer");
                    return Unit.Default;
                }
            }
            ResolveLocal(expr, expr.name);
            return Unit.Default;
        }

        private void ResolveLocal(Expr expr, Token name)
        {
            var scopes = this.scopes.ToArray();
            for (var i = 0; i < scopes.Length; i++)
            {
                var scope = scopes[i];
                if (scope.ContainsKey(name.lexeme))
                {
                    interpreter.Resolve(expr, scopes.Length - 1 - i);
                }
            }
        }

        public Unit VisitAssignExpr(Expr.Assign expr)
        {
            Resolve(expr.value);
            ResolveLocal(expr, expr.name);
            return Unit.Default;
        }
        
        public Unit VisitFunctionDeclStmt(Stmt.FunctionDecl stmt)
        {
            Declare(stmt.name);
            Define(stmt.name);
            ResolveFunction(stmt);
            return Unit.Default;
        }

        private void ResolveFunction(Stmt.FunctionDecl stmt)
        {
            BeginScope();
            foreach (var parameter in stmt.parameters)
            {
                Declare(parameter);
                Define(parameter);
            }
            Resolve(stmt.body);
            EndScope();
        }

        public Unit VisitBinaryExpr(Expr.Binary expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return Unit.Default;
        }

        public Unit VisitLogicExpr(Expr.Logic expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return Unit.Default;
        }

        public Unit VisitGroupingExpr(Expr.Grouping expr)
        {
            Resolve(expr.expression);
            return Unit.Default;
        }

        public Unit VisitLiteralExpr(Expr.Literal expr)
        {
            return Unit.Default;
        }

        public Unit VisitUnaryExpr(Expr.Unary expr)
        {
            Resolve(expr.right);
            return Unit.Default;
        }

        public Unit VisitTernaryExpr(Expr.Ternary expr)
        {
            Resolve(expr.left);
            Resolve(expr.mid);
            Resolve(expr.right);
            return Unit.Default;
        }


        public Unit VisitCallExpr(Expr.Call expr)
        {
            Resolve(expr.callee);
            foreach (var arg in expr.arguments)
            {
                Resolve(arg);
            }
            return Unit.Default;
        }

        public Unit VisitLambdaExpr(Expr.Lambda expr)
        {
            var decl = new Stmt.FunctionDecl(null, expr.parameters, expr.body);
            ResolveFunction(decl);
            return Unit.Default;
        }

        public Unit VisitExpressionStmt(Stmt.Expression stmt)
        {
            Resolve(stmt.expression);
            return Unit.Default;
        }

        public Unit VisitPrintStmt(Stmt.Print stmt)
        {
            Resolve(stmt.expression);
            return Unit.Default;
        }

        public Unit VisitIfStmt(Stmt.If stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.thenBranch);
            if (stmt.elseBranch != null)
            {
                Resolve(stmt.elseBranch);
            }
            return Unit.Default;
        }

        public Unit VisitWhileStmt(Stmt.While stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.body);
            return Unit.Default;
        }

        public Unit VisitBreakStmt(Stmt.Break stmt) { return Unit.Default;}

        public Unit VisitReturnStmt(Stmt.Return stmt)
        {
            Resolve(stmt.value);
            return Unit.Default;
        }
    }
}