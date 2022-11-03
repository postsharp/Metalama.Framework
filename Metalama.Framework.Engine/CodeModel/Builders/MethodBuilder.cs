// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal sealed class MethodBuilder : MemberBuilder, IMethodBuilder, IMethodImpl
    {
        private bool _isReadOnly;

        public ParameterBuilderList Parameters { get; } = new();

        public GenericParameterBuilderList TypeParameters { get; } = new();

        public bool IsReadOnly
        {
            get => this._isReadOnly;
            set
            {
                this.CheckNotFrozen();

                this._isReadOnly = value;
            }
        }

        // A builder is never accessed directly from user code and never represents a generic type instance,
        // so we don't need an implementation of GenericArguments.
        public IReadOnlyList<IType> TypeArguments => throw new NotSupportedException();

        [Memo]
        public IInvokerFactory<IMethodInvoker> Invokers
            => new InvokerFactory<IMethodInvoker>(
                ( order, invokerOperator ) => new MethodInvoker( this, order, invokerOperator ),
                this.OverriddenMethod != null );

        public IMethod? OverriddenMethod { get; set; }

        public MethodInfo ToMethodInfo() => CompileTimeMethodInfo.Create( this );

        IMemberWithAccessors? IMethod.DeclaringMember => null;

        public override void Freeze()
        {
            base.Freeze();

            foreach ( var parameter in this.Parameters )
            {
                parameter.Freeze();
            }

            foreach ( var typeParameter in this.TypeParameters )
            {
                typeParameter.Freeze();
            }

            this.ReturnParameter.Freeze();
        }

        public IParameterBuilder AddParameter( string name, IType type, RefKind refKind = RefKind.None, TypedConstant? defaultValue = null )
        {
            this.CheckNotFrozen();

            var parameter = new ParameterBuilder( this, this.Parameters.Count, name, type, refKind, this.ParentAdvice );
            parameter.DefaultValue = defaultValue;
            this.Parameters.Add( parameter );

            return parameter;
        }

        public IParameterBuilder AddParameter( string name, Type type, RefKind refKind = RefKind.None, TypedConstant? defaultValue = null )
        {
            this.CheckNotFrozen();

            var iType = this.Compilation.Factory.GetTypeByReflectionType( type );
            var typeConstant = defaultValue != null ? TypedConstant.Create( defaultValue, iType ) : default;

            return this.AddParameter( name, iType, refKind, typeConstant );
        }

        public ITypeParameterBuilder AddTypeParameter( string name )
        {
            this.CheckNotFrozen();

            var builder = new TypeParameterBuilder( this, this.TypeParameters.Count, name );
            this.TypeParameters.Add( builder );

            return builder;
        }

        IParameterBuilder IMethodBuilder.ReturnParameter => this.ReturnParameter;

        public IType ReturnType
        {
            get => this.ReturnParameter.Type;
            set
            {
                this.CheckNotFrozen();

                this.ReturnParameter.Type = value ?? throw new ArgumentNullException( nameof(value) );
            }
        }

        IType IMethod.ReturnType => this.ReturnParameter.Type;

        public ParameterBuilder ReturnParameter { get; }

        IParameter IMethod.ReturnParameter => this.ReturnParameter;

        IParameterList IHasParameters.Parameters => this.Parameters;

        IGenericParameterList IGeneric.TypeParameters => this.TypeParameters;

        public bool IsGeneric => this.TypeParameters.Count > 0;

        public bool IsCanonicalGenericInstance => true;

        // We don't currently support adding other methods than default ones.
        public MethodKind MethodKind
            => this.DeclarationKind switch
            {
                DeclarationKind.Method => MethodKind.Default,
                DeclarationKind.Operator => MethodKind.Operator,
                DeclarationKind.Finalizer => MethodKind.Finalizer,
                _ => throw new AssertionFailedException( $"Unexpected DeclarationKind: {this.DeclarationKind}." )
            };

        System.Reflection.MethodBase IMethodBase.ToMethodBase() => this.ToMethodInfo();

        IGeneric IGenericInternal.ConstructGenericInstance( IReadOnlyList<IType> typeArguments ) => throw new NotImplementedException();

        public override DeclarationKind DeclarationKind { get; }

        public OperatorKind OperatorKind { get; }

        IMethod IMethod.MethodDefinition => this;

        bool IMethod.IsExtern => false;

        public IReadOnlyList<IMethod> ExplicitInterfaceImplementations { get; private set; } = Array.Empty<IMethod>();

        public MethodBuilder(
            Advice advice,
            INamedType targetType,
            string name,
            DeclarationKind declarationKind = DeclarationKind.Method,
            OperatorKind operatorKind = OperatorKind.None )
            : base( targetType, name, advice )
        {
            Invariant.Assert(
                declarationKind == DeclarationKind.Operator
                                ==
                                (operatorKind != OperatorKind.None) );

            this.Name = name;
            this.DeclarationKind = declarationKind;
            this.OperatorKind = operatorKind;

            this.ReturnParameter =
                new ParameterBuilder(
                    this,
                    -1,
                    null,
                    this.Compilation.Factory.GetTypeByReflectionType( typeof(void) ).AssertNotNull(),
                    RefKind.None,
                    this.ParentAdvice );
        }

        public void SetExplicitInterfaceImplementation( IMethod interfaceMethod ) => this.ExplicitInterfaceImplementations = new[] { interfaceMethod };

        public override bool IsExplicitInterfaceImplementation => this.ExplicitInterfaceImplementations.Count > 0;

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Append( this.DeclaringType.ToDisplayString( format, context ) );
            stringBuilder.Append( "." );
            stringBuilder.Append( this.Name );
            stringBuilder.Append( "(" );

            foreach ( var parameter in this.Parameters )
            {
                if ( parameter.Index > 0 )
                {
                    stringBuilder.Append( ", " );
                }

                stringBuilder.Append( parameter.Type.ToDisplayString( format, context ) );
            }

            stringBuilder.Append( ")" );

            return stringBuilder.ToString();
        }

        public override IMember? OverriddenMember => (IMemberImpl?) this.OverriddenMethod;

        public override IInjectMemberTransformation ToTransformation() => new IntroduceMethodTransformation( this.ParentAdvice, this );
    }
}