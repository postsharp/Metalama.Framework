class Target
    {

        int Foo()
{
    Console.WriteLine("Before");
    return (short)this.__Foo__OriginalImpl();
}

private int __Foo__OriginalImpl()
        {
            Console.WriteLine( "Original");
            return 42;
        }
    }