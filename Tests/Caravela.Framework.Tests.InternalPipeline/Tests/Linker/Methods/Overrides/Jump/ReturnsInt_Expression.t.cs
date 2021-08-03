class Target
    {
        int Foo(int x)
{return this.__Foo__OriginalImpl(x);}

private int __Foo__OriginalImpl(int x)
        {
            Console.WriteLine( "Original Start");
            if (x == 0)
            {
                return 42;
            }
            Console.WriteLine( "Original End");
            return x;
        }
    }