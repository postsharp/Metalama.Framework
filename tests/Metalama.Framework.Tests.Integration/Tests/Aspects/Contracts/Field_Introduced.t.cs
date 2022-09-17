// Final Compilation.Emit failed. 
// Error CS0102 on `IntroducedField`: `The type 'Target' already contains a definition for 'IntroducedField'`
// Error CS0229 on `IntroducedField`: `Ambiguity between 'Target.IntroducedField' and 'Target.IntroducedField'`
// Error CS0229 on `IntroducedField`: `Ambiguity between 'Target.IntroducedField' and 'Target.IntroducedField'`
[IntroduceAndFilter]
    internal class Target
    {


private global::System.String _existingField;


private global::System.String ExistingField 
{ get
{ 
            global::System.String returnValue;
returnValue=this._existingField;        if (returnValue == null)
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

private global::System.String? IntroducedField;

private global::System.String? IntroducedField 
{ get
{ 
            var returnValue = this.IntroducedField;
        if (returnValue == null)
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

        this.IntroducedField = value;
     
}
}    }