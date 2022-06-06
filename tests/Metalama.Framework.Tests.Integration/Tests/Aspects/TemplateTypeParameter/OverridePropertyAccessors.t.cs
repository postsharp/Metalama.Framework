public class Target
{
    [Aspect]
    public int P
    {
        get
        {
            global::System.Console.WriteLine(typeof(global::System.Int32));
            return (global::System.Int32)((global::System.Int32)this.P_Source);

        }
        set
        {
            global::System.Console.WriteLine(typeof(global::System.Int32));
            this.P_Source = value;

        }
    }

    private int P_Source { get; set; }
}