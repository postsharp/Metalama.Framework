using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Links;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class BuiltMethod : BuiltMember, IMethod, IMemberLink<IMethod>
    {
        public BuiltMethod(MethodBuilder builder,  CompilationModel compilation) : base(compilation)
        {
            this.MethodBuilder = builder;
        }
        
        public MethodBuilder MethodBuilder { get; }

        public override CodeElementBuilder Builder => this.MethodBuilder;

        public override MemberBuilder MemberBuilder => this.MethodBuilder;

        public IMethodList LocalFunctions => MethodList.Empty;

        [Memo]
        public IParameterList Parameters =>
            new ParameterList(
                this.MethodBuilder.Parameters.AsBuilderList.Select( 
                    CodeElementLink.FromLink<IParameter> ),
                this.Compilation );

        public MethodKind MethodKind => this.MethodBuilder.MethodKind;

        public dynamic Invoke( dynamic? instance, params dynamic[] args ) => throw new NotImplementedException();

        [Memo]
        public IParameter ReturnParameter => new BuiltParameter( this.MethodBuilder.ReturnParameter, this.Compilation );

        [Memo] 
        public IType ReturnType => this.Compilation.Factory.GetIType( this.MethodBuilder.ReturnParameter.ParameterType );

        [Memo]
        public IGenericParameterList GenericParameters =>
            new GenericParameterList( 
                this.MethodBuilder.GenericParameters.AsBuilderList.Select( 
                    CodeElementLink.FromLink<IGenericParameter> ),
                this.Compilation );

        public IReadOnlyList<IType> GenericArguments => throw new NotImplementedException();

        public bool IsOpenGeneric => true;

        public IMethod WithGenericArguments( params IType[] genericArguments ) => throw new NotImplementedException();

        public bool HasBase => throw new NotImplementedException();

        public IMethodInvocation Base => throw new NotImplementedException();

        IMethod ICodeElementLink<IMethod>.GetForCompilation( CompilationModel compilation ) => (IMethod) this.GetForCompilation( compilation );
    }
}