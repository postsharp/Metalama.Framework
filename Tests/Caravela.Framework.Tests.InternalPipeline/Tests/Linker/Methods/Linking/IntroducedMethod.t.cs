class Target
    {
        public void Foo()
{
    this.Foo_Override6();
}


public void Bar()
{
    this.Bar_Override5();
}
private void Bar_Empty()
{}

public void Foo_Override0()
{
    // Should invoke empty code.
    this.Bar_Empty();
    // Should invoke empty code.
    this.Bar_Empty();
    // Should invoke empty code.
    this.Bar_Empty();
    // Should invoke the final declaration.
    this.Bar();
}

public void Foo_Override2()
{
    // Should invoke empty code.
    this.Bar_Empty();
    // Should invoke override 1.
    this.Bar_Override1();
    // Should invoke override 1.
    this.Bar_Override1();
    // Should invoke the final declaration.
    this.Bar();
}

public void Foo_Override4()
{
    // Should invoke empty code.
    this.Bar_Empty();
    // Should invoke override 3.
    this.Bar_Override3();
    // Should invoke override 3.
    this.Bar_Override3();
    // Should invoke the final declaration.
    this.Bar();
}

public void Foo_Override6()
{
    // Should invoke empty code.
    this.Bar_Empty();
    // Should invoke the final declaration.
    this.Bar();
    // Should invoke the final declaration.
    this.Bar();
    // Should invoke the final declaration.
    this.Bar();
}

private void Bar_Override1()
{
    // Should invoke empty code.
    this.Bar_Empty();
    // Should invoke empty code.
    this.Bar_Empty();
    // Should invoke override 1.
    this.Bar_Override1();
    // Should invoke the final declaration.
    this.Bar();
}

private void Bar_Override3()
{
    // Should invoke empty code.
    this.Bar_Empty();
    // Should invoke override 1.
    this.Bar_Override1();
    // Should invoke override 3.
    this.Bar_Override3();
    // Should invoke the final declaration.
    this.Bar();
}

private void Bar_Override5()
{
    // Should invoke empty code.
    this.Bar_Empty();
    // Should invoke override 3.
    this.Bar_Override3();
    // Should invoke the final declaration.
    this.Bar();
    // Should invoke the final declaration.
    this.Bar();
}    }
