[MyAspect]
public class C
{


    private int _p;
    private int P
    {
        get
        {
            global::System.Console.WriteLine($"Getting C.");
            return this._p;
        }
        set
        {
            global::System.Console.WriteLine($"Setting C to '{value}'.");
            this._p = value;
        }
    }


    public global::System.String IntroducedProperty
    {
        get
        {
            global::System.Console.WriteLine($"Getting C.");
            return (global::System.String)"IntroducedProperty";
        }

        set
        {
            global::System.Console.WriteLine($"Setting C to '{value}'.");
        }
    }
}