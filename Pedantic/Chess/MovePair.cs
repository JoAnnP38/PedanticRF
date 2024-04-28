// <copyright file="MovePair.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Chess
{
    using System.Runtime.CompilerServices;

    public struct MovePair
    {
        private Move move1;
        private Move move2;

        public MovePair()
        {
            move1 = Move.NullMove;
            move2 = Move.NullMove;
        }

        public Move Move1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => move1;
        }

        public Move Move2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => move2;
        }

        public void Add(Move move)
        {
            if (move == move2)
            {
                (move1, move2) = (move2, move1);
            }
            else
            {
                move2 = move1;
                move1 = move;
            }
        }
    }
}
