class Target
    {

        void Foo()
{
    Console.WriteLine("Before");
    this.__Foo__OriginalImpl();
    Console.WriteLine("After");
}

private void __Foo__OriginalImpl()
        {
            Console.WriteLine( "Original");
        }
    }