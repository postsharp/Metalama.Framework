[Promote]
    [Override]
    internal class TargetClass
    {


private global::System.Int32 Field_Promote
{
    get
    {
            global::System.Console.WriteLine("This is aspect code.");
        return this._field;    
    }

    set
    {
            global::System.Console.WriteLine("This is aspect code.");
        this._field=value;        }
}

private global::System.Int32 _field;


public global::System.Int32 Field 
{ get
{ 
        global::System.Console.WriteLine("Override");
        return this.Field_Promote;

}
set
{ 
        global::System.Console.WriteLine("Override");
        this.Field_Promote = value;

}
}    }