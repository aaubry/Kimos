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
using System.Text;
using static Kimos.Drivers.PostgreSql.PostgreSqlDriver;

namespace Kimos.Drivers.PostgreSql
{
    internal class ConflictPredicateExpressionVisitor
    {

    }
    internal class InsertOrUpdateCommandGenerator : IInsertOrUpdateCommandGenerator
    {
        public string GenerateSqlCommand<TEntity, TParams, TResult>(IInsertOrUpdateCommand<TEntity, TParams, TResult> command, IQueryMetadata metadata)
        {
            var commandText = new StringBuilder();

            if (command.Insert != null)
            {
                // Generate 'insert...'
                commandText.AppendFormat("insert into {0} as T ", Quoter.QuoteTableName(metadata.TableSchema, metadata.TableName));

                command.Insert.GenerateInsertColumnList(commandText, Quoter, metadata);

                command.Insert.GenerateInsertValueList(commandText, Quoter, metadata);

                if (command.Update != null)
                {
                    // Generate 'on conflict (...) update ...'
                    commandText.Append("on conflict (");

                    var columns = command.ConflictColumns.ToColumnList();
                    foreach (var column in columns)
                    {
                        commandText
                            .Append(Quoter.QuoteName(metadata.ColumnMappings[column.Name]))
                            .Append(", ");
                    }

                    commandText.Length -= 2; // Remove last comma
                    commandText.AppendLine(") do update set ");
                }
            }
            else if (command.Update != null)
            {
                // Generate 'update ...'
                commandText.AppendFormat("update {0} as T set ", Quoter.QuoteTableName(metadata.TableSchema, metadata.TableName));
                commandText.AppendLine();
            }

            if (command.Update != null)
            {
                command.Update.GenerateUpdateSetList(commandText, Quoter, metadata, ParameterSyntaxType.Target);

                if (command.UpdatePredicate != null)
                {
                    commandText.Append("where ");
                    command.UpdatePredicate.GenerateWhereClause(commandText, Quoter, metadata, ParameterSyntaxType.Target);
                    commandText.AppendLine();
                }
            }

            if (command.Output != null)
            {
                // Generate 'returning ...'
                commandText.Append("returning ");

                new MemberAccessExpressionVisitor(
                    n => commandText.AppendFormat("{0} as {1}, ", Quoter.QuoteName(metadata.ColumnMappings[n.Member.Name]), n.Member.Name)
                ).Visit(command.Output);

                commandText.Length -= 2; // Remove last comma

                commandText.AppendLine();
            }

            return commandText.ToString();
        }
    }
}
