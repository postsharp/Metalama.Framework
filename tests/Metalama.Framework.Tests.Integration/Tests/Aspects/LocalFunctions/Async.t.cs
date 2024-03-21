// Warning CS1998 on `FooAsync_Source`: `This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.`
internal class C
{
  [Retry]
  private void Foo()
  {
    return;
  }
  [Retry]
  private async Task FooAsync()
  {
    async global::System.Threading.Tasks.Task<global::System.Object> ExecuteCoreAsync()
    {
      await this.FooAsync_Source();
      return default;
    }
    await global::System.Threading.Tasks.Task.Run(ExecuteCoreAsync);
    return;
  }
  private async Task FooAsync_Source()
  {
  }
}