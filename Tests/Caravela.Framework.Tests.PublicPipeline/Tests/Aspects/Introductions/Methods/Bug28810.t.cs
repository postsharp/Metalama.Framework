class Targets
    {    
        class NaturallyCloneable : ICloneable
        {
            public object Clone()
            {
                return new NaturallyCloneable();
            }
        }
    
        [DeepClone]
        class BaseClass:global::System.ICloneable        {
            int a;
            NaturallyCloneable b;


public global::Caravela.Framework.Tests.Integration.Tests.Aspects.Introductions.Methods.Bug28810.Targets.BaseClass Clone()
{
    return null;
}

global::System.Object global::System.ICloneable.Clone()
{
    return (global::System.Object)this.Clone();
}        }

    }