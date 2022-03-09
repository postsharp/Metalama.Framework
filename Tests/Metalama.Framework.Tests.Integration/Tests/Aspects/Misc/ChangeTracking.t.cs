[ChangeTrackingAspect]
internal class MyClass
    {


private int _a;
        public int A 
{ get
{ 
        return this._a;
}
set
{ 
        this._isASpecified = true;
        this._a=value;
}
}


private string? _b;
        public string? B 
{ get
{ 
        return this._b;
}
set
{ 
        this._isBSpecified = true;
        this._b=value;
}
}


private global::System.Boolean _isASpecified1;


public global::System.Boolean _isASpecified 
{ get
{ 
        return this._isASpecified1;
}
set
{ 
        this._isASpecified1=value;
}
}

private global::System.Boolean _isBSpecified1;


public global::System.Boolean _isBSpecified 
{ get
{ 
        return this._isBSpecified1;
}
set
{ 
        this._isBSpecified1=value;
}
}    }