class Target
{
  public void Foo()
  {
    this.Foo_Override6();
  }
  public void Foo_Override0()
  {
    // Should invoke source code.
    this.Bar_Source();
    // Should invoke source code.
    this.Bar_Source();
    // Should invoke source code.
    this.Bar_Source();
    // Should invoke the final declaration.
    this.Bar();
  }
  public void Foo_Override2()
  {
    // Should invoke source code.
    this.Bar_Source();
    // Should invoke override 1.
    this.Bar_Override1();
    // Should invoke override 1.
    this.Bar_Override1();
    // Should invoke the final declaration.
    this.Bar();
  }
  public void Foo_Override4()
  {
    // Should invoke source code.
    this.Bar_Source();
    // Should invoke override 3.
    this.Bar_Override3();
    // Should invoke override 3.
    this.Bar_Override3();
    // Should invoke the final declaration.
    this.Bar();
  }
  public void Foo_Override6()
  {
    // Should invoke source code.
    this.Bar_Source();
    // Should invoke the final declaration.
    this.Bar();
    // Should invoke the final declaration.
    this.Bar();
    // Should invoke the final declaration.
    this.Bar();
  }
  void Bar()
  {
    this.Bar_Override5();
  }
  private void Bar_Source()
  {
    Console.WriteLine("This is original code.");
  }
  void Bar_Override1()
  {
    // Should invoke source code.
    this.Bar_Source();
    // Should invoke source code.
    this.Bar_Source();
    // Should invoke override 1.
    this.Bar_Override1();
    // Should invoke the final declaration.
    this.Bar();
  }
  void Bar_Override3()
  {
    // Should invoke source code.
    this.Bar_Source();
    // Should invoke override 1.
    this.Bar_Override1();
    // Should invoke override 3.
    this.Bar_Override3();
    // Should invoke the final declaration.
    this.Bar();
  }
  void Bar_Override5()
  {
    // Should invoke source code.
    this.Bar_Source();
    // Should invoke override 3.
    this.Bar_Override3();
    // Should invoke the final declaration.
    this.Bar();
    // Should invoke the final declaration.
    this.Bar();
  }
}