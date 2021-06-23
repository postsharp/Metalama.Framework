// Warning CS0169 on `x`: `The field 'TargetClass.x' is never used`

// <target>
internal class TargetClass
{

    // CS0169 expected here.
    int x;
#pragma warning disable CS0169, CS0649

    [SuppressWarning]
    int y;
#pragma warning restore CS0169, CS0649
#pragma warning disable CS0169, CS0649

    [SuppressWarning]
    int w, z;
#pragma warning restore CS0169, CS0649


}