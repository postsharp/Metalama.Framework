// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects.AdvisedCode;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Reflection;
using MethodBase = System.Reflection.MethodBase;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class AdviceMethod : AdviceMember<IMethod>, IAdviceMethod
    {
        public AdviceMethod( IMethod underlying ) : base( underlying ) { }

        public IMethodList LocalFunctions => this.Underlying.LocalFunctions;

        public dynamic Invoke( params dynamic[] args )
        {
            if ( this.Invoker.Base != null )
            {
                return this.Invoker.Base.Invoke( this.IsStatic ? null : this.This, args );
            }
            else if ( this.ReturnType.Is( SpecialType.Void ) )
            {
                return new EmptyStatementDynamicExpression();
            }
            else
            {
                return new DynamicExpression(
                    (ExpressionSyntax) LanguageServiceFactory.CSharpSyntaxGenerator.DefaultExpression( this.ReturnType.GetSymbol() ),
                    this.ReturnType,
                    false );
            }
        }

        [Memo]
        public IAdviceParameterList Parameters => new AdviceParameterList( this.Underlying );

        IParameterList IHasParameters.Parameters => this.Underlying.Parameters;

        public MethodKind MethodKind => this.Underlying.MethodKind;

        public bool IsReadOnly => this.Underlying.IsReadOnly;

        public MethodBase ToMethodBase() => this.Underlying.ToMethodBase();

        public IParameter ReturnParameter => this.Underlying.ReturnParameter;

        public IType ReturnType => this.Underlying.ReturnType;

        public IGenericParameterList GenericParameters => this.Underlying.GenericParameters;

        public IReadOnlyList<IType> GenericArguments => this.Underlying.GenericArguments;

        public bool IsOpenGeneric => this.Underlying.IsOpenGeneric;

        public IMethod WithGenericArguments( params IType[] genericArguments ) => this.Underlying.WithGenericArguments( genericArguments );

        public IInvokerFactory<IMethodInvoker> Invoker => this.Underlying.Invoker;

        public IMethod? OverriddenMethod => this.Underlying.OverriddenMethod;

        public IReadOnlyList<IMethod> ExplicitInterfaceImplementations => this.Underlying.ExplicitInterfaceImplementations;

        public MethodInfo ToMethodInfo() => this.Underlying.ToMethodInfo();
    }
}