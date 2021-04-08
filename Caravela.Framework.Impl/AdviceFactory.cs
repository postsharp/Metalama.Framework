// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Sdk;

namespace Caravela.Framework.Impl
{
    internal class AdviceFactory : IAdviceFactory
    {
        private readonly INamedType _aspectType;
        private readonly AspectInstance _aspect;

        private readonly List<IAdvice> _advices = new();

        internal IReadOnlyList<IAdvice> Advices => this._advices;

        public AdviceFactory( INamedType aspectType, AspectInstance aspect )
        {
            this._aspectType = aspectType;
            this._aspect = aspect;
        }

        public IOverrideMethodAdvice OverrideMethod( IMethod targetMethod, string defaultTemplate, AspectLinkerOptions? aspectLinkerOptions = null )
        {
            var templateMethod = this._aspectType.Methods.Single( m => m.Name == defaultTemplate );
            var advice = new OverrideMethodAdvice( this._aspect, targetMethod, templateMethod, aspectLinkerOptions );
            this._advices.Add( advice );

            return advice;
        }

        public IIntroduceMethodAdvice IntroduceMethod( 
            INamedType targetType, 
            string defaultTemplate, 
            IntroductionScope scope = IntroductionScope.Default, 
            ConflictBehavior conflictBehavior = ConflictBehavior.Default, 
            AspectLinkerOptions? aspectLinkerOptions = null )
        {
            var templateMethod = this._aspectType.Methods.Single( m => m.Name == defaultTemplate );
            var advice = new IntroduceMethodAdvice( this._aspect, targetType, templateMethod, scope, conflictBehavior, aspectLinkerOptions );
            this._advices.Add( advice );

            return advice;
        }

        public IAdviceFactory ForLayer( string layerName ) => throw new System.NotImplementedException();
    }
}