[EnumViewModel]
internal class VisibilityViewModel
{
  private Visibility _value;
  public VisibilityViewModel(Visibility value)
  {
    _value = value;
  }
  public global::System.Boolean IsCollapsed
  {
    get
    {
      return (global::System.Boolean)(this._value == global::Metalama.Framework.Tests.Integration.Aspects.Samples.EnumViewModel.Visibility.Collapsed);
    }
  }
  public global::System.Boolean IsHidden
  {
    get
    {
      return (global::System.Boolean)(this._value == global::Metalama.Framework.Tests.Integration.Aspects.Samples.EnumViewModel.Visibility.Hidden);
    }
  }
  public global::System.Boolean IsVisible
  {
    get
    {
      return (global::System.Boolean)(this._value == global::Metalama.Framework.Tests.Integration.Aspects.Samples.EnumViewModel.Visibility.Visible);
    }
  }
}