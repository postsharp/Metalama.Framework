// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Transformations
{
    internal class OverriddenProperty : INonObservableTransformation, IMemberIntroduction, IOverriddenElement
    {
        public Advice Advice { get; }

        ICodeElement IOverriddenElement.OverriddenElement => this.OverriddenDeclaration;

        public IProperty OverriddenDeclaration { get; }

        public IMethod? GetTemplateMethod { get; }

        public IMethod? SetTemplateMethod { get; }

        public AspectLinkerOptions? LinkerOptions { get; }

        public OverriddenProperty(
            Advice advice,
            IProperty overriddenDeclaration,
            IMethod? getTemplateMethod,
            IMethod? setTemplateMethod,
            AspectLinkerOptions? linkerOptions = null )
        {
            Invariant.Assert( advice != null );
            Invariant.Assert( overriddenDeclaration != null );
            Invariant.Assert( getTemplateMethod != null );
            Invariant.Assert( setTemplateMethod != null );

            this.Advice = advice;
            this.OverriddenDeclaration = overriddenDeclaration;
            this.GetTemplateMethod = getTemplateMethod;
            this.SetTemplateMethod = setTemplateMethod;
            this.LinkerOptions = linkerOptions;
        }

        // TODO: Temporary
        public SyntaxTree TargetSyntaxTree
            => this.OverriddenDeclaration is ISyntaxTreeTransformation introduction
                ? introduction.TargetSyntaxTree
                : ((NamedType) this.OverriddenDeclaration.DeclaringType).Symbol.DeclaringSyntaxReferences.First().SyntaxTree;

        public IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            throw new NotImplementedException();
        }

        public MemberDeclarationSyntax InsertPositionNode
        {
            get
            {
                // TODO: Select a good syntax reference if there are multiple (partial class, partial method).
                var methodSymbol = (this.OverriddenDeclaration as Property)?.Symbol;

                if ( methodSymbol != null )
                {
                    return methodSymbol.DeclaringSyntaxReferences.Select( x => (MethodDeclarationSyntax) x.GetSyntax() ).First();
                }

                var typeSymbol = ((NamedType) this.OverriddenDeclaration.DeclaringType).Symbol;

                return typeSymbol.DeclaringSyntaxReferences.Select( x => (TypeDeclarationSyntax) x.GetSyntax() ).First();
            }
        }
    }
}