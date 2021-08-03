// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

/*

using System;
using System.Runtime.CompilerServices;

namespace Caravela.Framework.Aspects
{

[CompileTimeOnly]
public sealed class DynamicAwaitable
{
    public dynamic Syntax { get; }

    internal DynamicAwaitable( dynamic syntax )
    {
        this.Syntax = syntax;
    }
    
    public Awaiter GetAwaiter() => default;

    public static DynamicAwaitable FromResult( dynamic? value ) => throw new NotSupportedException();
    
    public static DynamicAwaitable FromException( Exception exception ) => throw new NotSupportedException();

    [CompileTimeOnly]
    public struct Awaiter : INotifyCompletion 
    {
        public void OnCompleted( Action continuation ) => throw new NotSupportedException();

        public dynamic? GetResult() => throw new NotSupportedException();

        public bool IsCompleted => throw new NotSupportedException();
    }

    
    /*
    public class Builder
    {
        public static Builder Create()
            => new Builder();

        public void SetResult( dynamic? result ) { }

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        public DynamicAwaitable Task => default;

    }
    }
    
}

*/