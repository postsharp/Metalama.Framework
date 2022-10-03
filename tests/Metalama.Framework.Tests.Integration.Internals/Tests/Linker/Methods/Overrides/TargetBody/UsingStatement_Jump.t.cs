class Target
{
    void Foo(int x)
    {
        Console.WriteLine("Before aspect");
        if (x == 0)
        {
            goto __aspect_return_1;
        }
        Console.WriteLine("Before first dispose");
        using (var z1 = new Disposable())
        {
            Console.WriteLine("Before double dispose");
            using var z2 = new Disposable();
            using var z3 = new Disposable();
            Console.WriteLine("After dispose");
        }
    __aspect_return_1:
      Console.WriteLine("After aspect");
    }
}