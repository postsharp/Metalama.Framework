using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.TestInputs.Highlighting.IfStatements.TemplateKeywords
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            meta.CompileTime( 0 );
            meta.CompileTime<long>( 0 );
            meta.Comment( "Hey" );
            return meta.Proceed();
        }
    }
}
