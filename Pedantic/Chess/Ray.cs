// <copyright file="Ray.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Chess
{
    using System.Runtime.CompilerServices;

    public readonly struct Ray
    {
        [InlineArray(MAX_DIRECTIONS)]
        private struct DirArray
        {
            public Bitboard _element0;
        }

        private readonly DirArray rays;

        public Ray(ulong north, ulong northEast, ulong east, ulong southEast, ulong south, ulong southWest, ulong west, ulong northWest)
        {
            North = (Bitboard)north;
            NorthEast = (Bitboard)northEast;
            East = (Bitboard)east;
            SouthEast = (Bitboard)southEast;
            South = (Bitboard)south;
            SouthWest = (Bitboard)southWest;
            West = (Bitboard)west;
            NorthWest = (Bitboard)northWest;
        }

        public Bitboard North 
        {
            get => rays[(int)Direction.North];
            init => rays[(int)Direction.North] = value;
        }

        public Bitboard NorthEast
        {
            get => rays[(int)Direction.NorthEast];
            init => rays[(int)Direction.NorthEast] = value;
        }

        public Bitboard East
        {
            get => rays[(int)Direction.East];
            init => rays[(int)Direction.East] = value;
        }

        public Bitboard SouthEast
        {
            get => rays[(int)Direction.SouthEast];
            init => rays[(int)Direction.SouthEast] = value;
        }

        public Bitboard South
        {
            get => rays[(int)Direction.South];
            init => rays[(int)Direction.South] = value;
        }

        public Bitboard SouthWest
        {
            get => rays[(int)Direction.SouthWest];
            init => rays[(int)Direction.SouthWest] = value;
        }

        public Bitboard West
        {
            get => rays[(int)Direction.West];
            init => rays[(int)Direction.West] = value;
        }

        public Bitboard NorthWest
        {
            get => rays[(int)Direction.NorthWest];
            init => rays[(int)Direction.NorthWest] = value;
        }

        public Bitboard this[Direction index] => rays[(int)index];
    }
}

