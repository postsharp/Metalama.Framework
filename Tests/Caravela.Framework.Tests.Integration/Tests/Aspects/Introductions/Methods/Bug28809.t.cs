[IntroducePropertyChangedAspect]
class TargetCode
{


    public event global::System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(global::System.String propertyName)
    {
        this.PropertyChanged?.Invoke((global::System.Object)(this), (global::System.ComponentModel.PropertyChangedEventArgs)(new global::System.ComponentModel.PropertyChangedEventArgs(propertyName)));
    }    }