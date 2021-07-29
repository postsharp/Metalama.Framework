// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Transformations
{
    internal class IntroducedInterface : IIntroducedInterface
    {
        public IDeclaration ContainingDeclaration => this.TargetType;

        public INamedType InterfaceType { get; }

        public ImplementInterfaceAdvice Advice { get; }

        Advice ITransformation.Advice => this.Advice;

        public INamedType TargetType { get; }

        public SyntaxTree TargetSyntaxTree => this.TargetType.GetSymbol().GetPrimarySyntaxReference().AssertNotNull().SyntaxTree;

        public IReadOnlyDictionary<IMember, IMember> MemberMap { get; }

        public IntroducedInterface(
            ImplementInterfaceAdvice implementInterfaceAdvice,
            INamedType targetType,
            INamedType interfaceType,
            Dictionary<IMember, IMember> memberMap )
        {
            this.Advice = implementInterfaceAdvice;
            this.TargetType = targetType;
            this.InterfaceType = interfaceType;
            this.MemberMap = memberMap;
        }

        public IEnumerable<BaseTypeSyntax> GetIntroducedInterfaceImplementations()
        {
            if ( !this.TargetType.ImplementedInterfaces.Contains( this.InterfaceType ) )
            {
                // The type already implements the interface itself.
                return new[]
                {
                    (BaseTypeSyntax) SimpleBaseType( LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( this.InterfaceType.GetSymbol() ) )
                };
            }
            else
            {
                return Enumerable.Empty<BaseTypeSyntax>();
            }
        }
    }
}