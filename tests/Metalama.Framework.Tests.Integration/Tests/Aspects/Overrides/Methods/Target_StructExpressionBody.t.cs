    internal struct TargetStruct
    {
        [Override]
        public void TargetMethod_Void()
            
{ 
    global::System.Console.WriteLine("This is the overriding method.");
    Console.WriteLine("This is the original method.");goto __aspect_return_1;
__aspect_return_1:    return;

}

        [Override]
        public void TargetMethod_Void(int x, int y)
            
{ 
    global::System.Console.WriteLine("This is the overriding method.");
    Console.WriteLine($"This is the original method {x} {y}.");goto __aspect_return_1;
__aspect_return_1:    return;

}        

        [Override]
        public int TargetMethod_Int()
            
{ 
    global::System.Console.WriteLine("This is the overriding method.");
    return 42;
}

        [Override]
        public int TargetMethod_Int(int x, int y)
            
{ 
    global::System.Console.WriteLine("This is the overriding method.");
    return x + y;
}

        [Override]
        public static void TargetMethod_Static()
            
{ 
    global::System.Console.WriteLine("This is the overriding method.");
    Console.WriteLine("This is the original static method.");goto __aspect_return_1;
__aspect_return_1:    return;

}

        [Override]
        public void TargetMethod_Out(out int x)
            
{ 
    global::System.Console.WriteLine("This is the overriding method.");
    x = 42;goto __aspect_return_1;
__aspect_return_1:    return;

}

        [Override]
        public void TargetMethod_Ref(ref int x)
            
{ 
    global::System.Console.WriteLine("This is the overriding method.");
    x = 42;goto __aspect_return_1;
__aspect_return_1:    return;

}        

        [Override]
        public void TargetMethod_In(in DateTime x)
            
{ 
    global::System.Console.WriteLine("This is the overriding method.");
    Console.WriteLine($"This is the original method {x}.");goto __aspect_return_1;
__aspect_return_1:    return;

}
    }