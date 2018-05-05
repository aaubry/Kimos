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

using Kimos.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using static Kimos.Drivers.SqlServer.SqlServerDriver;

namespace Kimos.Drivers.SqlServer
{
    internal class InsertOrUpdateCommandGenerator : IInsertOrUpdateCommandGenerator
    {
        public string GenerateSqlCommand<TEntity, TParams, TResult>(IInsertOrUpdateCommand<TEntity, TParams, TResult> command, IQueryMetadata metadata)
        {
            var commandText = new StringBuilder();

            if (command.Update != null && command.Insert != null)
            {
                // Generate 'merge ...'
                commandText.AppendFormat("merge {0} with (holdlock) as T using ( select ", Quoter.QuoteTableName(metadata.TableSchema, metadata.TableName));

                var columns = command.ConflictColumns.ToColumnList();

                var parametersPredicateParameter = command.Insert.Parameters[0];
                var visitedMembers = new HashSet<MemberInfo>();
                new NewObjectExpressionVisitor(
                    m =>
                    {
                        var assignment = m as MemberAssignment;
                        if (assignment != null && columns.Contains(m.Member))
                        {
                            new MemberAccessExpressionVisitor(
                                ma =>
                                {
                                    if (ma.Expression == parametersPredicateParameter && visitedMembers.Add(ma.Member))
                                    {
                                        commandText.AppendFormat("@{0} as {1}, ", metadata.GetParameterName(ma), Quoter.QuoteName(ma.Member.Name));
                                    }
                                }
                            ).Visit(assignment.Expression);
                        }
                    }
                ).Visit(command.Insert);

                commandText.Length -= 2; // Remove last comma
                commandText.AppendLine(" ) as S");


                // Generate 'on (S.A = T.A and ...'
                commandText.Append("on (");

                new NewObjectExpressionVisitor(
                    m =>
                    {
                        var assignment = m as MemberAssignment;
                        if (assignment != null && columns.Contains(m.Member))
                        {
                            commandText.AppendFormat("T.{0} = ", Quoter.QuoteName(metadata.ColumnMappings[m.Member.Name]));

                            new SqlCodeGeneratorExpressionVisitor(commandText, new Dictionary<Expression, ParameterSyntaxType>
                                {
                                    { command.Insert.Parameters[0], ParameterSyntaxType.Source },
                                }, metadata, Quoter).Visit(assignment.Expression);
                        }
                    }
                ).Visit(command.Insert);

                commandText.AppendLine(")");


                // Generate 'when matched then ...'
                commandText.Append("when matched");

                if (command.UpdatePredicate != null)
                {
                    commandText.Append(" and (");
                    command.UpdatePredicate.GenerateWhereClause(commandText, Quoter, metadata, ParameterSyntaxType.Target);
                    commandText.Append(")");
                }

                commandText.AppendLine(" then update set ");
                command.Update.GenerateUpdateSetList(commandText, Quoter, metadata, ParameterSyntaxType.Target);

                // Generate 'when not matched then ...'
                commandText.AppendLine("when not matched then insert ");
            }
            else if (command.Insert != null)
            {
                // Generate 'insert into ...'
                commandText.AppendFormat("insert into {0} ", Quoter.QuoteTableName(metadata.TableSchema, metadata.TableName));
            }
            else if (command.Update != null)
            {
                // Generate 'update ...'
                commandText.AppendFormat("update {0} set ", Quoter.QuoteTableName(metadata.TableSchema, metadata.TableName));
                commandText.AppendLine();
                command.Update.GenerateUpdateSetList(commandText, Quoter, metadata, ParameterSyntaxType.None);

                if (command.Output != null)
                {
                    GenerateOutputClause(command, metadata, commandText);
                }

                if (command.UpdatePredicate != null)
                {
                    commandText.Append("where ");
                    command.UpdatePredicate.GenerateWhereClause(commandText, Quoter, metadata, ParameterSyntaxType.None);
                    commandText.AppendLine();
                }
            }

            if (command.Insert != null)
            {
                command.Insert.GenerateInsertColumnList(commandText, Quoter, metadata);

                if (command.Output != null && command.Update == null)
                {
                    GenerateOutputClause(command, metadata, commandText);
                }

                command.Insert.GenerateInsertValueList(commandText, Quoter, metadata);
            }

            if (command.Output != null && command.Update != null && command.Insert != null)
            {
                GenerateOutputClause(command, metadata, commandText);
            }

            commandText.Append(';');

            return commandText.ToString();
        }

        private static void GenerateOutputClause<TEntity, TParams, TResult>(IInsertOrUpdateCommand<TEntity, TParams, TResult> command, IQueryMetadata metadata, StringBuilder commandText)
        {
            // Generate 'output ...'
            commandText.Append("output ");

            new MemberAccessExpressionVisitor(
                n => commandText.AppendFormat("inserted.{0} as {1}, ", Quoter.QuoteName(metadata.ColumnMappings[n.Member.Name]), n.Member.Name)
            ).Visit(command.Output);

            commandText.Length -= 2; // Remove last comma

            commandText.AppendLine();
        }
    }
}
