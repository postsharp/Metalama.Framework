[TestOutput]
[NotifyPropertyChanged]
class Car : global::System.ComponentModel.INotifyPropertyChanged
{
    public string? Make
    {
        get
        {
            return this.__Make__BackingField;
        }

        set
        {
            var value = value;
            if (value != this.Make)
            {
                this.OnPropertyChanged("Make");
                global::System.String dummy;
                this.__Make__BackingField = value;
            }

            return;
        }
    }

    private string? __Make__BackingField;
    public double Power
    {
        get
        {
            return this.__Power__BackingField;
        }

        set
        {
            var value = value;
            if (value != this.Power)
            {
                this.OnPropertyChanged("Power");
                global::System.Double dummy;
                this.__Power__BackingField = value;
            }

            return;
        }
    }

    private double __Power__BackingField;
    protected void OnPropertyChanged(global::System.String name)
    {
        this.PropertyChanged?.Invoke(this, new global::System.ComponentModel.PropertyChangedEventArgs("name"));
    }

    public event global::System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
}