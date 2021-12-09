[Dirty]
public class TargetClass
: global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.IDirty
{
    private int _a; public int A
    {
        get
        {
            return this._a;
        }

        set
        {
            this._a = value; if (this.DirtyState == global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.DirtyState.Clean)
            {
                this.DirtyState = global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.DirtyState.Dirty;
            }
        }
    }


    public global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.DirtyState DirtyState
    {
        get;
        protected set;
    }
}