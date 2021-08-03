class Target
    {
        int Foo()
{
    Console.WriteLine("Before");
    int x;
            Console.WriteLine( "Original");
x=42;
goto __aspect_return_1;
__aspect_return_1:    Console.WriteLine("After");
    return x;
}
    }