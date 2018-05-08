// Copyright (C) 2018 Antoine Aubry
// 
// This file is part of Kimos.
// 
// Kimos is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Kimos is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Kimos.  If not, see <http://www.gnu.org/licenses/>.
// 

using Kimos.Drivers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Kimos.Helpers
{
    public class SqlCodeGeneratorExpressionVisitor : ExpressionVisitor
	{
		private readonly StringBuilder output;
		private readonly Dictionary<Expression, ParameterSyntaxType> parameterMappings;
		private readonly IQueryMetadata tableMetadata;
        private readonly IdentifierQuoter quoter;

        public SqlCodeGeneratorExpressionVisitor(StringBuilder output, Dictionary<Expression, ParameterSyntaxType> parameterMappings, IQueryMetadata tableMetadata, IdentifierQuoter quoter)
		{
			this.output = output;
			this.parameterMappings = parameterMappings;
			this.tableMetadata = tableMetadata;
            this.quoter = quoter;
        }

		protected override Expression VisitMember(MemberExpression node)
		{
            ParameterSyntaxType type;
            if (parameterMappings == null)
			{
				type = ParameterSyntaxType.Argument;
			}
			else
			{
				Expression parameterNode;
				MemberExpression current = node;
				do
				{
					parameterNode = current.Expression;
					current = parameterNode as MemberExpression;
				} while (current != null);

				type = parameterMappings[parameterNode];
			}

			switch (type)
			{
				case ParameterSyntaxType.None:
					output.Append(quoter.QuoteName(tableMetadata.ColumnMappings[node.Member.Name]));
					break;

				case ParameterSyntaxType.Target:
					output.Append("T.").Append(quoter.QuoteName(tableMetadata.ColumnMappings[node.Member.Name]));
					break;

				case ParameterSyntaxType.Source:
					output.Append("S.").Append(quoter.QuoteName(tableMetadata.GetParameterName(node)));
					break;

				case ParameterSyntaxType.Argument:
					output.AppendFormat("@{0}", tableMetadata.GetParameterName(node));
					break;
			}
			return node;
		}

		protected override Expression VisitConstant(ConstantExpression node)
		{
			var value = node.Value;
			if (value == null)
			{
				output.Append("NULL");
			}
			else
			{
				var type = Nullable.GetUnderlyingType(node.Type) ?? node.Type;
				switch (Type.GetTypeCode(type))
				{
					case TypeCode.Boolean:
						output.Append(true.Equals(value) ? 1 : 0);
						break;
					
					default:
						output.Append(value);
						break;
				}
			}
			return node;
		}

		private static readonly Dictionary<ExpressionType, string> operators = new Dictionary<ExpressionType, string>
		{
			{ ExpressionType.Add, "+" },
			{ ExpressionType.Subtract, "-" },
			{ ExpressionType.Multiply, "*" },
			{ ExpressionType.Divide, "/" },
			{ ExpressionType.Equal, "=" },
			{ ExpressionType.NotEqual, "<>" },
			{ ExpressionType.GreaterThan, ">" },
			{ ExpressionType.GreaterThanOrEqual, ">=" },
			{ ExpressionType.LessThan, "<" },
			{ ExpressionType.LessThanOrEqual, "<=" },
			{ ExpressionType.AndAlso, "and" },
			{ ExpressionType.OrElse, "or" },
		};

		protected override Expression VisitBinary(BinaryExpression node)
		{
			output.Append('(');
			Visit(node.Left);
			output.Append(' ').Append(operators[node.NodeType]).Append(' ');
			Visit(node.Right);
			output.Append(')');
			return node;
		}

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            output.Append("case when ");
            Visit(node.Test);
            output.Append(" then ");
            Visit(node.IfTrue);
            output.Append(" else ");
            Visit(node.IfFalse);
            output.Append(" end");
            return node;
        }

        private Exception ExpressionNotSupported(Expression node) => new NotSupportedException($"{node.NodeType} expressions are not supported. Cannot convert expression {node} to SQL");

        protected override Expression VisitBlock(BlockExpression node) => throw ExpressionNotSupported(node);
        protected override Expression VisitDebugInfo(DebugInfoExpression node) => throw ExpressionNotSupported(node);
        protected override Expression VisitDefault(DefaultExpression node) => throw ExpressionNotSupported(node);
        protected override Expression VisitDynamic(DynamicExpression node) => throw ExpressionNotSupported(node);
        protected override Expression VisitExtension(Expression node) => throw ExpressionNotSupported(node);
        protected override Expression VisitGoto(GotoExpression node) => throw ExpressionNotSupported(node);
        protected override Expression VisitIndex(IndexExpression node) => throw ExpressionNotSupported(node);
        protected override Expression VisitInvocation(InvocationExpression node) => throw ExpressionNotSupported(node);
        protected override Expression VisitLabel(LabelExpression node) => throw ExpressionNotSupported(node);
        protected override Expression VisitLambda<T>(Expression<T> node) => throw ExpressionNotSupported(node);
        protected override Expression VisitListInit(ListInitExpression node) => throw ExpressionNotSupported(node);
        protected override Expression VisitLoop(LoopExpression node) => throw ExpressionNotSupported(node);
        protected override Expression VisitMemberInit(MemberInitExpression node) => throw ExpressionNotSupported(node);
        protected override Expression VisitMethodCall(MethodCallExpression node) => throw ExpressionNotSupported(node);
        protected override Expression VisitNew(NewExpression node) => throw ExpressionNotSupported(node);
        protected override Expression VisitNewArray(NewArrayExpression node) => throw ExpressionNotSupported(node);
        protected override Expression VisitParameter(ParameterExpression node) => throw ExpressionNotSupported(node);
        protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node) => throw ExpressionNotSupported(node);
        protected override Expression VisitSwitch(SwitchExpression node) => throw ExpressionNotSupported(node);
        protected override Expression VisitTry(TryExpression node) => throw ExpressionNotSupported(node);
        protected override Expression VisitTypeBinary(TypeBinaryExpression node) => throw ExpressionNotSupported(node);
        protected override Expression VisitUnary(UnaryExpression node) => throw ExpressionNotSupported(node);

        protected override ElementInit VisitElementInit(ElementInit node) => throw new NotSupportedException($"ElementInit in expressions are not supported. Cannot convert {node} to SQL");
        protected override LabelTarget VisitLabelTarget(LabelTarget node) => throw new NotSupportedException($"LabelTarget in expressions are not supported. Cannot convert {node} to SQL");
        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node) => throw new NotSupportedException($"MemberAssignment in expressions are not supported. Cannot convert {node} to SQL");
        protected override MemberBinding VisitMemberBinding(MemberBinding node) => throw new NotSupportedException($"MemberBinding in expressions are not supported. Cannot convert {node} to SQL");
        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node) => throw new NotSupportedException($"MemberListBinding in expressions are not supported. Cannot convert {node} to SQL");
        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node) => throw new NotSupportedException($"MemberMemberBinding in expressions are not supported. Cannot convert {node} to SQL");
        protected override SwitchCase VisitSwitchCase(SwitchCase node) => throw new NotSupportedException($"SwitchCase in expressions are not supported. Cannot convert {node} to SQL");
        protected override CatchBlock VisitCatchBlock(CatchBlock node) => throw new NotSupportedException($"CatchBlock in expressions are not supported. Cannot convert {node} to SQL");
    }
}