// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Builders;
using Caravela.Framework.Impl.CodeModel.InternalInterfaces;
using System;
using System.Reflection;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class BuiltParameter : BuiltDeclaration, IParameterInternal
    {
        public BuiltParameter( IParameterBuilder builder, CompilationModel compilation ) : base( compilation )
        {
            this.ParameterBuilder = builder;
        }

        public IParameterBuilder ParameterBuilder { get; }

        public override DeclarationBuilder Builder => (DeclarationBuilder) this.ParameterBuilder;

        public RefKind RefKind => this.ParameterBuilder.RefKind;

        [Memo]
        public IType ParameterType => this.Compilation.Factory.GetIType( this.ParameterBuilder.ParameterType );

        public string Name => this.ParameterBuilder.Name;

        public int Index => this.ParameterBuilder.Index;

        public TypedConstant DefaultValue => this.ParameterBuilder.DefaultValue;

        public bool IsParams => this.ParameterBuilder.IsParams;

        [Memo]
        public IMemberOrNamedType DeclaringMember => this.Compilation.Factory.GetDeclaration( this.ParameterBuilder.DeclaringMember );

        public ParameterInfo ToParameterInfo() => throw new NotImplementedException();
    }
}