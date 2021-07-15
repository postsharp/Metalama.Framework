using System;
using System.Collections.Generic;
using Caravela.Framework.Code;
using Caravela.Framework;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

// This checks that throw expressions in expression bodies work properly.


namespace Caravela.Framework.Tests.Integration.Aspects.Bugs.Bug28880
{
    class MethodAspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod() => throw new NotSupportedException();
    }
    
    class PropertyAspect : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty 
        { 
            get => throw new NotSupportedException(); 
            set => throw new NotSupportedException(); 
       }
    }
    
    class PropertyAspect2 : Attribute, IAspect<IFieldOrProperty>
    {
        [Template]
        public dynamic OverrideProperty => throw new NotSupportedException(); 
        
        public void BuildAspect( IAspectBuilder<IFieldOrProperty> builder )
        {
            builder.AdviceFactory.OverrideFieldOrProperty( builder.TargetDeclaration, nameof(OverrideProperty));
        }
    }
    
    class EventAspect : OverrideEventAspect
    {
        public override void OverrideAdd(dynamic handler)
         => throw new NotImplementedException();

        public override void OverrideRemove(dynamic handler)
         => throw new NotImplementedException();
        
    }

    // <target>
    class TargetCode
    {
        [MethodAspect]
        int Method(int a)
        {
            return a;
        }
        
        // TODO BUg 28882
       // [PropertyAspect]
       // int field;
        
        [PropertyAspect]
        int Property { get; set; }
        
        [PropertyAspect2]
        int Property2 { get; set; }
    }
}