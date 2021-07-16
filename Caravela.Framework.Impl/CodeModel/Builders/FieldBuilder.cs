// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Builders;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel.Invokers;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.RunTime;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class FieldBuilder : MemberBuilder, IFieldBuilder
    {
        public override DeclarationKind DeclarationKind => throw new NotImplementedException();

        public IType Type { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IMethod? Getter => throw new NotImplementedException();

        public IMethod? Setter => throw new NotImplementedException();

        public override bool IsExplicitInterfaceImplementation => false;

        [Memo]
        public IInvokerFactory<IFieldOrPropertyInvoker> Invokers
            => new InvokerFactory<IFieldOrPropertyInvoker>( order => new FieldOrPropertyInvoker( this, order ), false );

        IType IFieldOrProperty.Type => throw new NotImplementedException();

        public Writeability Writeability => throw new NotImplementedException();

        public bool IsAutoPropertyOrField => throw new NotImplementedException();

        public override InsertPosition InsertPosition
            => new(
                InsertPositionRelation.Within,
                (MemberDeclarationSyntax) ((NamedType) this.DeclaringType).Symbol.GetPrimaryDeclaration().AssertNotNull() );

        public FieldBuilder( Advice parentAdvice, INamedType targetType, string name )
            : base( parentAdvice, targetType, name ) { }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            throw new NotImplementedException();
        }

        [return: RunTimeOnly]
        public FieldOrPropertyInfo ToFieldOrPropertyInfo()
        {
            throw new NotImplementedException();
        }
    }
}