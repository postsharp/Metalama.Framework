class Target
    {
        public static void Foo()
{
    Foo_Override6();
}


public static void Foo_Override0()
{
    // Should invoke source code.
    Target.Bar_Source();
    // Should invoke source code.
    Target.Bar_Source();
    // Should invoke source code.
    Target.Bar_Source();
    // Should invoke the final declaration.
    Target.Bar();
}

public static void Foo_Override2()
{
    // Should invoke source code.
    Target.Bar_Source();
    // Should invoke override 1.
    Target.Bar_Override1();
    // Should invoke override 1.
    Target.Bar_Override1();
    // Should invoke the final declaration.
    Target.Bar();
}

public static void Foo_Override4()
{
    // Should invoke source code.
    Target.Bar_Source();
    // Should invoke override 3.
    Target.Bar_Override3();
    // Should invoke override 3.
    Target.Bar_Override3();
    // Should invoke the final declaration.
    Target.Bar();
}

public static void Foo_Override6()
{
    // Should invoke source code.
    Target.Bar_Source();
    // Should invoke the final declaration.
    Target.Bar();
    // Should invoke the final declaration.
    Target.Bar();
    // Should invoke the final declaration.
    Target.Bar();
}
        static void Bar()
{
    Bar_Override5();
}

private static void Bar_Source()
        {
            Console.WriteLine("This is original code.");
        }


static void Bar_Override1()
{
    // Should invoke source code.
    Target.Bar_Source();
    // Should invoke source code.
    Target.Bar_Source();
    // Should invoke override 1.
    Target.Bar_Override1();
    // Should invoke the final declaration.
    Target.Bar();
}

static void Bar_Override3()
{
    // Should invoke source code.
    Target.Bar_Source();
    // Should invoke override 1.
    Target.Bar_Override1();
    // Should invoke override 3.
    Target.Bar_Override3();
    // Should invoke the final declaration.
    Target.Bar();
}

static void Bar_Override5()
{
    // Should invoke source code.
    Target.Bar_Source();
    // Should invoke override 3.
    Target.Bar_Override3();
    // Should invoke the final declaration.
    Target.Bar();
    // Should invoke the final declaration.
    Target.Bar();
}    }