[Test]
public class TestClass
{
  public string DoSomething(string text)
  {
    if (text is null)
    {
      throw new global::System.ArgumentNullException("text");
    }
    global::System.String returnValue;
    Console.WriteLine("Hello");
    returnValue = null !;
    if (returnValue is null)
    {
      throw new global::System.InvalidOperationException("Method returned null");
    }
    return returnValue;
  }
  public Task DoSomethingAsync(string text)
  {
    if (text is null)
    {
      throw new global::System.ArgumentNullException("text");
    }
    return this.DoSomethingAsync_Source(text);
  }
  private async Task DoSomethingAsync_Source(string text)
  {
    await Task.Yield();
    Console.WriteLine("Hello");
  }
  public async Task<string> DoSomethingAsyncT(string text)
  {
    if (text is null)
    {
      throw new global::System.ArgumentNullException("text");
    }
    var returnValue = (await this.DoSomethingAsyncT_Source(text));
    if (returnValue is null)
    {
      throw new global::System.InvalidOperationException("Method returned null");
    }
    return returnValue;
  }
  private async Task<string> DoSomethingAsyncT_Source(string text)
  {
    await Task.Yield();
    Console.WriteLine("Hello");
    return null !;
  }
  public async void DoSomethingAsyncVoid(string text)
  {
    if (text is null)
    {
      throw new global::System.ArgumentNullException("text");
    }
    await this.DoSomethingAsyncVoid_Source(text);
  }
  private async global::System.Threading.Tasks.ValueTask DoSomethingAsyncVoid_Source(string text)
  {
    await Task.Yield();
    Console.WriteLine("Hello");
  }
}