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

		public abstract T Accept<T>(IVisitor<T> visitor);
	}
}
