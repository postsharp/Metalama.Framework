[Test]
public class TestClass
{
  public IEnumerable<string> Enumerable
  {
    get
    {
      return this.Enumerable_Source;
    }
    set
    {
      if (value is null)
      {
        throw new global::System.ArgumentNullException();
      }
      this.Enumerable_Source = value;
    }
  }
  private IEnumerable<string> Enumerable_Source
  {
    get
    {
      yield return "Hello";
    }
    set
    {
    }
  }
  public IEnumerator<string> Enumerator
  {
    get
    {
      return this.Enumerator_Source;
    }
    set
    {
      if (value is null)
      {
        throw new global::System.ArgumentNullException();
      }
      this.Enumerator_Source = value;
    }
  }
  private IEnumerator<string> Enumerator_Source
  {
    get
    {
      yield return "Hello";
    }
    set
    {
    }
  }
}