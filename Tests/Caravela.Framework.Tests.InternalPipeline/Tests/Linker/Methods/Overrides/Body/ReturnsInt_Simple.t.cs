class Target
    {
        int Foo(int x)
{
    Console.WriteLine("Before");
    int result;
            Console.WriteLine( "Original");
result=x;
goto __aspect_return_1;
__aspect_return_1:    Console.WriteLine("After");
    return result;
}
    }