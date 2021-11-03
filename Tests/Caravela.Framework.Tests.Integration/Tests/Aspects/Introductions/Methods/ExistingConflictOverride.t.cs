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


public override global::System.Int32 ExistingBaseMethod()
{
    global::System.Console.WriteLine("This is introduced method.");
    return base.ExistingBaseMethod();
}

public global::System.Int32 NotExistingMethod()
{
    global::System.Console.WriteLine("This is introduced method.");
    return default(global::System.Int32);
}

public static global::System.Int32 NotExistingMethod_Static()
{
    global::System.Console.WriteLine("This is introduced method.");
    return default(global::System.Int32);
}    }