﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
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
    internal class FieldBuilder : MemberBuilder, IFieldBuilder
    {
        public override MemberDeclarationSyntax InsertPositionNode => throw new NotImplementedException();

        public override DeclarationKind DeclarationKind => throw new NotImplementedException();

        public IType Type { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IMethod? Getter => throw new NotImplementedException();

        public IMethod? Setter => throw new NotImplementedException();

        public IFieldOrPropertyInvoker? BaseInvoker => throw new NotImplementedException();

        public IFieldOrPropertyInvoker Invoker => throw new NotImplementedException();

        public IFieldOrPropertyInvoker Base => throw new NotImplementedException();

        public dynamic Value { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        IType IFieldOrProperty.Type => throw new NotImplementedException();

        public AspectLinkerOptions? LinkerOptions { get; }

        public FieldBuilder( Advice parentAdvice, INamedType targetType, string name, AspectLinkerOptions? linkerOptions )
            : base( parentAdvice, targetType, name )
        {
            this.LinkerOptions = linkerOptions;
        }

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