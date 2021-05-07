// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Templating.MetaModel;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.CodeGeneration;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using Accessibility = Caravela.Framework.Code.Accessibility;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;
using RefKind = Caravela.Framework.Code.RefKind;
using TypedConstant = Caravela.Framework.Code.TypedConstant;

namespace Caravela.Framework.Impl.CodeModel
{
    internal static class CodeElementExtensions
    {
        public static CodeElementKind GetCodeElementKind( this ISymbol symbol )
            => symbol switch
            {
                INamespaceSymbol => CodeElementKind.Compilation,
                INamedTypeSymbol => CodeElementKind.Type,
                IMethodSymbol method => method.MethodKind == MethodKind.Constructor || method.MethodKind == MethodKind.StaticConstructor
                    ? CodeElementKind.Constructor
                    : CodeElementKind.Method,
                IPropertySymbol => CodeElementKind.Property,
                IFieldSymbol => CodeElementKind.Field,
                ITypeParameterSymbol => CodeElementKind.GenericParameter,
                IAssemblySymbol => CodeElementKind.Compilation,
                IParameterSymbol => CodeElementKind.Parameter,
                IEventSymbol => CodeElementKind.Event,
                ITypeSymbol => CodeElementKind.None,
                _ => throw new ArgumentException( nameof(symbol), $"Unexpected symbol: {symbol.GetType().Name}." )
            };

        /// <summary>
        /// Gets a value indicating whether a symbol is exposed to the user code model.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static bool IsVisible( this ISymbol m ) => !m.IsImplicitlyDeclared || (m.Kind == SymbolKind.Method && m.MetadataName == ".ctor");

