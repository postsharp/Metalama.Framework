// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using RoslynMethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Metalama.Framework.Engine.CodeModel;

internal sealed class Method : MethodBase, IMethodImpl
{
    public Method( IMethodSymbol symbol, CompilationModel compilation ) : base( symbol, compilation )
    {
        if ( symbol.MethodKind is RoslynMethodKind.Constructor or RoslynMethodKind.StaticConstructor )
        {
            throw new ArgumentOutOfRangeException( nameof(symbol), "Cannot use the Method class with constructors." );
        }

        // This is intentionally debug-only to not break existing code. Should be non-conditional on the next non-RC version.
#if DEBUG
        if ( symbol.PartialDefinitionPart != null )
        {
            throw new ArgumentOutOfRangeException( nameof(symbol), "Cannot use partial implementation to instantiate the Method class." );
        }
#endif
    }

    [Memo]
    public IParameter ReturnParameter => new MethodReturnParameter( this );

    [Memo]
    public IType ReturnType => this.Compilation.Factory.GetIType( this.MethodSymbol.ReturnType );

    [Memo]
    public IGenericParameterList TypeParameters
        => new TypeParameterList(
            this,
            this.MethodSymbol.TypeParameters.Select( x => Ref.FromSymbol<ITypeParameter>( x, this.Compilation.CompilationContext ) )
                .ToList() );

    [Memo]
    public IReadOnlyList<IType> TypeArguments => this.MethodSymbol.TypeArguments.Select( t => this.Compilation.Factory.GetIType( t ) ).ToImmutableArray();

    public override DeclarationKind DeclarationKind => DeclarationKind.Method;

    public OperatorKind OperatorKind => this.MethodSymbol.GetOperatorKind();

    [Memo]
    public IMethod MethodDefinition
        => this.MethodSymbol == this.MethodSymbol.OriginalDefinition ? this : this.Compilation.Factory.GetMethod( this.MethodSymbol.OriginalDefinition );

    public bool IsPartial => this.MethodSymbol.IsPartialDefinition || this.MethodSymbol.PartialDefinitionPart != null;

    public bool IsExtern => this.MethodSymbol.IsExtern;

    public IMethodInvoker With( InvokerOptions options ) => new MethodInvoker( this, options );

    public IMethodInvoker With( object? target, InvokerOptions options = default ) => new MethodInvoker( this, options, target );

    public object? Invoke( params object?[] args ) => new MethodInvoker( this ).Invoke( args );

    public object? Invoke( IEnumerable<IExpression> args ) => new MethodInvoker( this ).Invoke( args );

    public bool IsGeneric => this.MethodSymbol.TypeParameters.Length > 0;

    IGeneric IGenericInternal.ConstructGenericInstance( IReadOnlyList<IType> typeArguments )
    {
        var symbolWithGenericArguments = this.MethodSymbol.Construct( typeArguments.SelectAsArray( a => a.GetSymbol() ) );

        return new Method( symbolWithGenericArguments, this.Compilation );
    }

    public bool IsReadOnly => this.MethodSymbol.IsReadOnly;

    public override bool IsExplicitInterfaceImplementation => !this.MethodSymbol.ExplicitInterfaceImplementations.IsEmpty;

    [Memo]
    public override bool IsAsync
        => this.MethodSymbol.MetadataToken == 0
            ? this.MethodSymbol.IsAsync
            : this.MethodSymbol.GetAttributes()
                .Any( a => a.AttributeConstructor?.ContainingType.Name is nameof(AsyncStateMachineAttribute) or nameof(AsyncIteratorStateMachineAttribute) );

    public IMethod? OverriddenMethod
    {
        get
        {
            var overriddenMethod = this.MethodSymbol.OverriddenMethod;

            if ( overriddenMethod != null )
            {
                return this.Compilation.Factory.GetMethod( overriddenMethod );
            }
            else
            {
                return null;
            }
        }
    }

    [Memo]
    public IReadOnlyList<IMethod> ExplicitInterfaceImplementations
        => ((IMethodSymbol) this.Symbol).ExplicitInterfaceImplementations.Select( m => this.Compilation.Factory.GetMethod( m ) )
            .ToList();

    public MethodInfo ToMethodInfo() => CompileTimeMethodInfo.Create( this );

    public IHasAccessors? DeclaringMember
        => this.MethodSymbol.AssociatedSymbol != null
            ? this.Compilation.Factory.GetDeclaration( this.MethodSymbol.AssociatedSymbol ) as IHasAccessors
            : null;

    public override System.Reflection.MethodBase ToMethodBase() => this.ToMethodInfo();

    public IMember? OverriddenMember => this.OverriddenMethod;

    public bool IsCanonicalGenericInstance => ReferenceEquals( this.Symbol.OriginalDefinition, this.Symbol );

    public bool? IsIteratorMethod => IteratorHelper.IsIteratorMethod( this.MethodSymbol );

    [Memo]
    public override ImmutableArray<SourceReference> Sources => this.GetSourcesImpl();

    private ImmutableArray<SourceReference> GetSourcesImpl()
    {
        if ( this.MethodSymbol.PartialImplementationPart != null )
        {
            var sources = ImmutableArray.CreateBuilder<SourceReference>( 2 );
            sources.Add( new SourceReference( this.MethodSymbol.DeclaringSyntaxReferences[0].GetSyntax(), SyntaxReferenceImpl.Instance ) );

            sources.Add(
                new SourceReference( this.MethodSymbol.PartialImplementationPart.DeclaringSyntaxReferences[0].GetSyntax(), SyntaxReferenceImpl.Instance ) );

            return sources.MoveToImmutable();
        }
        else
        {
            return base.Sources;
        }
    }
}