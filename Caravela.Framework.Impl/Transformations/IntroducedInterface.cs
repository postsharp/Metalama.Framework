// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Impl.Transformations
{
    internal class IntroducedInterface : IObservableTransformation, IInterfaceImplementationIntroduction, IMemberIntroduction
    {
        public ICodeElement ContainingElement => this.TargetType;

        public INamedType InterfaceType { get; }

        public IntroduceInterfaceAdvice IntroduceInterfaceAdvice { get; }

        public INamedType TargetType { get; }

        public bool IsExplicit { get; }

        public IReadOnlyDictionary<IMember, IMember> MemberMap { get; }

        public SyntaxTree TargetSyntaxTree => throw new NotImplementedException();

        public MemberDeclarationSyntax InsertPositionNode => throw new NotImplementedException();

        public IntroducedInterface( IntroduceInterfaceAdvice introduceInterfaceAdvice, INamedType targetType, INamedType interfaceType, bool isExplicit, IReadOnlyDictionary<IMember, IMember>? memberMap )
        {
            this.IntroduceInterfaceAdvice = introduceInterfaceAdvice;
            this.TargetType = targetType;
            this.InterfaceType = interfaceType;
            this.IsExplicit = isExplicit;
            this.MemberMap = memberMap;
        }

        public IEnumerable<BaseTypeSyntax> GetIntroducedInterfaceImplementations()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            throw new NotImplementedException();
        }
    }
}
