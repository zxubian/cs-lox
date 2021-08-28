using System.Collections.Generic;

namespace cslox
{

	public abstract class Stmt
	{
		public interface IVisitor<T>
		{
			 T VisitExpressionStmt(Expression stmt);
			 T VisitPrintStmt(Print stmt);
			 T VisitVarDeclStmt(VarDecl stmt);
			 T VisitFunctionDeclStmt(FunctionDecl stmt);
			 T VisitBlockStmt(Block stmt);
			 T VisitIfStmt(If stmt);
			 T VisitWhileStmt(While stmt);
			 T VisitBreakStmt(Break stmt);
			 T VisitReturnStmt(Return stmt);
			 T VisitClassDeclStmt(ClassDecl stmt);
		}
		public class Expression : Stmt
		{
			public Expression (Expr expression)
			{
			this.expression = expression;
			}

			public readonly Expr expression;
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitExpressionStmt(this);
		}
		public class Print : Stmt
		{
			public Print (Expr expression)
			{
			this.expression = expression;
			}

			public readonly Expr expression;
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitPrintStmt(this);
		}
		public class VarDecl : Stmt
		{
			public VarDecl (Token name, Expr initializer)
			{
			this.name = name;
			this.initializer = initializer;
			}

			public readonly Token name;
			public readonly Expr initializer;
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitVarDeclStmt(this);
		}
		public class FunctionDecl : Stmt
		{
			public FunctionDecl (Token name, List<Token> parameters, List<Stmt> body)
			{
			this.name = name;
			this.parameters = parameters;
			this.body = body;
			}

			public readonly Token name;
			public readonly List<Token> parameters;
			public readonly List<Stmt> body;
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitFunctionDeclStmt(this);
		}
		public class Block : Stmt
		{
			public Block (List<Stmt> statements)
			{
			this.statements = statements;
			}

			public readonly List<Stmt> statements;
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitBlockStmt(this);
		}
		public class If : Stmt
		{
			public If (Expr condition, Stmt thenBranch, Stmt elseBranch)
			{
			this.condition = condition;
			this.thenBranch = thenBranch;
			this.elseBranch = elseBranch;
			}

			public readonly Expr condition;
			public readonly Stmt thenBranch;
			public readonly Stmt elseBranch;
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitIfStmt(this);
		}
		public class While : Stmt
		{
			public While (Expr condition, Stmt body)
			{
			this.condition = condition;
			this.body = body;
			}

			public readonly Expr condition;
			public readonly Stmt body;
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitWhileStmt(this);
		}
		public class Break : Stmt
		{
			public Break (Token keyword)
			{
			this.keyword = keyword;
			}

			public readonly Token keyword;
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitBreakStmt(this);
		}
		public class Return : Stmt
		{
			public Return (Token keyword, Expr value)
			{
			this.keyword = keyword;
			this.value = value;
			}

			public readonly Token keyword;
			public readonly Expr value;
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitReturnStmt(this);
		}
		public class ClassDecl : Stmt
		{
			public ClassDecl (Token name, List<Stmt.FunctionDecl> methods)
			{
			this.name = name;
			this.methods = methods;
			}

			public readonly Token name;
			public readonly List<Stmt.FunctionDecl> methods;
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitClassDeclStmt(this);
		}

		public abstract T Accept<T>(IVisitor<T> visitor);
	}
}
