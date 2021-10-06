class Target
    {
        public static void Foo()
{
    Foo_Override6();
}


public static void Bar()
{
    Bar_Override5();
}
private static void Bar_Empty()
{}

public static void Foo_Override0()
{
    // Should invoke empty code.
    Target.Bar_Empty();
    // Should invoke empty code.
    Target.Bar_Empty();
    // Should invoke empty code.
    Target.Bar_Empty();
    // Should invoke the final declaration.
    Target.Bar();
}

public static void Foo_Override2()
{
    // Should invoke empty code.
    Target.Bar_Empty();
    // Should invoke override 1.
    Target.Bar_Override1();
    // Should invoke override 1.
    Target.Bar_Override1();
    // Should invoke the final declaration.
    Target.Bar();
}

public static void Foo_Override4()
{
    // Should invoke empty code.
    Target.Bar_Empty();
    // Should invoke override 3.
    Target.Bar_Override3();
    // Should invoke override 3.
    Target.Bar_Override3();
    // Should invoke the final declaration.
    Target.Bar();
}

public static void Foo_Override6()
{
    // Should invoke empty code.
    Target.Bar_Empty();
    // Should invoke the final declaration.
    Target.Bar();
    // Should invoke the final declaration.
    Target.Bar();
    // Should invoke the final declaration.
    Target.Bar();
}

private static void Bar_Override5()
{
    // Should invoke empty code.
    Target.Bar_Empty();
    // Should invoke override 3.
    Target.Bar_Override3();
    // Should invoke the final declaration.
    Target.Bar();
    // Should invoke the final declaration.
    Target.Bar();
}

private static void Bar_Override3()
{
    // Should invoke empty code.
    Target.Bar_Empty();
    // Should invoke override 1.
    Target.Bar_Override1();
    // Should invoke override 3.
    Target.Bar_Override3();
    // Should invoke the final declaration.
    Target.Bar();
}

private static void Bar_Override1()
{
    // Should invoke empty code.
    Target.Bar_Empty();
    // Should invoke empty code.
    Target.Bar_Empty();
    // Should invoke override 1.
    Target.Bar_Override1();
    // Should invoke the final declaration.
    Target.Bar();
}    }