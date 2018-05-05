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
    internal class DeleteCommandGenerator : IDeleteCommandGenerator
    {
        public string GenerateSqlCommand<TEntity, TParams>(IDeleteCommand<TEntity, TParams> command, IQueryMetadata metadata)
        {
            var commandText = new StringBuilder();

            // Generate 'delete from ...'
            commandText
                .Append("delete from ")
                .Append(Quoter.QuoteTableName(metadata.TableSchema, metadata.TableName));

            // Generate 'where ...'
            commandText.Append(" where ");
            command.Predicate.GenerateWhereClause(commandText, Quoter, metadata, ParameterSyntaxType.None);

            return commandText.ToString();
        }
    }
}
