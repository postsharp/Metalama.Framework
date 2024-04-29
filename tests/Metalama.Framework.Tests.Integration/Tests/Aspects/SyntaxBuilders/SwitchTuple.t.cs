// Warning CS8618 on `A`: `Non-nullable property 'A' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.`

// Warning CS8618 on `B`: `Non-nullable property 'B' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.`

[TheAspect]
internal class C
{
    private string A { get; set; }
    private string B { get; set; }

    public void TheMethod(global::System.String a, global::System.String b)
    {
        switch (a, b)
        {
            case ("A", "xxx"):
                global::System.Console.WriteLine("A");
                break;
            case ("B", "xxx"):
                global::System.Console.WriteLine("B");
                break;
            default:
                return;
        }
    }
}
