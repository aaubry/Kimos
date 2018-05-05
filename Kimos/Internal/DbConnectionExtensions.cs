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
using System.Data;

namespace Kimos.Internal
{
    internal static class DbConnectionExtensions
	{
		public static IDbCommand CreateCommand(this IDbConnection connection, string commandText, CommandType commandType = CommandType.Text, IDbTransaction transaction = null)
		{
			var command = connection.CreateCommand();
			command.CommandText = commandText;
			command.CommandType = commandType;
			if (transaction != null)
			{
				command.Transaction = transaction;
			}
			return command;
		}

		public static int ExecuteNonQuery(this IDbConnection connection, string commandText, CommandType commandType = CommandType.Text, IDbTransaction transaction = null)
		{
			using (var command = connection.CreateCommand(commandText, commandType, transaction))
			{
				return command.ExecuteNonQuery();
			}
		}

		public static IDisposable OpenAndTrack(this IDbConnection connection)
		{
			if (connection.State == ConnectionState.Closed)
			{
				connection.Open();
				return new DbConnectionCloser(connection);
			}
			else
			{
				return new Nop();
			}
		}

		private class DbConnectionCloser : IDisposable
		{
			private readonly IDbConnection connection;

			public DbConnectionCloser(IDbConnection connection)
			{
				this.connection = connection;
			}

			void IDisposable.Dispose()
			{
				connection.Close();
			}
		}

		private class Nop : IDisposable
		{
			void IDisposable.Dispose()
			{
			}
		}
	}
}