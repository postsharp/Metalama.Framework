public class Target
{
    [Aspect1]
    [Aspect2]
    public void Foo()
    {
        Console.WriteLine("Aspect1");
        Console.WriteLine("Aspect2");
        Console.WriteLine("Foo");
        goto __aspect_return_1;
    __aspect_return_1:
        return;
    }

    [Aspect1]
    [Aspect2]
    public int Bar()
    {
        Console.WriteLine("Aspect1");

        Console.WriteLine("Aspect2");

        Console.WriteLine("Bar");
        return 42;
    }
}