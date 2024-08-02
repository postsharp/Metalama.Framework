// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Metrics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using MethodKind = Metalama.Framework.Code.MethodKind;
using RefKind = Metalama.Framework.Code.RefKind;
using TypedConstant = Metalama.Framework.Code.TypedConstant;

namespace Metalama.Framework.Engine.CodeModel.Substituted;

internal sealed class SubstitutedMethod : SubstitutedMember, IMethodImpl
{
    internal BuiltMethod SourceMethod { get; }

    public SubstitutedMethod( BuiltMethod sourceMethod, INamedTypeSymbol substitutedType )
        : base( sourceMethod, substitutedType )
    {
        this.SourceMethod = sourceMethod;
    }

    public System.Reflection.MethodBase ToMethodBase() => throw new NotImplementedException();

    [Memo]
    private BoxedRef<IMethod> BoxedRef => new BoxedRef<IMethod>( this.ToValueTypedRef() );

    IRef<IMethod> IMethod.ToRef() => this.BoxedRef;

    IRef<IMethodBase> IMethodBase.ToRef() => this.BoxedRef;

    public IGenericParameterList TypeParameters => this.SourceMethod.TypeParameters;

    public IReadOnlyList<IType> TypeArguments => this.SourceMethod.TypeArguments;

    public bool IsGeneric => this.SourceMethod.IsGeneric;

    public bool IsCanonicalGenericInstance => false;

    IGeneric IGenericInternal.ConstructGenericInstance( IReadOnlyList<IType> typeArguments ) => throw new NotImplementedException();

    // TODO: test invocations and invokers
    public object? Invoke( params object?[] args ) => new MethodInvoker( this ).Invoke( args );

    public object? Invoke( IEnumerable<IExpression> args ) => new MethodInvoker( this ).Invoke( args );

    public IMethodInvoker With( IExpression target, InvokerOptions options = default ) => new MethodInvoker( this, options, target );

    public IExpression CreateInvokeExpression( IEnumerable<IExpression> args ) => new MethodInvoker( this ).CreateInvokeExpression( args );

    public IMethodInvoker With( InvokerOptions options ) => new MethodInvoker( this, options );

    public IMethodInvoker With( object? target, InvokerOptions options = default ) => new MethodInvoker( this, options, target );

    bool IMethod.IsPartial => ((IMethod) this.SourceMethod).IsPartial;

    public MethodKind MethodKind => this.SourceMethod.MethodKind;

    [Memo]
    public IParameter ReturnParameter => new Parameter( this, this.SourceMethod.ReturnParameter );

    [Memo]
    public IParameterList Parameters => new ParameterList( this, this.SourceMethod.Parameters.SelectAsImmutableArray( param => new Parameter( this, param ) ) );

    public IType ReturnType => this.Substitute( this.SourceMethod.ReturnType );

    public IMethod? OverriddenMethod => (IMethod?) this.OverriddenMember;

    public IReadOnlyList<IMethod> ExplicitInterfaceImplementations
        => this.SourceMethod.ExplicitInterfaceImplementations.SelectAsReadOnlyList(
            m => SubstitutedMemberFactory.Substitute( m, this.GenericMap, this.SubstitutedType ).GetTarget( ReferenceResolutionOptions.Default ) );

    public MethodInfo ToMethodInfo() => throw new NotImplementedException();

    // TODO: this is correct for BuiltMethod, but not for BuiltAccessor
    IHasAccessors? IMethod.DeclaringMember => null;

    public bool IsReadOnly => this.SourceMethod.IsReadOnly;

    public OperatorKind OperatorKind => this.SourceMethod.OperatorKind;

    public new IMethod Definition => this.SourceMethod;

    bool IMethod.IsExtern => ((IMethod) this.SourceMethod).IsExtern;

    public bool? IsIteratorMethod => this.SourceMethod.IsIteratorMethod;

    public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
    {
        var parameterTypes = this.Parameters.AsEnumerable().Select( p => p.Type );

        return DisplayStringFormatter.Format( format, context, $"{this.DeclaringType}.{this.Name}({parameterTypes})" );
    }

    // TODO: move out and use for indexers
    private sealed class Parameter : IParameterImpl, ISubstitutedDeclaration
    {
        private readonly SubstitutedMethod _targetMethod;
        private readonly IParameter _sourceParameter;

        public Parameter( SubstitutedMethod targetMethod, IParameter sourceParameter )
        {
            this._targetMethod = targetMethod;
            this._sourceParameter = sourceParameter;
        }

        public CompilationModel Compilation => this._targetMethod.Compilation;

        GenericMap ISubstitutedDeclaration.GenericMap => this._targetMethod.GenericMap;

        ICompilation ICompilationElement.Compilation => this.Compilation;

