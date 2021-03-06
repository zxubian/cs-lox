using System.Collections.Generic;
using System.Linq;
using cslox.UtilityTypes;

namespace cslox
{
    public class Resolver : Expr.IVisitor<Unit>, Stmt.IVisitor<Unit>
    {
        private enum FunctionType
        {
            None,
            Function,
            Lambda,
            // function in a class
            Method,
            // method with name 'init', special case because we do not allow
            // returning from constructors
            Initializer,
            StaticMethod,
            Getter
        }

        private enum LoopType
        {
            None,
            While
        }

        private enum ClassType
        {
            None,
            Class,
            Subclass
        }
        
        private readonly Interpreter interpreter;
        private readonly Stack<Dictionary<string, VariableData>> scopes = new Stack<Dictionary<string, VariableData>>();
        private FunctionType currentFunction = FunctionType.None;
        private LoopType currentLoop = LoopType.None;
        private ClassType currentClass = ClassType.None;

        public class State
        {
            private readonly Dictionary<string, VariableData> globalScope = new Dictionary<string, VariableData>();
            public void DeclareGlobal(string name, Stmt declaration) => globalScope[name] = new VariableData(declaration);
            public bool DefineGlobal(string name)
            {
                var found = globalScope.TryGetValue(name, out var data);
                if(found)
                {
                    data.Initialized = true;
                }
                return found;
            }
            public bool UseGlobal(string name)
            {
                var found = globalScope.TryGetValue(name, out var data);
                if(found)
                {
                    data.Used = true;
                }
                return found;
            }

            public bool IsInitializedGlobal(string name) =>
                globalScope.TryGetValue(name, out var data) && data.Initialized;

            public bool IsDeclaredGlobal(string name) => globalScope.ContainsKey(name);
        }

        private class VariableData
        {
            public bool Initialized;
            public bool Used;
            
            public readonly Stmt Declaration;
            public VariableData(Stmt declaration)
            {
                Declaration = declaration;
            }
        }

        private State currentState;

        public Resolver(Interpreter interpreter, ref State state)
        {
            this.interpreter = interpreter;
            currentState = state;
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
            scopes.Push(new Dictionary<string, VariableData>());
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
            var scopeToEnd = scopes.Pop();
            var unusedVariables = scopeToEnd
                .Where(x => !x.Value.Used);
            string type = "";
            Token token;
            foreach (var (name, variableData) in unusedVariables)
            {
                switch (variableData.Declaration)
                {
                    case Stmt.VarDecl varDecl:
                        type = "variable";
                        token = varDecl.name;
                        break;
                    case Stmt.FunctionDecl functionDecl:
                        type = "function";
                        token = functionDecl.name;
                        break;
                    default:
                        type = "unknown type";
                        token = new Token(TokenType.IDENTIFIER, name, null, -1);
                        break;
                }
                cslox.Error(token, $"Unused {type} {token.lexeme}.");
            }
        }
        
        public Unit VisitVarDeclStmt(Stmt.VarDecl stmt)
        {
            Declare(stmt.name, stmt);
            if (stmt.initializer != null)
            {
                Resolve(stmt.initializer);
                Define(stmt.name);
            }
            return Unit.Default;
        }
        
        private void Declare(Token name, Stmt declaration)
        {
            if (scopes.Count == 0)
            {
                currentState.DeclareGlobal(name.lexeme, declaration);
                return;
            }
            var scope = scopes.Peek();
            if (scope.ContainsKey(name.lexeme))
            {
                if (name.lexeme == "this")
                {
                    cslox.Error(name, $"Cannot override implicit variable 'this' in a class.");
                }
                else
                {
                    cslox.Error(name, $"A variable called '{name.lexeme}' already exists in this scope.");
                }
            }
            var varData = new VariableData(declaration);
            scope[name.lexeme] = varData;
        }
        
        private void Resolve(Expr expr)
        {
            expr.Accept(this);
        }

        private void Define(Token name)
        {
            if (scopes.Count == 0)
            {
                if (!currentState.DefineGlobal(name.lexeme))
                {
                    cslox.Error(name, "Attempting to initialize undeclared variable.");
                }
                return;
            }
            scopes.Peek()[name.lexeme].Initialized = true;
        }
        
        public Unit VisitVariableExpr(Expr.Variable expr)
        {
            if (scopes.Count > 0)
            {
                if (scopes.Peek().TryGetValue(expr.name.lexeme, out var varData) && !varData.Initialized)
                {
                    cslox.Error(expr.name, "Cannot read local variable in its own initializer.");
                    return Unit.Default;
                }
            }
            else if (scopes.Count == 0)
            {
                if (!currentState.IsDeclaredGlobal(expr.name.lexeme))
                {
                    cslox.Error(expr.name, "Attempting to use undeclared variable.");
                }
                else if (!currentState.IsInitializedGlobal(expr.name.lexeme))
                {
                    cslox.Error(expr.name, "Attempting to use uninitialized variable");
                }
                else
                {
                    currentState.UseGlobal(expr.name.lexeme);
                }
                return Unit.Default;
            }
            ResolveLocal(expr, expr.name);
            return Unit.Default;
        }

