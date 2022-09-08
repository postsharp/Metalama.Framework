[Introduction]
[Override]
internal class TargetClass : DerivedClass
{
    public event EventHandler ExistingEvent
    {
        add
        {
            global::System.Console.WriteLine("Override.");
            this.ExistingEvent += value;
        }

        remove
        {
            global::System.Console.WriteLine("Override.");
            this.ExistingEvent += value;
        }
    }

    public event EventHandler ExistingVirtualEvent
    {
        add
        {
            global::System.Console.WriteLine("Override.");
            this.ExistingVirtualEvent += value;
        }

        remove
        {
            global::System.Console.WriteLine("Override.");
            this.ExistingVirtualEvent += value;
        }
    }


    public new event global::System.EventHandler BaseClassAbstractEvent
    {
        add
        {
            global::System.Console.WriteLine("Override.");
            this.BaseClassAbstractEvent += value;
        }

        remove
        {
            global::System.Console.WriteLine("Override.");
            this.BaseClassAbstractEvent += value;
        }
    }

    public new event global::System.EventHandler BaseClassEvent
    {
        add
        {
            global::System.Console.WriteLine("Override.");
            this.BaseClassEvent += value;
        }

        remove
        {
            global::System.Console.WriteLine("Override.");
            this.BaseClassEvent += value;
        }
    }

    public new event global::System.EventHandler BaseClassVirtualEvent
    {
        add
        {
            global::System.Console.WriteLine("Override.");
            this.BaseClassVirtualEvent += value;
        }

        remove
        {
            global::System.Console.WriteLine("Override.");
            this.BaseClassVirtualEvent += value;
        }
    }

    public new event global::System.EventHandler DerivedClassEvent
    {
        add
        {
            global::System.Console.WriteLine("Override.");
            this.DerivedClassEvent += value;
        }

        remove
        {
            global::System.Console.WriteLine("Override.");
            this.DerivedClassEvent += value;
        }
    }

    public new event global::System.EventHandler DerivedClassVirtualEvent
    {
        add
        {
            global::System.Console.WriteLine("Override.");
            this.DerivedClassVirtualEvent += value;
        }

        remove
        {
            global::System.Console.WriteLine("Override.");
            this.DerivedClassVirtualEvent += value;
        }
    }
}