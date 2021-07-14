class TargetClass
{
    private readonly IServiceProvider? _serviceProvider;
    private IFormatProvider? _formatProvider;
    [ImportServiceAspect]
    private IFormatProvider? FormatProvider
    {
        get
        {
            return (System.IFormatProvider?)this._serviceProvider.GetService(global::System.Type.GetTypeFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle("T:System.IFormatProvider")));
        }
    }

    public string? Format(object? o)
    {
        return ((ICustomFormatter?)this.FormatProvider?.GetFormat(typeof(ICustomFormatter)))?.Format(null, o, this.FormatProvider);
    }
}