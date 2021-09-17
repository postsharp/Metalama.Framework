using System;
using Caravela.Framework.Aspects;

#pragma warning disable CS0067

namespace Caravela.Framework.Tests.Integration.Tests.Formatting.AllDeclarations
{
    // We need at least an aspect otherwise the template annotator does not run.
    class Aspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod() => null;
    }
    
    [CompileTimeOnly]
    struct CompileTimeStruct
    {
        public event EventHandler FieldEvent;
        public event EventHandler ManualEvent
        {
            add => throw new Exception();
            remove => throw new Exception();
        }
        
        public string Property { get; set; }
    }

    struct RunTimeStruct
    {
        public event EventHandler FieldEvent;
        public event EventHandler ManualEvent
        {
            add => throw new Exception();
            remove => throw new Exception();
        }
        
        public string Property { get; set; }
    }

    [CompileTimeOnly]
    record CompileTimeRecord( int f );

    record RunTimeRecord( int f );

    [CompileTimeOnly]
    delegate void CompileTimeDelegate();

    delegate void RunTimeDelegate();



}