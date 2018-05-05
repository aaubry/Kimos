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

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Kimos.Drivers;

namespace Kimos.Helpers
{
    public static class SqlGenerator
    {
        public static void GenerateInsertColumnList<TEntity, TParams>(this Expression<InsertSpecificationDelegate<TEntity, TParams>> insert, StringBuilder commandText, IdentifierQuoter quoter, IQueryMetadata metadata)
        {
            // Generate insert values
            commandText.Append("( ");
            new NewObjectExpressionVisitor(
                n => commandText.AppendFormat("{0}, ", quoter.QuoteName(metadata.ColumnMappings[n.Member.Name]))
            ).Visit(insert);

            commandText.Length -= 2; // Remove last comma
            commandText.AppendLine(" )");
        }

        public static void GenerateInsertValueList<TEntity, TParams>(this Expression<InsertSpecificationDelegate<TEntity, TParams>> insert, StringBuilder commandText, IdentifierQuoter quoter, IQueryMetadata metadata)
        {
            // Generate 'values ...'
            commandText.Append("values ( ");

            new NewObjectExpressionVisitor(
                n =>
                {
                    new SqlCodeGeneratorExpressionVisitor(commandText, null, metadata, quoter).Visit(((MemberAssignment)n).Expression);
                    commandText.Append(", ");
                }
            ).Visit(insert);

            commandText.Length -= 2; // Remove last comma
            commandText.AppendLine(" )");
        }

        public static void GenerateUpdateSetList<TEntity, TParams>(this Expression<UpdateSpecificationDelegate<TEntity, TParams>> update, StringBuilder commandText, IdentifierQuoter quoter, IQueryMetadata metadata, ParameterSyntaxType entityParameterType)
        {
            new NewObjectExpressionVisitor(
                n =>
                {
                    commandText.AppendFormat("{0} = ", quoter.QuoteName(metadata.ColumnMappings[n.Member.Name]));
                    new SqlCodeGeneratorExpressionVisitor(commandText, new Dictionary<Expression, ParameterSyntaxType>
                            {
                                { update.Parameters[0], entityParameterType },
                                { update.Parameters[1], ParameterSyntaxType.Argument },
                            }, metadata, quoter).Visit(((MemberAssignment)n).Expression);
                    commandText.Append(",");
                }
            ).Visit(update);

            commandText.Length--; // Remove last comma
            commandText.AppendLine();
        }

        public static void GenerateWhereClause<TEntity, TParams>(this Expression<PredicateSpecificationDelegate<TEntity, TParams>> predicate, StringBuilder commandText, IdentifierQuoter quoter, IQueryMetadata metadata, ParameterSyntaxType entityParameterType)
        {
            var parameterMappings = new Dictionary<Expression, ParameterSyntaxType>
            {
                { predicate.Parameters[0], entityParameterType },
                { predicate.Parameters[1], ParameterSyntaxType.Argument },
            };

            new SqlCodeGeneratorExpressionVisitor(commandText, parameterMappings, metadata, quoter).Visit(predicate.Body);
        }
    }
}