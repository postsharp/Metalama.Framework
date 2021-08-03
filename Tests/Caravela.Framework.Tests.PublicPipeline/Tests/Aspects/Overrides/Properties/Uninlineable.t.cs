internal class TargetClass
{
    private int _field;

    [FirstOverride]
    [SecondOverride]
    public int Property
    {get    {
            global::System.Console.WriteLine("This is the overridden getter.");
            _ = this.__Override__Property__By__SecondOverrideAttribute;
            return (int)this.__Override__Property__By__SecondOverrideAttribute;
        }

        set    {
            global::System.Console.WriteLine("This is the overridden setter.");
            this.__Override__Property__By__SecondOverrideAttribute= value;
            this.__Override__Property__By__SecondOverrideAttribute= value;
        }
    }

    private int __Property__OriginalImpl
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


    public global::System.Int32 __Override__Property__By__SecondOverrideAttribute
    {get    {
            global::System.Console.WriteLine("This is the overridden getter.");
            _ = this.__Property__OriginalImpl;
            return (int)this.__Property__OriginalImpl;
        }

        set    {
            global::System.Console.WriteLine("This is the overridden setter.");
            this.__Property__OriginalImpl= value;
            this.__Property__OriginalImpl= value;
        }
    }    }