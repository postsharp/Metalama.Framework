// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.RunTime;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MethodKind = Metalama.Framework.Code.MethodKind;
using RefKind = Metalama.Framework.Code.RefKind;

namespace Metalama.Framework.Engine.CodeModel
{
    internal sealed class Indexer : PropertyOrIndexer, IIndexerImpl
    {
        public Indexer( IPropertySymbol symbol, CompilationModel compilation ) : base( symbol, compilation ) { }

        [Memo]
        public IParameterList Parameters
            => new ParameterList(
                this,
                this.PropertySymbol.Parameters.Select( p => new Ref<IParameter>( p, this.Compilation.RoslynCompilation ) ) );

        public IIndexer? OverriddenIndexer
        {
            get
            {
                var overriddenProperty = this.PropertySymbol.OverriddenProperty;

                if ( overriddenProperty != null )
                {
                    return this.Compilation.Factory.GetIndexer( overriddenProperty );
                }
                else
                {
                    return null;
                }
            }
        }

        public IMember? OverriddenMember => this.OverriddenIndexer;

        [Memo]
        public IReadOnlyList<IIndexer> ExplicitInterfaceImplementations
            => this.PropertySymbol.ExplicitInterfaceImplementations.Select( p => this.Compilation.Factory.GetIndexer( p ) ).ToList();

        [Memo]
        public IInvokerFactory<IIndexerInvoker> Invokers
            => new InvokerFactory<IIndexerInvoker>( ( order, invokerOperator ) => new IndexerInvoker( this, order, invokerOperator ) );

        public override DeclarationKind DeclarationKind => DeclarationKind.Indexer;
    }

    internal abstract class PropertyOrIndexer : Member, IPropertyOrIndexer
    {
        protected IPropertySymbol PropertySymbol { get; }

        public PropertyOrIndexer( IPropertySymbol symbol, CompilationModel compilation ) : base( compilation )
        {
            this.PropertySymbol = symbol;
        }

        public override ISymbol Symbol => this.PropertySymbol;

        public RefKind RefKind => this.PropertySymbol.RefKind.ToOurRefKind();

        public override bool IsImplicit => false;

        public override bool IsExplicitInterfaceImplementation => !this.PropertySymbol.ExplicitInterfaceImplementations.IsEmpty;

        [Memo]
        public IType Type => this.Compilation.Factory.GetIType( this.PropertySymbol.Type );

        [Memo]
        public IMethod? GetMethod => this.PropertySymbol.GetMethod == null ? null : this.Compilation.Factory.GetMethod( this.PropertySymbol.GetMethod );

        [Memo]

        // TODO: get-only properties
        public IMethod? SetMethod => this.PropertySymbol.SetMethod == null ? null : this.Compilation.Factory.GetMethod( this.PropertySymbol.SetMethod );

        public override MemberInfo ToMemberInfo() => this.ToPropertyInfo();

        public PropertyInfo ToPropertyInfo() => CompileTimePropertyInfo.Create( this );

        public override string ToString() => this.PropertySymbol.ToString();

        // TODO: Memo does not work here.
        // [Memo]
        public Writeability Writeability
            => this.PropertySymbol switch
            {
                { IsReadOnly: true } => Writeability.None,
                { SetMethod: { IsInitOnly: true } _ } => Writeability.InitOnly,
                _ => Writeability.All
            };

        public override bool IsAsync => false;

        public IMethod? GetAccessor( MethodKind methodKind )
            => methodKind switch
            {
                MethodKind.PropertyGet => this.GetMethod,
                MethodKind.PropertySet => this.SetMethod,
                _ => null
            };

        public IEnumerable<IMethod> Accessors
        {
            get
            {
                if ( this.GetMethod != null )
                {
                    yield return this.GetMethod;
                }

                if ( this.SetMethod != null )
                {
                    yield return this.SetMethod;
                }
            }
        }
    }

    internal sealed class Property : PropertyOrIndexer, IPropertyImpl
    {
        public Property( IPropertySymbol symbol, CompilationModel compilation ) : base( symbol, compilation ) { }

        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => CompileTimeFieldOrPropertyInfo.Create( this );

        public override MemberInfo ToMemberInfo() => this.ToFieldOrPropertyInfo();

        IInvokerFactory<IFieldOrPropertyInvoker> IFieldOrProperty.Invokers => this.Invokers;

        [Memo]
        public IInvokerFactory<IFieldOrPropertyInvoker> Invokers
            => new InvokerFactory<IFieldOrPropertyInvoker>( ( order, invokerOperator ) => new FieldOrPropertyInvoker( this, order, invokerOperator ) );

        [Memo]
        public bool IsAutoPropertyOrField => this.PropertySymbol.IsAutoProperty();

        public IProperty? OverriddenProperty
        {
            get
            {
                var overriddenProperty = this.PropertySymbol.OverriddenProperty;

                if ( overriddenProperty != null )
                {
                    return this.Compilation.Factory.GetProperty( overriddenProperty );
                }
                else
                {
                    return null;
                }
            }
        }

        public IMember? OverriddenMember => this.OverriddenProperty;

        [Memo]
        public IReadOnlyList<IProperty> ExplicitInterfaceImplementations
            => this.PropertySymbol.ExplicitInterfaceImplementations.Select( p => this.Compilation.Factory.GetProperty( p ) ).ToList();

        public override DeclarationKind DeclarationKind => DeclarationKind.Property;
    }
}