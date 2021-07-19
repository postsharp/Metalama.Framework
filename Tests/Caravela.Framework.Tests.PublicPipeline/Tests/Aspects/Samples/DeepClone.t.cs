[DeepClone]
    class AutomaticallyCloneable
: global::System.ICloneable    {
        int a;
    
        ManuallyCloneable? b;
    
        AutomaticallyCloneable? c;
    
    
public global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.AutomaticallyCloneable Clone()
{
    var clone = (global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.AutomaticallyCloneable? )(null);
    clone = ((global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.AutomaticallyCloneable)(base.MemberwiseClone()));
    ((global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.AutomaticallyCloneable)(clone)).b= ((global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.ManuallyCloneable? )(((global::System.ICloneable)this.b).Clone()));
    ((global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.AutomaticallyCloneable)(clone)).c= ((global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.AutomaticallyCloneable)(((global::System.ICloneable)this.c).Clone()));
    return (Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.AutomaticallyCloneable)clone;
}
    
global::System.Object global::System.ICloneable.Clone()
{
    return (object)this.Clone();
}    }