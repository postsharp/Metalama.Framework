    [NotifyPropertyChanged]
    internal class Car:global::System.ComponentModel.INotifyPropertyChanged    {


private string? _make;
        public string? Make { get
{ 
        return this.Make_Source;

}
set
{ 
        var value_1 = value;
        if (value_1 != this.Make_Source)
        {
            this.OnPropertyChanged("Make");
            this.Make_Source = value;
        }

        return;

}
}

private string? Make_Source
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

        public double Power { get
{ 
        return this.Power_Source;

}
set
{ 
        var value_1 = value;
        if (value_1 != this.Power_Source)
        {
            this.OnPropertyChanged("Power");
            this.Power_Source = value;
        }

        return;

}
}

private double Power_Source
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

public event global::System.ComponentModel.PropertyChangedEventHandler PropertyChanged;    }
