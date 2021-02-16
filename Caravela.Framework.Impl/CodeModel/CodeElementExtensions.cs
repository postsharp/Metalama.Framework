using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Symbolic;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Accessibility = Caravela.Framework.Code.Accessibility;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal static class CodeElementExtensions
    {
        /// <summary>
        /// Select all code elements recursively contained in a given code element (i.e. all children of the tree).
        /// </summary>
        /// <param name="codeElement"></param>
        /// <returns></returns>
        public static IEnumerable<ICodeElement> SelectContainedElements( this ICodeElement codeElement ) =>
            new[] { codeElement }.SelectDescendants(
                child => child switch
                {
                    ICompilation compilation => compilation.DeclaredTypes,
                    INamedType namedType => namedType.NestedTypes
                        .Concat<ICodeElement>( namedType.Methods )
                        .Concat( namedType.Properties )
                        .Concat( namedType.Events ),
                    IMethod method => method.LocalFunctions
                        .Concat<ICodeElement>( method.Parameters )
                        .Concat( method.GenericParameters )
                        .ConcatNotNull( method.ReturnParameter ),
                    _ => null
                } );

        public static Location? GetLocation( this ICodeElement codeElement )
            => codeElement switch
            {
                IHasLocation hasLocation => hasLocation.Location,
                _ => null
            };

        internal static void CheckArguments( this CodeElement codeElement, IReadOnlyList<IParameter> parameters, object[] arguments )
        {
            // TODO: somehow provide locations for the diagnostics?
            if ( parameters.LastOrDefault()?.IsParams == true )
            {
                // all non-params arguments have to be set + any number of params arguments
                var requiredArguments = parameters.Count - 1;
                if ( arguments.Length < requiredArguments )
                {
                    throw new CaravelaException( GeneralDiagnosticDescriptors.MemberRequiresAtLeastNArguments, codeElement, requiredArguments );
                }
            }
            else
            {
                if ( arguments.Length != parameters.Count )
                {
                    throw new CaravelaException( GeneralDiagnosticDescriptors.MemberRequiresNArguments, codeElement, parameters.Count );
                }
            }
        }

        internal static ArgumentSyntax[] GetArguments( this CodeElement codeElement, IReadOnlyList<IParameter> parameters, RuntimeExpression[] args )
        {
            CheckArguments( codeElement, parameters, args );

            var arguments = new List<ArgumentSyntax>( args.Length );

            for ( var i = 0; i < args.Length; i++ )
            {
                var arg = args[i];

                ArgumentSyntax argument;
                var parameter = parameters[i];
                if ( i >= parameters.Count || parameter.IsParams )
                {
                    // params methods can be called as params or direcly with an array
                    // so it's probably best to not do any typecheking for them

                    argument = SyntaxFactory.Argument( arg.Syntax );

                }
                else
                {

                    if ( parameter.IsOut() || parameter.IsRef() )
                    {
                        // With out and ref parameters, we unconditionally add the out or ref modifier, and "hope" the code will later compile.
                        // We also intentionally omit to cast the value since it would be illegal.

                        if ( !arg.IsReferenceable )
                        {
                            throw new CaravelaException( GeneralDiagnosticDescriptors.CannotPassExpressionToByRefParameter,
                                arg.Syntax, parameter.Name, parameter.ContainingElement );
                        }

                        var syntax = parameter.IsRef() ? SyntaxKind.RefKeyword : SyntaxKind.OutKeyword;

                        argument = SyntaxFactory.Argument( null, SyntaxFactory.Token( syntax ), arg.Syntax );
                    }
                    else
                    {
                        argument = SyntaxFactory.Argument( arg.ToTypedExpression( parameter.ParameterType ) );
                    }
                }


                arguments.Add( argument );
            }

            return arguments.ToArray();
        }

        internal static ExpressionSyntax GetReceiverSyntax<T>( this T codeElement, RuntimeExpression instance )
            where T : CodeElement, IMember
        {

            if ( codeElement.IsStatic )
            {
                if ( instance != null )
                {
                    throw new CaravelaException( GeneralDiagnosticDescriptors.CannotProvideInstanceForStaticMember, codeElement );
                }

                return (ExpressionSyntax) codeElement.Compilation.SyntaxGenerator.TypeExpression( codeElement.DeclaringType!.GetSymbol() );
            }
            else
            {
                if ( instance == null )
                {
                    throw new CaravelaException( GeneralDiagnosticDescriptors.MustProvideInstanceForInstanceMember, codeElement );
                }

                return instance.ToTypedExpression( codeElement.DeclaringType, true );
            }
        }

        internal static ExpressionSyntax? ToExpressionSyntax( this in OptionalValue value, CompilationModel compilation )
        {
            if ( value.HasValue )
            {
                return compilation.Factory.Serializers.SerializeToRoslynCreationExpression( value.Value );
            }
            else
            {
                return null;
            }
        }

        internal static RefKind ToOurRefKind( this Microsoft.CodeAnalysis.RefKind roslynRefKind ) => roslynRefKind switch
        {
            Microsoft.CodeAnalysis.RefKind.None => RefKind.None,
            Microsoft.CodeAnalysis.RefKind.Ref => RefKind.Ref,
            Microsoft.CodeAnalysis.RefKind.RefReadOnly => RefKind.RefReadOnly,
            _ => throw new InvalidOperationException( $"Roslyn RefKind {roslynRefKind} not recognized here." )
        };

        internal static Microsoft.CodeAnalysis.RefKind ToRoslynRefKind( this RefKind ourRefKind ) => ourRefKind switch
        {
            RefKind.None => Microsoft.CodeAnalysis.RefKind.None,
            RefKind.Ref => Microsoft.CodeAnalysis.RefKind.Ref,
            RefKind.RefReadOnly => Microsoft.CodeAnalysis.RefKind.RefReadOnly,
            _ => throw new InvalidOperationException( $"RefKind {ourRefKind} not recognized." )
        };

        internal static Accessibility ToOurVisibility( this Microsoft.CodeAnalysis.Accessibility accessibility ) => accessibility switch
        {
            Microsoft.CodeAnalysis.Accessibility.NotApplicable => Accessibility.Private,
            Microsoft.CodeAnalysis.Accessibility.Private => Accessibility.Private,
            Microsoft.CodeAnalysis.Accessibility.ProtectedAndInternal => Accessibility.ProtectedAndInternal,
            Microsoft.CodeAnalysis.Accessibility.Protected => Accessibility.Protected,
            Microsoft.CodeAnalysis.Accessibility.Internal => Accessibility.Internal,
            Microsoft.CodeAnalysis.Accessibility.ProtectedOrInternal => Accessibility.ProtectedOrInternal,
            Microsoft.CodeAnalysis.Accessibility.Public => Accessibility.Public,
            _ => throw new ArgumentOutOfRangeException()
        };

        internal static Microsoft.CodeAnalysis.Accessibility ToRoslynAccessibility( this Accessibility accessibility ) => accessibility switch
        {
            Accessibility.Private => Microsoft.CodeAnalysis.Accessibility.Private,
            Accessibility.ProtectedAndInternal => Microsoft.CodeAnalysis.Accessibility.ProtectedAndInternal,
            Accessibility.Protected => Microsoft.CodeAnalysis.Accessibility.Protected,
            Accessibility.Internal => Microsoft.CodeAnalysis.Accessibility.Internal,
            Accessibility.ProtectedOrInternal => Microsoft.CodeAnalysis.Accessibility.ProtectedOrInternal,
            Accessibility.Public => Microsoft.CodeAnalysis.Accessibility.Public,
            _ => throw new ArgumentOutOfRangeException()
        };

        internal static DeclarationModifiers ToDeclarationModifiers( this IMember member )
        {
            var modifiers = DeclarationModifiers.None;

            if ( member.IsAbstract )
            {
                modifiers |= DeclarationModifiers.Abstract;
            }

            if ( member.IsSealed )
            {
                modifiers |= DeclarationModifiers.Sealed;
            }

            if ( member.IsReadOnly )
            {
                modifiers |= DeclarationModifiers.ReadOnly;
            }

            if ( member.IsStatic )
            {
                modifiers |= DeclarationModifiers.Static;
            }

            if ( member.IsVirtual )
            {
                modifiers |= DeclarationModifiers.Virtual;
            }

            if ( member.IsOverride )
            {
                modifiers |= DeclarationModifiers.Override;
            }

            if ( member.IsNew )
            {
                modifiers |= DeclarationModifiers.New;
            }

            if ( member.IsAsync )
            {
                modifiers |= DeclarationModifiers.Async;
            }

            return modifiers;
        }
    }
}