        /// <summary>
        /// Select all code elements recursively contained in a given code element (i.e. all children of the tree).
        /// </summary>
        /// <param name="codeElement"></param>
        /// <returns></returns>
        public static IEnumerable<ICodeElement> GetContainedElements( this ICodeElement codeElement )
            => codeElement.SelectManyRecursive(
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

        /// <summary>
        /// Select all code elements recursively contained in a given code element (i.e. all children of the tree).
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static IEnumerable<ISymbol> GetContainedSymbols( this ISymbol symbol )
            => symbol switch
            {
                IAssemblySymbol compilation => compilation.GetTypes(),
                INamedTypeSymbol namedType => namedType.GetMembers().Where( IsVisible ).Concat( namedType.TypeParameters ),
                IMethodSymbol method => method.Parameters.Concat<ISymbol>( method.TypeParameters ),
                IPropertySymbol property => property.Parameters,
                _ => Array.Empty<ISymbol>()
            };

        public static IEnumerable<AttributeLink> ToAttributeLinks( this IEnumerable<AttributeData> attributes, ISymbol declaringSymbol )
            => attributes.Select( a => new AttributeLink( a, CodeElementLink.FromSymbol( declaringSymbol ) ) );

        public static IEnumerable<AttributeLink> GetAllAttributes( this ISymbol symbol )
            => symbol switch
            {
                IMethodSymbol method => method
                    .GetAttributes()
                    .ToAttributeLinks( method )
                    .Concat(
                        method.GetReturnTypeAttributes()
                            .Select( a => new AttributeLink( a, CodeElementLink.ReturnParameter( method ) ) ) ),
                _ => symbol.GetAttributes().ToAttributeLinks( symbol )
            };

        public static CodeElementLink<ICodeElement> ToLink( this ISymbol symbol ) => CodeElementLink.FromSymbol( symbol );

        public static CodeElementLink<T> ToLink<T>( this T codeElement )
            where T : class, ICodeElement
            => ((ICodeElementInternal) codeElement).ToLink().Cast<T>();

        public static MemberLink<T> ToMemberLink<T>( this T member )
            where T : class, IMember
            => new( ((ICodeElementInternal) member).ToLink() );

        public static Location? GetDiagnosticLocation( this ICodeElement codeElement )
            => codeElement switch
            {
                IHasDiagnosticLocation hasLocation => hasLocation.DiagnosticLocation,
                _ => null
            };

        internal static void CheckArguments( this ICodeElement codeElement, IReadOnlyList<IParameter> parameters, RuntimeExpression[]? arguments )
        {
            // TODO: somehow provide locations for the diagnostics?
            var argumentsLength = arguments?.Length ?? 0;

            if ( parameters.LastOrDefault()?.IsParams == true )
            {
                // all non-params arguments have to be set + any number of params arguments
                var requiredArguments = parameters.Count - 1;

                if ( argumentsLength < requiredArguments )
                {
                    throw GeneralDiagnosticDescriptors.MemberRequiresAtLeastNArguments.CreateException( (codeElement, requiredArguments) );
                }
            }
            else
            {
                if ( argumentsLength != parameters.Count )
                {
                    throw GeneralDiagnosticDescriptors.MemberRequiresNArguments.CreateException( (codeElement, parameters.Count) );
                }
            }
        }

        internal static ArgumentSyntax[] GetArguments( this ICodeElement codeElement, IReadOnlyList<IParameter> parameters, RuntimeExpression[]? args )
        {
            CheckArguments( codeElement, parameters, args );

            if ( args == null || args.Length == 0 )
            {
                return Array.Empty<ArgumentSyntax>();
            }

            var arguments = new List<ArgumentSyntax>( args.Length );

            for ( var i = 0; i < args.Length; i++ )
            {
                var arg = args[i];

                ArgumentSyntax argument;
                var parameter = parameters[i];

                if ( i >= parameters.Count || parameter.IsParams )
                {
                    // params methods can be called as params or directly with an array
                    // so it's probably best to not do any type-checking for them

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
                            throw new InvalidUserCodeException(
                                GeneralDiagnosticDescriptors.CannotPassExpressionToByRefParameter.CreateDiagnostic(
                                    null,
                                    (arg.Syntax.ToString(), parameter.Name, parameter.DeclaringMember) ) );
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

        internal static ExpressionSyntax GetReceiverSyntax<T>( this T codeElement, RuntimeExpression? instance )
            where T : IMember
        {
            if ( codeElement.IsStatic )
            {
                if ( instance != null )
                {
                    throw GeneralDiagnosticDescriptors.CannotProvideInstanceForStaticMember.CreateException( codeElement );
                }

                return (ExpressionSyntax) CSharpSyntaxGenerator.Instance.TypeExpression( codeElement.DeclaringType!.GetSymbol() );
            }

            if ( instance == null )
            {
                throw GeneralDiagnosticDescriptors.MustProvideInstanceForInstanceMember.CreateException( codeElement );
            }

            return instance.ToTypedExpression( codeElement.DeclaringType, true );
        }

        internal static ExpressionSyntax? ToExpressionSyntax( this in TypedConstant value, CompilationModel compilation )
        {
            if ( value.IsAssigned )
            {
                return compilation.Factory.Serializers.Serialize( value.Value, compilation.ReflectionMapper );
            }

            return null;
        }

        internal static RefKind ToOurRefKind( this Microsoft.CodeAnalysis.RefKind roslynRefKind )
            => roslynRefKind switch
            {
                Microsoft.CodeAnalysis.RefKind.None => RefKind.None,
                Microsoft.CodeAnalysis.RefKind.Ref => RefKind.Ref,
                Microsoft.CodeAnalysis.RefKind.RefReadOnly => RefKind.RefReadOnly,
                _ => throw new InvalidOperationException( $"Roslyn RefKind {roslynRefKind} not recognized here." )
            };

        internal static Microsoft.CodeAnalysis.RefKind ToRoslynRefKind( this RefKind ourRefKind )
            => ourRefKind switch
            {
                RefKind.None => Microsoft.CodeAnalysis.RefKind.None,
                RefKind.Ref => Microsoft.CodeAnalysis.RefKind.Ref,
                RefKind.RefReadOnly => Microsoft.CodeAnalysis.RefKind.RefReadOnly,
                _ => throw new InvalidOperationException( $"RefKind {ourRefKind} not recognized." )
            };

        internal static Accessibility ToOurVisibility( this Microsoft.CodeAnalysis.Accessibility accessibility )
            => accessibility switch
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

        internal static Microsoft.CodeAnalysis.Accessibility ToRoslynAccessibility( this Accessibility accessibility )
            => accessibility switch
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

        internal static string ToDisplayString( this CodeElementKind kind )
            => kind switch
            {
                CodeElementKind.GenericParameter => "generic parameter",
                _ => kind.ToString().ToLowerInvariant()
            };
    }
}