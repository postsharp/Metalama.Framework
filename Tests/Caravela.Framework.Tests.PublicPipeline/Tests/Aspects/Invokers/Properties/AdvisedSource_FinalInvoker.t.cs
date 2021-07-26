internal class TargetClass
    {
        [Test]
        public int Property {get    {
        return (int)this.Property;
    }
    
set    {
        this.Property= value;
    }
}
    }