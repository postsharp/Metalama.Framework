[Dirty]
    public class TargetClass
: global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.IDirty    {
        public int A {get    {
    return this._a;
    }

set    {
        global::System.Int32 __;
this._a=value;        if (this.DirtyState == global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.DirtyState.Clean)
        {
            this.DirtyState = global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.DirtyState.Dirty;
        }
    }
}
private int _a;

public global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.DirtyState DirtyState
{
    get;
    protected set;
}    }