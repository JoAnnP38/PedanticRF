// <copyright file="PedanticFormat.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

using Pedantic.Utilities;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

using File = Pedantic.Chess.File;

namespace Pedantic.Chess.DataGen
{
    // sizeof(PedanticFormat) == 42
    [StructLayout(LayoutKind.Explicit, Size = 42)]
    public unsafe struct PedanticFormat : 
        IComparable<PedanticFormat>, 
        IEquatable<PedanticFormat>, 
        IFileRecord<PedanticFormat>
    {
        [FieldOffset(0)] private ulong hash;
        [FieldOffset(8)] private Bitboard occupancy;
        [FieldOffset(16)] private fixed byte pieces[16];
        // | LSB              <--- 32 bits --->                MSB |
        // xxxxxxxxxx   xxxxxxxxxx  xxxx        xxxx     xx   x    x
        // ply          max-ply     castling    ep file  wdl  stm  filter
        [FieldOffset(32)] private uint bits;
        [FieldOffset(36)] private short eval;
        [FieldOffset(38)] private ushort moveCounter;
        [FieldOffset(40)] private byte halfMoveClock;
        [FieldOffset(41)] private byte extra;

        [FieldOffset(0)] private fixed byte rec[42];

        public ulong Hash
        {
            get => hash;
            set => hash = value;
        }

        public Bitboard Occupancy
        {
            get => occupancy;
            set => occupancy = value;
        }

        public short Eval
        {
            get => eval;
            set => eval = value;
        }

        public ushort MoveCounter
        {
            get => moveCounter;
            set => moveCounter = value;
        }

        public byte HalfMoveClock
        {
            get => halfMoveClock;
            set => halfMoveClock = value;
        }

        public byte Extra
        {
            get => extra;
            set => extra = value;
        }

        public ushort Ply
        {
            get => (ushort)Bmi1.BitFieldExtract(bits, 0, 10);
            set => bits = BitFieldSet(bits, Math.Clamp(value, (ushort)0, (ushort)1024), 0, 10);
        }

        public ushort MaxPly
        {
            get => (ushort)Bmi1.BitFieldExtract(bits, 10, 10);
            set => bits = BitFieldSet(bits, Math.Clamp(value, (ushort)0, (ushort)1024), 10, 10);
        }

        public CastlingRights Castling
        {
            get => (CastlingRights)Bmi1.BitFieldExtract(bits, 20, 4);
            set => bits = BitFieldSet(bits, (byte)value, 20, 4);
        }

        public File EpFile
        {
            get => (File)((int)Bmi1.BitFieldExtract(bits, 24, 4) - 1);
            set => bits = BitFieldSet(bits, (uint)(value + 1), 24, 4);
        }

        public Result Wdl
        {
            get => (Result)(Bmi1.BitFieldExtract(bits, 28, 2) - 1);
            set => bits = BitFieldSet(bits, (uint)value + 1, 28, 2);
        }

        public Color Stm
        {
            get => (Color)Bmi1.BitFieldExtract(bits, 30, 1);
            set => bits = BitFieldSet(bits, (uint)value, 30, 1);
        }

        public bool Filter
        {
            get => Bmi1.BitFieldExtract(bits, 31, 1) == 1;
            set => bits = BitFieldSet(bits, (uint)(value ? 1 : 0), 31, 1);
        }

        public Span<byte> Pieces
        {
            get
            {
                Span<byte> pieceSpan;
                fixed (byte* p = pieces)
                {
                    pieceSpan = MemoryMarshal.CreateSpan(ref p[0], 16);

                }
                return pieceSpan;
            }

            set
            {
                for (int n = 0; n < Math.Min(value.Length, 16); n++)
                {
                    pieces[n] = value[n];
                }
            }
        }

        public Span<byte> Rec
        {
            get
            {
                Span<byte> recSpan;
                fixed (void* p = rec)
                {
                    recSpan = new Span<byte>(p, 42);
                }
                return recSpan;
            }
        }

        public void SetPiece(int index, Color color, Piece piece)
        {
            Util.Assert(index >= 0 && index < 32);
            Util.Assert(color != Color.None);
            Util.Assert(piece != Piece.None);

            int n = index >> 1;
            bool upper = (index & 1) != 0;

            byte pcVal = (byte)((int)color << 3 | (int)piece);
            if (upper)
            {
                pcVal <<= 4;
            }
            byte mask = (byte)(upper ? 0x0f : 0xf0);
            pieces[n] = (byte)(pieces[n] & mask | pcVal);
        }

        public (Color Color, Piece Piece) GetPiece(int index)
        {
            Util.Assert(index >= 0 && index < 32);

            int n = index >> 1;
            bool upper = (index & 1) != 0;
            byte mask = (byte)(upper ? 0xf0 : 0x0f);
            int shift = upper ? 4 : 0;
            byte pcVal = (byte)((pieces[n] & mask) >> shift);
            return ((pcVal & 0x08) != 0 ? Color.Black : Color.White, (Piece)(pcVal & 0x07));
        }

        public static long Size
        {
            get => 42;
        }

        public static bool Load(BinaryReader br, ref PedanticFormat prec)
        {
            return br.Read(prec.Rec) == prec.Rec.Length;
        }

        public static void Store(BinaryWriter bw, ref PedanticFormat prec)
        {
            bw.Write(prec.Rec);
        }

        internal static uint BitFieldSet(uint bits, uint value, byte start, byte length)
        {
            uint mask = (1u << length) - 1 << start;
            return bits & ~mask | value << start & mask;
        }

        public int CompareTo(PedanticFormat pdata)
        {
            int compare = hash.CompareTo(pdata.hash);
            if (compare != 0)
            {
                return compare;
            }

            compare = occupancy.CompareTo(pdata.occupancy);
            if (compare != 0)
            {
                return compare;
            }

            compare = Stm.CompareTo(pdata.Stm);
            if (compare != 0)
            {
                return compare;
            }

            return Pieces.SequenceCompareTo(pdata.Pieces);
        }

        public bool Equals(PedanticFormat other)
        {
            return Rec.SequenceEqual(other.Rec);
        }
    }


}
