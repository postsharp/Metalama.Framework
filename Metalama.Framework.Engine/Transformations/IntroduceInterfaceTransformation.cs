// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Transformations
{
    internal class IntroduceInterfaceTransformation : IIntroduceInterfaceTransformation
    {
        public IDeclaration ContainingDeclaration => this.TargetType;

        public bool IsDesignTime => true;

        public INamedType InterfaceType { get; }

        private ImplementInterfaceAdvice Advice { get; }

        Advice ITransformation.Advice => this.Advice;

        public INamedType TargetType { get; }

        public SyntaxTree TargetSyntaxTree => this.TargetType.GetSymbol().GetPrimarySyntaxReference().AssertNotNull().SyntaxTree;

        public IReadOnlyDictionary<IMember, IMember> MemberMap { get; }

        public IntroduceInterfaceTransformation(
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

        public BaseTypeSyntax GetSyntax()
        {
            if ( !this.TargetType.ImplementedInterfaces.Contains( this.InterfaceType ) )
            {
                var targetSyntax = this.TargetType.GetSymbol().GetPrimarySyntaxReference().AssertNotNull();

                var generationContext = SyntaxGenerationContext.Create(
                    this.TargetType.Compilation.Project.ServiceProvider,
                    this.TargetType.GetCompilationModel().RoslynCompilation,
                    targetSyntax.SyntaxTree,
                    targetSyntax.Span.Start );

                // The type already implements the interface members itself.
                return SimpleBaseType( generationContext.SyntaxGenerator.Type( this.InterfaceType.GetSymbol() ) );
            }
            else
            {
                // Transformation should not be created iff the interface is not present on the target type.
                throw new AssertionFailedException();
            }
        }
    }
}