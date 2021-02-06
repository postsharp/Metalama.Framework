﻿using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Reactive;

namespace Caravela.Framework.Impl
{
    internal class AdviceFactory : IAdviceFactory
    {
        private readonly ICompilation _compilation;
        private readonly INamedType _aspectType;
        private readonly IAspect _aspect;

        private readonly List<IAdvice> _advices = new ();

        internal IReadOnlyList<IAdvice> Advices => this._advices;

        public AdviceFactory( ICompilation compilation, INamedType aspectType, IAspect aspect )
        {
            this._compilation = compilation;
            this._aspectType = aspectType;
            this._aspect = aspect;
        }

        public IOverrideMethodAdvice OverrideMethod( IMethod targetMethod, string defaultTemplate )
        {
            var templateMethod = this._aspectType.Methods.Where( m => m.Name == defaultTemplate ).GetValue().Single();
            var advice = new OverrideMethodAdvice( this._aspect, targetMethod, templateMethod );
            this._advices.Add( advice );

            return advice;
        }

        public IIntroductionAdvice IntroduceMethod( INamedType targetType, string defaultTemplate )
        {
            // TODO: signature matching.
            var templateMethod = this._aspectType.Methods.Where( m => m.Name == defaultTemplate ).GetValue().Single();
            var advice = new IntroduceMethodAdvice( this._aspect, targetType, templateMethod );
            this._advices.Add( advice );

            return advice;
        }
    }
}