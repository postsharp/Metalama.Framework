// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;


namespace Caravela.Framework.TestApp
{
    public class IntroduceSomeMethodAspect : Attribute, IAspect<INamedType>
    {
        private readonly string[] _methodNames;

        public IntroduceSomeMethodAspect(params string[] methodNames)
        {
            this._methodNames = methodNames;
        }

        public void Initialize( IAspectBuilder<INamedType> aspectBuilder )
        {
            foreach ( var methodName in this._methodNames )
            {
                var advice = aspectBuilder.AdviceFactory.IntroduceMethod( aspectBuilder.TargetDeclaration, nameof( SomeIntroducedMethod ) );
                advice.Builder.Name = methodName;
            }
        }

        [IntroduceMethodTemplate]
        public static void SomeIntroducedMethod()
        {
            Console.WriteLine( "From IntroduceSomeMethodAspect!" );

            var x = meta.Proceed();
        }

        [IntroduceMethod]
        public void SomeOtherIntroducedMethod()
        {
            
        }

        [IntroduceMethod]
        public void SomeOtherIntroducedMethod5()
        {

        }
    }
}
