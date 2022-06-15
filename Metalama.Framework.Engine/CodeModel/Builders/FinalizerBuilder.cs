// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using ParameterList = Metalama.Framework.Engine.CodeModel.Collections.ParameterList;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class FinalizerBuilder : MemberBuilder, IFinalizerBuilder, IFinalizerImpl
    {
        // This is intended only for inserting empty finalizer into code and it's never used directly.

        public MethodKind MethodKind => MethodKind.Finalizer;

        public IParameterList Parameters => ParameterList.Empty;

        public override bool IsExplicitInterfaceImplementation => false;

        public override IMember? OverriddenMember => null;

        public override string Name
        {
            get => "Finalize";
            set => throw new NotSupportedException();
        }

        public override bool IsImplicit => false;

        public override DeclarationKind DeclarationKind => DeclarationKind.Finalizer;

        public IFinalizer OverriddenFinalizer { get; }

        public FinalizerBuilder( Advice parentAdvice, INamedType targetType, IObjectReader tags )
            : base( parentAdvice, targetType, tags )
        {
            Invariant.Assert( targetType.TypeKind == TypeKind.Class || targetType.TypeKind == TypeKind.RecordClass );

            this.OverriddenFinalizer = targetType.Finalizer.AssertNotNull().OverriddenFinalizer.AssertNotNull();
        }

        public IParameterBuilder AddParameter( string name, IType type, RefKind refKind = RefKind.None, TypedConstant defaultValue = default )
        {
            throw new NotSupportedException();
        }

        public IParameterBuilder AddParameter( string name, Type type, RefKind refKind = RefKind.None, object? defaultValue = null )
        {
            throw new NotSupportedException();
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            var syntax =
                DestructorDeclaration(
                    List<AttributeListSyntax>(),
                    TokenList(),
                    ((TypeDeclarationSyntax) this.DeclaringType.GetPrimaryDeclaration().AssertNotNull()).Identifier,
                    ParameterList(),
                    Block().WithGeneratedCodeAnnotation( this.ParentAdvice.Aspect.AspectClass.GeneratedCodeAnnotation ),
                    null );

            return new[] { new IntroducedMember( this, syntax, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, this ) };
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