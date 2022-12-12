using Metalama.Testing.AspectTesting;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.TestInputs.Highlighting.IfStatements.TemplateKeywords
{
    [RunTimeOrCompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            meta.CompileTime( 0 );
            meta.CompileTime<long>( 0 );
            meta.InsertComment( "Hey" );
            return meta.Proceed();
        }
    }
}
