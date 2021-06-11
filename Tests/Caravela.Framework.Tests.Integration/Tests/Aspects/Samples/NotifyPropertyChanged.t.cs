
[TestOutput]
[NotifyPropertyChanged]
internal class TargetClass
: global::System.ComponentModel.INotifyPropertyChanged
{
    public int Property1
    {
        get
        {
            return this.__Property1__BackingField;
        }

        set
        {
            if (value != this.Property1)
            {
                this.OnPropertyChanged("Property1");
                global::System.Int32 result;
                this.__Property1__BackingField = value;
            }
        }
    }

    private int __Property1__BackingField;

    public int Property2
    {
        get
        {
            return this.__Property2__BackingField;
        }

        set
        {
            if (value != this.Property2)
            {
                this.OnPropertyChanged("Property2");
                global::System.Int32 result;
                this.__Property2__BackingField = value;
            }
        }
    }

    private int __Property2__BackingField;

    public event global::System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(global::System.String name)
    {
        this.PropertyChanged?.Invoke(this, new global::System.ComponentModel.PropertyChangedEventArgs(name));
    }

    event global::System.ComponentModel.PropertyChangedEventHandler? global::System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    {
        add
        {
            this.PropertyChanged += value;
        }

        remove
        {
            this.PropertyChanged -= value;
        }
    }
}

