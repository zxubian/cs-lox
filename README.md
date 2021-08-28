# cs-lox
A C# implementation of a Lox interpreter (adapted from jlox, credit to "Crafting Interpreters" ([book](https://craftinginterpreters.com/), [repo](https://github.com/munificent/craftinginterpreters)) by Robert Nystrom)


# Chapters Complete:
- Scanning
- Representing Code
- Parsing Expressions
- Evaluating Expressions
- Statements and State
- Functions
- Resolving and Binding
- Classes

# Language Tweaks

- Language supports nill equality comparison
 ```javascript
 var a = nil;
 var b = nil;
 print a == b;
 >> True
 var c = "not nil";
 print a == c;
 >> False
 ```
 - Static methods are supported:
  ```javascript
class Greeter{
    class SayHello(){
        print "Hello, I am a static method."
    }
}
Greeter.SayHello();
>> Hello, I am a static method.
var a = Greeter();
a.SayHello();
>> Hello, I am a static method.
 ```

 - "Getter" propeties are supported:
  ```javascript
class Person{
    init(name, surname){
        this.name = name;
        this.surname = surname;
    }
    fullname{
        return name + " " + surname;
    }
}

var john = Person("John", "Smith");
print john.fullname;
>> John Smith
 ```
 Furthermore, a compile-error is thrown if the getter never returns a value.
 
 - Other features: ternary conditional (a ? b : c), comma operator support, breaking out of loops, C-style block comments ( ```/*...*/ ```)

 
# Implementation Notes:

## Statements and State

- jlox uses the Void type for its Statement Visitor:
 
 ```java
 class Interpreter implements Expr.Visitor<Object>,
                             Stmt.Visitor<Void> {

  void interpret(Expr expression) { 
 ```
 C#, however, [does not allow this](https://github.com/dotnet/csharplang/discussions/696), so I instead opted to use a Unit return type (I used a handy one I saw in [UniRx](https://github.com/neuecc/UniRx)).
 
 ## Functions
 
 - jlox uses an anonymous object to implement the native 'clock' function. However, C# [does not allow anonymous types to implement interfaces](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/types/anonymous-types), so I manually created a separate Clock type.
