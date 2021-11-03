using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

#pragma warning disable CS8618

namespace Caravela.Framework.Tests.InternalPipeline.Templating.Syntax.Invocation.RunTimeTargets
{
    class Aspect : Attribute
    {
    
        [TestTemplate]
        dynamic? Template( Action<int,int> parameter )
        {
            // Method
            TargetCode.Method( 0, 1 );
        
            // Local variable.
            var local = new Func<int,int>( x => x );
            _ = local(0);
            
            // Parameter
            parameter( 0, 1 );
            
            // Field
            TargetCode.Field(0,1);
            
            // Property
            TargetCode.Property(0,1);
            
            // Expression
            _ = new Func<int,int>( x => x )(0);
            
            // Run-time dynamic field.
            TargetCode.DynamicField(0, 1);
            
            
            
            return null;
        }
        
        
        static IExpression? BuildTimeMethod( int x, int y) => null;
    }

    class TargetCode
    {
        public static dynamic DynamicField {get; }
    
        public static Action<int,int> Field;
        
        public static Action<int,int> Property { get; }
        
        [Aspect]
        public static void Method( int a, int b ) {}
    
    }
}