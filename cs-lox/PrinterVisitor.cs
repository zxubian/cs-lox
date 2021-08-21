using System;
using System.Text;

namespace cslox
{
    public class PrinterVisitor : Expr.IVisitor<string>
    {
        public string Print(Expr expr)
        {
            return expr.Accept(this);
        }

        private string Parenthesize(string name, params Expr[] exprs)
        {
            var builder = new StringBuilder("(");
            builder.Append(name);
            foreach (var expr in exprs)
            {
                builder.Append(" ");
                builder.Append(expr.Accept(this));
            }
            builder.Append(")");
            return builder.ToString();
        }

        public string VisitAssignExpr(Expr.Assign expr)
        {
            return $"{expr.name.lexeme} -> {Print(expr.value)}";
        }

        public string VisitBinaryExpr(Expr.Binary expr)
        {
            return Parenthesize(expr.operatorToken.lexeme, expr.left, expr.right);
        }

        public string VisitGroupingExpr(Expr.Grouping expr)
        {
            return Parenthesize("group", expr.expression);
        }

        public string VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value == null ? "nil" : expr.value.ToString();
        }

        public string VisitUnaryExpr(Expr.Unary expr)
        {
            return Parenthesize(expr.operatorToken.lexeme, expr.right);
        }

        public string VisitTernaryExpr(Expr.Ternary expr)
        {
            return Parenthesize(
                $"{Print(expr.left)} ? {Print(expr.mid)} : {Print(expr.right)}");
        }

        public string VisitVariableExpr(Expr.Variable expr)
        {
            throw new NotImplementedException();
        }
    }
}