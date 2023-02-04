using Metalama.Framework.Aspects;
using System;
using System.Linq;

#pragma warning disable CS0414

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Events.InitializerTemplate
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce]
        public event EventHandler? IntroducedEventField = meta.Target.Event.DeclaringType.Fields.OfName( "Foo" ).Single().GetValue( null );

        [Introduce]
        public static event EventHandler? IntroducedEventField_Static = meta.Target.Event.DeclaringType.Fields.OfName( "Foo" ).Single().GetValue( null );
    }

    // <target>
    [Introduction]
    internal class TargetClass
    {
        public static EventHandler Foo = new( Bar );

        public static void Bar( object? sender, EventArgs eventArgs ) { }
    }
}