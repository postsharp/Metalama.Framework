internal class TargetClass
    {
        private int _field;

        [FirstOverride]
        [SecondOverride]
        public int Property
{get    {
        global::System.Console.WriteLine("This is the overridden getter.");
        _ = this.Property_SecondOverride;
        return this.Property_SecondOverride;
    }

set    {
        global::System.Console.WriteLine("This is the overridden setter.");
        this.Property_SecondOverride= value;
        this.Property_SecondOverride= value;
    }
}

private int Property_Source
        {
            get
            {
                return this._field;
            }

            set
            {
                this._field = value;
            }
        }


public global::System.Int32 Property_SecondOverride
{get    {
        global::System.Console.WriteLine("This is the overridden getter.");
        _ = this.Property_Source;
        return this.Property_Source;
    }

set    {
        global::System.Console.WriteLine("This is the overridden setter.");
        this.Property_Source= value;
        this.Property_Source= value;
    }
}    }