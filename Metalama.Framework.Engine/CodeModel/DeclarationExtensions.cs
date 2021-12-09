// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using Accessibility = Metalama.Framework.Code.Accessibility;
using DeclarationKind = Metalama.Framework.Code.DeclarationKind;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;
using RefKind = Metalama.Framework.Code.RefKind;

namespace Metalama.Framework.Engine.CodeModel
{
    internal static class DeclarationExtensions
    {
        public static DeclarationKind GetDeclarationKind( this ISymbol symbol )
            => symbol switch
            {
                INamespaceSymbol => DeclarationKind.Namespace,
                INamedTypeSymbol => DeclarationKind.NamedType,
                IMethodSymbol method => method.MethodKind == MethodKind.Constructor || method.MethodKind == MethodKind.StaticConstructor
                    ? DeclarationKind.Constructor
                    : DeclarationKind.Method,
                IPropertySymbol => DeclarationKind.Property,
                IFieldSymbol => DeclarationKind.Field,
                ITypeParameterSymbol => DeclarationKind.TypeParameter,
                IAssemblySymbol => DeclarationKind.Compilation,
                IParameterSymbol => DeclarationKind.Parameter,
                IEventSymbol => DeclarationKind.Event,
                ITypeSymbol => DeclarationKind.None,
                IModuleSymbol => DeclarationKind.Compilation,
                _ => throw new ArgumentException( nameof(symbol), $"Unexpected symbol: {symbol.GetType().Name}." )
            };

        /// <summary>
        /// Gets a value indicating whether a symbol is exposed to the user code model.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static bool IsVisible( this ISymbol m )
            => !m.IsImplicitlyDeclared || (m.Kind == SymbolKind.Method && string.Equals( m.MetadataName, ".ctor", StringComparison.Ordinal ));

        /// <summary>
        /// Select all declarations recursively contained in a given declaration (i.e. all children of the tree).
        /// </summary>
        /// <param name="declaration"></param>
        /// <returns></returns>
        public static IEnumerable<IDeclaration> GetContainedDeclarations( this IDeclaration declaration )
            => declaration.SelectManyRecursive(
                child => child switch
                {
                    ICompilation compilation => compilation.Types,
                    INamedType namedType => namedType.NestedTypes
                        .Concat<IDeclaration>( namedType.Methods )
                        .Concat( namedType.Constructors )
                        .Concat( namedType.Fields )
                        .Concat( namedType.Properties )
                        .Concat( namedType.Events )
                        .Concat( namedType.TypeParameters )
                        .ConcatNotNull( namedType.StaticConstructor ),
                    IMethod method => method.LocalFunctions
                        .Concat<IDeclaration>( method.Parameters )
                        .Concat( method.TypeParameters )
                        .ConcatNotNull( method.ReturnParameter ),
                    IMemberWithAccessors member => member.Accessors,
                    _ => null
                } );

        /// <summary>
        /// Select all declarations recursively contained in a given declaration (i.e. all children of the tree).
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

        public static IEnumerable<AttributeRef> ToAttributeLinks( this IEnumerable<AttributeData> attributes, ISymbol declaringSymbol, Compilation compilation )
            => attributes.Select( a => new AttributeRef( a, Ref.FromSymbol( declaringSymbol, compilation ) ) );

        public static IEnumerable<AttributeRef> GetAllAttributes( this ISymbol symbol, Compilation compilation )
            => symbol switch
            {
                IMethodSymbol method => method
                    .GetAttributes()
                    .ToAttributeLinks( method, compilation )
                    .Concat(
                        method.GetReturnTypeAttributes()
                            .Select( a => new AttributeRef( a, Ref.ReturnParameter( method, compilation ) ) ) ),
                _ => symbol.GetAttributes().ToAttributeLinks( symbol, compilation )
            };

        public static Ref<IDeclaration> ToTypedRef( this ISymbol symbol, Compilation compilation ) => Ref.FromSymbol( symbol, compilation );

        public static Ref<T> ToTypedRef<T>( this T declaration )
            where T : class, IDeclaration
            => ((IDeclarationImpl) declaration).ToRef().As<T>();

        public static ISymbol? GetSymbol( this IDeclaration declaration, Compilation compilation )
            => declaration.GetSymbol().Translate( declaration.GetCompilationModel().RoslynCompilation, compilation );

        public static MemberRef<T> ToMemberRef<T>( this T member )
            where T : class, IMemberOrNamedType
            => new( ((IDeclarationImpl) member).ToRef() );

        public static Location? GetDiagnosticLocation( this IDeclaration declaration )
            => declaration switch
            {
                IDiagnosticLocationImpl hasLocation => hasLocation.DiagnosticLocation,
                _ => null
            };

