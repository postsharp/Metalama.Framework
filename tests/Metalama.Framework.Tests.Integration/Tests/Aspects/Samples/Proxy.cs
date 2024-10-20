using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Samples;

[assembly: GenerateProxyAspect( typeof(ISomeInterface), "Metalama.Samples.Proxy.Tests", "SomeProxy" )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Samples;

[AttributeUsage( AttributeTargets.Assembly, AllowMultiple = true )]
public class GenerateProxyAspect : CompilationAspect
{
    private readonly Type _interfaceType;
    private readonly string _ns;
    private readonly string _typeName;

    public GenerateProxyAspect( Type interfaceType, string ns, string typeName )
    {
        _interfaceType = interfaceType;
        _ns = ns;
        _typeName = typeName;
    }

    public override void BuildAspect( IAspectBuilder<ICompilation> builder )
    {
        base.BuildAspect( builder );

        // Add a type.
        var type = builder.WithNamespace( _ns )
            .IntroduceClass(
                _typeName,
                buildType: t => t.Accessibility = Accessibility.Public );

        // Add a field with the intercepted object.
        var interceptedField = type.IntroduceField(
                "_intercepted",
                _interfaceType,
                IntroductionScope.Instance )
            .Declaration;

        // Add a field for the interceptor.
        var interceptorField = type.IntroduceField( "_interceptor", typeof(IInterceptor) )
            .Declaration;

        // Implement the interface.
        type.ImplementInterface( _interfaceType );

        // Implement interface members.
        var namedType = (INamedType)TypeFactory.GetType( _interfaceType );

        foreach (var method in namedType.Methods)
        {
            var argsType = TupleHelper.CreateTupleType( method );

            type.IntroduceMethod(
                method.ReturnType.SpecialType == SpecialType.Void
                    ? nameof(VoidMethodTemplate)
                    : nameof(NonVoidMethodTemplate),
                IntroductionScope.Instance,
                buildMethod: methodBuilder =>
                {
                    methodBuilder.Accessibility = Accessibility.Public;
                    methodBuilder.Name = method.Name;
                    methodBuilder.ReturnType = method.ReturnType;

                    foreach (var parameter in method.Parameters)
                    {
                        methodBuilder.AddParameter(
                            parameter.Name,
                            parameter.Type,
                            parameter.RefKind );
                    }
                },
                args:
                new
                {
                    TArgs = argsType,
                    TResult = method.ReturnType,
                    method,
                    interceptedField,
                    interceptorField
                } );
        }

        // Add the constructor.
        type.IntroduceConstructor(
            nameof(Constructor),
            buildConstructor: constructorBuilder
                => constructorBuilder.AddParameter( "intercepted", _interfaceType ),
            args: new { interceptedField, interceptorField } );
    }

    [Template]
    private void VoidMethodTemplate<[CompileTime] TArgs>( IMethod method, IField interceptedField, IField interceptorField )
        where TArgs : struct, ITuple
    {
        // Prepare the context.
        var args = (TArgs)TupleHelper.CreateTupleExpression( method ).Value!;
        var argsExpression = ExpressionFactory.Capture( args );

        // Get writable parameters.
        var writableParameters = method.Parameters.Where(
                p =>
                    p.RefKind is RefKind.Out or RefKind.Ref )
            .ToList();

        // Invoke the interceptor.
        if (writableParameters.Count == 0)
        {
            // We don't need a try...finally if we don't have to write back writable parameters.
            ( (IInterceptor)interceptorField.Value! ).Invoke( ref args, Invoke );
        }
        else
        {
            try
            {
                ( (IInterceptor)interceptorField.Value! ).Invoke( ref args, Invoke );
            }
            finally
            {
                // Copy back parameters.
                foreach (var parameter in writableParameters)
                {
                    parameter.Value =
                        TupleHelper.GetTupleItemExpression( argsExpression, parameter.Index );
                }
            }
        }

        ValueTuple Invoke( ref TArgs receivedArgs )
        {
            var receivedArgsExpression = ExpressionFactory.Parse( "receivedArgs" );

            var arguments = method.Parameters.Select(
                p =>
                    TupleHelper.GetTupleItemExpression( receivedArgsExpression, p.Index ) );

            method.With( interceptedField ).Invoke( arguments );

            return default;
        }
    }

    [Template]
    private TResult NonVoidMethodTemplate<[CompileTime] TArgs, [CompileTime] TResult>( IMethod method, IField interceptedField, IField interceptorField )
        where TArgs : struct, ITuple
    {
        // Prepare the context.
        var args = (TArgs)TupleHelper.CreateTupleExpression( method ).Value!;
        var argsExpression = ExpressionFactory.Capture( args );

        // Get writable parameters.
        var writableParameters = method.Parameters.Where(
                p =>
                    p.RefKind is RefKind.Out or RefKind.Ref )
            .ToList();

        // Invoke the interceptor.
        if (writableParameters.Count == 0)
        {
            // We don't need a try...finally if we don't have to write back writable parameters.
            return ( (IInterceptor)interceptorField.Value! ).Invoke( ref args, Invoke );
        }
        else
        {
            try
            {
                return ( (IInterceptor)interceptorField.Value! ).Invoke( ref args, Invoke );
            }
            finally
            {
                // Copy back parameters.
                foreach (var parameter in writableParameters)
                {
                    parameter.Value =
                        TupleHelper.GetTupleItemExpression( argsExpression, parameter.Index );
                }
            }
        }

        TResult Invoke( ref TArgs receivedArgs )
        {
            var receivedArgsExpression = ExpressionFactory.Parse( "receivedArgs" );

            var arguments = method.Parameters.Select(
                p =>
                    TupleHelper.GetTupleItemExpression( receivedArgsExpression, p.Index ) );

            return method.With( interceptedField ).Invoke( arguments )!;
        }
    }

    [Template]
    public void Constructor(
        IInterceptor interceptor,
        IField interceptedField,
        IField interceptorField )
    {
        interceptorField.Value = interceptor;
        interceptedField.Value = meta.Target.Parameters["intercepted"].Value;
    }
}

public interface IInterceptor
{
    public TResult Invoke<TArgs, TResult>( ref TArgs args, InterceptorDelegate<TArgs, TResult> proceed ) where TArgs : struct, ITuple;

    public Task<TResult> InvokeAsync<TArgs, TResult>( ref TArgs args, AsyncInterceptorDelegate<TArgs, TResult> proceed ) where TArgs : struct, ITuple;
}

public delegate TResult InterceptorDelegate<TArgs, out TResult>( ref TArgs args )
    where TArgs : struct, ITuple;

public delegate Task<TResult> AsyncInterceptorDelegate<TArgs, TResult>( in TArgs args ) where TArgs : struct, ITuple;

[CompileTime]
internal static class TupleHelper
{
    public static IExpression CreateTupleExpression( IMethod method )
    {
        if (method.Parameters.Count == 0)
        {
            return ExpressionFactory.Default( typeof(ValueTuple) );
        }

        var expressionBuilder = new ExpressionBuilder();

        if (method.Parameters.Count == 1)
        {
            expressionBuilder.AppendTypeName( typeof(ValueTuple) );
            expressionBuilder.AppendVerbatim( "." );
            expressionBuilder.AppendVerbatim( nameof(ValueTuple.Create) );
            expressionBuilder.AppendVerbatim( "(" );
            AppendParameterValue( 0 );
            expressionBuilder.AppendVerbatim( ")" );
        }
        else
        {
            expressionBuilder.AppendVerbatim( "(" );

            for (var index = 0; index < method.Parameters.Count; index++)
            {
                if (index > 0)
                {
                    expressionBuilder.AppendVerbatim( ", " );
                }

                AppendParameterValue( index );
            }

            expressionBuilder.AppendVerbatim( ")" );

            return expressionBuilder.ToExpression().WithType( CreateTupleType( method ) );
        }

        return expressionBuilder.ToExpression();

        void AppendParameterValue( int index )
        {
            var parameter = method.Parameters[index];

            if (parameter.RefKind != RefKind.Out)
            {
                expressionBuilder.AppendExpression( parameter );
            }
            else
            {
                expressionBuilder.AppendExpression( ExpressionFactory.Default( parameter.Type ) );
            }
        }
    }

    public static IExpression GetTupleItemExpression( IExpression tuple, int index )
    {
        var expressionBuilder = new ExpressionBuilder();

        expressionBuilder.AppendExpression( tuple );

        for (var i = 0; i < index / 7; i++)
        {
            expressionBuilder.AppendVerbatim( ".Rest" );
        }

        var finalIndex = ( index % 7 ) + 1;
        expressionBuilder.AppendVerbatim( ".Item" );
        expressionBuilder.AppendVerbatim( finalIndex.ToString( CultureInfo.InvariantCulture ) );

        return expressionBuilder.ToExpression();
    }

    public static IType CreateTupleType( IMethod method )
    {
        if (method.Parameters.Count == 0)
        {
            return TypeFactory.GetType( typeof(ValueTuple) );
        }
        else
        {
            return CreateTupleTypeRecursive( 0 );
        }

        IType CreateTupleTypeRecursive( int firstParameterIndex )
        {
            var lastDirectParameterIndex =
                Math.Min( method.Parameters.Count, firstParameterIndex + 7 );

            var typeArguments = new List<IType>();

            for (var index = firstParameterIndex; index < lastDirectParameterIndex; index++)
            {
                typeArguments.Add( method.Parameters[index].Type );
            }

            if (method.Parameters.Count > lastDirectParameterIndex)
            {
                var restType = CreateTupleTypeRecursive( firstParameterIndex + 7 );
                typeArguments.Add( restType );
            }

            if (typeArguments.Count == 0)
            {
                return (INamedType)TypeFactory.GetType( typeof(ValueTuple) );
            }
            else
            {
                var typeDefinition =
                    TypeFactory.GetType( typeof(ValueTuple).FullName + "`" + typeArguments.Count );

                return typeDefinition.WithTypeArguments( typeArguments.ToArray() );
            }
        }
    }
}

// <target>
public interface ISomeInterface
{
    void VoidMethod( int a, string b );

    int NonVoidMethod( int a, string b );

    void VoidNoParamMethod();
}