        private void ResolveLocal(Expr expr, Token name)
        {
            var found = false;
            var i = 0;
            foreach(var scope in scopes)
            {
                if (scope.TryGetValue(name.lexeme, out var varData))
                {
                    interpreter.Resolve(expr, i);
                    varData.Used = true;
                    found = true;
                    break;
                }
                i++;
            }
            if (!found)
            {
                if (!currentState.IsDeclaredGlobal(name.lexeme))
                {
                    cslox.Error(name, "Attempting to use undeclared variable.");
                }
                else if (!currentState.IsInitializedGlobal(name.lexeme))
                {
                    cslox.Error(name, "Attempting to use uninitialized variable");
                }
                else
                {
                    currentState.UseGlobal(name.lexeme);
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
            Declare(stmt.name, stmt);
            Define(stmt.name);
            ResolveFunction(stmt, FunctionType.Function);
            return Unit.Default;
        }

        private void ResolveFunction(Stmt.FunctionDecl stmt, FunctionType type)
        {
            var enclosingFunction = currentFunction;
            currentFunction = type;
            BeginScope();
            if (stmt.parameters != null)
            {
                foreach (var parameter in stmt.parameters)
                {
                    Declare(parameter, new Stmt.VarDecl(parameter, null));
                    Define(parameter);
                }
            }
            Resolve(stmt.body);
            if (currentFunction == FunctionType.Getter && !stmt.body.Any(x => x is Stmt.Return))
            {
                cslox.Error(stmt.name, "A get property must return a value.");
            }
            EndScope();
            currentFunction = enclosingFunction;
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
            ResolveFunction(decl, FunctionType.Lambda);
            return Unit.Default;
        }

        public Unit VisitGetExpr(Expr.Get expr)
        {
            Resolve(expr.obj);
            return Unit.Default;
        }

        public Unit VisitSetExpr(Expr.Set expr)
        {
            Resolve(expr.obj);
            Resolve(expr.value);
            return Unit.Default;
        }

        public Unit VisitThisExpr(Expr.This expr)
        {
            if (currentClass == ClassType.None)
            {
                cslox.Error(expr.keyword, "Cannot use 'this' outside of a class.");
            }
            if (currentFunction == FunctionType.StaticMethod)
            {
                cslox.Error(expr.keyword, "Cannot use 'this' in static method.");
            }
            ResolveLocal(expr, expr.keyword);
            return Unit.Default;
        }

        public Unit VisitSuperExpr(Expr.Super expr)
        {
            switch (currentClass)
            {
                // TODO: check
                case ClassType.None:
                    cslox.Error(expr.keyword, "Cannot use 'super' outside of a class.");
                    break;
                case ClassType.Class:
                    cslox.Error(expr.keyword, "Cannot use 'super' in a class that does not inherit from any superclass.");
                    break;
            }
            if (currentFunction == FunctionType.StaticMethod)
            {
                cslox.Error(expr.keyword, "Cannot use 'super' in static method.");
            }
            ResolveLocal(expr, expr.keyword);
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
            var enclosingLoop = currentLoop;
            currentLoop = LoopType.While;
            Resolve(stmt.condition);
            Resolve(stmt.body);
            currentLoop = enclosingLoop;
            return Unit.Default;
        }

        public Unit VisitBreakStmt(Stmt.Break stmt)
        {
            if(currentLoop == LoopType.None)
            {
                cslox.Error(stmt.keyword, "Cannot use 'break' outside of a loop.");
            }
            return Unit.Default;
        }
        
        public Unit VisitReturnStmt(Stmt.Return stmt)
        {
            switch (currentFunction)
            {
                case FunctionType.None:
                    cslox.Error(stmt.keyword, "Cannot return from top-level code.");
                    break;
                case FunctionType.Initializer:
                    cslox.Error(stmt.keyword, "Cannot return from initializer. Initializer always implicitly returns the new instance.");
                    break;
            }
            Resolve(stmt.value);
            return Unit.Default;
        }

        public Unit VisitClassDeclStmt(Stmt.ClassDecl stmt)
        {
            var oldClassType = currentClass;
            currentClass = stmt.superClass == null ? ClassType.Class : ClassType.Subclass;
            Declare(stmt.name, stmt);
            Define(stmt.name);
            if (stmt.superClass != null)
            {
                if (stmt.superClass.name == stmt.name)
                {
                    cslox.Error(stmt.superClass.name, "A class cannot inherit from itself.");
                }
                Resolve(stmt.superClass);
                BeginScope();
                scopes.Peek()["super"] = new VariableData(stmt){Initialized =  true, Used = true};
            }
            var instanceMethods = stmt.methods;
            var staticMethods = stmt.staticMethods;
            var getterProperties = stmt.getProperties;
            var methodsByName = instanceMethods
                .Concat(staticMethods)
                .Concat(getterProperties)
                .GroupBy(x => x.name.lexeme);
            foreach (var overloadGroup in methodsByName)
            {
                var seenSignatures = new List<int>();
                foreach (var overload in overloadGroup)
                {
                    var parameterCount = overload.parameters?.Count ?? 0;
                    if
                    (
                        seenSignatures.Any ( x => x == parameterCount )
                    )
                    {
                        cslox.Error(overload.name, "A method declaration with the same signature already exists.");
                    }
                    else
                    {
                        seenSignatures.Add(parameterCount);
                    }
                }
            }
            foreach (var staticMethod in staticMethods)
            {
                ResolveFunction(staticMethod, FunctionType.StaticMethod);
            }
            BeginScope();
            scopes.Peek()["this"] = new VariableData(stmt){Used = true, Initialized = true};
            foreach (var methodDecl in stmt.methods)
            {
                var type = methodDecl.name.lexeme == "init" ? FunctionType.Initializer : FunctionType.Method;
                ResolveFunction(methodDecl, type);
            }
            foreach (var getterProperty in getterProperties)
            {
                ResolveFunction(getterProperty, FunctionType.Getter);
            }
            EndScope();
            if (stmt.superClass != null)
            {
                EndScope();
            }
            currentClass = oldClassType;
            return Unit.Default;
        }
    }
}