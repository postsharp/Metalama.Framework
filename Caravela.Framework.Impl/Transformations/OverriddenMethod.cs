using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Transformations
{

    /// <summary>
    /// Represents an override, not an introduction.
    /// </summary>
    internal interface IOverridenElement
    {
        
        /// <summary>
        /// Gets the <see cref="AspectPart"/> that produced the <see cref="IOverridenElement"/>. This is
        /// used to order the different overrides of the same member.
        /// </summary>
        AspectPart AspectPart { get; }
        
    }
    
    internal class OverriddenMethod : INonObservableTransformation, IMemberIntroduction, IOverridenElement
    {
        public IMethod OverridenDeclaration { get; }

        public IMethod TemplateMethod { get; }

        public OverriddenMethod( IAdvice advice, IMethod overriddenDeclaration, IMethod templateMethod ) 
        {
            this.TemplateMethod = templateMethod;
        }

       
        public SyntaxTree TargetSyntaxTree => throw new NotImplementedException();

        public IEnumerable<MemberDeclarationSyntax> GetIntroducedMembers()
        {
            // TODO: Emit a method named __OriginalName__AspectShortName_
       
            // TODO: This is temporary.
            var compiledTemplateMethodName = this.TemplateMethod.Name + TemplateCompiler.TemplateMethodSuffix;
            var newMethodBody = new TemplateDriver( this.Advice.Aspect.GetType().GetMethod( compiledTemplateMethodName ) ).ExpandDeclaration( this.Advice.Aspect, this.OverridenDeclaration, compilation );

            // TODO: other method kinds (constructors).
            var originalSyntax = (MethodDeclarationSyntax) ((IToSyntax) this.OverridenDeclaration).GetSyntaxNode();

            var overrides = new[] {
                SyntaxFactory.MethodDeclaration(
                    originalSyntax.AttributeLists,
                    originalSyntax.Modifiers,
                    originalSyntax.ReturnType,
                    originalSyntax.ExplicitInterfaceSpecifier,
                    SyntaxFactory.Identifier(originalSyntax.Identifier.ValueText + "_Override_" + Guid.NewGuid().ToString( "N" ) ), // TODO: The name is temporary.
                    originalSyntax.TypeParameterList,
                    originalSyntax.ParameterList,
                    originalSyntax.ConstraintClauses,
                    newMethodBody,
                    null,
                    originalSyntax.SemicolonToken),
            };

            return _overrides;
        }

        public MemberDeclarationSyntax InsertPositionNode => throw new NotImplementedException();

        public AspectPart AspectPart => throw new NotImplementedException();
    }
}