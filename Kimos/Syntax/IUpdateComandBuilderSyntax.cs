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

using System.Linq.Expressions;

namespace Kimos.Syntax
{
    public interface IUpdateComandBuilderSyntax<TEntity, TParams> : IUpdateWhereComandBuilderSyntax<TEntity, TParams>
    {
        /// <summary>
        /// Restricts which rows will be affected by the command.
        /// </summary>
        /// <param name="predicate">An expression that specifies whether a row is to be affected.</param>
        IUpdateWhereComandBuilderSyntax<TEntity, TParams> Where(Expression<PredicateSpecificationDelegate<TEntity, TParams>> predicate);
    }
}