class Target
    {
        void Foo()
{
    this.Foo_Override2();
}

private void __Foo__OriginalImpl()
        {
            Console.WriteLine( "Original");
        }


void Foo_Override2()
{
    Console.WriteLine("Before2");
    Console.WriteLine("Before1");
    this.__Foo__OriginalImpl();
    Console.WriteLine("After1");
    Console.WriteLine("After2");
}    }