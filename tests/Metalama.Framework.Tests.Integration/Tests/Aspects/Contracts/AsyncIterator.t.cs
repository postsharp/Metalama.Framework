[Test]
public class TestClass
{
  public IAsyncEnumerable<string> AsyncEnumerable(string text)
  {
    if (text is null)
    {
      throw new global::System.ArgumentNullException("text");
    }
    return this.AsyncEnumerable_Source(text);
  }
  private async IAsyncEnumerable<string> AsyncEnumerable_Source(string text)
  {
    await Task.Yield();
    yield return "Hello";
    await Task.Yield();
    yield return text;
  }
  public IAsyncEnumerator<string> AsyncEnumerator(string text)
  {
    if (text is null)
    {
      throw new global::System.ArgumentNullException("text");
    }
    return this.AsyncEnumerator_Source(text);
  }
  private async IAsyncEnumerator<string> AsyncEnumerator_Source(string text)
  {
    await Task.Yield();
    yield return "Hello";
    await Task.Yield();
    yield return text;
  }
}