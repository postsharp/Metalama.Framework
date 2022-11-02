internal class C
{
  private readonly int _property1;
  public int Property1
  {
    [MyAspect]
    get
    {
      global::System.Console.WriteLine("Overridden.");
      return this._property1;
    }
    private init
    {
      this._property1 = value;
    }
  }
  private int _property2;
  public int Property2
  {
    [MyAspect]
    get
    {
      global::System.Console.WriteLine("Overridden.");
      return this._property2;
    }
    [MyAspect]
    set
    {
      global::System.Console.WriteLine("Overridden.");
      this._property2 = value;
      return;
    }
  }
  public event Action Event1
  {
    [MyAspect]
    add
    {
      global::System.Console.WriteLine("Overridden.");
    }
    [MyAspect]
    remove
    {
      global::System.Console.WriteLine("Overridden.");
      return;
    }
  }
  public event Action Event2
  {
    [MyAspect]
    add
    {
      global::System.Console.WriteLine("Overridden.");
      return;
    }
    remove
    {
    }
  }
}