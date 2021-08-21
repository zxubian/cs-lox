using System;
using System.Collections.Generic;

namespace cslox
{

	public abstract class Stmt
	{
		public interface IVisitor<T>
		{
			 T VisitExpressionStmt(Expression stmt);
			 T VisitPrintStmt(Print stmt);
			 T VisitVarStmt(Var stmt);
			 T VisitBlockStmt(Block stmt);
			 T VisitIfStmt(If stmt);
			 T VisitWhileStmt(While stmt);
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
		public class Var : Stmt
		{
			public Var (Token name, Expr initializer)
			{
			this.name = name;
			this.initializer = initializer;
			}

			public readonly Token name;
			public readonly Expr initializer;
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitVarStmt(this);
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

		public abstract T Accept<T>(IVisitor<T> visitor);
	}
}
