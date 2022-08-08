// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Maintenance;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Utilities.UserCode
{
    /// <summary>
    /// Invokes some user code, handles exceptions, switches the <see cref="UserCodeExecutionContext"/>,
    /// and optionally invokes an <see cref="IUserCodeInvokerHook"/> (this hook is used by Try Metalama).
    /// </summary>
    internal sealed class UserCodeInvoker : IService
    {
        private readonly IUserCodeInvokerHook? _hook;

        public UserCodeInvoker( IServiceProvider serviceProvider )
        {
            this._hook = serviceProvider.GetService<IUserCodeInvokerHook>();
        }

        private static bool OnException( Exception e, in UserCodeExecutionContext context )
        {
            var compileTimeProject = context.ServiceProvider.GetService<CompileTimeProject>();

            var userException = e switch
            {
                TargetInvocationException { InnerException: { } innerException } => innerException,
                _ => e
            };

            Location? exactLocation = null;

            if ( compileTimeProject != null )
            {
                var stackTrace = new StackTrace( userException, true );

                exactLocation = GetSourceCodeLocation( stackTrace, compileTimeProject );
            }

            if ( userException is DiagnosticException invalidUserCodeException )
            {
                foreach ( var diagnostic in invalidUserCodeException.Diagnostics )
                {
                    var betterLocation = exactLocation ?? (diagnostic.Location == Location.None ? context.InvokedMember.GetDiagnosticLocation() : null);

                    if ( betterLocation != null )
                    {
                        // Report the original diagnostics, but with the fixed location.
                        context.Diagnostics.Report(
                            Diagnostic.Create(
                                diagnostic.Id,
                                diagnostic.Descriptor.Category,
                                new NonLocalizedString( diagnostic.GetMessage() ),
                                diagnostic.Severity,
                                diagnostic.DefaultSeverity,
                                true,
                                diagnostic.WarningLevel,
                                diagnostic.Descriptor.Title,
                                diagnostic.Descriptor.Description,
                                diagnostic.Descriptor.HelpLinkUri,
                                betterLocation,
                                diagnostic.AdditionalLocations,
                                properties: diagnostic.Properties ) );
                    }
                    else
                    {
                        // If we don't have a better location, report the original diagnostic.
                        context.Diagnostics.Report( diagnostic );
                    }
                }
            }
            else
            {
                var location = exactLocation ?? context.InvokedMember.GetDiagnosticLocation() ?? Location.None;

                var tempFileManager = context.ServiceProvider.GetRequiredBackstageService<ITempFileManager>();
                var applicationInfoProvider = context.ServiceProvider.GetRequiredBackstageService<IApplicationInfoProvider>();
                string reportFile;

                if ( applicationInfoProvider.CurrentApplication.ShouldCreateLocalCrashReports )
                {
                    reportFile = tempFileManager.GetTempDirectory( "CrashReports", CleanUpStrategy.Always );
                    File.WriteAllText( reportFile, e.ToString() );
                }
                else
                {
                    reportFile = "(none)";
                }

                var exceptionMessage = userException.Message.TrimSuffix( "." );
                var exceptionType = userException.GetType().Name;

                if ( context.TargetDeclaration != null )
                {
                    context.Diagnostics.Report(
                        GeneralDiagnosticDescriptors.ExceptionInUserCodeWithTarget.CreateRoslynDiagnostic(
                            location,
                            (context.InvokedMember,
                             context.TargetDeclaration,
                             exceptionType,
                             exceptionMessage,
                             reportFile) ) );
                }
                else
                {
                    context.Diagnostics.Report(
                        GeneralDiagnosticDescriptors.ExceptionInUserCodeWithoutTarget.CreateRoslynDiagnostic(
                            location,
                            (context.InvokedMember,
                             exceptionType,
                             exceptionMessage,
                             reportFile) ) );
                }
            }

            return true;
        }

        public bool TryInvoke<T>( Func<T> func, UserCodeExecutionContext context, out T? result )
        {
            try
            {
                result = this.Invoke( func, context );

                return true;
            }
            catch ( Exception e ) when ( OnException( e, context ) )
            {
                result = default;

                return false;
            }
        }

        public bool TryInvokeEnumerable<T>( Func<IEnumerable<T>> func, UserCodeExecutionContext context, [NotNullWhen( true )] out List<T>? result )
        {
            try
            {
                result = new List<T>();

                try
                {
                    var enumerable = this.Invoke( func, context );
                    var enumerator = this.Invoke( enumerable.GetEnumerator, context );

                    while ( this.Invoke( enumerator.MoveNext, context ) )
                    {
                        result.Add( this.Invoke( () => enumerator.Current, context ) );
                    }
                }
                catch ( Exception e ) when ( OnException( e, context ) )
                {
                    result = null;

                    return false;
                }

                return true;
            }
            catch ( Exception e ) when ( OnException( e, context ) )
            {
                result = default;

                return false;
            }
        }

        public bool TryInvoke( Action action, UserCodeExecutionContext context )
            => this.TryInvoke(
                () =>
                {
                    action();

                    return true;
                },
                context,
                out _ );

        public void Invoke( Action action, UserCodeExecutionContext context )
        {
            _ = this.Invoke(
                () =>
                {
                    action();

                    return true;
                },
                context );
        }

        public T Invoke<T>( Func<T> func, UserCodeExecutionContext context )
        {
            var adapter = new UserCodeFuncAdapter<T>( func );

            return this.Invoke( adapter.UserCodeFunc, ref adapter, context );
        }

        public TResult Invoke<TResult, TPayload>( UserCodeFunc<TResult, TPayload> func, ref TPayload payload, UserCodeExecutionContext? context )
        {
            using ( UserCodeExecutionContext.WithContext( context ) )
            {
                if ( this._hook != null )
                {
                    return this._hook.Invoke( func, ref payload );
                }
                else
                {
                    return func( ref payload );
                }
            }
        }

        public Task InvokeAsync( Func<Task> func, UserCodeExecutionContext? context )
        {
            async Task<bool> Wrapper()
            {
                await func();

                return true;
            }

            return this.InvokeAsync( Wrapper, context );
        }

        public async Task<TResult> InvokeAsync<TResult>( Func<Task<TResult>> func, UserCodeExecutionContext? context )
        {
            using ( UserCodeExecutionContext.WithContext( context ) )
            {
                if ( this._hook != null )
                {
                    return await this._hook.InvokeAsync( func );
                }
                else
                {
                    return await func();
                }
            }
        }

        private static Location? GetSourceCodeLocation( StackTrace stackTrace, CompileTimeProject compileTimeProject )
        {
            // TODO: This method needs to be rewritten. Ideally, the PDB would be mapped to the source file, it would not be necessary
            // to perform the mapping here.

            // Get the syntax tree where the exception happened.
            var stackFrames = stackTrace.GetFrames() ?? Array.Empty<StackFrame>();

            var frame =
                stackFrames
                    .Where( f => f.GetFileName() != null )
                    .Select( f => (Frame: f, File: compileTimeProject.FindCodeFileFromTransformedPath( f.GetFileName()! )) )
                    .FirstOrDefault( i => i.File != null );

            if ( frame.File == null )
            {
                return null;
            }

            // Check if we have a location map for this file anyway.
            var textMap = compileTimeProject.GetTextMap( frame.Frame.GetFileName()! );

            if ( textMap == null )
            {
                return null;
            }

            var transformedFileFullPath = Path.Combine( compileTimeProject.Directory!, frame.File.TransformedPath );

            var transformedText = SourceText.From( File.ReadAllText( transformedFileFullPath ) );

            // Find the node in the syntax tree.
            var textLines = transformedText.Lines;
            var lineNumber = frame.Frame.GetFileLineNumber();

            if ( lineNumber == 0 )
            {
                return null;
            }

            var columnNumber = frame.Frame.GetFileColumnNumber();

            if ( lineNumber > textLines.Count )
            {
                return null;
            }

            var textLine = textLines[lineNumber - 1];

            if ( columnNumber > textLine.End )
            {
                return null;
            }

            var position = textLine.Start + columnNumber - 1;

            var transformedTree = CSharpSyntaxTree.ParseText( transformedText );

            var node = transformedTree.GetRoot().FindNode( TextSpan.FromBounds( position, position ) );
            node = FindPotentialExceptionSource( node );

            if ( node != null )
            {
                var targetLocation = node.GetLocation();
                var sourceLocation = textMap.GetSourceLocation( targetLocation.SourceSpan );

                return sourceLocation ?? targetLocation;
            }
            else
            {
                // TODO: We could report the whole line here.
                return Location.None;
            }

            // Finds a parent node that is a potential source of exception. We take the farthest ancestor that has the same span start.
            static SyntaxNode? FindPotentialExceptionSource( SyntaxNode? node )
                => node switch
                {
                    null => null,
                    { Parent: { } parent } when parent.SpanStart == node.SpanStart => FindPotentialExceptionSource( node.Parent ),
                    _ => node
                };
        }
    }
}