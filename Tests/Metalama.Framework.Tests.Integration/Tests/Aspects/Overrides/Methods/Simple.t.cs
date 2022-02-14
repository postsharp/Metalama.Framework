internal class TargetClass
    {
        [Override]
        public void TargetMethod_Void()
{
    global::System.Console.WriteLine("This is the overriding method.");
            Console.WriteLine("This is the original method.");
}
    
        [Override]
        public void TargetMethod_Void(int x, int y)
{
    global::System.Console.WriteLine("This is the overriding method.");
            Console.WriteLine($"This is the original method {x} {y}.");
}
    
        [Override]
        public int TargetMethod_Int()
{
    global::System.Console.WriteLine("This is the overriding method.");
            Console.WriteLine("This is the original method.");
            return 42;
}
    
        [Override]
        public int TargetMethod_Int(int x, int y)
{
    global::System.Console.WriteLine("This is the overriding method.");
            Console.WriteLine($"This is the original method {x} {y}.");
            return x + y;
}
    
        [Override]
        public static void TargetMethod_Static()
{
    global::System.Console.WriteLine("This is the overriding method.");
            Console.WriteLine("This is the original static method.");
}
    
        [Override]
        public void TargetMethod_Out(out int x)
{
    global::System.Console.WriteLine("This is the overriding method.");
            Console.WriteLine("This is the original method.");
            x = 42;
}
    
        [Override]
        public void TargetMethod_Ref(ref int x)
{
    global::System.Console.WriteLine("This is the overriding method.");
            Console.WriteLine($"This is the original method {x}.");
            x = 42;
}
    
        [Override]
        public void TargetMethod_In(in DateTime x)
{
    global::System.Console.WriteLine("This is the overriding method.");
            Console.WriteLine($"This is the original method {x}.");
}
    }