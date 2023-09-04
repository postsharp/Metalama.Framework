class Target
{
    [IntroduceAttributeAspect]
    class IntroduceTarget
    {
        /// <summary>
        /// Gets or sets a test property value.
        /// </summary>
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia.NewAttribute]
        public static object? TestProperty { get; set; }
        /// <summary>
        /// A test method.
        /// </summary>
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia.NewAttribute]
        public static void TestMethod()
        {
        }
        // nested class
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia.NewAttribute]
        class Nested
        {
        }
        /// <summary>
        /// Field.
        /// </summary>
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia.NewAttribute]
        int _field;
        // multifield
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia.NewAttribute]
        int _f1;
        // multifield
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia.NewAttribute]
        int _f2;
        /// <summary>
        /// Event.
        /// </summary>
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia.NewAttribute]
        public event EventHandler? Event;
        /// <summary>
        /// Another event.
        /// </summary>
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia.NewAttribute]
        public event EventHandler? Event2
        {
            add
            {
            }
            remove
            {
            }
        }
    }
    [RemoveAttributeAspect]
    class RemoveTarget
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
        class Nested
        {
        }
        /// <summary>
        /// Field.
        /// </summary>
        int _field;
        // multifield
        int _f1;
        // multifield
        int _f2;
        /// <summary>
        /// Event.
        /// </summary>
        public event EventHandler? Event;
        /// <summary>
        /// Another event.
        /// </summary>
        public event EventHandler? Event2
        {
            add
            {
            }
            remove
            {
            }
        }
    }
    [RemoveAttributeAspect, IntroduceAttributeAspect]
    class ReplaceTarget
    {
        /// <summary>
        /// Gets or sets a test property value.
        /// </summary>
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia.NewAttribute]
        public static object? TestProperty { get; set; }
        /// <summary>
        /// A test method.
        /// </summary>
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia.NewAttribute]
        public static void TestMethod()
        {
        }
        // nested class
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia.NewAttribute]
        class Nested
        {
        }
        /// <summary>
        /// Field.
        /// </summary>
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia.NewAttribute]
        int _field;
        // multifield
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia.NewAttribute]
        int _f1;
        // multifield
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia.NewAttribute]
        int _f2;
        /// <summary>
        /// Event.
        /// </summary>
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia.NewAttribute]
        public event EventHandler? Event;
        /// <summary>
        /// Another event.
        /// </summary>
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia.NewAttribute]
        public event EventHandler? Event2
        {
            add
            {
            }
            remove
            {
            }
        }
    }
    [RemoveAttributeAspect2, IntroduceAttributeAspect]
    class ReplaceTarget2
    {
        /// <summary>
        /// Gets or sets a test property value.
        /// </summary>
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia.NewAttribute]
        public static object? TestProperty { get; set; }
        /// <summary>
        /// A test method.
        /// </summary>
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia.NewAttribute]
        public static void TestMethod()
        {
        }
        // nested class
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia.NewAttribute]
        class Nested
        {
        }
        /// <summary>
        /// Field.
        /// </summary>
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia.NewAttribute]
        int _field;
        // multifield
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia.NewAttribute]
        int _f1;
        // multifield
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia.NewAttribute]
        int _f2;
        /// <summary>
        /// Event.
        /// </summary>
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia.NewAttribute]
        public event EventHandler? Event;
        /// <summary>
        /// Another event.
        /// </summary>
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia.NewAttribute]
        public event EventHandler? Event2
        {
            add
            {
            }
            remove
            {
            }
        }
    }
}
