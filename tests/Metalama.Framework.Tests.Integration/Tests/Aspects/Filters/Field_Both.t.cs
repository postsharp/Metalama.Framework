internal class Target
    {


private global::System.String _q1;


private global::System.String q 
{ get
{ 
        global::System.String returnValue ;returnValue = this._q1;
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

        this._q1=value;
}
}    }