﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel
{
    internal sealed class Property : PropertyOrIndexer, IPropertyImpl
    {
        public Property( IPropertySymbol symbol, CompilationModel compilation ) : base( symbol, compilation ) { }

        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => CompileTimeFieldOrPropertyInfo.Create( this );

        public override MemberInfo ToMemberInfo() => this.ToFieldOrPropertyInfo();

        public bool IsRequired
#if ROSLYN_4_4_0_OR_GREATER
            => this.PropertySymbol.IsRequired;
#else
            => false;
#endif
        [Memo]
        public bool? IsAutoPropertyOrField => this.PropertySymbol.IsAutoProperty();

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

        [Memo]
        public IProperty Definition
            => this.PropertySymbol == this.PropertySymbol.OriginalDefinition
                ? this
                : this.Compilation.Factory.GetProperty( this.PropertySymbol.OriginalDefinition );

        IRef<IFieldOrProperty> IFieldOrProperty.ToRef() => this.BoxedRef;

        protected override IMemberOrNamedType GetDefinition() => this.Definition;

        public IMember? OverriddenMember => this.OverriddenProperty;

        [Memo]
        public IReadOnlyList<IProperty> ExplicitInterfaceImplementations
            => this.PropertySymbol.ExplicitInterfaceImplementations.Select( p => this.Compilation.Factory.GetProperty( p ) ).ToReadOnlyList();

        public override DeclarationKind DeclarationKind => DeclarationKind.Property;

        [Memo]
        public IExpression? InitializerExpression => this.GetInitializerExpressionCore();

        public IFieldOrPropertyInvoker With( InvokerOptions options ) => new FieldOrPropertyInvoker( this, options );

        public IFieldOrPropertyInvoker With( object? target, InvokerOptions options = default ) => new FieldOrPropertyInvoker( this, options, target );

        public ref object? Value => ref new FieldOrPropertyInvoker( this ).Value;

        public TypedExpressionSyntax ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
            => new FieldOrPropertyInvoker( this )
                .ToTypedExpressionSyntax( syntaxGenerationContext );

        private IExpression? GetInitializerExpressionCore()
        {
            // The declaration for properties created from record primary constructor parameters is ParameterSyntax.
            // Since those don't have a normal initializer, ignore them here.
            var initializer = (this.PropertySymbol.GetPrimaryDeclaration() as PropertyDeclarationSyntax)?.Initializer;

            if ( initializer == null )
            {
                return null;
            }
            else
            {
                return new SourceUserExpression( initializer.Value, this.Type );
            }
        }

        bool IExpression.IsAssignable => this.Writeability != Writeability.None;

        [Memo]
        private BoxedRef<IProperty> BoxedRef => new BoxedRef<IProperty>( this.ToValueTypedRef() );

        private protected override IRef<IDeclaration> ToDeclarationRef() => this.BoxedRef;

        IRef<IProperty> IProperty.ToRef() => this.BoxedRef;

        protected override IRef<IPropertyOrIndexer> ToPropertyOrIndexerRef() => this.BoxedRef;

        protected override IRef<IFieldOrPropertyOrIndexer> ToFieldOrPropertyOrIndexerRef() => this.BoxedRef;

        protected override IRef<IMember> ToMemberRef() => this.BoxedRef;

        protected override IRef<IMemberOrNamedType> ToMemberOrNamedTypeRef() => this.BoxedRef;
    }
}