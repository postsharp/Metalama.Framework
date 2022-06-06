    internal class Targets
        {
            private class BaseClass
            {
                [Aspect]
                public virtual int P 
    { get
    { 
            return (global::System.Int32)(this.P_Source + 1);
    
    }
    set
    { 
            this.P_Source = value - 1;
    
    }
    }
    
    private int P_Source { get; set; }
            }
    
            private class DerivedClass : BaseClass
            {
                public override int P 
    { get
    { 
            return (global::System.Int32)(this.P_Source + 1);
    
    }
    set
    { 
            this.P_Source = value - 1;
    
    }
    }
    
    private int P_Source { get; set; }
            }
        }