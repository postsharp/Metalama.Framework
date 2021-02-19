using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel.Symbolic;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
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
            Invariant.Assert( advice != null, $"{nameof( advice )} should not be null." );
            Invariant.Assert( overriddenDeclaration != null, $"{nameof( overriddenDeclaration )} should not be null." );
            Invariant.Assert( templateMethod != null, $"{nameof( templateMethod )} should not be null." );

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

        public IEnumerable<IntroducedMember> GetIntroducedMembers( IntroductionContext context )
        {

            // Emit a method named __{OriginalName}__{AspectShortName}_{PartName}
            string methodName =
                this.Advice.PartName != null
                ? $"__{this.OverriddenDeclaration.Name}__{this.Advice.Aspect.Aspect.GetType().Name}__{this.Advice.PartName}"
                : $"__{this.OverriddenDeclaration.Name}__{this.Advice.Aspect.Aspect.GetType().Name}";
            
            Invariant.Assert( DiagnosticContext.Current.Sink != null, "DiagnosticContext.Current.Sink must be set" );

            // TODO: This is temporary.
            var expansionContext = new TemplateExpansionContext( 
                this.Advice.Aspect.Aspect,
                this.OverriddenDeclaration,
                this.OverriddenDeclaration.Compilation,
                new LinkerCallProceedImpl( this.OverriddenDeclaration, this.Advice.AspectPartId ));
            var compiledTemplateMethodName = this.TemplateMethod.Name + TemplateCompiler.TemplateMethodSuffix;
            
            var newMethodBody = new TemplateDriver(
                this.Advice.Aspect.Aspect.GetType().GetMethod( compiledTemplateMethodName ).AssertNotNull() )
                .ExpandDeclaration( expansionContext );

            var overrides = new[] 
            {
                new IntroducedMember(
                    this,
                    MethodDeclaration(
                        List<AttributeListSyntax>(),
                        this.OverriddenDeclaration.GetSyntaxModifierList(),
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

        public MemberDeclarationSyntax InsertPositionNode
        {
            get
            {
                // TODO: Select a good syntax reference if there are multiple (partial class, partial method).
                var methodSymbol = (this.OverriddenDeclaration as Method)?.Symbol;

                if (methodSymbol != null)
                {
                    return methodSymbol.DeclaringSyntaxReferences.Select( x => (MethodDeclarationSyntax) x.GetSyntax() ).First();
                }

                var typeSymbol = ((NamedType) this.OverriddenDeclaration.DeclaringType).Symbol;

                return typeSymbol.DeclaringSyntaxReferences.Select( x => (TypeDeclarationSyntax) x.GetSyntax() ).First();
            }
        }
    }
}