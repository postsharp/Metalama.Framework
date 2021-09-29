using System;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Fabrics;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Fabrics.NamespaceFabricAddAspects
{
    internal class Fabric : INamespaceFabric
    {
        public void BuildNamespace( INamespaceFabricBuilder builder )
        {
            builder
                .WithMembers( c => c.AllTypes
                    .SelectMany( t => t.Methods )
                    .Where( m => m.ReturnType.Is( typeof(string) ) ) )
                .AddAspect<Aspect>();
        }
    }

    internal class Aspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine( "overridden" );

            return meta.Proceed();
        }
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