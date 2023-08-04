class TargetCode
{
    [Aspect]
    async Task AsyncTaskMethod()
    {
        await this.AsyncTaskMethod_Source();
    }
    private async Task AsyncTaskMethod_Source()
    {
        await Task.Yield();
    }
}
