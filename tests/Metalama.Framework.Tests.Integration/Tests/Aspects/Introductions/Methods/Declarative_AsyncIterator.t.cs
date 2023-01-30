// Warning CS1998 on `IntroducedMethod_AsyncEnumerable_Empty`: `This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.`
// Warning CS1998 on `IntroducedMethod_AsyncEnumerator_Empty`: `This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.`
[Introduction]
internal class TargetClass
{
  public async global::System.Collections.Generic.IAsyncEnumerable<global::System.Int32> IntroducedMethod_AsyncEnumerable()
  {
    global::System.Console.WriteLine("This is introduced method.");
    await global::System.Threading.Tasks.Task.Yield();
    yield return 42;
    await foreach (var x in (await global::Metalama.Framework.RunTime.RunTimeAspectHelper.BufferAsync(this.IntroducedMethod_AsyncEnumerable_Empty())))
    {
      yield return x;
    }
  }
  private async global::System.Collections.Generic.IAsyncEnumerable<global::System.Int32> IntroducedMethod_AsyncEnumerable_Empty()
  {
    yield break;
  }
  public async global::System.Collections.Generic.IAsyncEnumerator<global::System.Int32> IntroducedMethod_AsyncEnumerator()
  {
    global::System.Console.WriteLine("This is introduced method.");
    await global::System.Threading.Tasks.Task.Yield();
    yield return 42;
    var enumerator = (await global::Metalama.Framework.RunTime.RunTimeAspectHelper.BufferAsync(this.IntroducedMethod_AsyncEnumerator_Empty()));
    while (await enumerator.MoveNextAsync())
    {
      yield return enumerator.Current;
    }
  }
  private async global::System.Collections.Generic.IAsyncEnumerator<global::System.Int32> IntroducedMethod_AsyncEnumerator_Empty()
  {
    yield break;
  }
}