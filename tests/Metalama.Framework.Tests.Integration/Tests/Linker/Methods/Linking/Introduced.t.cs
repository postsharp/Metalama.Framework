class Target
{
  public void Foo()
  {
    this.Foo_Override6();
  }
  public void Bar()
  {
    this.Bar_Override5_2();
  }
  private void Bar_Empty()
  {
  }
  private void Bar_Override1_1()
  {
    // Should invoke empty code.
    this.Bar_Empty();
    // Should invoke empty code.
    this.Bar_Empty();
    // Should invoke override 1_2.
    this.Bar_Override1_2();
    // Should invoke the final declaration.
    this.Bar();
  }
  private void Bar_Override1_2()
  {
    // Should invoke empty code.
    this.Bar_Empty();
    // Should invoke override 1_1.
    this.Bar_Override1_1();
    // Should invoke override 1_2.
    this.Bar_Override1_2();
    // Should invoke the final declaration.
    this.Bar();
  }
  private void Bar_Override3_1()
  {
    // Should invoke override 1_2.
    this.Bar_Override1_2();
    // Should invoke override 1_2.
    this.Bar_Override1_2();
    // Should invoke override 3_2.
    this.Bar_Override3_2();
    // Should invoke the final declaration.
    this.Bar();
  }
  private void Bar_Override3_2()
  {
    // Should invoke override 1_2.
    this.Bar_Override1_2();
    // Should invoke override 3_1.
    this.Bar_Override3_1();
    // Should invoke override 3_2.
    this.Bar_Override3_2();
    // Should invoke the final declaration.
    this.Bar();
  }
  private void Bar_Override5_1()
  {
    // Should invoke override 3_2.
    this.Bar_Override3_2();
    // Should invoke override 3_2.
    this.Bar_Override3_2();
    // Should invoke the final declaration.
    this.Bar();
    // Should invoke the final declaration.
    this.Bar();
  }
  private void Bar_Override5_2()
  {
    // Should invoke override 3_2.
    this.Bar_Override3_2();
    // Should invoke override 5_1.
    this.Bar_Override5_1();
    // Should invoke the final declaration.
    this.Bar();
    // Should invoke the final declaration.
    this.Bar();
  }
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
    // Should invoke override 1_2.
    this.Bar_Override1_2();
    // Should invoke override 1_2.
    this.Bar_Override1_2();
    // Should invoke override 1_2.
    this.Bar_Override1_2();
    // Should invoke the final declaration.
    this.Bar();
  }
  public void Foo_Override4()
  {
    // Should invoke override 3_2.
    this.Bar_Override3_2();
    // Should invoke override 3_2.
    this.Bar_Override3_2();
    // Should invoke override 3_2.
    this.Bar_Override3_2();
    // Should invoke the final declaration.
    this.Bar();
  }
  public void Foo_Override6()
  {
    // Should invoke the final declaration.
    this.Bar();
    // Should invoke the final declaration.
    this.Bar();
    // Should invoke the final declaration.
    this.Bar();
    // Should invoke the final declaration.
    this.Bar();
  }
}