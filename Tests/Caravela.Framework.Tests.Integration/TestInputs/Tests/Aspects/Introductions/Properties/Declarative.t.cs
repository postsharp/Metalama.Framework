// <target>
[Introduction]
internal class TargetClass
{


    public global::System.Int32 IntroducedProperty_Accessors
    {
        get
        {
            global::System.Console.WriteLine("Get");
            return (int)42;
        }

        set
        {
            global::System.Console.WriteLine(value);
        }
    }
}