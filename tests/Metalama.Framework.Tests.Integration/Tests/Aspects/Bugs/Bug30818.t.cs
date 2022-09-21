[OnPropertyChangedAspect]
internal class Foo
{
    [ValidationAspect]
    public string Name
    {
        get
        {
            global::System.String returnValue;
            returnValue = this.Name_Source;

            if (returnValue is not null)
            {
                throw new global::System.Exception($"The property 'Name' must not be set to null!");
            }

            return returnValue;


        }
        set
        {
            if (value is not null)
            {
                throw new global::System.Exception($"The property 'Name' must not be set to null!");
            }

            if (this.Name_Source == value)
                goto __aspect_return_1;
            OnChanged("Name", this.Name_Source, value);
            this.Name_Source = value;
        __aspect_return_1:;

        }
    }
    private string Name_Source
    { get; set; } = null!;

    private void OnChanged(global::System.String propertyName, global::System.Object oldValue, global::System.Object newValue)
    {
    }
}