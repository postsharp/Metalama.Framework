// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.RunTime;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class BuiltIndexer : BuiltMember, IIndexerImpl
    {
        public BuiltIndexer( IndexerBuilder builder, CompilationModel compilation ) : base( compilation, builder )
        {
            this.IndexerBuilder = builder;
        }

        public IndexerBuilder IndexerBuilder { get; }

        public override MemberBuilder MemberBuilder => this.IndexerBuilder;

        public override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this.IndexerBuilder;

        public RefKind RefKind => this.IndexerBuilder.RefKind;

        public Writeability Writeability => this.IndexerBuilder.Writeability;

        [Memo]
        public IType Type => this.Compilation.Factory.GetIType( this.IndexerBuilder.Type );

        [Memo]
        public IParameterList Parameters
            => new ParameterList(
                this,
                this.GetCompilationModel().GetParameterCollection( this.IndexerBuilder.ToTypedRef<IHasParameters>() ) );

        [Memo]
        public IMethod? GetMethod => this.IndexerBuilder.GetMethod != null ? new BuiltAccessor( this, (AccessorBuilder) this.IndexerBuilder.GetMethod ) : null;

        [Memo]
        public IMethod? SetMethod => this.IndexerBuilder.SetMethod != null ? new BuiltAccessor( this, (AccessorBuilder) this.IndexerBuilder.SetMethod ) : null;

        [Memo]
        public IInvokerFactory<IIndexerInvoker> Invokers => new InvokerFactory<IIndexerInvoker>( ( order, _ ) => new IndexerInvoker( this, order ) );

        [Memo]
        public IIndexer? OverriddenIndexer => this.Compilation.Factory.GetDeclaration( this.IndexerBuilder.OverriddenIndexer );

        // TODO: When an interface is introduced, explicit implementation should appear here.
        [Memo]
        public IReadOnlyList<IIndexer> ExplicitInterfaceImplementations
            => this.IndexerBuilder.ExplicitInterfaceImplementations.SelectArray( i => this.Compilation.Factory.GetDeclaration( i ) );

        public FieldOrPropertyInfo ToFieldOrPropertyOrIndexerInfo() => this.IndexerBuilder.ToFieldOrPropertyOrIndexerInfo();

        public PropertyInfo ToPropertyInfo() => this.IndexerBuilder.ToPropertyInfo();

        public IMethod? GetAccessor( MethodKind methodKind ) => this.GetAccessorImpl( methodKind );

        public IEnumerable<IMethod> Accessors => this.IndexerBuilder.Accessors.Select( a => this.Compilation.Factory.GetDeclaration( a ) );
    }
}