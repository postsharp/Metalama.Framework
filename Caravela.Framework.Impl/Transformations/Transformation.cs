using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Caravela.Framework.Impl.Transformations
{
    internal abstract class Transformation
    {
        public IAdvice Advice { get; }

        public INamedType ContainingType
        {
            get { throw new System.NotImplementedException(); }
        }

        /// <summary>
        /// Gets the syntax tree in which it is optimal to perform the transformation, or <c>null</c>
        /// if a new syntax tree must be created.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public SyntaxTree SyntaxTree => throw new System.NotImplementedException();

        public MemberDeclarationSyntax InsertPositionNode => throw new NotImplementedException();

        public InsertPosition InsertPosition => throw new NotImplementedException();

        public MemberDeclarationSyntax GeneratePreLinkerCode() => throw new NotImplementedException();

        public Transformation( IAdvice advice )
        {
            this.Advice = advice;
        }
    }

    enum InsertPosition
    {
        AfterSibling,
        End
        
    }

    internal class IntroducedManagedResource : Transformation
    {
        public IntroducedManagedResource(IAdvice advice) : base(advice)
        {
            // TODO
        }
        
        
        public ResourceDescription ToResourceDescription()
        {
            throw new System.NotImplementedException();
        }
    }
}
