[IntroduceAndFilter]
    internal class Target
    {


private string? _existingProperty;
        public string? ExistingProperty 
{ get
{ 
        global::System.String? returnValue ;returnValue = this._existingProperty;
goto __aspect_return_1;
__aspect_return_1:        if (returnValue == null)
        {
            throw new global::System.ArgumentNullException();
        }

        return returnValue;

}
set
{ 
        if (value == null)
        {
            throw new global::System.ArgumentNullException();
        }

        this._existingProperty=value;
}
}


private global::System.String? _introducedProperty;


public global::System.String? IntroducedProperty 
{ get
{ 
        global::System.String? returnValue ;returnValue = this._introducedProperty;
goto __aspect_return_1;
__aspect_return_1:        if (returnValue == null)
        {
            throw new global::System.ArgumentNullException();
        }

        return returnValue;

}
set
{ 
        if (value == null)
        {
            throw new global::System.ArgumentNullException();
        }

        this._introducedProperty=value;
}
}    }