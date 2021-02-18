using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Aspects.UnitTests.Introductions.Methods.Programmatic
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void Initialize( IAspectBuilder<INamedType> aspectBuilder )
        {
            var advice = aspectBuilder.AdviceFactory.IntroduceMethod( aspectBuilder.TargetDeclaration, nameof( Template ) );

            advice.Builder.Name = "IntroducedMethod";
            advice.Builder.ReturnType = advice.Builder.Compilation.TypeFactory.GetTypeByReflectionType( typeof( int ) );
            advice.Builder.AddParameter( "x", advice.Builder.Compilation.TypeFactory.GetTypeByReflectionType( typeof( int ) ) );
            advice.Builder.AddParameter( "y", advice.Builder.Compilation.TypeFactory.GetTypeByReflectionType( typeof( int ) ) );
        }

        [IntroduceMethodTemplate]
        public dynamic Template()
        {
            Console.WriteLine( "This is introduced method." );
            return proceed();
        }
    }

    #region Target
    [Introduction]
    internal class TargetClass
    {
    }
    #endregion
}
