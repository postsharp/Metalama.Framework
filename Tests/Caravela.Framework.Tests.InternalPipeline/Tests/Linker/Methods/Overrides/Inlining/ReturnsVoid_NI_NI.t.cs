class Target
    {
        void Foo()
{
    this.Foo_Override();
}

private void __Foo__OriginalImpl()
        {
            Console.WriteLine( "Original");
        }


void Foo_Override()
{
    Console.WriteLine("Before");
    this.__Foo__OriginalImpl();
    Console.WriteLine("After");
}    }