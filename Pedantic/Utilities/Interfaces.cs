// <copyright file="Interfaces.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Utilities
{
    public interface IPooledObject<out T>
    {
        public void Clear();
    }
}
