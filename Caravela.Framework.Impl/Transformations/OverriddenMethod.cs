using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel.Symbolic;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Transformations
{

    internal class OverriddenMethod : INonObservableTransformation, IMemberIntroduction, IOverriddenElement
    {
        public Advice Advice { get; }

        ICodeElement IOverriddenElement.OverriddenElement => this.OverriddenDeclaration;

        public IMethod OverriddenDeclaration { get; }

        public IMethod TemplateMethod { get; }

        public OverriddenMethod( Advice advice, IMethod overriddenDeclaration, IMethod templateMethod )
        {
            this.Advice = advice;
            this.OverriddenDeclaration = overriddenDeclaration;
            this.TemplateMethod = templateMethod;
        }

        // TODO: Temporary
        public SyntaxTree TargetSyntaxTree =>
            this.OverriddenDeclaration is ISyntaxTreeTransformation introduction
            ? introduction.TargetSyntaxTree
            :
            ((NamedType) this.OverriddenDeclaration.DeclaringType).Symbol.DeclaringSyntaxReferences.First().SyntaxTree;

        public IEnumerable<IntroducedMember> GetIntroducedMembers()
        {
            // Emit a method named __{OriginalName}__{AspectShortName}_{PartName}
            string methodName =
                this.Advice.PartName != null
                ? $"__{this.OverriddenDeclaration.Name}__{this.Advice.Aspect.Aspect.GetType().Name}__{this.Advice.PartName}"
                : $"__{this.OverriddenDeclaration.Name}__{this.Advice.Aspect.Aspect.GetType().Name}";

            // TODO: This is temporary.
            var compiledTemplateMethodName = this.TemplateMethod.Name + TemplateCompiler.TemplateMethodSuffix;
            var newMethodBody = new TemplateDriver(
                this.Advice.Aspect.Aspect.GetType().GetMethod( compiledTemplateMethodName ).AssertNotNull() )
                .ExpandDeclaration(
                    this.Advice.Aspect.Aspect,
                    this.OverriddenDeclaration,
                    this.OverriddenDeclaration.Compilation,
                    new ProceedInvokeMethod( this.OverriddenDeclaration, this.Advice.AspectPartId ) );

            var overrides = new[] 
            {
                new IntroducedMember(
                    this,
                    MethodDeclaration(
                        List<AttributeListSyntax>(),
                        this.OverriddenDeclaration.GetSyntaxModifiers(),
                        this.OverriddenDeclaration.GetSyntaxReturnType(),
                        null,
                        Identifier( methodName ),
                        this.OverriddenDeclaration.GetSyntaxTypeParameterList(),
                        this.OverriddenDeclaration.GetSyntaxParameterList(),
                        this.OverriddenDeclaration.GetSyntaxConstraintClauses(),
                        newMethodBody,
                        null),
                    this.Advice.AspectPartId,
                    IntroducedMemberSemantic.MethodOverride )
            };

            return overrides;
        }

        public MemberDeclarationSyntax InsertPositionNode => ((NamedType) this.OverriddenDeclaration.DeclaringType).Symbol.DeclaringSyntaxReferences.SelectMany( x => ((TypeDeclarationSyntax) x.GetSyntax()).Members ).First();

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