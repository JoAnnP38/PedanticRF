namespace Pedantic.Utilities
{
    public static class ArrayEx
    {
        public static T[] Clone<T>(T[] array)
        {
            var clone = new T[array.Length];
            Array.Copy(array, clone, array.Length);
            return clone;
        }
    }
}
