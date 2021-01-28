using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Transformations;
using Caravela.Reactive;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl
{
    class AdviceFactory : IAdviceFactory
    {
        private readonly ICompilation _compilation;
        private readonly INamedType _aspectType;
        private readonly IAspect _aspect;

        private readonly List<AdviceInstance> _advices = new();
        internal IReadOnlyList<AdviceInstance> Advices => this._advices;

        public AdviceFactory( ICompilation compilation, INamedType aspectType, IAspect aspect )
        {
            this._compilation = compilation;
            this._aspectType = aspectType;
            this._aspect = aspect;
        }

        public IOverrideMethodAdvice OverrideMethod( IMethod targetMethod, string defaultTemplate )
        {
            var templateMethod = this._aspectType.Methods.Where( m => m.Name == defaultTemplate ).GetValue().Single();

            string templateMethodName = templateMethod.Name + TemplateCompiler.TemplateMethodSuffix;

            var methodBody = new TemplateDriver( this._aspect.GetType().GetMethod( templateMethodName ) ).ExpandDeclaration( this._aspect, targetMethod, this._compilation );

            var result = new OverrideMethodAdvice( targetMethod, new OverriddenMethod( targetMethod, methodBody ) );

            this._advices.Add( new AdviceInstance( result ) );

            return result;
        }

        public IIntroductionAdvice IntroduceMethod( INamedType targetType, string template )
        {
            // TODO: signature matching.
            var templateMethod = this._aspectType.Methods.Where( m => m.Name == template ).GetValue().Single();
            string templateMethodName = templateMethod.Name + TemplateCompiler.TemplateMethodSuffix;
            var templateReflectionMethod = this._aspect.GetType().GetMethod( templateMethodName );
            var targetMethod = targetType.Methods.Where( m => m.Name == template ).GetValue().SingleOrDefault();

            IntroducedMethod introducedMethod;
            BlockSyntax methodBody;

            if (targetMethod != null)
            {
                methodBody = new TemplateDriver( templateReflectionMethod ).ExpandDeclaration( this._aspect, targetMethod, this._compilation );
                introducedMethod = new IntroducedMethod( targetType, targetMethod, templateMethod, methodBody );
            }
            else
            {
                // TODO: Newly created method based on template needs to be created.
                introducedMethod = new IntroducedMethod( targetType, targetMethod, templateMethod, null );
                methodBody = new TemplateDriver( templateReflectionMethod ).ExpandDeclaration( this._aspect, introducedMethod, this._compilation );
                introducedMethod = new IntroducedMethod( targetType, targetMethod, templateMethod, methodBody );
            }

            var result = new IntroduceMethodAdvice( targetType, introducedMethod );

            this._advices.Add( new AdviceInstance( result ) );

            return result;
        }
    }
}