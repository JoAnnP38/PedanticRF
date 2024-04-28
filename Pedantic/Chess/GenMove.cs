// <copyright file="GenMove.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Chess
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    public readonly struct GenMove : IEquatable<GenMove>
    {
        public GenMove(Move move, MoveGenPhase phase)
        {
            Move = move;
            MovePhase = phase;
        }

        public Move Move
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get; 

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            init;
        }

        public MoveGenPhase MovePhase
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            init;
        }

        public bool Equals(GenMove other)
        {
            return Move == other.Move && MovePhase == other.MovePhase;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj == null || obj is not GenMove)
            {
                return false;
            }
            return Equals((GenMove)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Move, MovePhase);
        }

        public static bool operator ==(GenMove lhs, GenMove rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(GenMove lhs, GenMove rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}