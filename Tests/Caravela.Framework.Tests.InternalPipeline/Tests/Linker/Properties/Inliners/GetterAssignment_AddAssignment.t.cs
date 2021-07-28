class Target
    {
        int Foo
{get    {
        Console.WriteLine("Before");
        int x = 0;
        x += this.__Foo__OriginalImpl;
        Console.WriteLine("After");
        return x;
    }
}

private int __Foo__OriginalImpl
        {
            get
            {
                Console.WriteLine( "Original");
                return 42;
            }
        }
    }