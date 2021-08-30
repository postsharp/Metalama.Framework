class Target
    {
        public void Foo()
{
    this.Foo_Override6();
}
    
    
public void Foo_Override0()
{
    // Should invoke original code.
    this.__Bar__OriginalImpl();
    // Should invoke original code.
    this.__Bar__OriginalImpl();
    // Should invoke original code.
    this.__Bar__OriginalImpl();
    // Should invoke the final declaration.
    this.Bar();
}
    
public void Foo_Override2()
{
    // Should invoke original code.
    this.__Bar__OriginalImpl();
    // Should invoke override 1.
    this.Bar_Override1();
    // Should invoke override 1.
    this.Bar_Override1();
    // Should invoke the final declaration.
    this.Bar();
}
    
public void Foo_Override4()
{
    // Should invoke original code.
    this.__Bar__OriginalImpl();
    // Should invoke override 3.
    this.Bar_Override2();
    // Should invoke override 3.
    this.Bar_Override2();
    // Should invoke the final declaration.
    this.Bar();
}
    
public void Foo_Override6()
{
    // Should invoke original code.
    this.__Bar__OriginalImpl();
    // Should invoke the final declaration.
    this.Bar();
    // Should invoke the final declaration.
    this.Bar();
    // Should invoke the final declaration.
    this.Bar();
}
        void Bar()
{
    this.Bar_Override3();
}
    
private void __Bar__OriginalImpl()
        {
            Console.WriteLine("This is original code.");
        }
    
    
void Bar_Override1()
{
    // Should invoke original code.
    this.__Bar__OriginalImpl();
    // Should invoke original code.
    this.__Bar__OriginalImpl();
    // Should invoke override 1.
    this.Bar_Override1();
    // Should invoke the final declaration.
    this.Bar();
}
    
void Bar_Override2()
{
    // Should invoke original code.
    this.__Bar__OriginalImpl();
    // Should invoke override 1.
    this.Bar_Override1();
    // Should invoke override 3.
    this.Bar_Override2();
    // Should invoke the final declaration.
    this.Bar();
}
    
void Bar_Override3()
{
    // Should invoke original code.
    this.__Bar__OriginalImpl();
    // Should invoke override 3.
    this.Bar_Override2();
    // Should invoke the final declaration.
    this.Bar();
    // Should invoke the final declaration.
    this.Bar();
}    }