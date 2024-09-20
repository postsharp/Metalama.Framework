// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class BuiltParameter : BuiltDeclaration, IParameterImpl
{
    private readonly BaseParameterBuilder _parameterBuilder;

    public BuiltParameter( BaseParameterBuilder builder, CompilationModel compilation, IGenericContext genericContext ) : base( compilation, genericContext )
    {
        this._parameterBuilder = builder;
    }

    public override DeclarationBuilder Builder => this._parameterBuilder;

    public RefKind RefKind => this._parameterBuilder.RefKind;

    [Memo]
    public IType Type => this.MapType( this._parameterBuilder.Type );

    public string Name => this._parameterBuilder.Name;

    public int Index => this._parameterBuilder.Index;

    public TypedConstant? DefaultValue => this._parameterBuilder.DefaultValue;

    public bool IsParams => this._parameterBuilder.IsParams;

    [Memo]
    public IHasParameters DeclaringMember
        => this.Compilation.Factory.Translate(
                this._parameterBuilder.DeclaringMember,
                ReferenceResolutionOptions.CanBeMissing,
                genericContext: this.GenericMap )
            .AssertNotNull();

    public ParameterInfo ToParameterInfo() => this._parameterBuilder.ToParameterInfo();

    public bool IsReturnParameter => this._parameterBuilder.IsReturnParameter;

    [Memo]
    private IRef<IParameter> Ref => this.RefFactory.FromBuilt<IParameter>( this );

    public IRef<IParameter> ToRef() => this.Ref;

    private protected override IRef<IDeclaration> ToDeclarationRef() => this.Ref;

    bool IExpression.IsAssignable => true;

    public ref object? Value => ref this._parameterBuilder.Value;

    public TypedExpressionSyntax ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
        => this._parameterBuilder.ToTypedExpressionSyntax( syntaxGenerationContext );

    public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = default )
        => ((IMemberImpl) this.DeclaringMember).GetDerivedDeclarations( options )
            .Select( d => ((IHasParameters) d).Parameters[this.Index] );
}