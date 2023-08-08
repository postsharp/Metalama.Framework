// Final Compilation.Emit failed.
// Error CS0126 on `return`: `An object of a type convertible to 'int' is required`
// Warning CS0162 on `return`: `Unreachable code detected`
[Aspect]
private int Method()
{
    return;
    return default;
}
