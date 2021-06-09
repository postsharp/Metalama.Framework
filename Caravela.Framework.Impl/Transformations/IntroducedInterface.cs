// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Transformations
{
    internal class IntroducedInterface : IInterfaceImplementationIntroduction
    {
        public IDeclaration ContainingDeclaration => this.TargetType;

        public INamedType InterfaceType { get; }

        public IntroduceInterfaceAdvice Advice { get; }

        public INamedType TargetType { get; }

        public SyntaxTree TargetSyntaxTree => this.TargetType.GetSymbol().DeclaringSyntaxReferences.First().SyntaxTree;

        public IReadOnlyDictionary<IMember, IMember> MemberMap { get; }

        public AspectLinkerOptions? LinkerOptions { get; }

        public IntroducedInterface(
            IntroduceInterfaceAdvice introduceInterfaceAdvice,
            INamedType targetType,
            INamedType interfaceType,
            Dictionary<IMember, IMember> memberMap,
            AspectLinkerOptions? linkerOptions)
        {
            this.Advice = introduceInterfaceAdvice;
            this.TargetType = targetType;
            this.InterfaceType = interfaceType;
            this.MemberMap = memberMap;
            this.LinkerOptions = linkerOptions;
        }

        public IEnumerable<BaseTypeSyntax> GetIntroducedInterfaceImplementations()
        {
            if ( !this.TargetType.ImplementedInterfaces.Contains( this.InterfaceType ) )
            {
                // The type already implements the interface itself.
                return new[]
                {
                    (BaseTypeSyntax) SimpleBaseType(
                        (TypeSyntax) LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( this.InterfaceType.GetSymbol() ) )
                };
            }
            else
            {
                return Enumerable.Empty<BaseTypeSyntax>();
            }
        }
    }
}