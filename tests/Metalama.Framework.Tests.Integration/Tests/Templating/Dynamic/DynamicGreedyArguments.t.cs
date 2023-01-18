int Method(PropertyChangedEventArgs a)
{
  this.Method((global::System.ComponentModel.PropertyChangedEventArgs)new global::System.ComponentModel.PropertyChangedEventArgs("a"));
  this.PropertyChanged.Invoke(new global::System.ComponentModel.PropertyChangedEventArgs("a"));
  this.PropertyChanged.Invoke(new global::System.ComponentModel.PropertyChangedEventArgs("a"));
  return default;
}