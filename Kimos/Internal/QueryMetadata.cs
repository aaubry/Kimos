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
using System.Linq.Expressions;
using Kimos.Drivers;

namespace Kimos.Internal
{
    internal class QueryMetadata : IQueryMetadata
	{
		public string TableName { get; set; }
		public string TableSchema { get; set; }
		public IDictionary<string, string> ColumnMappings { get; set; }

		public string GetParameterName(MemberExpression node) => node.Member.Name;
	}

	internal class ParameterInfo<TParams> : IParameterInfo<TParams>
    {
		public Type Type { get; private set; }
		public Func<TParams, object> Accessor { get; private set; }

		public ParameterInfo(Type type, Func<TParams, object> accessor)
		{
			Type = type;
			Accessor = accessor;
		}
	}
}