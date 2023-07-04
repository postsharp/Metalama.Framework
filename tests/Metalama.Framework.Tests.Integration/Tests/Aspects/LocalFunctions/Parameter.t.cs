class C
{
    [Aspect]
    int M(int i) => 42;
    public global::System.Func<global::System.Object?, global::System.Object?[], global::System.Object?> GetOriginalMethodInvoker()
    {
        return (global::System.Func<global::System.Object?, global::System.Object?[], global::System.Object?>)Invoke;
        object? Invoke(object? instance, object?[] args)
        {
            return ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.LocalFunctions.Parameter.C)instance).M((global::System.Int32)args[0]!);
        }
    }
}
