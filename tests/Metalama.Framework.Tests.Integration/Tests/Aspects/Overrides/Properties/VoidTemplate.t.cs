// Warning CS0472 on `value == null`: `The result of the expression is always 'false' since a value of type 'int' is never equal to 'null' of type 'int?'`
internal class TargetClass
{
    private int _p;
    [Override]
    int P
    {
        get
        {
            var value = this._p;
            return (global::System.Int32)(value == null ? default : value);
        }
        set
        {
            this._p = value;
        }
    }
}
