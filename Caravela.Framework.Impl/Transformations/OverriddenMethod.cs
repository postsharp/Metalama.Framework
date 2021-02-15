using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel.Symbolic;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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
            this.OverridenDeclaration = overriddenDeclaration;
            this.TemplateMethod = templateMethod;
        }

        // TODO: Temporary
        public SyntaxTree TargetSyntaxTree =>
            this.OverridenDeclaration is ISyntaxTreeIntroduction introduction
            ? introduction.TargetSyntaxTree
            :
            ((NamedType) this.OverridenDeclaration.DeclaringType).Symbol.DeclaringSyntaxReferences.First().SyntaxTree;

        public IEnumerable<IntroducedMember> GetIntroducedMembers()
        {
            // TODO: Emit a method named __OriginalName__AspectShortName_
            string methodName =
                this.Advice.PartName != null
                ? $"__OriginalName__{this.Advice.Aspect.GetType().Name}__{this.Advice.PartName}"
                : $"__OriginalName__{this.Advice.Aspect.GetType().Name}";

            // TODO: This is temporary.
            var compiledTemplateMethodName = this.TemplateMethod.Name + TemplateCompiler.TemplateMethodSuffix;
            var newMethodBody = new TemplateDriver(
                this.Advice.Aspect.GetType().GetMethod( compiledTemplateMethodName ).AssertNotNull() )
                .ExpandDeclaration( this.Advice.Aspect, this.OverridenDeclaration, this.OverridenDeclaration.Compilation );

            var overrides = new[] {
                new IntroducedMember(
                MethodDeclaration(
                    List<AttributeListSyntax>(),
                    this.OverridenDeclaration.GetSyntaxModifiers(),
                    this.OverridenDeclaration.GetSyntaxReturnType(),
                    null,
                    Identifier( methodName ), // TODO: The name is temporary.
                    this.OverridenDeclaration.GetSyntaxTypeParameterList(),
                    this.OverridenDeclaration.GetSyntaxParameterList(),
                    this.OverridenDeclaration.GetSyntaxConstraintClauses(),
                    newMethodBody,
                    null,
                    Token(SyntaxKind.SemicolonToken)),
                this.AspectPart.ToAspectPartId(),
                IntroducedMemberSemantic.MethodOverride )
            };

            return overrides;
        }

        public MemberDeclarationSyntax InsertPositionNode => ((NamedType) this.OverridenDeclaration.DeclaringType).Symbol.DeclaringSyntaxReferences.SelectMany( x => ((TypeDeclarationSyntax) x.GetSyntax()).Members ).First();

        public AspectPart AspectPart => throw new NotImplementedException();

        private class ProceedToNext : IProceedImpl
        {
            public StatementSyntax CreateAssignStatement( string returnValueLocalName )
            {
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