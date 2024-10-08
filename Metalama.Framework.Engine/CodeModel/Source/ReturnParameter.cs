// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Reflection;
using RefKind = Microsoft.CodeAnalysis.RefKind;
using SyntaxReference = Microsoft.CodeAnalysis.SyntaxReference;
using TypedConstant = Metalama.Framework.Code.TypedConstant;

namespace Metalama.Framework.Engine.CodeModel.Source;

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

    public override IAssembly DeclaringAssembly => this.DeclaringMember.DeclaringAssembly;

    IDeclarationOrigin IDeclaration.Origin => this.DeclaringMember.Origin;

    public override IDeclaration ContainingDeclaration => this.DeclaringMember;

    public override DeclarationKind DeclarationKind => DeclarationKind.Parameter;

    public override CompilationModel Compilation => this.ContainingDeclaration.AssertNotNull().GetCompilationModel();

    public override bool Equals( IDeclaration? other )
        => other is ReturnParameter returnParameter && this.DeclaringMember.Equals( returnParameter.DeclaringMember );

    public override Location? DiagnosticLocation => this.DeclaringMember.GetDiagnosticLocation();

    public abstract ISymbol? Symbol { get; }

    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ((IDeclarationImpl) this.DeclaringMember).DeclaringSyntaxReferences;

    public override bool CanBeInherited => ((IDeclarationImpl) this.DeclaringMember).CanBeInherited;

    public override string ToString() => this.DeclaringMember + "/" + this.Name;

    public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
        => this.DeclaringMember.ToDisplayString( format, context ) + "/" + this.Name;

    public override IDeclarationOrigin Origin => this.DeclaringMember.Origin;

    protected override int GetHashCodeCore() => this.DeclaringMember.GetHashCode() + 7;

    bool IExpression.IsAssignable => throw new NotSupportedException( "Cannot use the return parameter as an expression." );

    public ref object? Value => throw new NotSupportedException( "Cannot use the return parameter as an expression." );

    public TypedExpressionSyntax ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
        => throw new NotSupportedException( "Cannot use the return parameter as an expression." );

    public override bool BelongsToCurrentProject => this.ContainingDeclaration.BelongsToCurrentProject;

    [Memo]
    private IFullRef<IParameter> Ref
        => this.RefFactory.FromSymbol<IParameter>(
            (IMethodSymbol) this.DeclaringMember.GetSymbol().AssertSymbolNotNull(),
            RefTargetKind.Return );

    private protected override IFullRef<IDeclaration> ToDeclarationRef() => this.Ref;

    IRef<IParameter> IParameter.ToRef() => this.Ref;

    internal override GenericContext GenericContext => (GenericContext) this.ContainingDeclaration.GenericContext;
}