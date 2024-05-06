using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia;

#pragma warning disable CS0067, CS0169

[assembly: AspectOrder(typeof(RemoveAttributeAspect), typeof(IntroduceAttributeAspect), typeof(RemoveAttributeAspect2), ApplyToDerivedTypes = false)]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia;

public class OldAttribute : Attribute { }

public class NewAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DescriptionAttribute : Attribute
{
    public DescriptionAttribute(string description) { }
}

public class IntroduceAttributeAspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        foreach (var member in builder.Target.Members().Cast<IDeclaration>().Concat(builder.Target.NestedTypes))
        {
            builder.Advice.IntroduceAttribute(member, AttributeConstruction.Create(typeof(NewAttribute)));
        }
    }
}

public class RemoveAttributeAspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        foreach (var member in builder.Target.Members().Cast<IDeclaration>().Concat(builder.Target.NestedTypes))
        {
            builder.Advice.RemoveAttributes(member, typeof(OldAttribute));
        }
    }
}

public class RemoveAttributeAspect2 : RemoveAttributeAspect { }

// <target>
class Target
{
    [IntroduceAttributeAspect]
    class IntroduceTarget
    {
        /// <summary>
        /// Gets or sets a test property value.
        /// </summary>
        public static object? TestProperty { get; set; }

        /// <summary>
        /// A test method.
        /// </summary>
        public static void TestMethod()
        {
        }

        // nested class
        class Nested { }

        /// <summary>
        /// Field.
        /// </summary>
        int _field;

        // multifield
        int _f1, _f2;

        /// <summary>
        /// Event.
        /// </summary>
        public event EventHandler? Event;

        /// <summary>
        /// Another event.
        /// </summary>
        public event EventHandler? Event2 { add { } remove { } }
    }

    [RemoveAttributeAspect]
    class RemoveTarget
    {
        /// <summary>
        /// Gets or sets a test property value.
        /// </summary>
        [OldAttribute]
        public static object? TestProperty { get; set; }

        /// <summary>
        /// A test method.
        /// </summary>
        [OldAttribute]
        public static void TestMethod()
        {
        }

        // nested class
        [OldAttribute]
        class Nested { }

        /// <summary>
        /// Field.
        /// </summary>
        [OldAttribute]
        int _field;

        // multifield
        [OldAttribute]
        int _f1, _f2;

        /// <summary>
        /// Event.
        /// </summary>
        [OldAttribute]
        public event EventHandler? Event;

        /// <summary>
        /// Another event.
        /// </summary>
        [OldAttribute]
        public event EventHandler? Event2 { add { } remove { } }
    }

    [RemoveAttributeAspect, IntroduceAttributeAspect]
    class ReplaceTarget
    {
        /// <summary>
        /// Gets or sets a test property value.
        /// </summary>
        [OldAttribute]
        public static object? TestProperty { get; set; }

        /// <summary>
        /// A test method.
        /// </summary>
        [OldAttribute]
        public static void TestMethod()
        {
        }

        // nested class
        [OldAttribute]
        class Nested { }

        /// <summary>
        /// Field.
        /// </summary>
        [OldAttribute]
        int _field;

        // multifield
        [OldAttribute]
        int _f1, _f2;

        /// <summary>
        /// Event.
        /// </summary>
        [OldAttribute]
        public event EventHandler? Event;

        /// <summary>
        /// Another event.
        /// </summary>
        [OldAttribute]
        public event EventHandler? Event2 { add { } remove { } }
    }

    [RemoveAttributeAspect2, IntroduceAttributeAspect]
    class ReplaceTarget2
    {
        /// <summary>
        /// Gets or sets a test property value.
        /// </summary>
        [OldAttribute]
        public static object? TestProperty { get; set; }

        /// <summary>
        /// A test method.
        /// </summary>
        [OldAttribute]
        public static void TestMethod()
        {
        }

        // nested class
        [OldAttribute]
        class Nested { }

        /// <summary>
        /// Field.
        /// </summary>
        [OldAttribute]
        int _field;

        // multifield
        [OldAttribute]
        int _f1, _f2;

        /// <summary>
        /// Event.
        /// </summary>
        [OldAttribute]
        public event EventHandler? Event;

        /// <summary>
        /// Another event.
        /// </summary>
        [OldAttribute]
        public event EventHandler? Event2 { add { } remove { } }
    }
}