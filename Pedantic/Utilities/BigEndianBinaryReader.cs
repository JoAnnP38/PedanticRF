// <copyright file="BigEndianBinaryReader.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Utilities
{
    using System.Text;

    public class BigEndianBinaryReader : BinaryReader
    {
        public BigEndianBinaryReader(Stream stream)
            : base(stream, Encoding.ASCII)
        { }

        public override ushort ReadUInt16()
        {
            byte[] data = ReadBytes(2);
            Array.Reverse(data);
            return BitConverter.ToUInt16(data);
        }

        public override uint ReadUInt32()
        {
            byte[] data = ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToUInt32(data);
        }

        public override ulong ReadUInt64()
        {
            byte[] data = ReadBytes(8);
            Array.Reverse(data);
            return BitConverter.ToUInt64(data);
        }
    }
}
