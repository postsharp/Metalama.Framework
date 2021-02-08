using System;
using System.Collections.Generic;
using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Templating.MetaModel;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Transformations
{
    internal abstract class OverriddenElement : Transformation
    {
        public ICodeElement OverridenDeclaration { get; }

        public abstract IList<MemberDeclarationSyntax> GetOverrides( ICompilation compilation );

        public OverriddenElement( IAdvice advice, ICodeElement overridenDeclaration ) : base( advice )
        {
            this.OverridenDeclaration = overridenDeclaration;
        }
    }

    internal class OverriddenMethod : OverriddenElement
    {
        public new IMethod OverridenDeclaration => (IMethod) base.OverridenDeclaration;

        public IMethod TemplateMethod { get; }

        public OverriddenMethod( IAdvice advice, IMethod overridenDeclaration, IMethod templateMethod ) : base( advice, overridenDeclaration )
        {
            this.TemplateMethod = templateMethod;
        }

        public override IList<MemberDeclarationSyntax> GetOverrides( ICompilation compilation )
        {
            // TODO: This is temporary.
            var compiledTemplateMethodName = this.TemplateMethod.Name + TemplateCompiler.TemplateMethodSuffix;
            var newMethodBody = new TemplateDriver( this.Advice.Aspect.GetType().GetMethod( compiledTemplateMethodName ) ).ExpandDeclaration( this.Advice.Aspect, this.OverridenDeclaration, compilation );

            // TODO: other method kinds (constructors).
            var originalSyntax = (MethodDeclarationSyntax) ((IToSyntax) this.OverridenDeclaration).GetSyntaxNode();

            return new[] {
                MethodDeclaration(
                    originalSyntax.AttributeLists,
                    originalSyntax.Modifiers,
                    originalSyntax.ReturnType,
                    originalSyntax.ExplicitInterfaceSpecifier,
                    Identifier(originalSyntax.Identifier.ValueText + "_Override_" + Guid.NewGuid().ToString()), // TODO: This should be deterministic.
                    originalSyntax.TypeParameterList,
                    originalSyntax.ParameterList,
                    originalSyntax.ConstraintClauses,
                    newMethodBody,
                    null,
                    originalSyntax.SemicolonToken)
            };
        }

        private class LinkerProceed : IProceedImpl
        {
            public LinkerProceed()
            {
            }

            public StatementSyntax CreateAssignStatement( string returnValueLocalName )
            {
                // Assign result
                throw new NotImplementedException();
            }

            public StatementSyntax CreateReturnStatement()
            {
                throw new NotImplementedException();
            }

            public TypeSyntax CreateTypeSyntax()
            {
                throw new NotImplementedException();
            }
        }
    }
}
