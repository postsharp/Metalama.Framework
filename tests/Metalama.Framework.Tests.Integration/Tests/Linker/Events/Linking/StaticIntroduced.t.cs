class Target
{
  public static event System.EventHandler Foo
  {
    add
    {
      Foo_Override6 += value;
    }
    remove
    {
      Foo_Override6 -= value;
    }
  }
  public static event System.EventHandler Bar
  {
    add
    {
      Bar_Override5_2 += value;
    }
    remove
    {
      Bar_Override5_2 -= value;
    }
  }
  private static event System.EventHandler Bar_Empty
  {
    add
    {
      System.Console.WriteLine("SHOULD BE DISCARDED (this is introduced code).");
    }
    remove
    {
      System.Console.WriteLine("SHOULD BE DISCARDED (this is introduced code).");
    }
  }
  private static event System.EventHandler Bar_Override1_1
  {
    add
    {
      // Should invoke empty code.
      global::Metalama.Framework.Tests.Integration.Tests.Linker.Events.Linking.StaticIntroduced.Target.Bar_Empty += value;
      // Should invoke empty code.
      global::Metalama.Framework.Tests.Integration.Tests.Linker.Events.Linking.StaticIntroduced.Target.Bar_Empty += value;
      // Should invoke override 1_2.
      Target.Bar_Override1_2 += value;
      // Should invoke the final declaration.
      Target.Bar += value;
    }
    remove
    {
      // Should invoke empty code.
      global::Metalama.Framework.Tests.Integration.Tests.Linker.Events.Linking.StaticIntroduced.Target.Bar_Empty -= value;
      // Should invoke empty code.
      global::Metalama.Framework.Tests.Integration.Tests.Linker.Events.Linking.StaticIntroduced.Target.Bar_Empty -= value;
      // Should invoke override 1_2.
      Target.Bar_Override1_2 -= value;
      // Should invoke the final declaration.
      Target.Bar -= value;
    }
  }
  private static event System.EventHandler Bar_Override1_2
  {
    add
    {
      // Should invoke empty code.
      global::Metalama.Framework.Tests.Integration.Tests.Linker.Events.Linking.StaticIntroduced.Target.Bar_Empty += value;
      // Should invoke override 1_1.
      Target.Bar_Override1_1 += value;
      // Should invoke override 1_2.
      Target.Bar_Override1_2 += value;
      // Should invoke the final declaration.
      Target.Bar += value;
    }
    remove
    {
      // Should invoke empty code.
      global::Metalama.Framework.Tests.Integration.Tests.Linker.Events.Linking.StaticIntroduced.Target.Bar_Empty -= value;
      // Should invoke override 1_1.
      Target.Bar_Override1_1 -= value;
      // Should invoke override 1_2.
      Target.Bar_Override1_2 -= value;
      // Should invoke the final declaration.
      Target.Bar -= value;
    }
  }
  private static event System.EventHandler Bar_Override3_1
  {
    add
    {
      // Should invoke override 1_2.
      Target.Bar_Override1_2 += value;
      // Should invoke override 1_2.
      Target.Bar_Override1_2 += value;
      // Should invoke override 3_2.
      Target.Bar_Override3_2 += value;
      // Should invoke the final declaration.
      Target.Bar += value;
    }
    remove
    {
      // Should invoke override 1_2.
      Target.Bar_Override1_2 -= value;
      // Should invoke override 1_2.
      Target.Bar_Override1_2 -= value;
      // Should invoke override 3_2.
      Target.Bar_Override3_2 -= value;
      // Should invoke the final declaration.
      Target.Bar -= value;
    }
  }
  private static event System.EventHandler Bar_Override3_2
  {
    add
    {
      // Should invoke override 1_2.
      Target.Bar_Override1_2 += value;
      // Should invoke override 3_1.
      Target.Bar_Override3_1 += value;
      // Should invoke override 3_2.
      Target.Bar_Override3_2 += value;
      // Should invoke the final declaration.
      Target.Bar += value;
    }
    remove
    {
      // Should invoke override 1_2.
      Target.Bar_Override1_2 -= value;
      // Should invoke override 3_1.
      Target.Bar_Override3_1 -= value;
      // Should invoke override 3_2.
      Target.Bar_Override3_2 -= value;
      // Should invoke the final declaration.
      Target.Bar -= value;
    }
  }
  private static event System.EventHandler Bar_Override5_1
  {
    add
    {
      // Should invoke override 3_2.
      Target.Bar_Override3_2 += value;
      // Should invoke override 3_2.
      Target.Bar_Override3_2 += value;
      // Should invoke the final declaration.
      Target.Bar += value;
      // Should invoke the final declaration.
      Target.Bar += value;
    }
    remove
    {
      // Should invoke override 3_2.
      Target.Bar_Override3_2 -= value;
      // Should invoke override 3_2.
      Target.Bar_Override3_2 -= value;
      // Should invoke the final declaration.
      Target.Bar -= value;
      // Should invoke the final declaration.
      Target.Bar -= value;
    }
  }
  private static event System.EventHandler Bar_Override5_2
  {
    add
    {
      // Should invoke override 3_2.
      Target.Bar_Override3_2 += value;
      // Should invoke override 5_1.
      Target.Bar_Override5_1 += value;
      // Should invoke the final declaration.
      Target.Bar += value;
      // Should invoke the final declaration.
      Target.Bar += value;
    }
    remove
    {
      // Should invoke override 3_2.
      Target.Bar_Override3_2 -= value;
      // Should invoke override 5_1.
      Target.Bar_Override5_1 -= value;
      // Should invoke the final declaration.
      Target.Bar -= value;
      // Should invoke the final declaration.
      Target.Bar -= value;
    }
  }
  public static event System.EventHandler Foo_Override0
  {
    add
    {
      // Should invoke empty code.
      global::Metalama.Framework.Tests.Integration.Tests.Linker.Events.Linking.StaticIntroduced.Target.Bar_Empty += value;
      // Should invoke empty code.
      global::Metalama.Framework.Tests.Integration.Tests.Linker.Events.Linking.StaticIntroduced.Target.Bar_Empty += value;
      // Should invoke empty code.
      global::Metalama.Framework.Tests.Integration.Tests.Linker.Events.Linking.StaticIntroduced.Target.Bar_Empty += value;
      // Should invoke the final declaration.
      Target.Bar += value;
    }
    remove
    {
      // Should invoke empty code.
      global::Metalama.Framework.Tests.Integration.Tests.Linker.Events.Linking.StaticIntroduced.Target.Bar_Empty -= value;
      // Should invoke empty code.
      global::Metalama.Framework.Tests.Integration.Tests.Linker.Events.Linking.StaticIntroduced.Target.Bar_Empty -= value;
      // Should invoke empty code.
      global::Metalama.Framework.Tests.Integration.Tests.Linker.Events.Linking.StaticIntroduced.Target.Bar_Empty -= value;
      // Should invoke the final declaration.
      Target.Bar -= value;
    }
  }
  public static event System.EventHandler Foo_Override2
  {
    add
    {
      // Should invoke override 1_2.
      Target.Bar_Override1_2 += value;
      // Should invoke override 1_2.
      Target.Bar_Override1_2 += value;
      // Should invoke override 1_2.
      Target.Bar_Override1_2 += value;
      // Should invoke the final declaration.
      Target.Bar += value;
    }
    remove
    {
      // Should invoke override 1_2.
      Target.Bar_Override1_2 -= value;
      // Should invoke override 1_2.
      Target.Bar_Override1_2 -= value;
      // Should invoke override 1_2.
      Target.Bar_Override1_2 -= value;
      // Should invoke the final declaration.
      Target.Bar -= value;
    }
  }
  public static event System.EventHandler Foo_Override4
  {
    add
    {
      // Should invoke override 3_2.
      Target.Bar_Override3_2 += value;
      // Should invoke override 3_2.
      Target.Bar_Override3_2 += value;
      // Should invoke override 3_2.
      Target.Bar_Override3_2 += value;
      // Should invoke the final declaration.
      Target.Bar += value;
    }
    remove
    {
      // Should invoke override 3_2.
      Target.Bar_Override3_2 -= value;
      // Should invoke override 3_2.
      Target.Bar_Override3_2 -= value;
      // Should invoke override 3_2.
      Target.Bar_Override3_2 -= value;
      // Should invoke the final declaration.
      Target.Bar -= value;
    }
  }
  public static event System.EventHandler Foo_Override6
  {
    add
    {
      // Should invoke the final declaration.
      Target.Bar += value;
      // Should invoke the final declaration.
      Target.Bar += value;
      // Should invoke the final declaration.
      Target.Bar += value;
      // Should invoke the final declaration.
      Target.Bar += value;
    }
    remove
    {
      // Should invoke the final declaration.
      Target.Bar -= value;
      // Should invoke the final declaration.
      Target.Bar -= value;
      // Should invoke the final declaration.
      Target.Bar -= value;
      // Should invoke the final declaration.
      Target.Bar -= value;
    }
  }
}