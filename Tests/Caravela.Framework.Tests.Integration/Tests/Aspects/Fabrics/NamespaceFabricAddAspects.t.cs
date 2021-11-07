using System;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Fabrics;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Fabrics.NamespaceFabricAddAspects
{
#pragma warning disable CS0067
    internal class Fabric : NamespaceFabric
    {
        public override void AmendNamespace(INamespaceAmender amender) => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time.");

    }
#pragma warning restore CS0067
#pragma warning disable CS0067

    internal class Aspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod() => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time.");

    }
#pragma warning restore CS0067

    internal class TargetCode
    {
        private int Method1( int a ) => a;
        private string Method2( string s ) {
    global::System.Console.WriteLine("overridden");
return s;};
    }
    
    namespace Sub
    {
        class AnotherClass
        {
            private int Method1( int a ) => a;
            private string Method2( string s ) {
    global::System.Console.WriteLine("overridden");
return s;};
        }
    }
}