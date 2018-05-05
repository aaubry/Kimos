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

using Kimos.Syntax;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace Kimos.Tests
{
    internal static class TestLoggingExtensions
    {
        public static TSyntax Log<TSyntax>(this TSyntax syntax, DbContext dbContext, ITestOutputHelper output)
            where TSyntax : ICommandTextBuilderSyntax
        {
            var text = syntax.BuildCommandText(dbContext);
            output.WriteLine(text);
            return syntax;
        }
    }
}
