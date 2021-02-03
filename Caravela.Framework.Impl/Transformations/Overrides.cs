﻿using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Transformations
{
    abstract class OverriddenElement : Transformation
    {
        public ICodeElement OverridenDeclaration { get; }

        public abstract IList<MemberDeclarationSyntax> GetOverrides();

        public OverriddenElement(IAdvice advice, ICodeElement overridenDeclaration ) : base(advice)
        {
            this.OverridenDeclaration = overridenDeclaration;
        }
    }

    class OverriddenMethod : OverriddenElement
    {
        public new IMethod OverridenDeclaration => (IMethod)base.OverridenDeclaration;
        public IMethod TemplateMethod { get; }

        public OverriddenMethod( IAdvice advice, IMethod overridenDeclaration, IMethod templateMethod ) : base(advice, overridenDeclaration)
        {
            this.TemplateMethod = templateMethod;
        }

        public override IList<MemberDeclarationSyntax> GetOverrides()
        {
            // TODO: This is temporary.
            string compiledTemplateMethodName = this.TemplateMethod.Name + TemplateCompiler.TemplateMethodSuffix;
            var newMethodBody = new TemplateDriver( this.Advice.Aspect.GetType().GetMethod( compiledTemplateMethodName) ).ExpandDeclaration( this.Advice.Aspect, this.OverridenDeclaration, GetCompilation( this.OverridenDeclaration ) );

            var originalSyntax = (MethodDeclarationSyntax) ((IToSyntax) this.OverridenDeclaration).GetSyntaxNode();

            return new[] {
                MethodDeclaration(
                    originalSyntax.AttributeLists,
                    originalSyntax.Modifiers,
                    originalSyntax.ReturnType,
                    originalSyntax.ExplicitInterfaceSpecifier,
                    Identifier(originalSyntax.Identifier.ValueText + "_" + Guid.NewGuid().ToString()), // TODO: This should be deterministic.
                    originalSyntax.TypeParameterList,
                    originalSyntax.ParameterList,
                    originalSyntax.ConstraintClauses,
                    newMethodBody,
                    null,
                    originalSyntax.SemicolonToken
                    )
            };
        }

        static ICompilation GetCompilation(ICodeElement element)
        {
            // This is temporary.
            while ( element is not ICompilation )
                element = element.ContainingElement!;

            return (ICompilation)element;
        }
    }
}
