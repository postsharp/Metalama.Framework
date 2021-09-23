[Aspect]
    class TargetCode : List<int>
    {


public new void Add(global::System.Int32 value)
{
    global::System.Console.WriteLine("Oops");
    base.Add(value);
}    
    }
