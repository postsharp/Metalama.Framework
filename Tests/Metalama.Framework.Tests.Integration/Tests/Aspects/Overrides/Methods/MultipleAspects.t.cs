    internal class TargetClass
    {
        [InnerOverride]
        [OuterOverride]
        public void TargetMethod_Void()
        {
    global::System.Console.WriteLine("This is the outer overriding template method.");
        global::System.Console.WriteLine("This is the inner overriding template method.");
                Console.WriteLine("This is the original method.");
    goto __aspect_return_1;

__aspect_return_1:    return;
        }

        [InnerOverride]
        [OuterOverride]
        public void TargetMethod_Void(int x, int y)
        {
    global::System.Console.WriteLine("This is the outer overriding template method.");
        global::System.Console.WriteLine("This is the inner overriding template method.");
                Console.WriteLine($"This is the original method {x} {y}.");
    goto __aspect_return_1;

__aspect_return_1:    return;
        }

        [InnerOverride]
        [OuterOverride]
        public int TargetMethod_Int()
        {
    global::System.Console.WriteLine("This is the outer overriding template method.");
        global::System.Console.WriteLine("This is the inner overriding template method.");
                Console.WriteLine("This is the original method.");
            return 42;
        }

        [InnerOverride]
        [OuterOverride]
        public int TargetMethod_Int(int x, int y)
        {
    global::System.Console.WriteLine("This is the outer overriding template method.");
        global::System.Console.WriteLine("This is the inner overriding template method.");
                Console.WriteLine($"This is the original method {x} {y}.");
            return x + y;
        }
    }