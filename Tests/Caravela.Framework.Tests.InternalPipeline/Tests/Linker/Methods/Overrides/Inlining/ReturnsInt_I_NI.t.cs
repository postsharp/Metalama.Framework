class Target
    {
        int Foo()
{
    Console.WriteLine("Before");
    int result;
    result = this.__Foo__OriginalImpl();
    Console.WriteLine("After");
    return result;
}

private int __Foo__OriginalImpl()
        {
            Console.WriteLine( "Original");
            return 42;
        }
    }