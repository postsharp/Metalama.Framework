class Target
{
  public int Foo
  {
    get
    {
      return this.Foo_Override6;
    }
    set
    {
      this.Foo_Override6 = value;
    }
  }
  public int Foo_Override0
  {
    get
    {
      // Should invoke source code.
      _ = this.Bar_Source;
      // Should invoke source code.
      _ = this.Bar_Source;
      // Should invoke source code.
      _ = this.Bar_Source;
      // Should invoke the final declaration.
      _ = this.Bar;
      return 42;
    }
    set
    {
      // Should invoke source code.
      this.Bar_Source = value;
      // Should invoke source code.
      this.Bar_Source = value;
      // Should invoke source code.
      this.Bar_Source = value;
      // Should invoke the final declaration.
      this.Bar = value;
    }
  }
  public int Foo_Override2
  {
    get
    {
      // Should invoke source code.
      _ = this.Bar_Source;
      // Should invoke override 1.
      _ = this.Bar_Override1;
      // Should invoke override 1.
      _ = this.Bar_Override1;
      // Should invoke the final declaration.
      _ = this.Bar;
      return 42;
    }
    set
    {
      // Should invoke source code.
      this.Bar_Source = value;
      // Should invoke override 1.
      this.Bar_Override1 = value;
      // Should invoke override 1.
      this.Bar_Override1 = value;
      // Should invoke the final declaration.
      this.Bar = value;
    }
  }
  public int Foo_Override4
  {
    get
    {
      // Should invoke source code.
      _ = this.Bar_Source;
      // Should invoke override 3.
      _ = this.Bar_Override3;
      // Should invoke override 3.
      _ = this.Bar_Override3;
      // Should invoke the final declaration.
      _ = this.Bar;
      return 42;
    }
    set
    {
      // Should invoke source code.
      this.Bar_Source = value;
      // Should invoke override 3.
      this.Bar_Override3 = value;
      // Should invoke override 3.
      this.Bar_Override3 = value;
      // Should invoke the final declaration.
      this.Bar = value;
    }
  }
  public int Foo_Override6
  {
    get
    {
      // Should invoke source code.
      _ = this.Bar_Source;
      // Should invoke the final declaration.
      _ = this.Bar;
      // Should invoke the final declaration.
      _ = this.Bar;
      // Should invoke the final declaration.
      _ = this.Bar;
      return 42;
    }
    set
    {
      // Should invoke source code.
      this.Bar_Source = value;
      // Should invoke the final declaration.
      this.Bar = value;
      // Should invoke the final declaration.
      this.Bar = value;
      // Should invoke the final declaration.
      this.Bar = value;
    }
  }
  public int Bar
  {
    get
    {
      return this.Bar_Override5;
    }
    set
    {
      this.Bar_Override5 = value;
    }
  }
  private int Bar_Source
  {
    get
    {
      Console.WriteLine("This is original code.");
      return 0;
    }
    set
    {
      Console.WriteLine("This is original code.");
    }
  }
  public int Bar_Override1
  {
    get
    {
      // Should invoke source code.
      _ = this.Bar_Source;
      // Should invoke source code.
      _ = this.Bar_Source;
      // Should invoke override 1.
      _ = this.Bar_Override1;
      // Should invoke the final declaration.
      _ = this.Bar;
      return 42;
    }
    set
    {
      // Should invoke source code.
      this.Bar_Source = value;
      // Should invoke source code.
      this.Bar_Source = value;
      // Should invoke override 1.
      this.Bar_Override1 = value;
      // Should invoke the final declaration.
      this.Bar = value;
    }
  }
  public int Bar_Override3
  {
    get
    {
      // Should invoke source code.
      _ = this.Bar_Source;
      // Should invoke override 1.
      _ = this.Bar_Override1;
      // Should invoke override 3.
      _ = this.Bar_Override3;
      // Should invoke the final declaration.
      _ = this.Bar;
      return 42;
    }
    set
    {
      // Should invoke source code.
      this.Bar_Source = value;
      // Should invoke override 1.
      this.Bar_Override1 = value;
      // Should invoke override 3.
      this.Bar_Override3 = value;
      // Should invoke the final declaration.
      this.Bar = value;
    }
  }
  public int Bar_Override5
  {
    get
    {
      // Should invoke source code.
      _ = this.Bar_Source;
      // Should invoke override 3.
      _ = this.Bar_Override3;
      // Should invoke the final declaration.
      _ = this.Bar;
      // Should invoke the final declaration.
      _ = this.Bar;
      return 42;
    }
    set
    {
      // Should invoke source code.
      this.Bar_Source = value;
      // Should invoke override 3.
      this.Bar_Override3 = value;
      // Should invoke the final declaration.
      this.Bar = value;
      // Should invoke the final declaration.
      this.Bar = value;
    }
  }
}