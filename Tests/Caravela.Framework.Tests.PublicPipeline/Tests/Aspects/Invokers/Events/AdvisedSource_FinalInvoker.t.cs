internal class TargetClass
    {
        private EventHandler? _field;
    
        [Test]
        public event EventHandler Event
{add    {
this.Event+= value;
    }
    
remove    {
this.Event-= value;
    }
}
    
private EventHandler? __EventField__BackingField;
    
    
        public event EventHandler? EventField{add    {
this.EventField+= value;
    }
    
remove    {
this.EventField-= value;
    }
}
    }