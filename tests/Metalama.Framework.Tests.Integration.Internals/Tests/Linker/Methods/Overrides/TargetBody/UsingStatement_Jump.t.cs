class Target
{
    void Foo(int x)
    {
        Console.WriteLine("Before aspect2");
        using var z71 = new Disposable();
        using var z72 = new Disposable();
        if (x == 0)
        {
            return;
        }

        using var z81 = new Disposable();
        using var z82 = new Disposable();
        Console.WriteLine("Before aspect1");
        using (var z62 = new Disposable())
        {
            if (x == 0)
            {
                goto __aspect_return_1;
            }

            using (var z32 = new Disposable())
            {
                if (x == 0)
                {
                    goto __aspect_return_2;
                }

                Console.WriteLine("Before first dispose");

                Console.WriteLine("Before double dispose");

                Console.WriteLine("After dispose");
            }
        __aspect_return_2: Console.WriteLine("After aspect1");
        }
    __aspect_return_1: using var z91 = new Disposable();
        using var z92 = new Disposable();
        Console.WriteLine("After aspect2");
    }
}