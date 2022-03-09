internal class TargetCode
    {


private global::System.Int32 _f;


public global::System.Int32 F 
{ get
{ 
        return this._f;
}
set
{ 
        if (value == 0)
        {
            return;
        }

        this._f=value;
}
}

private global::System.String? _s;


public global::System.String? S 
{ get
{ 
        return this._s;
}
set
{ 
        if (value == "")
        {
            return;
        }

        this._s=value;
}
}

private global::Metalama.Framework.Tests.Integration.Aspects.Misc.WeaklyTypedSerialize.MyEnum _e;


public global::Metalama.Framework.Tests.Integration.Aspects.Misc.WeaklyTypedSerialize.MyEnum E 
{ get
{ 
        return this._e;
}
set
{ 
        if (value == 0)
        {
            return;
        }

        this._e=value;
}
}    }