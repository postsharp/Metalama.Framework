// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.RunTime;
using System.Collections.Generic;
using System.Reflection;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal sealed class BuiltField : BuiltMember, IFieldImpl
    {
        public BuiltField( FieldBuilder builder, CompilationModel compilation ) : base( compilation, builder )
        {
            this.FieldBuilder = builder;
        }

        public FieldBuilder FieldBuilder { get; }

        public override MemberBuilder MemberBuilder => this.FieldBuilder;

        public override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this.FieldBuilder;

        public Writeability Writeability => this.FieldBuilder.Writeability;

        public bool? IsAutoPropertyOrField => this.FieldBuilder.IsAutoPropertyOrField;

        public IType Type => this.FieldBuilder.Type;

        public RefKind RefKind => this.FieldBuilder.RefKind;

        [Memo]
        public IMethod GetMethod => new BuiltAccessor( this, (AccessorBuilder) this.FieldBuilder.GetMethod );

        [Memo]
        public IMethod SetMethod => new BuiltAccessor( this, (AccessorBuilder) this.FieldBuilder.SetMethod );

        [Memo]
        public IInvokerFactory<IFieldOrPropertyInvoker> Invokers
            => new InvokerFactory<IFieldOrPropertyInvoker>( ( order, invokerOperator ) => new FieldOrPropertyInvoker( this, order, invokerOperator ) );

        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => this.FieldBuilder.ToFieldOrPropertyInfo();

        public bool IsRequired => this.FieldBuilder.IsRequired;

        public IExpression? InitializerExpression => this.FieldBuilder.InitializerExpression;

        public FieldInfo ToFieldInfo() => this.FieldBuilder.ToFieldInfo();

        public TypedConstant? ConstantValue => this.FieldBuilder.ConstantValue;

        public IMethod? GetAccessor( MethodKind methodKind ) => this.GetAccessorImpl( methodKind );

        public IEnumerable<IMethod> Accessors => this.FieldBuilder.Accessors;
    }
}