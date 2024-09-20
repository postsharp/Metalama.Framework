// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Reflection;
using MethodKind = Metalama.Framework.Code.MethodKind;
using SymbolMethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Metalama.Framework.Engine.CodeModel
{
    internal abstract class MethodBase : Member, IMethodBase
    {
        public override ISymbol Symbol => this.MethodSymbol;

        internal IMethodSymbol MethodSymbol { get; }

        [Memo]
        public override IDeclaration? ContainingDeclaration
            => this.Symbol switch
            {
                IMethodSymbol
                {
                    MethodKind: SymbolMethodKind.PropertyGet or SymbolMethodKind.PropertySet or SymbolMethodKind.EventAdd or SymbolMethodKind.EventRemove
                    or SymbolMethodKind.EventRaise
                } method => this.Compilation.Factory.GetDeclaration( method.AssociatedSymbol.AssertSymbolNotNull() ),
                _ => base.ContainingDeclaration
            };

        protected MethodBase( IMethodSymbol symbol, CompilationModel compilation ) : base( compilation )
        {
            this.MethodSymbol = symbol;
        }

        [Memo]
        public IParameterList Parameters
            => new ParameterList(
                this,
                this.Compilation.GetParameterCollection( this.ToRef().GetDefinition() ) );

        public MethodKind MethodKind
            => this.MethodSymbol.MethodKind switch
            {
                SymbolMethodKind.Ordinary => MethodKind.Default,
                SymbolMethodKind.Destructor => MethodKind.Finalizer,
                SymbolMethodKind.PropertyGet => MethodKind.PropertyGet,
                SymbolMethodKind.PropertySet => MethodKind.PropertySet,
                SymbolMethodKind.EventAdd => MethodKind.EventAdd,
                SymbolMethodKind.EventRemove => MethodKind.EventRemove,
                SymbolMethodKind.EventRaise => MethodKind.EventRaise,
                SymbolMethodKind.ExplicitInterfaceImplementation => MethodKind.ExplicitInterfaceImplementation,
                SymbolMethodKind.Conversion => MethodKind.Operator,
                SymbolMethodKind.UserDefinedOperator => MethodKind.Operator,
                SymbolMethodKind.LocalFunction => MethodKind.LocalFunction,
                SymbolMethodKind.AnonymousFunction => MethodKind.Lambda,
                SymbolMethodKind.DelegateInvoke => MethodKind.DelegateInvoke,
                SymbolMethodKind.BuiltinOperator or
                    SymbolMethodKind.ReducedExtension or
                    SymbolMethodKind.DeclareMethod or
                    SymbolMethodKind.FunctionPointerSignature => throw new NotSupportedException(
                        $"The method '{this.Symbol}' is not supported because it is of kind {this.MethodSymbol.MethodKind}." ),
                _ => throw new AssertionFailedException( $"The method '{this.Symbol}' is not expected because it is of kind {this.MethodSymbol.MethodKind}." )
            };

        public abstract System.Reflection.MethodBase ToMethodBase();

        public IRef<IMethodBase> ToRef() => this.GetMethodBaseRef();

        protected abstract IRef<IMethodBase> GetMethodBaseRef();

        public override MemberInfo ToMemberInfo() => this.ToMethodBase();
    }
}