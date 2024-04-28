// <copyright file="ArrayEx.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Utilities
{
    public static class ArrayEx
    {
        public static T[] Clone<T>(T[] array)
        {
            var clone = new T[array.Length];
            Array.Copy(array, clone, array.Length);
            return clone;
        }
    }
}
