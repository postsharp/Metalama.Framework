// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using ParameterList = Metalama.Framework.Engine.CodeModel.Collections.ParameterList;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class ConstructorBuilder : MemberBuilder, IConstructorBuilder, IConstructorImpl, IReplaceMemberTransformation
    {
        public ConstructorInitializerKind InitializerKind => ConstructorInitializerKind.None;

        public IParameterList Parameters => ParameterList.Empty;

        public override bool IsExplicitInterfaceImplementation => false;

        public override IMember? OverriddenMember => null;

        // This is implemented by BuiltConstructor and there is no point to support it here.
        public IConstructor? GetBaseConstructor() => throw new NotSupportedException();

        public override string Name
        {
            get => this.IsStatic ? ".cctor" : ".ctor";
            set => throw new NotSupportedException();
        }

        public override DeclarationKind DeclarationKind => DeclarationKind.Constructor;

        public MemberRef<IMember> ReplacedMember { get; }

        public ConstructorBuilder( Advice parentAdvice, INamedType targetType )
            : base( parentAdvice, targetType, null! )
        {
            if ( targetType.Constructors.Any( c => c.GetSymbol().AssertNotNull().GetPrimarySyntaxReference() == null ) )
            {
                Invariant.Assert( targetType.Constructors.Count == 1 );
                this.ReplacedMember = targetType.Constructors.Single().ToMemberRef<IMember>();
            }
        }

        public IParameterBuilder AddParameter( string name, IType type, RefKind refKind = RefKind.None, TypedConstant? defaultValue = null )
        {
            throw new NotImplementedException();
        }

        public IParameterBuilder AddParameter( string name, Type type, RefKind refKind = RefKind.None, TypedConstant? defaultValue = null )
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            if ( this.IsStatic )
            {
                var syntax =
                    ConstructorDeclaration(
                        this.GetAttributeLists( context ),
                        TokenList( Token( SyntaxKind.StaticKeyword ) ),
                        ((TypeDeclarationSyntax) this.DeclaringType.GetPrimaryDeclarationSyntax().AssertNotNull()).Identifier,
                        ParameterList(),
                        null,
                        Block().WithGeneratedCodeAnnotation( this.ParentAdvice.Aspect.AspectClass.GeneratedCodeAnnotation ),
                        null );

                return new[] { new IntroducedMember( this, syntax, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, this ) };
            }
            else
            {
                var syntax =
                    ConstructorDeclaration(
                        this.GetAttributeLists( context ),
                        TokenList( Token( SyntaxKind.PublicKeyword ) ),
                        ((TypeDeclarationSyntax) this.DeclaringType.GetPrimaryDeclarationSyntax().AssertNotNull()).Identifier,
                        ParameterList(),
                        null,
                        Block().WithGeneratedCodeAnnotation( this.ParentAdvice.Aspect.AspectClass.GeneratedCodeAnnotation ),
                        null );

                return new[] { new IntroducedMember( this, syntax, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, this ) };
            }
        }

        public ConstructorInfo ToConstructorInfo()
        {
            throw new NotImplementedException();
        }

        public System.Reflection.MethodBase ToMethodBase()
        {
            throw new NotImplementedException();
        }
    }
}