class Target
    {
        int Foo()
{
    Console.WriteLine("Before");
            Console.WriteLine( "Original");
_=42;
goto __aspect_return_1;
__aspect_return_1:    Console.WriteLine("After");
    return 42;
}
    }