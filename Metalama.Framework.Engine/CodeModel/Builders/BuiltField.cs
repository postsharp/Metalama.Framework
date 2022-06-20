﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.RunTime;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class BuiltField : BuiltMember, IFieldImpl, IMemberRef<IField>
    {
        public BuiltField( FieldBuilder builder, CompilationModel compilation ) : base( compilation, builder )
        {
            this.FieldBuilder = builder;
        }

        public FieldBuilder FieldBuilder { get; }

        public override MemberBuilder MemberBuilder => this.FieldBuilder;

        public override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this.FieldBuilder;

        public Writeability Writeability => this.FieldBuilder.Writeability;

        public bool IsAutoPropertyOrField => this.FieldBuilder.IsAutoPropertyOrField;

        public IType Type => this.FieldBuilder.Type;

        [Memo]
        public IMethod? GetMethod => this.FieldBuilder.GetMethod != null ? new BuiltAccessor( this, (AccessorBuilder) this.FieldBuilder.GetMethod ) : null;

        [Memo]
        public IMethod? SetMethod => this.FieldBuilder.SetMethod != null ? new BuiltAccessor( this, (AccessorBuilder) this.FieldBuilder.SetMethod ) : null;

        IInvokerFactory<IFieldOrPropertyInvoker> IFieldOrProperty.Invokers => throw new NotImplementedException();

        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => this.FieldBuilder.ToFieldOrPropertyInfo();

        public FieldInfo ToFieldInfo() => this.FieldBuilder.ToFieldInfo();

        DeclarationSerializableId IRef<IField>.ToSerializableId() => throw new NotImplementedException();

        IField IRef<IField>.GetTarget( ICompilation compilation, ReferenceResolutionOptions options )
            => (IField) this.GetForCompilation( compilation, options );

        ISymbol? ISdkRef<IField>.GetSymbol( Compilation compilation, bool ignoreAssemblyKey ) => throw new NotSupportedException();

        public IMethod? GetAccessor( MethodKind methodKind ) => this.GetAccessorImpl( methodKind );

        public IEnumerable<IMethod> Accessors => this.FieldBuilder.Accessors;
    }
}