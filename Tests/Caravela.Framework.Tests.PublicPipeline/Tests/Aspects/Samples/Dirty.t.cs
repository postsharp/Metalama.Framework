[Dirty]
public class TargetClass
: global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.IDirty
{
    private int _a; public int A
    {
        get
        {
            return this._a;
        }

        set
        {
            this._a = value; if (this.DirtyState == global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.DirtyState.Clean)
            {
                this.DirtyState = global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.DirtyState.Dirty;
            }
        }
    }


    public global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.DirtyState DirtyState
    {
        get;
        protected set;
    }
}