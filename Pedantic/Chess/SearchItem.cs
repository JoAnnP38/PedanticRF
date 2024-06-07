// <copyright file="SearchItem.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Chess
{
    public struct SearchItem
    {
        public Move Move;
        public bool IsCheckingMove;
        public MovePair Killers;
        public short Eval;
        public short[]? Continuation;

        public SearchItem()
        {
            Move = Move.NullMove;
            IsCheckingMove = false;
            Killers = new();
            Eval = NO_SCORE;
            Continuation = null;
        }
    }
}
