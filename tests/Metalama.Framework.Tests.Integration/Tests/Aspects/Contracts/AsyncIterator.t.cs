[Test]
public class TestClass
{
  public async IAsyncEnumerable<string> AsyncEnumerable(string text)
  {
    if (text is null)
    {
      throw new global::System.ArgumentNullException("text");
    }
    await foreach (var returnItem in this.AsyncEnumerable_Source(text))
    {
      yield return returnItem;
    }
  }
  private async IAsyncEnumerable<string> AsyncEnumerable_Source(string text)
  {
    await Task.Yield();
    yield return "Hello";
    await Task.Yield();
    yield return text;
  }
  public async IAsyncEnumerator<string> AsyncEnumerator(string text)
  {
    if (text is null)
    {
      throw new global::System.ArgumentNullException("text");
    }
    var returnEnumerator = this.AsyncEnumerator_Source(text);
    while (await returnEnumerator.MoveNextAsync())
    {
      yield return returnEnumerator.Current;
    }
  }
  private async IAsyncEnumerator<string> AsyncEnumerator_Source(string text)
  {
    await Task.Yield();
    yield return "Hello";
    await Task.Yield();
    yield return text;
  }
}