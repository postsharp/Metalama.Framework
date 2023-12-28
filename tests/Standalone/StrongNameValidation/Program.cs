// See https://aka.ms/new-console-template for more information
using Metalama.Framework.RunTime;

var m = ReflectionHelper.GetMethod( typeof( Program ), "ToString", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, "System.String ToString()" );
Console.WriteLine( m );

