// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Transformations
{
    internal class IntroduceInterfaceTransformation : BaseTransformation, IIntroduceInterfaceTransformation
    {
        public IDeclaration ContainingDeclaration => this.TargetType;

        public bool IsDesignTime => true;

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
    }
}