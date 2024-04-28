// <copyright file="Mem.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Utilities
{
    public static class Mem
    {
        public static T[][] Allocate2D<T>(int size1, int size2)
        {
            T[][] obj = new T[size1][];
            for (int i = 0; i < size1; ++i)
            {
                obj[i] = new T[size2];
            }

            return obj;
        }

        public static void Copy<T>(T[][] source, T[][] destination)
        {
            if (source.Length != destination.Length)
            {
                throw new InvalidOperationException(@"Cannot copy jagged array of different sizes.");
            }

            for (int i = 0; i < source.Length; ++i)
            {
                Array.Copy(source[i], destination[i], source[i].Length);
            }
        }

        public static void Clear<T>(T[][] array)
        {
            for (int i = 0; i < array.Length; ++i)
            {
                Array.Clear(array[i]);
            }
        }

        public static void Fill<T>(T[][] array, T fillValue)
        {
            for (int i = 0; i < array.Length; ++i)
            {
                Array.Fill(array[i], fillValue);
            }
        }

    }
}
