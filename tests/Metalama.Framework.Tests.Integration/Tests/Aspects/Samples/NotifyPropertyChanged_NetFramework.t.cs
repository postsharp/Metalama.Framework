[NotifyPropertyChanged]
internal class Car : global::System.ComponentModel.INotifyPropertyChanged
{
  private string? _make;
  public string? Make
  {
    get
    {
      return this._make;
    }
    set
    {
      var value_1 = value;
      if (value_1 != this._make)
      {
        this.OnPropertyChanged("Make");
        this._make = value;
      }
      return;
    }
  }
  private double _power;
  public double Power
  {
    get
    {
      return this._power;
    }
    set
    {
      var value_1 = value;
      if (value_1 != this._power)
      {
        this.OnPropertyChanged("Power");
        this._power = value;
      }
      return;
    }
  }
  protected virtual void OnPropertyChanged(global::System.String name)
  {
    this.PropertyChanged?.Invoke(this, new global::System.ComponentModel.PropertyChangedEventArgs(name));
  }
  public event global::System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
}