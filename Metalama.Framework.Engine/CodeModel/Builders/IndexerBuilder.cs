// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.RunTime;
using System;
using System.Collections.Generic;
using System.Reflection;
using MethodKind = Metalama.Framework.Code.MethodKind;
using RefKind = Metalama.Framework.Code.RefKind;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class IndexerBuilder : MemberBuilder, IIndexerBuilder, IIndexerImpl
    {
        private IType _type;

        public bool HasInitOnlySetter { get; set; }

        public ParameterBuilderList Parameters { get; } = new();

        public RefKind RefKind { get; set; }

        public virtual Writeability Writeability
        {
            get => this switch
            {
                { SetMethod: null } => Writeability.None,
                { HasInitOnlySetter: true } => Writeability.InitOnly,
                _ => Writeability.All
            };

            set
            {
                switch ( this, value )
                {
                    case ({ SetMethod: not null }, Writeability.All ):
                        this.HasInitOnlySetter = false;
                        break;
                    case ({ SetMethod: not null }, Writeability.InitOnly ):
                        this.HasInitOnlySetter = true;
                        break;
                    default:
                        throw new InvalidOperationException( $"Writeability can only be set for indexers with a setter to either {Writeability.InitOnly} or {Writeability.All}." );
                }
            }
        }

        public IObjectReader InitializerTags { get; }

        public IType Type
        {
            get => this._type;
            set
            {
                this.CheckNotFrozen();

                this._type = value;
            }
        }

        IParameterList IHasParameters.Parameters => this.Parameters;

        public IMethodBuilder? GetMethod { get; }

        IMethod? IFieldOrPropertyOrIndexer.GetMethod => this.GetMethod;

        IMethod? IFieldOrPropertyOrIndexer.SetMethod => this.SetMethod;

        public IMethodBuilder? SetMethod { get; }

        protected virtual bool HasBaseInvoker => this.OverriddenIndexer != null;

        IInvokerFactory<IIndexerInvoker> IIndexer.Invokers => this.Invokers;

        [Memo]
        public IInvokerFactory<IIndexerInvoker> Invokers
            => new InvokerFactory<IIndexerInvoker>(
                ( order, _ ) => new IndexerInvoker( this, order ),
                this.HasBaseInvoker );

        public IIndexer? OverriddenIndexer { get; set; }

        public override DeclarationKind DeclarationKind => DeclarationKind.Indexer;

        public IReadOnlyList<IIndexer> ExplicitInterfaceImplementations { get; set; } = Array.Empty<IIndexer>();

        public override bool IsExplicitInterfaceImplementation => this.ExplicitInterfaceImplementations.Count > 0;

        public override IMember? OverriddenMember => this.OverriddenIndexer;

        public override IInjectMemberTransformation ToTransformation() => new IntroduceIndexerTransformation( this.ParentAdvice, this );

        public IndexerBuilder(
            Advice advice,
            INamedType targetType,
            bool hasGetter,
            bool hasSetter )
            : base( targetType, "this[]", advice )
        {
            Invariant.Assert( hasGetter || hasSetter );

            this._type = targetType.Compilation.GetCompilationModel().Factory.GetTypeByReflectionType( typeof(object) );

            if ( hasGetter )
            {
                this.GetMethod = new AccessorBuilder( this, MethodKind.PropertyGet, false );
            }

            if ( hasSetter )
            {
                this.SetMethod = new AccessorBuilder( this, MethodKind.PropertySet, false );
            }

            this.HasInitOnlySetter = false;
        }

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

        public PropertyInfo ToPropertyInfo() => CompileTimePropertyInfo.Create( this );

        public FieldOrPropertyOrIndexerInfo ToFieldOrPropertyOrIndexerInfo() => CompileTimeFieldOrPropertyOrIndexerInfo.Create( this );

        public void SetExplicitInterfaceImplementation( Indexer interfaceIndexer ) => this.ExplicitInterfaceImplementations = new[] { interfaceIndexer };

        public override void Freeze()
        {
            base.Freeze();

            ((DeclarationBuilder?) this.GetMethod)?.Freeze();
            ((DeclarationBuilder?) this.SetMethod)?.Freeze();
        }

        public IParameterBuilder AddParameter( string name, IType type, RefKind refKind = RefKind.None, TypedConstant? defaultValue = default )
        {
            this.CheckNotFrozen();

            var parameter = new ParameterBuilder( this, this.Parameters.Count, name, type, refKind, this.ParentAdvice );
            parameter.DefaultValue = defaultValue;
            this.Parameters.Add( parameter );

            return parameter;
        }

        public IParameterBuilder AddParameter( string name, Type type, RefKind refKind = RefKind.None, TypedConstant? defaultValue = default )
        {
            this.CheckNotFrozen();

            var iType = this.Compilation.Factory.GetTypeByReflectionType( type );
            var typeConstant = defaultValue != null ? TypedConstant.Create( defaultValue, iType ) : default;

            return this.AddParameter( name, iType, refKind, typeConstant );
        }
    }
}