[Introduction]
[Override]
internal class TargetClass : DerivedClass
{
    public event EventHandler ExistingEvent
    {
        add
        {
            global::System.Console.WriteLine("Override.");
            this.ExistingEvent_Introduction += value;
        }

        remove
        {
            global::System.Console.WriteLine("Override.");
            this.ExistingEvent_Introduction += value;
        }
    }

    private event EventHandler ExistingEvent_Source
    {
        add
        {
            Console.WriteLine("This is the original add.");
        }

        remove
        {
            Console.WriteLine("This is the original remove.");
        }
    }


    private event global::System.EventHandler ExistingEvent_Introduction
    {
        add
        {
            global::System.Console.WriteLine("This is introduced event.");
            this.ExistingEvent_Source += value;

        }

        remove
        {
            global::System.Console.WriteLine("This is introduced event.");
            this.ExistingEvent_Source -= value;
        }
    }
    public event EventHandler ExistingVirtualEvent
    {
        add
        {
            global::System.Console.WriteLine("Override.");
            this.ExistingVirtualEvent_Introduction += value;
        }

        remove
        {
            global::System.Console.WriteLine("Override.");
            this.ExistingVirtualEvent_Introduction += value;
        }
    }

    private event EventHandler ExistingVirtualEvent_Source
    {
        add
        {
            Console.WriteLine("This is the original add.");
        }

        remove
        {
            Console.WriteLine("This is the original remove.");
        }
    }


    private event global::System.EventHandler ExistingVirtualEvent_Introduction
    {
        add
        {
            global::System.Console.WriteLine("This is introduced event.");
            this.ExistingVirtualEvent_Source += value;

        }

        remove
        {
            global::System.Console.WriteLine("This is introduced event.");
            this.ExistingVirtualEvent_Source -= value;
        }
    }

    private event global::System.EventHandler BaseClassAbstractEvent_Introduction
    {
        add
        {
            global::System.Console.WriteLine("This is introduced event.");
            base.BaseClassAbstractEvent += value;

        }

        remove
        {
            global::System.Console.WriteLine("This is introduced event.");
            base.BaseClassAbstractEvent -= value;
        }
    }

    public new event global::System.EventHandler BaseClassAbstractEvent
    {
        add
        {
            global::System.Console.WriteLine("Override.");
            this.BaseClassAbstractEvent_Introduction += value;
        }

        remove
        {
            global::System.Console.WriteLine("Override.");
            this.BaseClassAbstractEvent_Introduction += value;
        }
    }

    private event global::System.EventHandler BaseClassEvent_Introduction
    {
        add
        {
            global::System.Console.WriteLine("This is introduced event.");
            base.BaseClassEvent += value;

        }

        remove
        {
            global::System.Console.WriteLine("This is introduced event.");
            base.BaseClassEvent -= value;
        }
    }

    public new event global::System.EventHandler BaseClassEvent
    {
        add
        {
            global::System.Console.WriteLine("Override.");
            this.BaseClassEvent_Introduction += value;
        }

        remove
        {
            global::System.Console.WriteLine("Override.");
            this.BaseClassEvent_Introduction += value;
        }
    }

    private event global::System.EventHandler BaseClassVirtualEvent_Introduction
    {
        add
        {
            global::System.Console.WriteLine("This is introduced event.");
            base.BaseClassVirtualEvent += value;

        }

        remove
        {
            global::System.Console.WriteLine("This is introduced event.");
            base.BaseClassVirtualEvent -= value;
        }
    }

    public new event global::System.EventHandler BaseClassVirtualEvent
    {
        add
        {
            global::System.Console.WriteLine("Override.");
            this.BaseClassVirtualEvent_Introduction += value;
        }

        remove
        {
            global::System.Console.WriteLine("Override.");
            this.BaseClassVirtualEvent_Introduction += value;
        }
    }

    private event global::System.EventHandler DerivedClassEvent_Introduction
    {
        add
        {
            global::System.Console.WriteLine("This is introduced event.");
            base.DerivedClassEvent += value;

        }

        remove
        {
            global::System.Console.WriteLine("This is introduced event.");
            base.DerivedClassEvent -= value;
        }
    }

    public new event global::System.EventHandler DerivedClassEvent
    {
        add
        {
            global::System.Console.WriteLine("Override.");
            this.DerivedClassEvent_Introduction += value;
        }

        remove
        {
            global::System.Console.WriteLine("Override.");
            this.DerivedClassEvent_Introduction += value;
        }
    }

    private event global::System.EventHandler DerivedClassVirtualEvent_Introduction
    {
        add
        {
            global::System.Console.WriteLine("This is introduced event.");
            base.DerivedClassVirtualEvent += value;

        }

        remove
        {
            global::System.Console.WriteLine("This is introduced event.");
            base.DerivedClassVirtualEvent -= value;
        }
    }

    public new event global::System.EventHandler DerivedClassVirtualEvent
    {
        add
        {
            global::System.Console.WriteLine("Override.");
            this.DerivedClassVirtualEvent_Introduction += value;
        }

        remove
        {
            global::System.Console.WriteLine("Override.");
            this.DerivedClassVirtualEvent_Introduction += value;
        }
    }
}
