[Dirty]
    public class TargetClass
:global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.IDirty    {
        public int A {get    {
    return this.__A__BackingField;
    }

set    {
        global::System.Int32 __;
this.__A__BackingField=value;        if (this.DirtyState == global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.DirtyState.Clean)
        {
            this.DirtyState = global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.DirtyState.Dirty;
        }
    }
}
private int __A__BackingField;

public global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.DirtyState DirtyState
{
    get;
    protected set;
}    }