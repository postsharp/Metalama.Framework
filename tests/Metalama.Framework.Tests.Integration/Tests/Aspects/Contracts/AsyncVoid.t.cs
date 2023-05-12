public class Class1
{
  public Task Execute_Task([NotNull] Action action)
  {
    if (action == null)
      throw new global::System.ArgumentNullException();
    return Task.CompletedTask;
  }
  public ValueTask Execute_ValueTask([NotNull] Action action)
  {
    if (action == null)
      throw new global::System.ArgumentNullException();
    return new(Task.CompletedTask);
  }
  public async Task ExecuteAsync_Task([NotNull] Action action)
  {
    if (action == null)
      throw new global::System.ArgumentNullException();
    await this.ExecuteAsync_Task_Source(action);
  }
  private async Task ExecuteAsync_Task_Source(Action action) => await Task.Yield();
  public async ValueTask ExecuteAsync_ValueTask([NotNull] Action action)
  {
    if (action == null)
      throw new global::System.ArgumentNullException();
    await this.ExecuteAsync_ValueTask_Source(action);
  }
  private async ValueTask ExecuteAsync_ValueTask_Source(Action action) => await Task.Yield();
  public async void ExecuteAsync_Void([NotNull] Action action)
  {
    if (action == null)
      throw new global::System.ArgumentNullException();
    await this.ExecuteAsync_Void_Source(action);
  }
  private async global::System.Threading.Tasks.ValueTask ExecuteAsync_Void_Source(Action action) => await Task.Yield();
}