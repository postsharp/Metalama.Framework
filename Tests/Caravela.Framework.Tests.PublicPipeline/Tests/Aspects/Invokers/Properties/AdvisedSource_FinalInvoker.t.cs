internal class TargetClass
    {
    
private int _property;
    
        [Test]
        public int Property {get    {
        return (int)this.Property;
    }
    
set    {
this.Property= value;
    }
}
    }