class Target : Base
{
  public event System.EventHandler Foo
  {
    add
    {
      this.Foo_Override6 += value;
    }
    remove
    {
      this.Foo_Override6 -= value;
    }
  }
  public override event System.EventHandler Bar
  {
    add
    {
      this.Bar_Override5_2 += value;
    }
    remove
    {
      this.Bar_Override5_2 -= value;
    }
  }
  private event System.EventHandler Bar_Source
  {
    add
    {
      System.Console.WriteLine("This is original code.");
    }
    remove
    {
      System.Console.WriteLine("This is original code.");
    }
  }
  private event System.EventHandler Bar_Override1_1
  {
    add
    {
      // Should invoke source code.
      this.Bar_Source += value;
      // Should invoke source code.
      this.Bar_Source += value;
      // Should invoke override 1_2.
      this.Bar_Override1_2 += value;
      // Should invoke the final declaration.
      this.Bar += value;
    }
    remove
    {
      // Should invoke source code.
      this.Bar_Source -= value;
      // Should invoke source code.
      this.Bar_Source -= value;
      // Should invoke override 1_2.
      this.Bar_Override1_2 -= value;
      // Should invoke the final declaration.
      this.Bar -= value;
    }
  }
  private event System.EventHandler Bar_Override1_2
  {
    add
    {
      // Should invoke source code.
      this.Bar_Source += value;
      // Should invoke override 1_1.
      this.Bar_Override1_1 += value;
      // Should invoke override 1_2.
      this.Bar_Override1_2 += value;
      // Should invoke the final declaration.
      this.Bar += value;
    }
    remove
    {
      // Should invoke source code.
      this.Bar_Source -= value;
      // Should invoke override 1_1.
      this.Bar_Override1_1 -= value;
      // Should invoke override 1_2.
      this.Bar_Override1_2 -= value;
      // Should invoke the final declaration.
      this.Bar -= value;
    }
  }
  private event System.EventHandler Bar_Override3_1
  {
    add
    {
      // Should invoke override 1_2.
      this.Bar_Override1_2 += value;
      // Should invoke override 1_2.
      this.Bar_Override1_2 += value;
      // Should invoke override 3_2.
      this.Bar_Override3_2 += value;
      // Should invoke the final declaration.
      this.Bar += value;
    }
    remove
    {
      // Should invoke override 1_2.
      this.Bar_Override1_2 -= value;
      // Should invoke override 1_2.
      this.Bar_Override1_2 -= value;
      // Should invoke override 3_2.
      this.Bar_Override3_2 -= value;
      // Should invoke the final declaration.
      this.Bar -= value;
    }
  }
  private event System.EventHandler Bar_Override3_2
  {
    add
    {
      // Should invoke override 1_2.
      this.Bar_Override1_2 += value;
      // Should invoke override 3_1.
      this.Bar_Override3_1 += value;
      // Should invoke override 3_2.
      this.Bar_Override3_2 += value;
      // Should invoke the final declaration.
      this.Bar += value;
    }
    remove
    {
      // Should invoke override 1_2.
      this.Bar_Override1_2 -= value;
      // Should invoke override 3_1.
      this.Bar_Override3_1 -= value;
      // Should invoke override 3_2.
      this.Bar_Override3_2 -= value;
      // Should invoke the final declaration.
      this.Bar -= value;
    }
  }
  private event System.EventHandler Bar_Override5_1
  {
    add
    {
      // Should invoke override 3_2.
      this.Bar_Override3_2 += value;
      // Should invoke override 3_2.
      this.Bar_Override3_2 += value;
      // Should invoke the final declaration.
      this.Bar += value;
      // Should invoke the final declaration.
      this.Bar += value;
    }
    remove
    {
      // Should invoke override 3_2.
      this.Bar_Override3_2 -= value;
      // Should invoke override 3_2.
      this.Bar_Override3_2 -= value;
      // Should invoke the final declaration.
      this.Bar -= value;
      // Should invoke the final declaration.
      this.Bar -= value;
    }
  }
  private event System.EventHandler Bar_Override5_2
  {
    add
    {
      // Should invoke override 3_2.
      this.Bar_Override3_2 += value;
      // Should invoke override 5_1.
      this.Bar_Override5_1 += value;
      // Should invoke the final declaration.
      this.Bar += value;
      // Should invoke the final declaration.
      this.Bar += value;
    }
    remove
    {
      // Should invoke override 3_2.
      this.Bar_Override3_2 -= value;
      // Should invoke override 5_1.
      this.Bar_Override5_1 -= value;
      // Should invoke the final declaration.
      this.Bar -= value;
      // Should invoke the final declaration.
      this.Bar -= value;
    }
  }
  public event System.EventHandler Foo_Override0
  {
    add
    {
      // Should invoke source code.
      this.Bar_Source += value;
      // Should invoke source code.
      this.Bar_Source += value;
      // Should invoke source code.
      this.Bar_Source += value;
      // Should invoke the final declaration.
      this.Bar += value;
    }
    remove
    {
      // Should invoke source code.
      this.Bar_Source -= value;
      // Should invoke source code.
      this.Bar_Source -= value;
      // Should invoke source code.
      this.Bar_Source -= value;
      // Should invoke the final declaration.
      this.Bar -= value;
    }
  }
  public event System.EventHandler Foo_Override2
  {
    add
    {
      // Should invoke override 1_2.
      this.Bar_Override1_2 += value;
      // Should invoke override 1_2.
      this.Bar_Override1_2 += value;
      // Should invoke override 1_2.
      this.Bar_Override1_2 += value;
      // Should invoke the final declaration.
      this.Bar += value;
    }
    remove
    {
      // Should invoke override 1_2.
      this.Bar_Override1_2 -= value;
      // Should invoke override 1_2.
      this.Bar_Override1_2 -= value;
      // Should invoke override 1_2.
      this.Bar_Override1_2 -= value;
      // Should invoke the final declaration.
      this.Bar -= value;
    }
  }
  public event System.EventHandler Foo_Override4
  {
    add
    {
      // Should invoke override 3_2.
      this.Bar_Override3_2 += value;
      // Should invoke override 3_2.
      this.Bar_Override3_2 += value;
      // Should invoke override 3_2.
      this.Bar_Override3_2 += value;
      // Should invoke the final declaration.
      this.Bar += value;
    }
    remove
    {
      // Should invoke override 3_2.
      this.Bar_Override3_2 -= value;
      // Should invoke override 3_2.
      this.Bar_Override3_2 -= value;
      // Should invoke override 3_2.
      this.Bar_Override3_2 -= value;
      // Should invoke the final declaration.
      this.Bar -= value;
    }
  }
  public event System.EventHandler Foo_Override6
  {
    add
    {
      // Should invoke the final declaration.
      this.Bar += value;
      // Should invoke the final declaration.
      this.Bar += value;
      // Should invoke the final declaration.
      this.Bar += value;
      // Should invoke the final declaration.
      this.Bar += value;
    }
    remove
    {
      // Should invoke the final declaration.
      this.Bar -= value;
      // Should invoke the final declaration.
      this.Bar -= value;
      // Should invoke the final declaration.
      this.Bar -= value;
      // Should invoke the final declaration.
      this.Bar -= value;
    }
  }
}