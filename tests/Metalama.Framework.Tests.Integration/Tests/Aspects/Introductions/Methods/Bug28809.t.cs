[IntroducePropertyChangedAspect]
internal class TargetCode
{
  protected virtual void OnPropertyChanged(global::System.String propertyName)
  {
    this.PropertyChanged?.Invoke(this, (global::System.ComponentModel.PropertyChangedEventArgs)new global::System.ComponentModel.PropertyChangedEventArgs(propertyName));
  }
  public event global::System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
}