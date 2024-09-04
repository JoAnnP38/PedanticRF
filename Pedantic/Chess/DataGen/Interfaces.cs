namespace Pedantic.Chess.DataGen
{
    public interface IFileRecord<T> where T : struct
    {
        public static abstract long Size { get; }
        public static abstract bool Load(BinaryReader reader, ref T rec);
        public static abstract void Store(BinaryWriter writer, ref T rec);
    }
}