internal class TargetClass
{
  private readonly IServiceProvider? _serviceProvider;
  [ImportServiceAspect]
  private IFormatProvider? FormatProvider
  {
    get
    {
      return (global::System.IFormatProvider? )this._serviceProvider.GetService(typeof(global::System.IFormatProvider));
    }
    init
    {
      throw new global::System.NotSupportedException();
    }
  }
  public string? Format(object? o)
  {
    return ((ICustomFormatter? )FormatProvider?.GetFormat(typeof(ICustomFormatter)))?.Format(null, o, FormatProvider);
  }
}