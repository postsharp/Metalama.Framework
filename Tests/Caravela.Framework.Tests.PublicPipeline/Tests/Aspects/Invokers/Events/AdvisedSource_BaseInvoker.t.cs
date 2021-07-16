internal class TargetClass
    {
        private EventHandler? _field;
    
        [Test]
        public event EventHandler Event
{add    {
this.__Event__OriginalImpl+= value;
    }
    
remove    {
this.__Event__OriginalImpl-= value;
    }
}
    
private event EventHandler __Event__OriginalImpl
        {
            add => this._field += value;
            remove => this._field -= value;
        }
    
private EventHandler? __EventField__BackingField;
    
    
        public event EventHandler? EventField{add    {
this.__EventField__OriginalImpl+= value;
    }
    
remove    {
this.__EventField__OriginalImpl-= value;
    }
}
    
private event EventHandler? __EventField__OriginalImpl
{
    add
    {
        this.__EventField__BackingField += value;
    }
    
    remove
    {
        this.__EventField__BackingField -= value;
    }
}
    
    }