private int Method( int a, string c, DateTime dt )
{
    global::System.Console.WriteLine($"type=object");
    var value = Test((sbyte)1, 1D, 1F, "s\"\n" 1M, 1L, 1UL, (byte)1, (sbyte)1, (short)1, (ushort)1, new global::System.Object[]{1, 2, 3}, a, typeof(global::System.Int32));
    return default;
}