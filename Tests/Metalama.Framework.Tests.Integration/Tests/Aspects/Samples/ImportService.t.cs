class TargetClass
{
    private readonly IServiceProvider? _serviceProvider;


    private global::System.IFormatProvider? _formatProvider;


    [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.ImportService.ImportServiceAspect]
    private global::System.IFormatProvider? FormatProvider
    {
        get
        {
            return (global::System.IFormatProvider?)this._serviceProvider.GetService(typeof(global::System.IFormatProvider));

        }
        set
        {
            this._formatProvider = value;
        }
    }
    public string? Format(object? o)
    {
        return ((ICustomFormatter?)this.FormatProvider?.GetFormat(typeof(ICustomFormatter)))?.Format(null, o, this.FormatProvider);
    }
}