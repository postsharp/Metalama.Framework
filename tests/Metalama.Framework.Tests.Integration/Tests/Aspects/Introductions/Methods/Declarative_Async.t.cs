// Warning CS1998 on `IntroducedMethod_TaskInt_Empty`: `This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.`
// Warning CS1998 on `IntroducedMethod_TaskVoid_Empty`: `This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.`
// Warning CS1998 on `IntroducedMethod_Void_Empty`: `This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.`
[Introduction]
internal class TargetClass
{
  public async global::System.Threading.Tasks.Task<global::System.Int32> IntroducedMethod_TaskInt()
  {
    global::System.Console.WriteLine("This is introduced method.");
    await global::System.Threading.Tasks.Task.Yield();
    return (await this.IntroducedMethod_TaskInt_Empty());
  }
  private async global::System.Threading.Tasks.Task<global::System.Int32> IntroducedMethod_TaskInt_Empty()
  {
    return default(global::System.Int32);
  }
  public async global::System.Threading.Tasks.Task IntroducedMethod_TaskVoid()
  {
    global::System.Console.WriteLine("This is introduced method.");
    await global::System.Threading.Tasks.Task.Yield();
    await this.IntroducedMethod_TaskVoid_Empty();
  }
  private async global::System.Threading.Tasks.Task IntroducedMethod_TaskVoid_Empty()
  {
  }
  public async void IntroducedMethod_Void()
  {
    global::System.Console.WriteLine("This is introduced method.");
    await global::System.Threading.Tasks.Task.Yield();
    await this.IntroducedMethod_Void_Empty();
  }
  private async global::System.Threading.Tasks.ValueTask IntroducedMethod_Void_Empty()
  {
  }
}