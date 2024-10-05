// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Built;

internal sealed class BuiltParameter : BuiltDeclaration, IParameterImpl
{
    private readonly ParameterBuilderData _parameterBuilder;

    public BuiltParameter( ParameterBuilderData builder, CompilationModel compilation, IGenericContext genericContext ) : base( compilation, genericContext )
    {
        this._parameterBuilder = builder;
    }

    public override DeclarationBuilderData BuilderData => this._parameterBuilder;

    public RefKind RefKind => this._parameterBuilder.RefKind;

    [Memo]
    public IType Type => this.MapType( this._parameterBuilder.Type );

    public string Name => this._parameterBuilder.Name;

    public int Index => this._parameterBuilder.Index;

    public TypedConstant? DefaultValue => this._parameterBuilder.DefaultValue;

    public bool IsParams => this._parameterBuilder.IsParams;

    [Memo]
    public IHasParameters DeclaringMember
        => this.MapDeclaration( this._parameterBuilder.ContainingDeclaration.As<IHasParameters>() )
            .AssertNotNull();

    public ParameterInfo ToParameterInfo() => throw new NotImplementedException();

    public bool IsReturnParameter => this.Index >= 0;

    [Memo]
    private IRef<IParameter> Ref => this.RefFactory.FromBuilt<IParameter>( this );

    public IRef<IParameter> ToRef() => this.Ref;

    private protected override IRef<IDeclaration> ToDeclarationRef() => this.Ref;

    bool IExpression.IsAssignable => true;

    public ref object? Value => ref RefHelper.Wrap( new SyntaxUserExpression( SyntaxFactory.IdentifierName( this.Name ), this.Type, true ) );

    public TypedExpressionSyntax ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
        => new(
            new TypedExpressionSyntaxImpl(
                SyntaxFactory.IdentifierName( this.Name ),
                this.Type,
                ((SyntaxSerializationContext) syntaxGenerationContext).CompilationModel,
                true ) );


    public override bool CanBeInherited => ((IDeclarationImpl) this.ContainingDeclaration).CanBeInherited;

    public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = default )
        => ((IMemberImpl) this.DeclaringMember).GetDerivedDeclarations( options )
            .Select( d => ((IHasParameters) d).Parameters[this.Index] );
}
