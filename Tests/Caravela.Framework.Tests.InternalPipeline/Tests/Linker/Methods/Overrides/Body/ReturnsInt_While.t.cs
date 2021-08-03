class Target
    {
        int Foo(int x)
{
    Console.WriteLine("Before");
    int i = 0;
    int k = 0;
    while (i < 0)
    {
        int result;
            Console.WriteLine( "Original");
result=x;
goto __aspect_return_1;
__aspect_return_1:        k += result;
        i++;
    }

    Console.WriteLine("After");
    return k;
}
    }