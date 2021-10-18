using System;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Fabrics;

using  Caravela.Framework.Tests.PublicPipeline.Aspects.Fabrics.NamespaceFabricAddAspectsToBadNs2;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Fabrics.NamespaceFabricAddAspectsToBadNs
{
    internal class Fabric : INamespaceFabric
    {
        public void AmendNamespace( INamespaceAmender builder )
        {
            builder
                .WithMembers<INamedType>( c => new[] { (INamedType) c.Compilation.TypeFactory.GetTypeByReflectionType( typeof(C2) ) } )
                .AddAspect<Aspect>();
        }
    }

    internal class Aspect : TypeAspect
    {
        
    }

    internal class TargetCode
    {
        private int Method1( int a ) => a;
        private string Method2( string s ) => s;
    }
    
    namespace Sub
    {
        class AnotherClass
        {
            private int Method1( int a ) => a;
            private string Method2( string s ) => s;
        }
    }
}

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Fabrics.NamespaceFabricAddAspectsToBadNs2
{
    public class C2 {}
}
