[TestOutput]
[Dirty]
public class TargetClass : global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.IDirty
{
    public int A
    {
        get
        {
            return this.__A__BackingField;
        }

        set
        {
            global::System.Int32 __;
            this.__A__BackingField = value;
            if (this.DirtyState == global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.DirtyState.Clean)
            {
                this.__DirtyState__BackingField = value;
            }
        }
    }

    private int __A__BackingField;
    public global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.DirtyState DirtyState
    {
        get
        {
            return this.__DirtyState__BackingField;
        }

        set
        {
            this.__DirtyState__BackingField = value;
        }
    }

    private global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.DirtyState __DirtyState__BackingField;
    global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.DirtyState global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.IDirty.DirtyState
    {
        get
        {
            return (Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.DirtyState)this.DirtyState;
        }
    }
}