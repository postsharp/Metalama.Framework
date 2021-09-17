class Target : Base
    {
        public void Foo()
{
    this.Foo_Override6();
}
    
    
public void Foo_Override0()
{
    // Should invoke base declaration.
    base.Bar();
    // Should invoke base declaration.
    base.Bar();
    // Should invoke base declaration.
    base.Bar();
    // Should invoke the final declaration.
    this.Bar();
}
    
public override void Bar()
{
    this.Bar_Override5();
}
    
private void Bar_Override1()
{
    // Should invoke base declaration.
    base.Bar();
    // Should invoke base declaration.
    base.Bar();
    // Should invoke override 1.
    this.Bar_Override1();
    // Should invoke the final declaration.
    this.Bar();
}
    
private void Bar_Override3()
{
    // Should invoke base declaration.
    base.Bar();
    // Should invoke override 1.
    this.Bar_Override1();
    // Should invoke override 3.
    this.Bar_Override3();
    // Should invoke the final declaration.
    this.Bar();
}
    
private void Bar_Override5()
{
    // Should invoke base declaration.
    base.Bar();
    // Should invoke override 3.
    this.Bar_Override3();
    // Should invoke the final declaration.
    this.Bar();
    // Should invoke the final declaration.
    this.Bar();
}
    
public void Foo_Override2()
{
    // Should invoke base declaration.
    base.Bar();
    // Should invoke override 1.
    this.Bar_Override1();
    // Should invoke override 1.
    this.Bar_Override1();
    // Should invoke the final declaration.
    this.Bar();
}
    
public void Foo_Override4()
{
    // Should invoke base declaration.
    base.Bar();
    // Should invoke override 3.
    this.Bar_Override3();
    // Should invoke override 3.
    this.Bar_Override3();
    // Should invoke the final declaration.
    this.Bar();
}
    
public void Foo_Override6()
{
    // Should invoke base declaration.
    base.Bar();
    // Should invoke the final declaration.
    this.Bar();
    // Should invoke the final declaration.
    this.Bar();
    // Should invoke the final declaration.
    this.Bar();
}    }