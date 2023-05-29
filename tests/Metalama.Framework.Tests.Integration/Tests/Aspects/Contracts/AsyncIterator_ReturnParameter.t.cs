[Test]
public class TestClass
{
  public async IAsyncEnumerable<string> AsyncEnumerable(string text)
  {
    var returnValue = this.AsyncEnumerable_Source(text);
    await foreach (var item in (global::System.Collections.Generic.IAsyncEnumerable<global::System.Object?>)returnValue)
    {
      if (item is null)
      {
        throw new global::System.ArgumentNullException("<return>");
      }
    }
    await foreach (var returnItem in returnValue)
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
    var returnValue = (await global::Metalama.Framework.RunTime.RunTimeAspectHelper.BufferAsync(this.AsyncEnumerator_Source(text)));
    var contractEnumerator = returnValue;
    while (await contractEnumerator.MoveNextAsync())
    {
      if (contractEnumerator.Current is null)
      {
        throw new global::System.ArgumentNullException("<return>");
      }
    }
    while (await returnValue.MoveNextAsync())
    {
      yield return returnValue.Current;
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