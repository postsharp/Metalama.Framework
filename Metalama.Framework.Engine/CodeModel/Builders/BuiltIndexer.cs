// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal sealed class BuiltIndexer : BuiltMember, IIndexerImpl
    {
        private readonly IndexerBuilder _indexerBuilder;

        public BuiltIndexer( IndexerBuilder builder, CompilationModel compilation ) : base( compilation, builder )
        {
            this._indexerBuilder = builder;
        }

        protected override MemberBuilder MemberBuilder => this._indexerBuilder;

        protected override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this._indexerBuilder;

        public RefKind RefKind => this._indexerBuilder.RefKind;

        public Writeability Writeability => this._indexerBuilder.Writeability;

        [Memo]
        public IType Type => this.Compilation.Factory.GetIType( this._indexerBuilder.Type );

        [Memo]
        public IParameterList Parameters
            => new ParameterList(
                this,
                this.GetCompilationModel().GetParameterCollection( this._indexerBuilder.ToTypedRef<IHasParameters>() ) );

        [Memo]
        public IMethod? GetMethod
            => this._indexerBuilder.GetMethod != null ? new BuiltAccessor( this, (AccessorBuilder) this._indexerBuilder.GetMethod ) : null;

        [Memo]
        public IMethod? SetMethod
            => this._indexerBuilder.SetMethod != null ? new BuiltAccessor( this, (AccessorBuilder) this._indexerBuilder.SetMethod ) : null;

        [Obsolete]
        IInvokerFactory<IIndexerInvoker> IIndexer.Invokers => throw new NotSupportedException();

        [Memo]
        public IIndexer? OverriddenIndexer => this.Compilation.Factory.GetDeclaration( this._indexerBuilder.OverriddenIndexer );

        public IIndexerInvoker GetInvoker( InvokerOptions options ) => new IndexerInvoker( this, options );

        public object GetValue( object? target, params object?[] args ) => new IndexerInvoker( this ).GetValue( target, args );

        public object? SetValue( object? target, object value, params object?[] args )
            => new IndexerInvoker( this ).SetValue( target, value, args );

        // TODO: When an interface is introduced, explicit implementation should appear here.
        [Memo]
        public IReadOnlyList<IIndexer> ExplicitInterfaceImplementations
            => this._indexerBuilder.ExplicitInterfaceImplementations.SelectAsImmutableArray( i => this.Compilation.Factory.GetDeclaration( i ) );

        public PropertyInfo ToPropertyInfo() => this._indexerBuilder.ToPropertyInfo();

        public IMethod? GetAccessor( MethodKind methodKind ) => this.GetAccessorImpl( methodKind );

        public IEnumerable<IMethod> Accessors => this._indexerBuilder.Accessors.Select( a => this.Compilation.Factory.GetDeclaration( a ) );
    }
}