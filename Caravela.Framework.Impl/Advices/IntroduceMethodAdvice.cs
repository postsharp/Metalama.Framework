using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Transformations;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Caravela.Framework.Impl.Advices
{

    internal sealed class IntroduceMethodAdvice : Advice, IIntroductionAdvice, IAdviceImplementation
    {
        public new INamedType TargetDeclaration => (INamedType) base.TargetDeclaration;

        public IMethod TemplateMethod { get; }

        public IntroductionScope? Scope { get; set; }

        public string? Name { get; set; }

        public bool? IsStatic { get; set; }

        public bool? IsVirtual { get; set; }

        public Visibility? Visibility { get; set; }

        public IntroduceMethodAdvice( IAspect aspect, INamedType targetDeclaration, IMethod templateMethod ) : base( aspect, targetDeclaration )
        {
            this.TemplateMethod = templateMethod;
        }

        public AdviceResult ToResult( ICompilation compilation )
        {
            var introducedMethod = new IntroducedMethod( this, this.TargetDeclaration, this.TemplateMethod, this.Scope, this.Name, this.IsStatic, this.IsVirtual, this.Visibility );
            var overridenMethod = new OverriddenMethod( this, introducedMethod, this.TemplateMethod );

            return new AdviceResult(
                ImmutableArray<Diagnostic>.Empty,
                ImmutableArray.Create<Transformation>(
                    introducedMethod,
                    overridenMethod
                    )
                );

            //string templateMethodName = this.TemplateMethod.Name + TemplateCompiler.TemplateMethodSuffix;
            //var templateReflectionMethod = this.Aspect.GetType().GetMethod( templateMethodName );
            //var targetMethod = this.TargetDeclaration.Methods.Where( m => m.Name == this.TemplateMethod.Name ).GetValue().SingleOrDefault();

            //IntroducedMethod introducedMethod;
            //BlockSyntax methodBody;

            //if ( targetMethod != null )
            //{
            //    // TODO: This should return just OverridenMethod transformation.
            //    methodBody = new TemplateDriver( templateReflectionMethod ).ExpandDeclaration( this.Aspect, targetMethod, compilation );
            //    introducedMethod = new IntroducedMethod( this.TargetDeclaration, targetMethod, this.TemplateMethod, methodBody );
            //}
            //else
            //{
            //    // TODO: This should return IntroducedMethod and OverridenMethod.
            //    introducedMethod = new IntroducedMethod( this.TargetDeclaration, targetMethod, this.TemplateMethod, null );
            //    methodBody = new TemplateDriver( templateReflectionMethod ).ExpandDeclaration( this.Aspect, introducedMethod, compilation );
            //    introducedMethod = new IntroducedMethod( this.TargetDeclaration, targetMethod, this.TemplateMethod, methodBody );
            //}

            //yield return introducedMethod;
        }

    }
}
