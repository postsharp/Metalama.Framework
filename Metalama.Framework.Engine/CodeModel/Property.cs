// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Invokers;
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

        public bool IsRequired
#if ROSLYN_4_4_0_OR_GREATER
            => this.PropertySymbol.IsRequired;
#else
            => false;
#endif
        public override MemberInfo ToMemberInfo() => this.ToFieldOrPropertyInfo();

        IInvokerFactory<IFieldOrPropertyInvoker> IFieldOrProperty.Invokers => this.Invokers;

        [Memo]
        public IInvokerFactory<IFieldOrPropertyInvoker> Invokers
            => new InvokerFactory<IFieldOrPropertyInvoker>( ( order, invokerOperator ) => new FieldOrPropertyInvoker( this, order, invokerOperator ) );

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

        public IMember? OverriddenMember => this.OverriddenProperty;

        [Memo]
        public IReadOnlyList<IProperty> ExplicitInterfaceImplementations
            => this.PropertySymbol.ExplicitInterfaceImplementations.Select( p => this.Compilation.Factory.GetProperty( p ) ).ToList();

        public override DeclarationKind DeclarationKind => DeclarationKind.Property;

        [Memo]
        public IExpression? InitializerExpression => this.GetInitializerExpressionCore();

        private IExpression? GetInitializerExpressionCore()
        {
            var initializer = ((VariableDeclaratorSyntax?) this.PropertySymbol.GetPrimaryDeclaration())?.Initializer;

            if ( initializer == null )
            {
                return null;
            }
            else
            {
                return new SourceUserExpression( initializer.Value, this.Type );
            }
        }
    }
}