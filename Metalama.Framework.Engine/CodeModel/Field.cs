﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.CodeModel.Pseudo;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Reflection;
using Accessibility = Metalama.Framework.Code.Accessibility;
using MethodKind = Metalama.Framework.Code.MethodKind;
using RefKind = Metalama.Framework.Code.RefKind;
using TypedConstant = Metalama.Framework.Code.TypedConstant;

namespace Metalama.Framework.Engine.CodeModel
{
    internal sealed class Field : Member, IFieldImpl
    {
        private readonly IFieldSymbol _symbol;

        public override DeclarationKind DeclarationKind => DeclarationKind.Field;

        public override ISymbol Symbol => this._symbol;

        public Field( IFieldSymbol symbol, CompilationModel compilation ) : base( compilation )
        {
            this._symbol = symbol;
        }

        [Obsolete]
        IInvokerFactory<IFieldOrPropertyInvoker> IFieldOrProperty.Invokers => throw new NotSupportedException();

        [Memo]
        public IType Type => this.Compilation.Factory.GetIType( this._symbol.Type );

        public RefKind RefKind
#if ROSLYN_4_4_0_OR_GREATER
            => this._symbol.RefKind.ToOurRefKind();
#else
            => RefKind.None;
#endif

        [Memo]
        public IMethod GetMethod => new PseudoGetter( this );

        [Memo]
        public IMethod? SetMethod
            => this.Writeability switch
            {
                Writeability.None => null,
                Writeability.ConstructorOnly => new PseudoSetter( this, Accessibility.Private ),
                Writeability.All => new PseudoSetter( this, null ),
                _ => throw new AssertionFailedException( $"Unexpected Writeability: {this.Writeability}." )
            };

        public Writeability Writeability
            => this._symbol switch
            {
                { IsConst: true } => Writeability.None,
                { IsReadOnly: true } => Writeability.ConstructorOnly,
                _ => Writeability.All
            };

        public bool? IsAutoPropertyOrField => true;

        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => CompileTimeFieldOrPropertyInfo.Create( this );

        public bool IsRequired
#if ROSLYN_4_4_0_OR_GREATER
            => this._symbol.IsRequired;
#else
            => false;
#endif

        [Memo]
        public IExpression? InitializerExpression => this.GetInitializerExpressionCore();

        public IFieldOrPropertyInvoker With( InvokerOptions options ) => new FieldOrPropertyInvoker( this, options );

        public IFieldOrPropertyInvoker With( object? target, InvokerOptions options = default ) => new FieldOrPropertyInvoker( this, options, target );

        public ref object? Value => ref new FieldOrPropertyInvoker( this ).Value;

        private IExpression? GetInitializerExpressionCore()
        {
            var expression = this._symbol.GetPrimaryDeclaration() switch
            {
                VariableDeclaratorSyntax variable => variable.Initializer?.Value,
                EnumMemberDeclarationSyntax enumMember => enumMember.EqualsValue?.Value,
                _ => null
            };

            if ( expression == null )
            {
                return null;
            }
            else
            {
                return new SourceUserExpression( expression, this.Type );
            }
        }

        public FieldInfo ToFieldInfo() => CompileTimeFieldInfo.Create( this );

        [Memo]
        public TypedConstant? ConstantValue => TypedConstant.Create( this._symbol.ConstantValue, this.Type );

        public override bool IsExplicitInterfaceImplementation => false;

        public override bool IsAsync => false;

        public IMember? OverriddenMember => null;

        public override MemberInfo ToMemberInfo() => this.ToFieldOrPropertyInfo();

        public IMethod? GetAccessor( MethodKind methodKind )
            => methodKind switch
            {
                MethodKind.PropertyGet => this.GetMethod,
                MethodKind.PropertySet => this.SetMethod,
                _ => throw new ArgumentOutOfRangeException()
            };

        public IEnumerable<IMethod> Accessors
        {
            get
            {
                yield return this.GetMethod;

                if ( this.SetMethod != null )
                {
                    yield return this.SetMethod;
                }
            }
        }

        bool IExpression.IsAssignable => this.Writeability != Writeability.None;
    }
}