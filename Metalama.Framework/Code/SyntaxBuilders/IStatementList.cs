// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Code.SyntaxBuilders;

/// <summary>
/// Represents a list of statements. This list cannot be enumerated because it is lazily evaluated. To create an
/// <see cref="IStatementList"/>, use <see cref="StatementFactory.List(Metalama.Framework.Code.SyntaxBuilders.IStatement[])"/>,
///  <see cref="StatementFactory.UnwrapBlock"/>, or <see cref="StatementExtensions.AsList(Metalama.Framework.Code.SyntaxBuilders.IStatement)"/>
/// has been added for convenience.
/// </summary>
public interface IStatementList;