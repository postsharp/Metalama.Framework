[Dirty]
    public class TargetClass:global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.IDirty    {


private int _a;
        public int A 
{ get
{ 
        return this._a;
}
set
{ 
        this._a=value;        if (this.DirtyState == global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.DirtyState.Clean)
        {
            this.DirtyState = global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.DirtyState.Dirty;
        }

}
}


private global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.DirtyState _dirtyState;


public global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.DirtyState DirtyState 
{ get
{ 
        return this._dirtyState;
}
protected set
{ 
        this._dirtyState=value;        if (this.DirtyState == global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.DirtyState.Clean)
        {
            this.DirtyState = global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.DirtyState.Dirty;
        }

}
}    }