[Introduction]
[Override]
internal class TargetClass : BaseClass
{
    public override event EventHandler BaseClassAbstractEvent
    {
        add
        {
            this.BaseClassAbstractEvent += value;
        }

        remove
        {
            this.BaseClassAbstractEvent -= value;
        }

    }

    public event EventHandler ExistingEvent
    {
        add
        {
            this.ExistingEvent += value;
        }

        remove
        {
            this.ExistingEvent -= value;
        }
    }

    public event EventHandler ExistingVirtualEvent
    {
        add
        {
            this.ExistingVirtualEvent += value;
        }

        remove
        {
            this.ExistingVirtualEvent -= value;
        }
    }


    public override event global::System.EventHandler BaseClassVirtualEvent
    {
        add
        {
            this.BaseClassVirtualEvent += value;
        }

        remove
        {
            this.BaseClassVirtualEvent -= value;
        }
    }
}