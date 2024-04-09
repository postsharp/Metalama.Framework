// Warning CS0169 on `x`: `The field 'TargetClass.x' is never used`
internal class TargetClass
{
    // CS0169 expected here.
    private int x;
    [SuppressWarning]
    private int y;
    [SuppressWarning]
    private int w, z;
}