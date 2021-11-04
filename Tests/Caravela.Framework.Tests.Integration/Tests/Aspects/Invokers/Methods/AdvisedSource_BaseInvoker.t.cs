    internal class TargetClass
        {
            [Test]
            public int Method(int x)
    {
        return this.Method_Source(x);
    }
    
    private int Method_Source(int x)
            {
                return x;
            }
        }