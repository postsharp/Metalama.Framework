// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection;
using Accessibility = Metalama.Framework.Code.Accessibility;

namespace Metalama.Framework.Engine.CodeModel.Builders
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

        public abstract bool IsDesignTime { get; }

        public MemberOrNamedTypeBuilder( Advice parentAdvice, INamedType declaringType, string name ) : base( parentAdvice )
        {
            this.DeclaringType = declaringType;
            this.Name = name;
        }

        public abstract IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context );

        public abstract InsertPosition InsertPosition { get; }

        // TODO: This is temporary.
        public virtual SyntaxTree TargetSyntaxTree => 
            ((NamedType) this.DeclaringType).Symbol.GetPrimarySyntaxReference().AssertNotNull().SyntaxTree;
    }
}