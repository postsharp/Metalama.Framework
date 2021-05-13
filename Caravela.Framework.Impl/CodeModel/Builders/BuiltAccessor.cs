// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Links;
using Caravela.Framework.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class BuiltAccessor : BuiltCodeElement, IMethod, IMemberLink<IMethod>
    {
        private readonly BuiltMember _builtMember;

        public BuiltAccessor( BuiltMember builtMember, AccessorBuilder builder ) : base( builtMember.Compilation )
        {
            this._builtMember = builtMember;
            this.AccessorBuilder = builder;
        }

        public AccessorBuilder AccessorBuilder { get; }

        public override CodeElementBuilder Builder => this.AccessorBuilder;

        public Accessibility Accessibility => this.AccessorBuilder.Accessibility;

        public string Name => this.AccessorBuilder.Name;

        public bool IsAbstract => this.AccessorBuilder.IsAbstract;

        public bool IsStatic => this.AccessorBuilder.IsStatic;

        public bool IsVirtual => this.AccessorBuilder.IsVirtual;

        public bool IsSealed => this.AccessorBuilder.IsSealed;

        public bool IsReadOnly => this.AccessorBuilder.IsReadOnly;

        public bool IsOverride => this.AccessorBuilder.IsOverride;

        public bool IsNew => this.AccessorBuilder.IsNew;

        public bool IsAsync => this.AccessorBuilder.IsAsync;

        public IMethodList LocalFunctions => MethodList.Empty;

        [Memo]
        public IParameterList Parameters
            => new ParameterList(
                this,
                this.AccessorBuilder.Parameters.AsBuilderList.Select( CodeElementLink.FromBuilder<IParameter, IParameterBuilder> ) );

        public MethodKind MethodKind => this.AccessorBuilder.MethodKind;

        public dynamic Invoke( dynamic? instance, params dynamic[] args ) => throw new NotImplementedException();

        [Memo]
        public IParameter ReturnParameter => new BuiltParameter( this.AccessorBuilder.ReturnParameter, this.Compilation );

        [Memo]
        public IType ReturnType => this.Compilation.Factory.GetIType( this.AccessorBuilder.ReturnParameter.ParameterType );

        [Memo]
        public IGenericParameterList GenericParameters => GenericParameterList.Empty;

        public IReadOnlyList<IType> GenericArguments => throw new NotImplementedException();

        public bool IsOpenGeneric => true;

        public IMethod WithGenericArguments( params IType[] genericArguments ) => throw new NotImplementedException();

        public bool HasBase => throw new NotImplementedException();

        public IMethodInvocation Base => throw new NotImplementedException();

        public IMethod? OverriddenMethod => throw new NotImplementedException();

        public INamedType DeclaringType => this._builtMember.DeclaringType;

        public object? Target => throw new NotImplementedException();

        IMethod ICodeElementLink<IMethod>.GetForCompilation( CompilationModel compilation ) => (IMethod) this.GetForCompilation( compilation );

        [return: RunTimeOnly]
        public MethodInfo ToMethodInfo()
        {
            throw new NotImplementedException();
        }

        [return: RunTimeOnly]
        public System.Reflection.MethodBase ToMethodBase()
        {
            throw new NotImplementedException();
        }

        [return: RunTimeOnly]
        public MemberInfo ToMemberInfo()
        {
            throw new NotImplementedException();
        }
    }
}