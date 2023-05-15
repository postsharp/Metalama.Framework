## Metalama: a Framework for Clean & Concise Code in C#


Metalama is a modern Roslyn-based meta-programming framework to improve your code quality and productivity in C#.

Using Metalama, you can:

* **Reduce boilerplate** by generating it dynamically during compilation. Your source code remains crystal-clear.
* **Verify code** in real time against architecture, patterns, and conventions. No need to wait for code reviews.
* **Provide coding assistance** to your team with customized feedback and suggestions.
* **Do it by your rules.** Automate your own implementation patterns and architecture guidelines.

For more information, see the [Metalama website](https://www.postsharp.net/metalama) or [Metalama documentation](https://doc.metalama.net).

## About this package

The `Metalama.Framework` package is the principal package of Metalama. Reference this package if you want to use Metalama in your code.

Referencing this package replaces the compiler with `Metalama.Compiler`, a shallow fork of Roslyn that adds an extension point to allow us to transform the syntax trees. If you need to reference Metalama without replacing the compiler, use the `Metalama.Framework.Redist` package.

## Change log

We maintain a detailed change log on [GitHub](https://github.com/postsharp/Metalama/discussions/categories/announcements).

## Feedback and support

If you have any feedback regarding Metalama, please [open an issue](https://github.com/postsharp/Metalama/issues/new),
 [start a discussion](https://github.com/postsharp/Metalama/discussions/new) on GitHub, or contact us directly at hello@postsharp.net.

 You can also join our Slack community and ask your technical questions in real time.

 [![Slack](https://img.shields.io/badge/Slack-4A154B?label=Chat%20with%20us%20on&style=flat&logo=slack&logoColor=white)](https://www.postsharp.net/slack)

