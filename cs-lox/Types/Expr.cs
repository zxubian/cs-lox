using System;
using System.Collections.Generic;

namespace cslox
{

	public abstract class Expr
	{
		public interface IVisitor<T>
		{
			 T VisitAssignExpr(Assign expr);
			 T VisitBinaryExpr(Binary expr);
			 T VisitLogicExpr(Logic expr);
			 T VisitGroupingExpr(Grouping expr);
			 T VisitLiteralExpr(Literal expr);
			 T VisitUnaryExpr(Unary expr);
			 T VisitTernaryExpr(Ternary expr);
			 T VisitVariableExpr(Variable expr);
			 T VisitCallExpr(Call expr);
			 T VisitLambdaExpr(Lambda expr);
			 T VisitGetExpr(Get expr);
			 T VisitSetExpr(Set expr);
		}
		public class Assign : Expr
		{
			public Assign (Token name, Expr value)
			{
			this.name = name;
			this.value = value;
			}

			public readonly Token name;
			public readonly Expr value;
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitAssignExpr(this);
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
		public class Logic : Expr
		{
			public Logic (Expr left, Token operatorToken, Expr right)
			{
			this.left = left;
			this.operatorToken = operatorToken;
			this.right = right;
			}

			public readonly Expr left;
			public readonly Token operatorToken;
			public readonly Expr right;
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitLogicExpr(this);
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
			public Literal (object value)
			{
			this.value = value;
			}

			public readonly object value;
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
		public class Variable : Expr
		{
			public Variable (Token name)
			{
			this.name = name;
			}

			public readonly Token name;
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitVariableExpr(this);
		}
		public class Call : Expr
		{
			public Call (Expr callee, Token closingParen, List<Expr> arguments)
			{
			this.callee = callee;
			this.closingParen = closingParen;
			this.arguments = arguments;
			}

			public readonly Expr callee;
			public readonly Token closingParen;
			public readonly List<Expr> arguments;
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitCallExpr(this);
		}
		public class Lambda : Expr
		{
			public Lambda (List<Token> parameters, List<Stmt> body)
			{
			this.parameters = parameters;
			this.body = body;
			}

			public readonly List<Token> parameters;
			public readonly List<Stmt> body;
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitLambdaExpr(this);
		}
		public class Get : Expr
		{
			public Get (Expr obj, Token name)
			{
			this.obj = obj;
			this.name = name;
			}

			public readonly Expr obj;
			public readonly Token name;
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitGetExpr(this);
		}
		public class Set : Expr
		{
			public Set (Expr obj, Token name, Expr value)
			{
			this.obj = obj;
			this.name = name;
			this.value = value;
			}

			public readonly Expr obj;
			public readonly Token name;
			public readonly Expr value;
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitSetExpr(this);
		}

		public abstract T Accept<T>(IVisitor<T> visitor);
	}
}
