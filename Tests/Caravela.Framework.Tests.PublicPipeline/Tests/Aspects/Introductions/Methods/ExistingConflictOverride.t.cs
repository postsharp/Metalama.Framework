[Introduction]
    internal class TargetClass : BaseClass
    {
        public int ExistingMethod()
{
    global::System.Console.WriteLine("This is introduced method.");
            return 27;
}
        public static int ExistingMethod_Static()
{
    global::System.Console.WriteLine("This is introduced method.");
            return 27;
}
    
public global::System.Int32 NonExistingMethod()
{
    global::System.Console.WriteLine("This is introduced method.");
    return default(global::System.Int32);
}
    
public static global::System.Int32 NonExistingMethod_Static()
{
    global::System.Console.WriteLine("This is introduced method.");
    return default(global::System.Int32);
}    }