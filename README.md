# cs-lox
A C# implementation of a Lox interpreter (adapted from jlox, credit to "Crafting Interpreters" ([book](https://craftinginterpreters.com/), [repo](https://github.com/munificent/craftinginterpreters)) by Robert Nystrom)


# Chapters Complete:
- Scanning
- Representing Code
- Parsing Expressions
- Evaluating Expressions
- Statements and State

# Implementation Notes:

## Statements and State

- jlox uses the Void type for its Statement Visitor:
 
 ```java
 class Interpreter implements Expr.Visitor<Object>,
                             Stmt.Visitor<Void> {

  void interpret(Expr expression) { 
 ```
 C#, however, [does not allow this](https://github.com/dotnet/csharplang/discussions/696), so I instead opted to use a Unit return type (I used a handy one I saw in [UniRx](https://github.com/neuecc/UniRx)).
