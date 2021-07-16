[NotifyPropertyChanged]
    class Car
: global::System.ComponentModel.INotifyPropertyChanged    {
    
private string? _make;
    
        public string? Make {get    {
        return this.__Make__OriginalImpl;
    }
    
set    {
        var value_1 = value;
        if (value_1 != this.__Make__OriginalImpl)
        {
this.OnPropertyChanged("Make");
this.__Make__OriginalImpl= value;
        }
    
        return;
    }
}
    
private string? __Make__OriginalImpl
{
    get
    {
        return this._make;
    }
    
    set
    {
        this._make = value;
    }
}
private double _power;
    
        public double Power {get    {
        return this.__Power__OriginalImpl;
    }
    
set    {
        var value_1 = value;
        if (value_1 != this.__Power__OriginalImpl)
        {
this.OnPropertyChanged("Power");
this.__Power__OriginalImpl= value;
        }
    
        return;
    }
}
    
private double __Power__OriginalImpl
{
    get
    {
        return this._power;
    }
    
    set
    {
        this._power = value;
    }
}
    
protected void OnPropertyChanged(global::System.String name)
{
this.PropertyChanged?.Invoke(this, new global::System.ComponentModel.PropertyChangedEventArgs(name));
}
    
public event global::System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }