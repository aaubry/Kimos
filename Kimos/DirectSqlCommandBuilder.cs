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
using Kimos.Drivers;
using Kimos.Drivers.PostgreSql;
using Kimos.Drivers.SqlServer;
using Kimos.Internal;
using Kimos.Syntax;

namespace Kimos
{
    /// <summary>
    /// Provides a fluent syntax to generate and execute SQL commands agains a <see cref="Microsoft.EntityFrameworkCore.DbContext" />.
    /// </summary>
    public sealed class DirectSqlCommandBuilder
	{
        private readonly IDatabaseDriverRegistry drivers;

        /// <summary>
        /// Initializes a new instance of <see cref="DirectSqlCommandBuilder" /> using the built-in database drivers.
        /// </summary>
        public DirectSqlCommandBuilder()
            : this(new DatabaseDriverRegistry
            {
                { SqlServerDriver.ProviderName, new SqlServerDriver() },
                { PostgreSqlDriver.ProviderName, new PostgreSqlDriver() },
            })
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DirectSqlCommandBuilder" /> using the specified database drivers.
        /// </summary>
        /// <param name="drivers">The database drivers that should be used.</param>
        public DirectSqlCommandBuilder(IDatabaseDriverRegistry drivers)
        {
            this.drivers = drivers ?? throw new ArgumentNullException(nameof(drivers));
        }

        /// <summary>
        /// Creates a SQL command that targets the specified entity.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public ICommandBuilderSyntax<TEntity> CreateCommand<TEntity>() where TEntity : class
		{
			return new CommandBuilderSyntax<TEntity>(drivers);
		}
	}

    /// <summary>
    /// Specifies how an entity is to be inserted.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TParams">The type of the command parameters.</typeparam>
    /// <param name="parameters">The command parameters.</param>
    /// <returns>Returns a new entity initialized with the values that will be inserted.</returns>
    /// <example>
    /// p =&gt; new MyEntity
    /// {
    ///   Name = p.Name,
    ///   Version = p.Version,
    /// }
    /// </example>
	public delegate TEntity InsertSpecificationDelegate<TEntity, TParams>(TParams parameters);

    /// <summary>
    /// Specifies how an entity is to be updated.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TParams">The type of the command parameters.</typeparam>
    /// <param name="entity">The version of the entity prior to the update.</param>
    /// <param name="parameters">The command parameters.</param>
    /// <returns>Returns a new entity initialized with the values that will be updated.</returns>
    /// <example>
    /// (e, p) =&gt; new MyEntity
    /// {
    ///   Name = p.Name,
    ///   Version = e.Version + 1,
    /// }
    /// </example>
	public delegate TEntity UpdateSpecificationDelegate<TEntity, TParams>(TEntity entity, TParams parameters);

    /// <summary>
    /// Specifies a query filtering predicate.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TParams">The type of the command parameters.</typeparam>
    /// <param name="entity">The entity to be tested.</param>
    /// <param name="parameters">The command parameters.</param>
    /// <returns>Returns true if the entity is to be included, otherwise false.</returns>
    /// <example>
    /// (e, p) =&gt; e.Version &gt;= p.MinVersion
    /// </example>
    public delegate bool PredicateSpecificationDelegate<TEntity, TParams>(TEntity entity, TParams parameters);

    /// <summary>
    /// Specifies which values should be returned by a command.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TResult">The type of the object that will hold the values.</typeparam>
    /// <param name="inserted">The version of the entity after the insertion / update.</param>
    /// <returns>Returns a new object containing the values to be returned.</returns>
    /// <example>
    /// e => new
    /// {
    ///   e.Id,
    ///   e.Version,
    /// }
    /// </example>
    public delegate TResult OutputSpecificationDelegate<TEntity, TResult>(TEntity inserted);

    /// <summary>
    /// Specifies a set of columns from an entity.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entity">The entity from which the columsn are to be selected.</param>
    /// <returns>Returns a new object containing the properties of the entity that are to be selected.</returns>
    /// <example>
    /// e =&gt; new { e.Id, e.Name }
    /// </example>
    /// <example>
    /// e =&gt; e.Id
    /// </example>
    public delegate object ColumnSpecificationDelegate<TEntity>(TEntity entity);
}
