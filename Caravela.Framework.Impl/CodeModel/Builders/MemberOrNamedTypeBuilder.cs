// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Builders;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection;
using Accessibility = Caravela.Framework.Code.Accessibility;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal abstract class MemberOrNamedTypeBuilder : DeclarationBuilder, IMemberOrNamedTypeBuilder, IMemberIntroduction, IObservableTransformation
    {
        public bool IsSealed { get; set; }

        public bool IsNew { get; set; }

        public INamedType DeclaringType { get; }

        public MemberInfo ToMemberInfo() => throw new NotImplementedException();

        public Accessibility Accessibility { get; set; }

        public string Name { get; set; }

        public bool IsAbstract { get; set; }

        public bool IsStatic { get; set; }

        public sealed override IDeclaration ContainingDeclaration => this.DeclaringType;

        public MemberOrNamedTypeBuilder( Advice parentAdvice, INamedType declaringType, string name ) : base( parentAdvice )
        {
            this.DeclaringType = declaringType;
            this.Name = name;
        }

        public abstract IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context );

        public abstract InsertPosition InsertPosition { get; }

        // TODO: This is temporary.
        SyntaxTree ISyntaxTreeTransformation.TargetSyntaxTree => ((NamedType) this.DeclaringType).Symbol.GetPrimarySyntaxReference().AssertNotNull().SyntaxTree;
    }
}