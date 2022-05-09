class TargetClass
{
    private readonly IServiceProvider? _serviceProvider;


    [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.ImportService.ImportServiceAspect]
    private global::System.IFormatProvider? FormatProvider
    {
        get
        {
            return (global::System.IFormatProvider?)this._serviceProvider.GetService(typeof(global::System.IFormatProvider));

        }
        set
        {
            throw new global::System.NotSupportedException();

        }
    }
    public string? Format(object? o)
    {
        return ((ICustomFormatter?)this.FormatProvider?.GetFormat(typeof(ICustomFormatter)))?.Format(null, o, this.FormatProvider);
    }
}