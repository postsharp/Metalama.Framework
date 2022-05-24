[OptionalValueType]
    internal class Account
    {
        public string? Name 
{ get
{ 
        return (global::System.String? )((global::Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues.Account.Optional)this.OptionalValues).Name.Value;

}
set
{ 
        ((global::Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues.Account.Optional)this.OptionalValues).Name = new global::Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues.OptionalValue<global::System.String?>(value);

}
}

        public Account? Parent 
{ get
{ 
        return (global::Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues.Account? )((global::Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues.Account.Optional)this.OptionalValues).Parent.Value;

}
set
{ 
        ((global::Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues.Account.Optional)this.OptionalValues).Parent = new global::Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues.OptionalValue<global::Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues.Account?>(value);

}
}

        // Currently Metalama cannot generate new classes, so we need to have
        // an empty class in the code.
        public class Optional { 

private global::Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues.OptionalValue<global::System.String?> _name;


public global::Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues.OptionalValue<global::System.String?> Name 
{ get
{ 
        return this._name;
}
set
{ 
        this._name=value;
}
}

private global::Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues.OptionalValue<global::Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues.Account?> _parent;


public global::Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues.OptionalValue<global::Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues.Account?> Parent 
{ get
{ 
        return this._parent;
}
set
{ 
        this._parent=value;
}
}}


private global::Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues.Account.Optional _optionalValues = new Optional();


public global::Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues.Account.Optional OptionalValues 
{ get
{ 
        return this._optionalValues;
}
private set
{ 
        this._optionalValues=value;
}
}     }