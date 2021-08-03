class Target
    {
        int Foo
{get    {
        Console.WriteLine("Before");
        return (short)this.__Foo__OriginalImpl;
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