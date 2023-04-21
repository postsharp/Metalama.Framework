class Target
{
  public int Foo
  {
    get
    {
      return this.Foo_Override9;
    }
    set
    {
      this.Foo_Override9 = value;
    }
  }
  public int Foo_Override0
  {
    get
    {
      // Should invoke empty code.
      _ = this.Bar_Empty;
      // Should invoke empty code.
      _ = this.Bar_Empty;
      // Should invoke empty code.
      _ = this.Bar_Empty;
      // Should invoke the final declaration.
      _ = this.Bar;
      return 42;
    }
    set
    {
      // Should invoke empty code.
      this.Bar_Empty = value;
      // Should invoke empty code.
      this.Bar_Empty = value;
      // Should invoke empty code.
      this.Bar_Empty = value;
      // Should invoke the final declaration.
      this.Bar = value;
    }
  }
  public int Foo_Override2
  {
    get
    {
      // Should invoke source code.
      _ = this._bar;
      // Should invoke source code.
      _ = this._bar;
      // Should invoke source code.
      _ = this._bar;
      // Should invoke the final declaration.
      _ = this.Bar;
      return 42;
    }
    set
    {
      // Should invoke source code.
      this._bar = value;
      // Should invoke source code.
      this._bar = value;
      // Should invoke source code.
      this._bar = value;
      // Should invoke the final declaration.
      this.Bar = value;
    }
  }
  public int Foo_Override5
  {
    get
    {
      // Should invoke override 4.
      _ = this.Bar_Override4;
      // Should invoke override 4.
      _ = this.Bar_Override4;
      // Should invoke override 4.
      _ = this.Bar_Override4;
      // Should invoke the final declaration.
      _ = this.Bar;
      return 42;
    }
    set
    {
      // Should invoke override 4.
      this.Bar_Override4 = value;
      // Should invoke override 4.
      this.Bar_Override4 = value;
      // Should invoke override 4.
      this.Bar_Override4 = value;
      // Should invoke the final declaration.
      this.Bar = value;
    }
  }
  public int Foo_Override7
  {
    get
    {
      // Should invoke override 6.
      _ = this.Bar_Override6;
      // Should invoke override 6.
      _ = this.Bar_Override6;
      // Should invoke override 6.
      _ = this.Bar_Override6;
      // Should invoke the final declaration.
      _ = this.Bar;
      return 42;
    }
    set
    {
      // Should invoke override 6.
      this.Bar_Override6 = value;
      // Should invoke override 6.
      this.Bar_Override6 = value;
      // Should invoke override 6.
      this.Bar_Override6 = value;
      // Should invoke the final declaration.
      this.Bar = value;
    }
  }
  public int Foo_Override9
  {
    get
    {
      // Should invoke the final declaration.
      _ = this.Bar;
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
      // Should invoke the final declaration.
      this.Bar = value;
      // Should invoke the final declaration.
      this.Bar = value;
      // Should invoke the final declaration.
      this.Bar = value;
      // Should invoke the final declaration.
      this.Bar = value;
    }
  }
  public int Bar_Override4
  {
    get
    {
      // Should invoke source code.
      _ = this._bar;
      // Should invoke source code.
      _ = this._bar;
      // Should invoke override 4.
      _ = this.Bar_Override4;
      // Should invoke the final declaration.
      _ = this.Bar;
      return 42;
    }
    set
    {
      // Should invoke source code.
      this._bar = value;
      // Should invoke source code.
      this._bar = value;
      // Should invoke override 4.
      this.Bar_Override4 = value;
      // Should invoke the final declaration.
      this.Bar = value;
    }
  }
  public int Bar_Override6
  {
    get
    {
      // Should invoke override 4.
      _ = this.Bar_Override4;
      // Should invoke override 4.
      _ = this.Bar_Override4;
      // Should invoke override 6.
      _ = this.Bar_Override6;
      // Should invoke the final declaration.
      _ = this.Bar;
      return 42;
    }
    set
    {
      // Should invoke override 4.
      this.Bar_Override4 = value;
      // Should invoke override 4.
      this.Bar_Override4 = value;
      // Should invoke override 6.
      this.Bar_Override6 = value;
      // Should invoke the final declaration.
      this.Bar = value;
    }
  }
  public int Bar_Override8
  {
    get
    {
      // Should invoke override 6.
      _ = this.Bar_Override6;
      // Should invoke override 6.
      _ = this.Bar_Override6;
      // Should invoke the final declaration.
      _ = this.Bar;
      // Should invoke the final declaration.
      _ = this.Bar;
      return 42;
    }
    set
    {
      // Should invoke override 6.
      this.Bar_Override6 = value;
      // Should invoke override 6.
      this.Bar_Override6 = value;
      // Should invoke the final declaration.
      this.Bar = value;
      // Should invoke the final declaration.
      this.Bar = value;
    }
  }
  private int _bar;
  public int Bar
  {
    get
    {
      return this.Bar_Override8;
    }
    set
    {
      this.Bar_Override8 = value;
    }
  }
  private int Bar_Empty
  {
    get => default(int);
    set
    {
    }
  }
}