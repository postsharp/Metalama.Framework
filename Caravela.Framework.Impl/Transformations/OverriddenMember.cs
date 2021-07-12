﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Transformations
{
    internal abstract class OverriddenMember : INonObservableTransformation, IMemberIntroduction, IOverriddenDeclaration
    {
        public Advice Advice { get; }

        public IMember OverriddenDeclaration { get; }

        public AspectLinkerOptions? LinkerOptions { get; }

        IDeclaration IOverriddenDeclaration.OverriddenDeclaration => this.OverriddenDeclaration;

        // TODO: Temporary
        public SyntaxTree TargetSyntaxTree
            => this.OverriddenDeclaration is ISyntaxTreeTransformation introduction
                ? introduction.TargetSyntaxTree
                : ((NamedType) this.OverriddenDeclaration.DeclaringType).Symbol.DeclaringSyntaxReferences.First().SyntaxTree;

        public OverriddenMember( Advice advice, IMember overriddenDeclaration, AspectLinkerOptions? linkerOptions = null )
        {
            Invariant.Assert( advice != null! );
            Invariant.Assert( overriddenDeclaration != null! );

            this.Advice = advice;
            this.OverriddenDeclaration = overriddenDeclaration;
            this.LinkerOptions = linkerOptions;
        }

        public abstract IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context );

        public InsertPosition InsertPosition
        {
            get
            {
                // TODO: Select a good syntax reference if there are multiple (partial class, partial method).
                var memberSymbol = (this.OverriddenDeclaration as Member)?.Symbol;

                if ( memberSymbol != null )
                {
                    var syntaxReference = (MemberDeclarationSyntax?) memberSymbol.DeclaringSyntaxReferences
                        .OrderBy( dsr => dsr.SyntaxTree.FilePath, StringComparer.Ordinal )
                        .FirstOrDefault()
                        ?.GetSyntax();

                    if ( syntaxReference != null )
                    {
                        return new InsertPosition(InsertPositionRelation.After, syntaxReference);
                    }
                }

                var typeSymbol = ((NamedType) this.OverriddenDeclaration.DeclaringType).Symbol;

                return new InsertPosition( InsertPositionRelation.Within, typeSymbol.DeclaringSyntaxReferences.Select( x => (TypeDeclarationSyntax) x.GetSyntax() ).First() );
            }
        }

        protected ExpressionSyntax CreateMemberAccessExpression( AspectReferenceTargetKind referenceTargetKind )
        {
            ExpressionSyntax expression;
            if ( !this.OverriddenDeclaration.IsStatic )
            {
                if ( this.OverriddenDeclaration.IsExplicitInterfaceImplementation )
                {
                    var implementedInterfaceMember = this.OverriddenDeclaration.GetExplicitInterfaceImplementation();

                    expression = MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ParenthesizedExpression(
                            CastExpression( 
                                LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression(implementedInterfaceMember.DeclaringType.GetSymbol()),
                                ThisExpression() ) ),
                        IdentifierName( this.OverriddenDeclaration.Name ) );
                }
                else
                {
                    expression = MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ThisExpression(),
                        IdentifierName( this.OverriddenDeclaration.Name ) );
                }
            }
            else
            {
                expression =
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( this.OverriddenDeclaration.DeclaringType.GetSymbol() ),
                        IdentifierName( this.OverriddenDeclaration.Name ) );
            }

            return expression
                .WithAspectReferenceAnnotation(
                    this.Advice.AspectLayerId,
                    AspectReferenceOrder.Base,
                    referenceTargetKind,
                    flags: AspectReferenceFlags.Inlineable );
        }
    }
}