        ImmutableArray<SyntaxReference> IDeclarationImpl.DeclaringSyntaxReferences => this._sourceParameter.GetDeclaringSyntaxReferences();

        public bool CanBeInherited => ((IParameterImpl) this._sourceParameter).CanBeInherited;

        SyntaxTree? IDeclarationImpl.PrimarySyntaxTree => this._sourceParameter.GetPrimarySyntaxTree();

        public IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = default )
            => ((IParameterImpl) this._sourceParameter).GetDerivedDeclarations( options )
                .Select( d => SubstitutedMemberFactory.Substitute( d, this._targetMethod.GenericMap ).GetTarget( ReferenceResolutionOptions.Default ) );

        public Ref<IDeclaration> ToValueTypedRef() => Ref.FromDeclarationId<IDeclaration>( this.GetSerializableId() );

        IRef<IDeclaration> IDeclaration.ToRef() => this.ToValueTypedRef();

        Ref<ICompilationElement> ICompilationElementImpl.ToRef() => this.ToValueTypedRef().As<ICompilationElement>();

        [Memo]
        private BoxedRef<IParameter> BoxedRef => new BoxedRef<IParameter>( this.ToValueTypedRef() );

        IRef<IParameter> IParameter.ToRef() => this.BoxedRef;

        public SerializableDeclarationId ToSerializableId() => this.GetSerializableId();

        public IAssembly DeclaringAssembly => this._targetMethod.DeclaringAssembly;

        public IDeclarationOrigin Origin => this._sourceParameter.Origin;

        public IDeclaration ContainingDeclaration => this._targetMethod;

        public DeclarationKind DeclarationKind => DeclarationKind.Parameter;

        public bool IsImplicitlyDeclared => this._sourceParameter.IsImplicitlyDeclared;

        public int Depth => this._sourceParameter.Depth;

        public bool BelongsToCurrentProject => this._targetMethod.BelongsToCurrentProject;

        public ImmutableArray<SourceReference> Sources => this._sourceParameter.Sources;

        public string Name => this._sourceParameter.Name;

        public IType Type => this._targetMethod.Substitute( this._sourceParameter.Type );

        public RefKind RefKind => this._sourceParameter.RefKind;

        bool IExpression.IsAssignable => true;

        public ref object? Value => ref this._sourceParameter.Value;

        public TypedExpressionSyntax ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
        {
            var sourceExpression = (TypedExpressionSyntaxImpl) this._sourceParameter.ToTypedExpressionSyntax( syntaxGenerationContext ).Implementation;

            return new TypedExpressionSyntax(
                new TypedExpressionSyntaxImpl(
                    sourceExpression.Syntax,
                    this.MapIType( sourceExpression.ExpressionType ),
                    ((SyntaxSerializationContext) syntaxGenerationContext).CompilationModel,
                    sourceExpression.IsReferenceable,
                    sourceExpression.CanBeNull ) );
        }

        public int Index => this._sourceParameter.Index;

        [Memo]
        public TypedConstant? DefaultValue
            => this._sourceParameter.DefaultValue is { } sourceValue
                ? TypedConstant.CreateUnchecked( sourceValue.Value, this.MapIType( sourceValue.Type ) )
                : null;

        public bool IsParams => this._sourceParameter.IsParams;

        public IHasParameters DeclaringMember => this._targetMethod;

        public ParameterInfo ToParameterInfo() => throw new NotImplementedException();

        public bool IsReturnParameter => this._sourceParameter.IsReturnParameter;

        public ISymbol? Symbol => null;

        T IMeasurableInternal.GetMetric<T>() => ((IParameterImpl) this._sourceParameter).GetMetric<T>();

        public Location? DiagnosticLocation => this._sourceParameter.GetDiagnosticLocation();

        [Memo]
        public IAttributeCollection Attributes => CreateSubstitutedAttributeCollection( this, this._sourceParameter.Attributes );

        public bool Equals( IDeclaration? other ) => ReferenceEquals( this, other );

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();
    }

    private sealed class ParameterList : IParameterList
    {
        private readonly SubstitutedMethod _method;
        private readonly ImmutableArray<Parameter> _parameters;

        public ParameterList( SubstitutedMethod method, ImmutableArray<Parameter> parameters )
        {
            this._method = method;
            this._parameters = parameters;
        }

        public IParameter this[ string name ]
            => this._parameters.SingleOrDefault( p => p.Name == name ) ??
               throw new ArgumentOutOfRangeException( nameof(name), $"The method '{this._method}' does not contain a parameter named '{name}'" );

        public IParameter this[ int index ] => this._parameters[index];

        public int Count => this._parameters.Length;

        public IEnumerator<IParameter> GetEnumerator() => this._parameters.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public object ToValueArray() => new ValueArrayExpression( this );
    }
}