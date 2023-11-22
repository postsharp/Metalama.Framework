using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Aspects.Bugs.Bug30076
{
   //The logging of parameters requires a little more work if we are to get anything meaningful back.
    //We'll use an interpolated string to render the parameter(s) and result.
    //We will aslo need to ensure that we allow for void methods that do not actually return anything.
    internal class LogAttribute : OverrideMethodAspect
    {
        #region Private Methods

        [CompileTime]
        private static InterpolatedStringBuilder BuildInterpolatedString()
        {
            var stringBuilder = new InterpolatedStringBuilder();
            
            stringBuilder.AddText(meta.Target.Type.ToDisplayString(CodeDisplayFormat.MinimallyQualified));
            stringBuilder.AddText(".");
            stringBuilder.AddText(meta.Target.Method.Name);
            stringBuilder.AddText("(");
            var i = meta.CompileTime(0);
            foreach(var p in meta.Target.Parameters)
            {
                var comma = i > 0 ? ", " : string.Empty;

                if(p.RefKind == RefKind.Out) //Refkind allows us to determine the type of referenced variable
                {
                    stringBuilder.AddText($"{comma}{p.Name} = <out> ");
                } else
                {
                    stringBuilder.AddText($"{comma}{p.Name} = {{");
                    stringBuilder.AddExpression(p.Value);
                    stringBuilder.AddText("}");
                }

                i++;
            }

            stringBuilder.AddText(")");
            return stringBuilder;
        }

        #endregion

        #region Public Methods
        public override dynamic? OverrideMethod()
        {
            var errorColour = ConsoleColor.Red;
            var resultColour = ConsoleColor.Green;
            //First we need to build a formatted string using our interpolated string builder
            var methodName = BuildInterpolatedString();

            //now we need an entry message for the method
            var entryMessage = methodName.Clone();
            entryMessage.AddText(" started.");
            Console.WriteLine(entryMessage.ToValue());

            try
            {
                //invoke the method
                var result = meta.Proceed();

                //Create a basic success message
                var successMessage = methodName.Clone();

                //find out if we're dealing with a void method
                if(meta.Target.Method.ReturnType.Is(typeof(void)))
                {
                    //the methood is a void method and won't return anything so all we need to do
                    //is inform that it has been successful.
                    successMessage.AddText(" succeeded.");
                } else
                {
                    //The method will return something, so in addition to informing of the success of the method
                    // we also need to inform about what was returned.
                    successMessage.AddText(" succeeded and returned ");
                    successMessage.AddExpression(result);
                    successMessage.AddText(".");
                }
                //Now instead of using a plain console writeline we can use our logging helper

                //Console.WriteLine(successMessage.ToValue());
                LoggingHelper.Log($"{successMessage.ToValue()}", resultColour);
                return result;
            } catch(Exception e)
            {
                //create and display a messasage indicating failure
                var failureMessage = methodName.Clone();
                failureMessage.AddText(" failed: ");
                failureMessage.AddExpression(e.Message);

                //And again here we can use our error colour instead of the plain console colour

                //Console.WriteLine(failureMessage.ToValue());
                LoggingHelper.Log($"{failureMessage.ToValue()}", errorColour);
                throw;
            }
        }

        #endregion
    }
    
    
    static class LoggingHelper
    {
        [ExcludeAspect(typeof(LogAttribute), Justification = "Avoid infinite recursion.") ]
        public static void Log( string message, ConsoleColor color )
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message );
            Console.ForegroundColor = oldColor;
        }
    }

   // <target>
    class Target
    {
        [Log]
        void M() { }
    }
}