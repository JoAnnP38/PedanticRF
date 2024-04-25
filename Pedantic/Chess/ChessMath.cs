using System.Runtime.CompilerServices;
using Pedantic.Utilities;
using static Pedantic.Chess.Constants;

namespace Pedantic.Chess
{
    public static class ChessMath
    {
        static ChessMath()
        {
            for (SquareIndex from = SquareIndex.A1; from <= SquareIndex.H8; from++)
            {
                for (SquareIndex to = SquareIndex.A1; to <= SquareIndex.H8; to++)
                {
                    directionBetween[(int)from * MAX_SQUARES + (int)to] = GetDirection(from, to);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SquareIndex ToSquareIndex(File file, Rank rank)
        {
            return file == File.None || rank == Rank.None ? 
                SquareIndex.None : (SquareIndex)((int)file + ((int)rank * 8));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SquareIndex ToSquareIndex(int file, int rank)
        {
            Util.Assert(IsValidCoord(file));
            Util.Assert(IsValidCoord(rank));
            return ToSquareIndex((File)file, (Rank)rank);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Distance(File f1, Rank r1, File f2, Rank r2)
        {
            return Math.Max(Math.Abs(f1 - f2), Math.Abs(r1 - r2));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Distance(int f1, int r1, int f2, int r2)
        {
            Util.Assert(IsValidCoord(f1));
            Util.Assert(IsValidCoord(r1));
            Util.Assert(IsValidCoord(f2));
            Util.Assert(IsValidCoord(r2));
            return Distance((File)f1, (Rank)r1, (File)f2, (Rank)r2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Distance(SquareIndex sq1, SquareIndex sq2)
        {
            return Distance(sq1.File(), sq1.Rank(), sq2.File(), sq2.Rank());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Distance(int sq1, int sq2)
        {
            Util.Assert(IsValidSquare(sq1));
            Util.Assert(IsValidSquare(sq2));
            return Distance((SquareIndex)sq1, (SquareIndex)sq2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ManhattanDistance(File f1, Rank r1, File f2, Rank r2)
        {
            return Math.Abs(f1 - f2) + Math.Abs(r1 - r2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ManhattanDistance(int f1, int r1, int f2, int r2)
        {
            Util.Assert(IsValidCoord(f1));
            Util.Assert(IsValidCoord(r1));
            Util.Assert(IsValidCoord(f2));
            Util.Assert(IsValidCoord(r2));
            return ManhattanDistance((File)f1, (Rank)r1, (File)f2, (Rank)r2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ManhattanDistance(SquareIndex sq1, SquareIndex sq2)
        {
            return ManhattanDistance(sq1.File(), sq1.Rank(), sq2.File(), sq2.Rank());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ManhattanDistance(int sq1, int sq2)
        {
            Util.Assert(IsValidSquare(sq1));
            Util.Assert(IsValidSquare(sq2));
            return ManhattanDistance((SquareIndex)sq1, (SquareIndex)sq1);
        }

        public static int CenterDistance(SquareIndex sq)
        {
            File ctrFile = (File)(((int)sq.File() / 4) + 3);
            Rank ctrRank = (Rank)(((int)sq.Rank() / 4) + 3);
            return Distance(sq, ToSquareIndex(ctrFile, ctrRank));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetDirection(SquareIndex from, SquareIndex to, out Direction direction)
        {
            Util.Assert(from > SquareIndex.None);
            Util.Assert(to > SquareIndex.None);
            direction = directionBetween[(int)from * MAX_SQUARES + (int)to];
            return direction != Direction.None;
        }

        private static Direction GetDirection(SquareIndex from, SquareIndex to)
        {
            Direction direction = Direction.None;
            if (from == to)
            {
                return direction;
            }

            var fromCoords = from.ToCoords();
            var toCoords = to.ToCoords();
            int fileDiff = toCoords.File - fromCoords.File;
            int rankDiff = toCoords.Rank - fromCoords.Rank;

            if (fileDiff == 0)
            {
                direction = rankDiff > 0 ? Direction.North : Direction.South;
            }
            else if (rankDiff == 0)
            {
                direction = fileDiff > 0 ? Direction.East : Direction.West;
            }
            else if (fileDiff + rankDiff == 0)
            {
                direction = rankDiff > 0 ? Direction.NorthWest : Direction.SouthEast;
            }
            else if (fileDiff - rankDiff == 0)
            {
                direction = rankDiff > 0 ? Direction.NorthEast : Direction.SouthWest;
            }
            return direction;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidCoord(int coord)
        {
            return coord >= 0 && coord < MAX_COORDS;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidSquare(int sq)
        {
            return sq >= 0 && sq < MAX_SQUARES;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rank Min(Rank r1, Rank r2)
        {
            return r1 < r2 ? r1 : r2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rank Max(Rank r1, Rank r2)
        {
            return r1 > r2 ? r1 : r2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static File Min(File f1, File f2)
        {
            return f1 < f2 ? f1 : f2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static File Max(File f1, File f2)
        {
            return f1 > f2 ? f1 : f2;
        }
        private static Direction[] directionBetween = new Direction[ MAX_SQUARES * MAX_SQUARES ];
    }
}
