﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.ReflectionMocks;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Reflection;
using RefKind = Microsoft.CodeAnalysis.RefKind;
using SyntaxReference = Microsoft.CodeAnalysis.SyntaxReference;
using TypedConstant = Metalama.Framework.Code.TypedConstant;

namespace Metalama.Framework.Engine.CodeModel
{
    internal abstract class ReturnParameter : BaseDeclaration, IParameterImpl
    {
        protected abstract RefKind SymbolRefKind { get; }

        public Code.RefKind RefKind => this.SymbolRefKind.ToOurRefKind();

        public abstract IType Type { get; }

        public string Name => "<return>";

        public int Index => -1;

        TypedConstant? IParameter.DefaultValue => default;

        public bool IsParams => false;

        public abstract IHasParameters DeclaringMember { get; }

        public ParameterInfo ToParameterInfo() => CompileTimeReturnParameterInfo.Create( this );

        public virtual bool IsReturnParameter => true;

        internal override Ref<IDeclaration> ToRef()
            => Ref.ReturnParameter( (IMethodSymbol) this.DeclaringMember.GetSymbol().AssertNotNull(), this.GetCompilationModel().RoslynCompilation );

        public override IAssembly DeclaringAssembly => this.DeclaringMember.DeclaringAssembly;

        DeclarationOrigin IDeclaration.Origin => DeclarationOrigin.Source;

        public override IDeclaration? ContainingDeclaration => this.DeclaringMember;

        public override DeclarationKind DeclarationKind => DeclarationKind.Parameter;

        public override CompilationModel Compilation => this.ContainingDeclaration?.GetCompilationModel() ?? throw new AssertionFailedException();

        public override bool Equals( IDeclaration? other )
            => other is ReturnParameter returnParameter && this.DeclaringMember.Equals( returnParameter.DeclaringMember );

        public override Location? DiagnosticLocation => this.DeclaringMember.GetDiagnosticLocation();

        public abstract ISymbol? Symbol { get; }

        public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ((IDeclarationImpl) this.DeclaringMember).DeclaringSyntaxReferences;

        public override bool CanBeInherited => ((IDeclarationImpl) this.DeclaringMember).CanBeInherited;

        public override string ToString() => this.DeclaringMember + "/" + this.Name;

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this.DeclaringMember.ToDisplayString( format, context ) + "/" + this.Name;

        public override DeclarationOrigin Origin => this.DeclaringMember.Origin;

        protected override int GetHashCodeCore() => this.DeclaringMember.GetHashCode() + 7;
    }
}