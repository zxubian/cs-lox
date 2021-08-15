using System;
using System.Collections.Generic;

namespace cslox
{

	public abstract class Expr
	{
		public interface IVisitor<T>
		{
			 T VisitBinaryExpr(Binary expr);
			 T VisitGroupingExpr(Grouping expr);
			 T VisitLiteralExpr(Literal expr);
			 T VisitUnaryExpr(Unary expr);
			 T VisitTernaryExpr(Ternary expr);
		}
		public class Binary : Expr
		{
			public Binary (Expr left, Token operatorToken, Expr right)
			{
			this.left = left;
			this.operatorToken = operatorToken;
			this.right = right;
			}

			public readonly Expr left;
			public readonly Token operatorToken;
			public readonly Expr right;
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitBinaryExpr(this);
		}
		public class Grouping : Expr
		{
			public Grouping (Expr expression)
			{
			this.expression = expression;
			}

			public readonly Expr expression;
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitGroupingExpr(this);
		}
		public class Literal : Expr
		{
			public Literal (Object value)
			{
			this.value = value;
			}

			public readonly Object value;
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitLiteralExpr(this);
		}
		public class Unary : Expr
		{
			public Unary (Token operatorToken, Expr right)
			{
			this.operatorToken = operatorToken;
			this.right = right;
			}

			public readonly Token operatorToken;
			public readonly Expr right;
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitUnaryExpr(this);
		}
		public class Ternary : Expr
		{
			public Ternary (Expr left, Token firstOperator, Expr mid, Token secondOperator, Expr right)
			{
			this.left = left;
			this.firstOperator = firstOperator;
			this.mid = mid;
			this.secondOperator = secondOperator;
			this.right = right;
			}

			public readonly Expr left;
			public readonly Token firstOperator;
			public readonly Expr mid;
			public readonly Token secondOperator;
			public readonly Expr right;
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitTernaryExpr(this);
		}

		public abstract T Accept<T>(IVisitor<T> visitor);
	}
}
