// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class PropertyBuilder : MemberBuilder, IProperty
    {
        public RefKind RefKind => throw new NotImplementedException();

        public bool IsByRef => throw new NotImplementedException();

        public bool IsRef => throw new NotImplementedException();

        public bool IsRefReadonly => throw new NotImplementedException();

        public IParameterList Parameters => throw new NotImplementedException();

        public IPropertyInvocation Base => throw new NotImplementedException();

        public IType Type => throw new NotImplementedException();

        public IMethod? Getter => throw new NotImplementedException();

        public IMethod? Setter => throw new NotImplementedException();

        public bool HasBase => throw new NotImplementedException();

        public dynamic Value { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        IFieldOrPropertyInvocation IFieldOrProperty.Base => throw new NotImplementedException();

        public AspectLinkerOptions? LinkerOptions { get; }

        public override MemberDeclarationSyntax InsertPositionNode => throw new NotImplementedException();

        public override CodeElementKind ElementKind => throw new NotImplementedException();

        public PropertyBuilder( Advice parentAdvice, INamedType targetType, string name, AspectLinkerOptions? linkerOptions )
            : base( parentAdvice, targetType, name )
        {
            this.LinkerOptions = linkerOptions;
        }

        public dynamic GetIndexerValue( dynamic? instance, params dynamic[] args )
        {
            throw new NotImplementedException();
        }

        public dynamic GetValue( dynamic? instance )
        {
            throw new NotImplementedException();
        }

        public dynamic SetIndexerValue( dynamic? instance, dynamic value, params dynamic[] args )
        {
            throw new NotImplementedException();
        }

        public dynamic SetValue( dynamic? instance, dynamic value )
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            throw new NotImplementedException();
        }
    }
}