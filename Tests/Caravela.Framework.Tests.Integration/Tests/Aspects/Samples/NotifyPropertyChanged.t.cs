[NotifyPropertyChanged]
    class Car
: global::System.ComponentModel.INotifyPropertyChanged    {
        public string? Make {get    {
    return this._make;
    }

set    {
        var value = value;
        if (value != this.Make)
        {
            this.OnPropertyChanged("Make");
            global::System.String dummy;
this._make=value;        }

        return;
    }
}
private string? _make;        public double Power {get    {
    return this._power;
    }

set    {
        var value = value;
        if (value != this.Power)
        {
            this.OnPropertyChanged("Power");
            global::System.Double dummy;
this._power=value;        }

        return;
    }
}
private double _power;

protected void OnPropertyChanged(global::System.String name)
{
    this.PropertyChanged?.Invoke(this, new global::System.ComponentModel.PropertyChangedEventArgs(name));
}

public event global::System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }