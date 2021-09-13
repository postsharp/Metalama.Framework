class Targets
    {
        [DeepClone]
        class AutomaticallyCloneable:global::System.ICloneable        {
            int a;
    
            ManuallyCloneable? b;
    
            AutomaticallyCloneable? c;
    
    
public virtual global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.Targets.AutomaticallyCloneable Clone()
{
    var clone = ((global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.Targets.AutomaticallyCloneable)(base.MemberwiseClone()));
    ((global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.Targets.AutomaticallyCloneable)(clone)).b= ((global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.ManuallyCloneable? )(((global::System.ICloneable)this.b).Clone()));
    ((global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.Targets.AutomaticallyCloneable)(clone)).c= ((global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.Targets.AutomaticallyCloneable)(((global::System.ICloneable)this.c).Clone()));
    return (global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.Targets.AutomaticallyCloneable)(clone);
}
private global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.Targets.AutomaticallyCloneable Clone_Source()
{
    return default(global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.Targets.AutomaticallyCloneable);
}
    
global::System.Object global::System.ICloneable.Clone()
{
    return (global::System.Object)(this.Clone());
}        }
    
        [DeepClone]
        class Derived : AutomaticallyCloneable
        {
            private string d;
    
    
public override global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.Targets.Derived Clone()
{
    var clone = ((global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.Targets.Derived)(base.Clone()));
    ((global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.Targets.Derived)(clone)).d= ((global::System.String)(((global::System.ICloneable)this.d).Clone()));
    return (global::Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.Targets.Derived)(clone);
}        }
    }