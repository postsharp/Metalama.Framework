public class Class1
{
  public ValueTask ExecuteAsync([NotNull] Action action)
  {
    if (action == null)
      throw new global::System.ArgumentNullException();
    return new(Task.CompletedTask);
  }
}