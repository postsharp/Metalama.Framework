[Introduction]
    [Override]
    internal class TargetClass : BaseClass
    {
        public override event EventHandler BaseClassAbstractEvent
        {
            add
            {
                        this.BaseClassAbstractEvent_Source+= value;
    

            }

            remove
            {
                        this.BaseClassAbstractEvent_Source-= value;
    
            }

        }

private event EventHandler BaseClassAbstractEvent_Source
        {
            add
            {
                Console.WriteLine("This is the original add.");
            }

            remove
            {
                Console.WriteLine("This is the original remove.");
            }

        }

        public event EventHandler ExistingEvent
        {
            add
            {
                        this.ExistingEvent_Introduction+= value;
    

            }

            remove
            {
                        this.ExistingEvent_Introduction-= value;
    
            }
        }

private event EventHandler ExistingEvent_Source
        {
            add
            {
                Console.WriteLine("This is the original add.");
            }

            remove
            {
                Console.WriteLine("This is the original remove.");
            }
        }


private event global::System.EventHandler ExistingEvent_Introduction
{
    add
    {
            global::System.Console.WriteLine("This is introduced event.");
        this.ExistingEvent_Source+= value;
    
    }

    remove
    {
            global::System.Console.WriteLine("This is introduced event.");
        this.ExistingEvent_Source-= value;
        }
}
        public event EventHandler ExistingVirtualEvent
        {
            add
            {
                        this.ExistingVirtualEvent_Introduction+= value;
    

            }

            remove
            {
                        this.ExistingVirtualEvent_Introduction-= value;
    
            }
        }

private event EventHandler ExistingVirtualEvent_Source
        {
            add
            {
                Console.WriteLine("This is the original add.");
            }

            remove
            {
                Console.WriteLine("This is the original remove.");
            }
        }


private event global::System.EventHandler ExistingVirtualEvent_Introduction
{
    add
    {
            global::System.Console.WriteLine("This is introduced event.");
        this.ExistingVirtualEvent_Source+= value;
    
    }

    remove
    {
            global::System.Console.WriteLine("This is introduced event.");
        this.ExistingVirtualEvent_Source-= value;
        }
}

private event global::System.EventHandler BaseClassVirtualEvent_Introduction
{
    add
    {
            global::System.Console.WriteLine("This is introduced event.");
        base.BaseClassVirtualEvent += value;
    
    }

    remove
    {
            global::System.Console.WriteLine("This is introduced event.");
        base.BaseClassVirtualEvent -= value;
        }
}

public override event global::System.EventHandler BaseClassVirtualEvent
{
    add
    {
                this.BaseClassVirtualEvent_Introduction+= value;
    


    }

    remove
    {
                this.BaseClassVirtualEvent_Introduction-= value;
    
    }
}    }