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
using System.Collections;
using System.Collections.Generic;

namespace Kimos.Drivers
{
    public class DatabaseDriverRegistry : IDatabaseDriverRegistry, IEnumerable
    {
        private readonly Dictionary<string, IDatabaseDriver> driversByProviderName = new Dictionary<string, IDatabaseDriver>();

        public IDatabaseDriver SelectDriver(string providerName)
        {
            return driversByProviderName.TryGetValue(providerName, out var driver)
                ? driver
                : throw new ArgumentException($"No driver is registered for database provider {providerName}");
        }

        public void Add(string providerName, IDatabaseDriver driver)
        {
            driversByProviderName.Add(providerName, driver);
        }

        IEnumerator IEnumerable.GetEnumerator() => driversByProviderName.GetEnumerator();
    }
}