        internal static void CheckArguments( this IDeclaration declaration, IReadOnlyList<IParameter> parameters, RuntimeExpression[]? arguments )
        {
            // TODO: somehow provide locations for the diagnostics?
            var argumentsLength = arguments?.Length ?? 0;

            if ( parameters.LastOrDefault()?.IsParams == true )
            {
                // all non-params arguments have to be set + any number of params arguments
                var requiredArguments = parameters.Count - 1;

                if ( argumentsLength < requiredArguments )
                {
                    throw GeneralDiagnosticDescriptors.MemberRequiresAtLeastNArguments.CreateException( (declaration, requiredArguments, argumentsLength) );
                }
            }
            else
            {
                if ( argumentsLength != parameters.Count )
                {
                    throw GeneralDiagnosticDescriptors.MemberRequiresNArguments.CreateException( (declaration, parameters.Count, argumentsLength) );
                }
            }
        }

        internal static ArgumentSyntax[] GetArguments(
            this IDeclaration declaration,
            IReadOnlyList<IParameter> parameters,
            RuntimeExpression[]? args )
        {
            CheckArguments( declaration, parameters, args );

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
                    if ( parameter.RefKind is RefKind.Out or RefKind.Ref )
                    {
                        // With out and ref parameters, we unconditionally add the out or ref modifier, and "hope" the code will later compile.
                        // We also intentionally omit to cast the value since it would be illegal.

                        if ( !arg.IsReferenceable )
                        {
                            throw new DiagnosticException(
                                GeneralDiagnosticDescriptors.CannotPassExpressionToByRefParameter.CreateDiagnostic(
                                    null,
                                    (arg.Syntax.ToString(), parameter.Name, parameter.DeclaringMember) ) );
                        }

                        var syntax = parameter.RefKind is RefKind.Ref ? SyntaxKind.RefKeyword : SyntaxKind.OutKeyword;

                        argument = SyntaxFactory.Argument( null, SyntaxFactory.Token( syntax ), arg.Syntax );
                    }
                    else
                    {
                        argument = SyntaxFactory.Argument( arg.ToTypedExpression( parameter.Type ) );
                    }
                }

                arguments.Add( argument );
            }

            return arguments.ToArray();
        }

        internal static ExpressionSyntax GetReceiverSyntax<T>( this T declaration, RuntimeExpression instance, SyntaxGenerationContext generationContext )
            where T : IMember
        {
            if ( declaration.IsStatic )
            {
                return generationContext.SyntaxGenerator.Type( declaration.DeclaringType.GetSymbol() );
            }

            if ( instance.Syntax.Kind() == SyntaxKind.NullLiteralExpression )
            {
                throw GeneralDiagnosticDescriptors.MustProvideInstanceForInstanceMember.CreateException( declaration );
            }

            return instance.ToTypedExpression( declaration.DeclaringType, true );
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
                Microsoft.CodeAnalysis.Accessibility.ProtectedAndInternal => Accessibility.PrivateProtected,
                Microsoft.CodeAnalysis.Accessibility.Protected => Accessibility.Protected,
                Microsoft.CodeAnalysis.Accessibility.Internal => Accessibility.Internal,
                Microsoft.CodeAnalysis.Accessibility.ProtectedOrInternal => Accessibility.ProtectedInternal,
                Microsoft.CodeAnalysis.Accessibility.Public => Accessibility.Public,
                _ => throw new ArgumentOutOfRangeException()
            };

        internal static Microsoft.CodeAnalysis.Accessibility ToRoslynAccessibility( this Accessibility accessibility )
            => accessibility switch
            {
                Accessibility.Private => Microsoft.CodeAnalysis.Accessibility.Private,
                Accessibility.PrivateProtected => Microsoft.CodeAnalysis.Accessibility.ProtectedAndInternal,
                Accessibility.Protected => Microsoft.CodeAnalysis.Accessibility.Protected,
                Accessibility.Internal => Microsoft.CodeAnalysis.Accessibility.Internal,
                Accessibility.ProtectedInternal => Microsoft.CodeAnalysis.Accessibility.ProtectedOrInternal,
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

            if ( member is IField field && field.Writeability == Writeability.ConstructorOnly )
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

        internal static string ToDisplayString( this DeclarationKind kind )
            => kind switch
            {
                DeclarationKind.TypeParameter => "generic parameter",
                _ => kind.ToString().ToLowerInvariant()
            };

        internal static bool IsAutoProperty( this IPropertySymbol symbol )
            => !symbol.IsAbstract
               && symbol.DeclaringSyntaxReferences.All(
                   sr =>
                       sr.GetSyntax() is BasePropertyDeclarationSyntax propertyDecl
                       && propertyDecl.AccessorList != null
                       && propertyDecl.AccessorList.Accessors.All( a => a.Body == null && a.ExpressionBody == null ) );

        internal static bool IsEventField( this IEventSymbol symbol )
            => !symbol.IsAbstract
               && symbol.DeclaringSyntaxReferences.All( sr => sr.GetSyntax() is VariableDeclaratorSyntax );

        internal static IMember GetExplicitInterfaceImplementation( this IMember member )
        {
            switch ( member )
            {
                case IMethod method:
                    return method.ExplicitInterfaceImplementations.Single();

                case IProperty property:
                    return property.ExplicitInterfaceImplementations.Single();

                case IEvent @event:
                    return @event.ExplicitInterfaceImplementations.Single();

                default:
                    throw new AssertionFailedException();
            }
        }
    }
}