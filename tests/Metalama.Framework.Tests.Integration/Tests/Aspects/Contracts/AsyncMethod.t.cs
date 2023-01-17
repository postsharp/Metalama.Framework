[NotNullCheck]
public class TestClass
{
    public string DoSomething(string text)
    {
        if (text is null)
        {
            throw new global::System.ArgumentNullException("text");
        }
        global::System.String returnValue;
        Console.WriteLine("Hello");
        returnValue = null!;
        if (returnValue is null)
        {
            throw new global::System.InvalidOperationException("Method returned null");
        }
        return returnValue;
    }
    public async Task<string> DoSomethingAsync(string text)
    {
        if (text is null)
        {
            throw new global::System.ArgumentNullException("text");
        }
        var returnValue = (await this.DoSomethingAsync_Source(text));
        if (returnValue is null)
        {
            throw new global::System.InvalidOperationException("Method returned null");
        }
        return returnValue;
    }
    private async Task<string> DoSomethingAsync_Source(string text)
    {
        await Task.Yield();
        Console.WriteLine("Hello");
        return null!;
    }
}