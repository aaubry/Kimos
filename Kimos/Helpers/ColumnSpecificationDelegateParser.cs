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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Kimos.Helpers
{
    public static class ColumnSpecificationDelegateParser
    {
        public static IEnumerable<PropertyInfo> ToColumnList<TEntity>(this Expression<ColumnSpecificationDelegate<TEntity>> columnSpecification)
        {
            var body = columnSpecification.Body;

            // Ignore Convert(...)
            var conversionExpression = body as UnaryExpression;
            if (conversionExpression != null && conversionExpression.NodeType == ExpressionType.Convert)
            {
                body = conversionExpression.Operand;
            }

            var newExpression = body as NewExpression;
            if (newExpression != null)
            {
                return newExpression.Arguments
                    .Select(m => m.ToProperty())
                    .ToList();
            }
            else
            {
                var property = AsProperty(body) ?? throw new ArgumentException($"Expression {body} should be either a property access expression or an anonymous type initialization expression");
                return new[] { property };
            }
        }

        private static PropertyInfo ToProperty(this Expression expression)
        {
            return AsProperty(expression) ?? throw new ArgumentException($"Expression {expression} is not a property access expression");
        }

        private static PropertyInfo AsProperty(Expression expression)
        {
            return (expression as MemberExpression)?.Member as PropertyInfo;
        }
    }
}