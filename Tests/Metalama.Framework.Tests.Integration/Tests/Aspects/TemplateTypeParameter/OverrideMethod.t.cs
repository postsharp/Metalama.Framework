// Final Compilation.Emit failed. 
// Warning CS0219 on `x`: `The variable 'x' is assigned but its value is never used`
// Warning CS8600 on `(T)meta.Proceed()`: `Converting null literal or possible null value to non-nullable type.`
// Warning CS8602 on `t`: `Dereference of a possibly null reference.`
// Error CS1061 on `ToBoolean`: `'int' does not contain a definition for 'ToBoolean' and no accessible extension method 'ToBoolean' accepting a first argument of type 'int' could be found (are you missing a using directive or an assembly reference?)`
public class Target
{
    [Aspect]
    public int M() 
{ 
    var x = default(global::System.Int32);
    var t = (global::System.Int32)this.M_Source();
    var z = t.ToBoolean(null);
    return (global::System.Int32)t;

}

private int M_Source()
=> 5}
