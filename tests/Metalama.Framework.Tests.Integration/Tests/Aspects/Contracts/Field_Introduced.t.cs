[IntroduceAndFilter]
    internal class Target
    {


private global::System.String _existingField;


private global::System.String ExistingField 
{ get
{ 
        global::System.String returnValue ;returnValue = this._existingField;
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

        this._existingField=value;
}
}

private global::System.String? _introducedField;


private global::System.String? IntroducedField 
{ get
{ 
        global::System.String? returnValue ;returnValue = this._introducedField;
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

        this._introducedField=value;
}
}    }