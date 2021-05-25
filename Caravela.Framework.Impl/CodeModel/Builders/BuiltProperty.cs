// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class BuiltProperty : BuiltMember, IProperty, IMemberRef<IProperty>
    {
        public BuiltProperty( PropertyBuilder builder, CompilationModel compilation ) : base( compilation )
        {
            this.PropertyBuilder = builder;
        }

        public PropertyBuilder PropertyBuilder { get; }

        public override DeclarationBuilder Builder => this.PropertyBuilder;

        public override MemberBuilder MemberBuilder => this.PropertyBuilder;

        [Memo]
        public IParameterList Parameters
            => new ParameterList(
                this,
                this.PropertyBuilder.Parameters.AsBuilderList.Select( DeclarationRef.FromBuilder<IParameter, IParameterBuilder> ) );

        public RefKind RefKind => this.PropertyBuilder.RefKind;

        public bool IsByRef => this.PropertyBuilder.IsByRef;

        public bool IsRef => this.PropertyBuilder.IsRef;

        public bool IsRefReadonly => this.PropertyBuilder.IsRefReadonly;

        public IPropertyInvocation Base => throw new NotImplementedException();

        public IType Type => this.PropertyBuilder.Type;

        [Memo]
        public IMethod? Getter => this.PropertyBuilder.Getter != null ? new BuiltAccessor( this, (AccessorBuilder) this.PropertyBuilder.Getter ) : null;

        [Memo]
        public IMethod? Setter => this.PropertyBuilder.Setter != null ? new BuiltAccessor( this, (AccessorBuilder) this.PropertyBuilder.Setter ) : null;

        public bool HasBase => throw new NotImplementedException();

        public dynamic Value { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        IFieldOrPropertyInvocation IFieldOrProperty.Base => throw new NotImplementedException();

        // TODO: When an interface is introduced, explicit implementation should appear here.
        public IReadOnlyList<IProperty> ExplicitInterfaceImplementations => Array.Empty<IProperty>();

        public dynamic GetIndexerValue( dynamic? instance, params dynamic[] args )
        {
            throw new NotImplementedException();
        }

        public dynamic GetValue( dynamic? instance )
        {
            throw new NotImplementedException();
        }

        public dynamic SetIndexerValue( dynamic? instance, dynamic value, params dynamic[] args )
        {
            throw new NotImplementedException();
        }

        public dynamic SetValue( dynamic? instance, dynamic value )
        {
            throw new NotImplementedException();
        }

        [return: RunTimeOnly]
        public FieldOrPropertyInfo ToFieldOrPropertyInfo()
        {
            throw new NotImplementedException();
        }

        [return: RunTimeOnly]
        public PropertyInfo ToPropertyInfo()
        {
            throw new NotImplementedException();
        }

        IProperty IDeclarationRef<IProperty>.GetForCompilation( CompilationModel compilation ) => (IProperty) this.GetForCompilation( compilation );
    }
}