using System;
using System.Collections.Generic;
using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Transformations
{
    internal abstract class OverriddenElement : Transformation
    {
        public ICodeElement OverridenDeclaration { get; }

        public abstract IList<MemberDeclarationSyntax> GetOverrides( ICompilation compilation );

        public OverriddenElement( IAdvice advice, ICodeElement overriddenDeclaration ) : base( advice )
        {
            this.OverridenDeclaration = overriddenDeclaration;
        }
    }

    internal class OverriddenMethod : OverriddenElement
    {
        public new IMethod OverridenDeclaration => (IMethod) base.OverridenDeclaration;

        public IMethod TemplateMethod { get; }

        public OverriddenMethod( IAdvice advice, IMethod overriddenDeclaration, IMethod templateMethod ) : base( advice, overriddenDeclaration )
        {
            this.TemplateMethod = templateMethod;
        }

        private IList<MemberDeclarationSyntax>? _overrides;

        public override IList<MemberDeclarationSyntax> GetOverrides( ICompilation compilation )
        {
            if (this._overrides != null)
            {
                return this._overrides;
            }

            // TODO: This is temporary.
            var compiledTemplateMethodName = this.TemplateMethod.Name + TemplateCompiler.TemplateMethodSuffix;
            var newMethodBody = new TemplateDriver( this.Advice.Aspect.GetType().GetMethod( compiledTemplateMethodName ) ).ExpandDeclaration( this.Advice.Aspect, this.OverridenDeclaration, compilation );

            // TODO: other method kinds (constructors).
            var originalSyntax = (MethodDeclarationSyntax) ((IToSyntax) this.OverridenDeclaration).GetSyntaxNode();

            this._overrides = new[] {
                MethodDeclaration(
                    originalSyntax.AttributeLists,
                    originalSyntax.Modifiers,
                    originalSyntax.ReturnType,
                    originalSyntax.ExplicitInterfaceSpecifier,
                    Identifier(originalSyntax.Identifier.ValueText + "_Override_" + Guid.NewGuid().ToString( "N" ) ), // TODO: The name is temporary.
                    originalSyntax.TypeParameterList,
                    originalSyntax.ParameterList,
                    originalSyntax.ConstraintClauses,
                    newMethodBody,
                    null,
                    originalSyntax.SemicolonToken)
            };

            return this._overrides;
        }
    }
}
