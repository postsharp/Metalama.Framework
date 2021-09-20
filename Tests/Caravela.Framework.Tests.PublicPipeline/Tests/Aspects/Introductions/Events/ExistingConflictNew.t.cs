    [Introduction]
    internal class TargetClass : DerivedClass
    {
        public event EventHandler ExistingEvent
{add    {
        global::System.Console.WriteLine("This is introduced event.");
                Console.WriteLine("This is original event.");
    }

remove    {
        global::System.Console.WriteLine("This is introduced event.");
                Console.WriteLine("This is original event.");
    }
}

        public static event EventHandler ExistingEvent_Static
{add    {
        global::System.Console.WriteLine("This is introduced event.");
                Console.WriteLine("This is original event.");
    }

remove    {
        global::System.Console.WriteLine("This is introduced event.");
                Console.WriteLine("This is original event.");
    }
}

        public virtual event EventHandler ExistingVirtualEvent
{add    {
        global::System.Console.WriteLine("This is introduced event.");
                Console.WriteLine("This is original event.");
    }

remove    {
        global::System.Console.WriteLine("This is introduced event.");
                Console.WriteLine("This is original event.");
    }
}


public new event global::System.EventHandler BaseClassEvent
{add    {
        global::System.Console.WriteLine("This is introduced event.");
    }

remove    {
        global::System.Console.WriteLine("This is introduced event.");
    }
}

public static new event global::System.EventHandler BaseClassEvent_Static
{add    {
        global::System.Console.WriteLine("This is introduced event.");
    }

remove    {
        global::System.Console.WriteLine("This is introduced event.");
    }
}

public new event global::System.EventHandler HiddenBaseClassEvent
{add    {
        global::System.Console.WriteLine("This is introduced event.");
    }

remove    {
        global::System.Console.WriteLine("This is introduced event.");
    }
}

public static new event global::System.EventHandler HiddenBaseClassEvent_Static
{add    {
        global::System.Console.WriteLine("This is introduced event.");
    }

remove    {
        global::System.Console.WriteLine("This is introduced event.");
    }
}

public new event global::System.EventHandler HiddenBaseClassVirtualEvent
{add    {
        global::System.Console.WriteLine("This is introduced event.");
    }

remove    {
        global::System.Console.WriteLine("This is introduced event.");
    }
}

public new event global::System.EventHandler HiddenVirtualBaseClassVirtualEvent
{add    {
        global::System.Console.WriteLine("This is introduced event.");
    }

remove    {
        global::System.Console.WriteLine("This is introduced event.");
    }
}

public new event global::System.EventHandler BaseClassVirtualEvent
{add    {
        global::System.Console.WriteLine("This is introduced event.");
    }

remove    {
        global::System.Console.WriteLine("This is introduced event.");
    }
}

public new event global::System.EventHandler BaseClassVirtualSealedEvent
{add    {
        global::System.Console.WriteLine("This is introduced event.");
    }

remove    {
        global::System.Console.WriteLine("This is introduced event.");
    }
}

public new event global::System.EventHandler BaseClassVirtualOverridenEvent
{add    {
        global::System.Console.WriteLine("This is introduced event.");
    }

remove    {
        global::System.Console.WriteLine("This is introduced event.");
    }
}

public new event global::System.EventHandler BaseClassAbstractEvent
{add    {
        global::System.Console.WriteLine("This is introduced event.");
    }

remove    {
        global::System.Console.WriteLine("This is introduced event.");
    }
}

public new event global::System.EventHandler BaseClassAbstractSealedEvent
{add    {
        global::System.Console.WriteLine("This is introduced event.");
    }

remove    {
        global::System.Console.WriteLine("This is introduced event.");
    }
}

public new event global::System.EventHandler DerivedClassEvent
{add    {
        global::System.Console.WriteLine("This is introduced event.");
    }

remove    {
        global::System.Console.WriteLine("This is introduced event.");
    }
}

public static new event global::System.EventHandler DerivedClassEvent_Static
{add    {
        global::System.Console.WriteLine("This is introduced event.");
    }

remove    {
        global::System.Console.WriteLine("This is introduced event.");
    }
}

public new event global::System.EventHandler DerivedClassVirtualEvent
{add    {
        global::System.Console.WriteLine("This is introduced event.");
    }

remove    {
        global::System.Console.WriteLine("This is introduced event.");
    }
}

public new event global::System.EventHandler DerivedClassVirtualSealedEvent
{add    {
        global::System.Console.WriteLine("This is introduced event.");
    }

remove    {
        global::System.Console.WriteLine("This is introduced event.");
    }
}

public event global::System.EventHandler NonExistentEvent
{add    {
        global::System.Console.WriteLine("This is introduced event.");
    }

remove    {
        global::System.Console.WriteLine("This is introduced event.");
    }
}

public static event global::System.EventHandler NonExistentEvent_Static
{add    {
        global::System.Console.WriteLine("This is introduced event.");
    }

remove    {
        global::System.Console.WriteLine("This is introduced event.");
    }
}    }