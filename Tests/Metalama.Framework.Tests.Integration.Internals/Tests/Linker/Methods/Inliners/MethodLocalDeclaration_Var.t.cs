class Target
    {
        int Foo()
{
    Console.WriteLine("Before");
global::System.Int32 x ;            Console.WriteLine( "Original");
x=42;
    Console.WriteLine("After");
    return x;
}
    }