using System;
using System.Collections.Generic;
using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Transformations
{

    internal class OverriddenMethod : INonObservableTransformation, IMemberIntroduction
    {
        public Advice Advice { get; }

        public IMethod OverridenDeclaration { get; }

        public IMethod TemplateMethod { get; }

        public OverriddenMethod( Advice advice, IMethod overriddenDeclaration, IMethod templateMethod )
        {
            this.Advice = advice;
            this.TemplateMethod = templateMethod;
        }

        public SyntaxTree TargetSyntaxTree => throw new NotImplementedException();

        public IEnumerable<IntroducedMember> GetIntroducedMembers()
        {
            // TODO: Emit a method named __OriginalName__AspectShortName_

            // TODO: This is temporary.
            var compiledTemplateMethodName = this.TemplateMethod.Name + TemplateCompiler.TemplateMethodSuffix;
            var newMethodBody = new TemplateDriver( 
                this.Advice.Aspect.GetType().GetMethod( compiledTemplateMethodName ) )
                .ExpandDeclaration( this.Advice.Aspect, this.OverridenDeclaration, this.OverridenDeclaration.Compilation );

            // TODO: other method kinds (constructors).
            MethodDeclarationSyntax originalSyntax = null; //(MethodDeclarationSyntax) ((ISyn) this.OverridenDeclaration).GetSyntaxNode();
            
            var overrides = new[] {
                new IntroducedMember(
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
                this.Advice.AspectPartId, IntroducedMemberSemantic.MethodOverride )
            };

            return overrides;
        }

        public MemberDeclarationSyntax InsertPositionNode => throw new NotImplementedException();

        public AspectPart AspectPart => throw new NotImplementedException();
    }
}