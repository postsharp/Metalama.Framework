// Warning CS0162 on `yield`: `Unreachable code detected`
// Warning CS0162 on `yield`: `Unreachable code detected`
class TargetCode
{
    [Aspect]
    public async IAsyncEnumerable<int> Enumerable(int a)
    {
        await global::System.Threading.Tasks.Task.Yield();
        throw new global::System.Exception();
        yield break;
    }
    [Aspect]
    public async IAsyncEnumerator<int> Enumerator(int a)
    {
        await global::System.Threading.Tasks.Task.Yield();
        throw new global::System.Exception();
        yield break;
    }
}
