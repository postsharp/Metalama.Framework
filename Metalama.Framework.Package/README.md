The `Metalama.Framework` package is the principal package of Metalama. Reference this package if you want to use Metalama in your code.

Referencing this package replaces the compiler with `Metalama.Compiler`, a shallow fork of Roslyn that adds an extension point to transform the syntax trees.

## What is Metalama?

Metalama is a modern, Roslyn-based meta-programming framework C# with focus on aspect-oriented programmning, code generation and code validation.

It is an extension of the Microsoft "Roslyn" C# compiler that allows you to automatically transform your source code at build time
or design time based on encapsulated code transformations called _aspects_. Metalama can be used for aspect-oriented programming,
code generation, or architecture validation.

Metalama is intended to replace the MSIL-based stack that is now the foundation of PostSharp.

## Documentation and examples

| Link                                                              | Description |
|-------------------------------------------------------------------|------------------------
| [Documentation](https://doc.metalama.net) | Conceptual and API documentation.
| [Try Metalama](https://try.metalama.net) | Try Metalama from your browser. Based on https://try.dot.net/ |
| [Metalama.Samples](https://github.com/postsharp/Metalama.Samples) | A dozen of examples in a GitHub repo. |
| [Metalama.Open.AutoCancellationToken](https://github.com/postsharp/Metalama.Open.AutoCancellationToken) | A low-level Metalama aspect that adds cancellation tokens to your method declarations and your method calls.
| [Metalama.Open.Virtuosity](https://github.com/postsharp/Metalama.Open.Virtuosity) | A low-level Metalama aspect that makes your methods virtual. (A fork of Virtuosity.Fody.)
| [Metalama.Open.Costura](https://github.com/postsharp/Metalama.Open.Costura) | A low-level Metalama aspect that embeds dependent assemblies as managed resources. (A fork of Costura.Fody.)
| [Metalama.Framework.Extensions](https://github.com/postsharp/Metalama.Framework.Extensions) | Open-source extensions to Metalama.Framework.

## Metalama components

For a diagram of the multiple packages that compose Metalama, see https://doc.metalama.net/deployment/packages.

## Feedback and support

If you have any feedback regarding Metalama, please [open an issue](https://github.com/postsharp/Metalama/issues/new),
 [start a discussion](https://github.com/postsharp/Metalama/discussions/new), or contact us directly at hello@postsharp.net.

 You can also join our Slack community and ask your technical questions in real time.

 [![Slack](https://img.shields.io/badge/Slack-4A154B?label=Chat%20with%20us%20on&style=flat&logo=slack&logoColor=white)](https://www.postsharp.net/slack)

