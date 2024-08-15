// <copyright file="Interfaces.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Chess
{
    public interface IHistory
    {
        public short this[Color stm, Piece piece, SquareIndex to] { get; }

        public short this[Move move] { get; }
    }

    public interface IInitialize
    {
        public static void Initialize()
        { }
    }

    public interface IEfficientlyUpdatable
    {
        public void Clear();

        public void Copy(IEfficientlyUpdatable updatable);

        public void AddPiece(Color color, Piece piece, SquareIndex sq);

        public void RemovePiece(Color color, Piece piece, SquareIndex sq);

        public void Update(Board board);

        public void UpdatePiece(Color color, Piece piece, SquareIndex from, SquareIndex to);

        public void PushState();

        public void PopState();

        public void RestoreState();
    }
}
