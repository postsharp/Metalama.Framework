﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Transformations
{
    internal class IntroduceInterfaceTransformation : BaseTransformation, IIntroduceInterfaceTransformation, IInjectInterfaceTransformation
    {
        public IDeclaration ContainingDeclaration => this.TargetType;

        public INamedType InterfaceType { get; }

        public INamedType TargetType { get; }

        public IReadOnlyDictionary<IMember, IMember> MemberMap { get; }

        public IntroduceInterfaceTransformation(
            ImplementInterfaceAdvice implementInterfaceAdvice,
            INamedType targetType,
            INamedType interfaceType,
            Dictionary<IMember, IMember> memberMap ) : base( implementInterfaceAdvice )
        {
            this.TargetType = targetType;
            this.InterfaceType = interfaceType;
            this.MemberMap = memberMap;
        }

        public BaseTypeSyntax GetSyntax()
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

        public override IDeclaration TargetDeclaration => this.TargetType;

        public override TransformationObservability Observability => TransformationObservability.Always;
    }
}