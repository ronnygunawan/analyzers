# RG.CodeAnalyzer

[![NuGet](https://img.shields.io/nuget/v/RG.CodeAnalyzer.svg)](https://www.nuget.org/packages/RG.CodeAnalyzer/) [![.NET](https://github.com/ronnygunawan/analyzers/actions/workflows/dotnet.yml/badge.svg)](https://github.com/ronnygunawan/analyzers/actions/workflows/dotnet.yml)

## Installation
From Package Manager Console:
```
Install-Package RG.CodeAnalyzer
```

Best used together with [Microsoft.CodeAnalysis.NetAnalyzers](https://www.nuget.org/packages/Microsoft.CodeAnalysis.NetAnalyzers/) and [Roslynator](https://www.nuget.org/packages/Roslynator.Analyzers/)

### Q: There is nothing wrong with my code. How do I get rid of the warnings?
You can either:
1. Set the diagnostic severity to None, or
2. Suppress the warning and write your justification

### Q: I think one of the warnings is important and I don't want my developers to get away with it. How do I do it?
You can set the diagnostic severity to Error. This will prevent build from succeeding.

To learn more about changing diagnostic severity and suppressing warnings, visit: https://docs.microsoft.com/en-us/visualstudio/code-quality/use-roslyn-analyzers?view=vs-2019

## Analyzers:
### 1. Do not `await` asynchronous operation inside a loop
```cs
for (;;) {
    await Task.Delay(1000); // RG0001: Asynchronous operation awaited inside for loop.
}

foreach (var x in y) {
    await Task.Delay(1000); // RG0001: Asynchronous operation awaited inside foreach loop.
}

while (true) {
    await Task.Delay(1000); // RG0001: Asynchronous operation awaited inside while loop.
}

do {
    await Task.Delay(1000); // RG0001: Asynchronous operation awaited inside do..while loop.
} while (true);
```

### 2. Do not return `Task` from a method that disposes object
```cs
Task Foo() { // RG0002: Method 'Foo' disposes an object and shouldn't return Task.
    using var cts = new CancellationTokenSource();
    return Task.Delay(1000, cts.Token);
}
```

### 3. Identifiers declared in Internal namespace must be internal.
```cs
namespace Foo.Internal.Models {
    public class Bar { // RG0003: Identifier 'Bar' is declared in 'Foo.Internal.Models' namespace, and thus must be declared internal.
    }
}
```

### 4. Do not access `private` fields using member access operator
```cs
class Foo {
    private int x;

    public void SetX(Foo obj, int value) {
        obj.x = value; // RG0004: Private field 'x' should not be accessed directly.
    }
}
```

### 5. Do not call `Dispose()` on static readonly fields
```cs
class Foo : IDisposable {
    private static readonly HttpClient _client = new HttpClient();_

    public void Dispose() {
        _client.Dispose(); // RG0005: Field '_client' is marked 'static readonly' and should not be disposed.
    }
}
```

### 6. Do not call `Task.Wait()` to invoke a `Task`
```cs
class Program {
    static void Main() {
        Foo.ListenAsync().Wait(); // RG0006: Calling Task.Wait() blocks current thread and is not recommended. Use await instead.
    }
}
```

### 7. Do not access Task<>.Result to invoke a Task
```cs
class Foo {
    private async Task<int> GetValueAsync() {
        return await Task.FromResult(0);
    }

    public int GetValue() {
        return GetValueAsync().Result; // RG0007: Accessing Task<>.Result blocks current thread and is not recommended. Use await instead.
    }
}
```

### 8. Tuple element names must be in PascalCase
```cs
(int firstItem, string secondItem) tuple = (1, "2"); // RG0008: 'firstItem' is not a proper name of a tuple element.
                                                     // RG0008: 'secondItem' is not a proper name of a tuple element.
(int a, string b) = (tuple.firstItem, tuple.secondItem);
```

### 9. Use overload which accepts `CancellationToken` whenever possible
```cs
public async Task<List<Product>> GetAllAsync(int id, CancellationToken cancellationToken) {
    return await _dbContext.Products.ToListAsync(); // RG0009: This method has an overload that accepts CancellationToken.
}
```

### 10. `var`'s inferred type is obsolete
```cs
var obj = GetObjOfObsoleteClass(); // RG0010: 'ObsoleteClass' is obsolete.
```

### 11. Interfaces shouldn't derive from IDisposable
```cs
interface I : IDisposable { } // RG0011: 'I' derives from IDisposable.
```

### 12. Task is unresolved
```cs
// TODO: implement this method please, Bob
// RG0012: Unresolved TODO: implement this method please, Bob
public int Foo() {
    // HACK: throw this exception until implemented
    // RG0012: Unresolved HACK: throw this exception until implemented
    throw new NotImplementedException();
}
```

### 13. `with` shouldn't be used outside its record declaration
```cs
record Foo(int X);

Foo f = new(0);
f = f with { X = 1 }; // RG0013: 'with' used outside 'MyApp.Foo'
```
[Suggested best practice](https://github.com/ronnygunawan/analyzers/issues/33)

### 14. Do not parse using `Convert`
```cs
int i = Convert.ToInt32("100"); // RG0014: Parsing 'int' using 'Convert.ToInt32'
                                // Code fix: Change to 'int.Parse'
```

### 15. Records should not contain `set` accessor
```cs
record Foo {
    public int X { get; init; }
    public int Y { get; set; } // RG0015: 'Y' should not have set accessor because it's declared in a record
}
```

---
**MUTABLE RECORDS**

[![NuGet](https://img.shields.io/nuget/v/RG.Annotations.svg)](https://www.nuget.org/packages/RG.Annotations/)

You can annotate records with `[Mutable]` attribute from [RG.Annotations](https://www.nuget.org/packages/RG.Annotations/) to skip all immutability checks (RG0015 to RG0020).

---

### 16. Records should not contain mutable field
```cs
record Foo {
    public readonly int A, B;
    public const int C;
    public int D; // RG0016: 'D' should not be mutable because it's declared in a record
}
```

### 17. Records should not contain mutable collection
```cs
record Foo(
    ImmutableArray<int> A,
    int[] B // RG0017: 'B' is a mutable collection and should not be used in a record
);
```

### 18. Records should not contain reference to `class` or `struct` type
```cs
class C { }
struct S { }
record R { }

record Foo(
    C C, // RG0018: 'C' is class type and should not be used in a record
    S S, // RG0018: 'S' is struct type and should not be used in a record
    R R
);
```

#### Banned types
- `class` (See "Allowed classes")
- `struct` (See "Allowed structs")
- `interface`
- `dynamic`
- `object`
- `Tuple`

#### Allowed types
- `record`
- `enum`
- `delegate`

#### Allowed `class`es
- `System.Uri`
- `System.Type`
- `System.Reflection.Module`
- `System.Reflection.Assembly`
- `System.Reflection.TypeInfo`
- `System.Reflection.MethodInfo`
- `System.Reflection.PropertyInfo`
- `System.Reflection.FieldInfo`
- `System.Reflection.ConstructorInfo`
- `System.Reflection.ParameterInfo`
- `System.Reflection.EventInfo`
- `System.Reflection.LocalVariableInfo`
- `System.Reflection.MemberInfo`
- `System.Reflection.ManifestResourceInfo`
- `System.Reflection.MethodBase`
- `System.Reflection.MethodBody`
- `System.Net.IPAddress`

#### Allowed `struct`s
- Primitive value types
- `struct`s in `System` namespace
- `struct`s in [`UnitsNet`](https://www.nuget.org/packages/UnitsNet/) namespace (3rd party library)

### 19. Required record property should be initialized
```cs
record Foo {
    public int X { get; init; }
    public int Y { get; init; }

    [Required]
    public int Z { get; init; }
}

Foo foo = new() { // RG0019: 'Z' is a required property and should be initialized
    X = 0
};
```

#### Shorthand for `[Required]` attribute
You can also put an `@` prefix to record property name to mark it as a required property
```cs
record Foo {
    public int X { get; init; }
    public int Y { get; init; }
    public int @Z { get; init; }
}

Foo foo = new() { // RG0019: 'Z' is a required property and should be initialized
    X = 0
};
```

### 20. (Reserved for [#43](/../../issues/43))

### 21. Local is readonly
Put an `@` prefix to local name to mark it as a readonly local
```cs
int @max = 100;
max = 50; // RG0021: 'max' is a readonly local variable

ref int @refLocal = ref someVar; // OK if someVar is readonly
ref int @refLocal2 = ref mutableVar; // RG0021: cannot assign mutable source to readonly ref local

ref readonly int @refReadonlyLocal = ref someVar; // OK, ref readonly can reference any non-ref-parameter
```

---
**NOTE**

This feature now supports `ref` locals and `ref readonly` locals. Ref reassignments (e.g., `ref x = ref y;` where x is already declared) are not yet fully implemented.

---

### 22. Parameter is readonly
```cs
void Foo(int @x) {
    x = 0; // RG0022: 'x' is a readonly parameter
}
```

### 23. Ref or out parameter cannot be readonly
```cs
void Foo(out int @x) { // RG0023: 'out' parameter 'x' cannot be readonly
}
```

### 24. In argument should be readonly
```cs
void Foo(in int value) {
    // ...
}

int mutableVar = 0;
Foo(in mutableVar); // RG0024: 'in' argument 'mutableVar' should be readonly

int @readonlyVar = 0;
Foo(in readonlyVar); // OK
```

### 25. Casting to an incompatible enum
```cs
enum X { A, B, C }
enum Y { A, B, C, D }

X x = (X)Y.A; // RG0025: Casting to an incompatible enum; Value 3 is missing from 'X'
Y y = (Y)X.A; // no warning
```

### 26. Possibly casting to an incompatible enum: some names have different value
```cs
enum X { A, B, C }
enum Y { A, C, B }

X x = (X)Y.A; // Info: RG0026: Possibly casting to an incompatible enum; 'B' doesn't have the same value in 'X' and in 'Y'
```

### 27. Possibly casting to an incompatible enum: some values have different name
```cs
enum X { A, BB, C }
enum Y { A, B, C }

X x = (X)Y.A; // Info: RG0027: Possibly casting to an incompatible enum; Value 1 doesn't have a same name in 'X' and in 'Y'
```

### 28. All properties in protobuf message need to be initialized
```protobuf
// .proto
message X {
  int32 foo = 1;
  string bar = 2;
}
```

```cs
// .cs
X x = new() { // RG0028: 'Bar' is a required protobuf property and should be initialized
    Foo = 1
};
```

### 29. Only one of properties in a Oneof case can be initialized
```protobuf
// .proto
message X {
  oneof which {
    int32 foo = 1;
    string bar = 2;
  }
}
```

```cs
// .cs
X x = new() {
    Foo = 1,
    Bar = "Hello" // RG0029: 'Bar' cannot be initialized because 'Foo' has been initialized
};
```

### 30. Argument must be locked
Methods can require that certain arguments be locked before calling the method by using the `[MustBeLocked]` attribute from the `RG.Annotations` package.

```cs
using RG.Annotations;

class MyClass {
    private object _resource = new();
    
    void ProcessResource([MustBeLocked] object resource) {
        // Process the resource safely
    }
    
    void Example() {
        ProcessResource(_resource); // RG0030: Argument must be locked before calling this method
    }
}
```

Correct usage:
```cs
using RG.Annotations;

class MyClass {
    private object _resource = new();
    
    void ProcessResource([MustBeLocked] object resource) {
        // Process the resource safely
    }
    
    void Example() {
        lock (_resource) {
            ProcessResource(_resource); // OK - resource is locked
        }
    }
}
```

This analyzer helps enforce thread-safety requirements by ensuring that objects marked with `[MustBeLocked]` are properly locked before use.

### 31. Do not use `dynamic` type
```cs
dynamic x = 1; // RG0031: Do not use dynamic type
```

### 32. Static classes containing extension methods should have an 'Extensions' suffix
```cs
static class Utilities { // RG0032: 'Utilities' contains extension methods and should have an 'Extensions' suffix
    public static void Foo(this string bar) {
    }
}
```
Code fix: Rename to `UtilitiesExtensions`

### 33. Use overload without CancellationToken if default or CancellationToken.None was supplied
```cs
var list = await _dbContext.Items.ToListAsync(CancellationToken.None); // RG0033: Use overload without CancellationToken instead
```
If you explicitly pass `CancellationToken.None` or `default` to a method that has an overload without the CancellationToken parameter, use the overload without it instead.

Code fix: Remove the CancellationToken argument

### 34. Service registered with Add{Lifetime} must have corresponding lifetime attribute
Services registered with dependency injection must be marked with the appropriate lifetime attribute.

```cs
using Microsoft.Extensions.DependencyInjection;

namespace MyApp {
    class MyService { // RG0034: Service 'MyService' registered with AddSingleton must be marked with [Singleton] attribute
    }
    
    class Startup {
        void ConfigureServices(IServiceCollection services) {
            services.AddSingleton<MyService>();
        }
    }
}
```

Correct usage:
```cs
using Microsoft.Extensions.DependencyInjection;
using RG.Annotations;

namespace MyApp {
    [Singleton]
    class MyService {
    }
    
    class Startup {
        void ConfigureServices(IServiceCollection services) {
            services.AddSingleton<MyService>();
        }
    }
}
```

### 35. Service with longer lifetime cannot depend on service with shorter lifetime
A service with a longer lifetime (Singleton > Scoped > Transient) cannot depend on a service with a shorter lifetime.

```cs
using RG.Annotations;

namespace MyApp {
    [Transient]
    class TransientService {
    }
    
    [Singleton]
    class SingletonService {
        // RG0035: Singleton service 'SingletonService' cannot depend on Transient service 'TransientService'
        public SingletonService(TransientService service) {
        }
    }
}
```

This rule also applies to property and field dependencies.

### 36. DI implementation classes should be internal
Classes registered with dependency injection should be internal to hide implementation details. The interface can remain public.

```cs
using Microsoft.Extensions.DependencyInjection;
using RG.Annotations;

namespace MyApp {
    public interface IMyService {
    }
    
    [Transient]
    public class MyService : IMyService { // RG0036: Class 'MyService' is registered with DI and should be internal
    }
    
    class Startup {
        void ConfigureServices(IServiceCollection services) {
            services.AddTransient<IMyService, MyService>();
        }
    }
}
```

Correct usage:
```cs
using Microsoft.Extensions.DependencyInjection;
using RG.Annotations;

namespace MyApp {
    public interface IMyService {
    }
    
    [Transient]
    internal class MyService : IMyService { // OK
    }
    
    class Startup {
        void ConfigureServices(IServiceCollection services) {
            services.AddTransient<IMyService, MyService>();
        }
    }
}
```

Code fix: Make the implementation class internal

### 37. Usage is restricted to a specific namespace
Classes, structs, or other symbols marked with the `[RestrictTo]` attribute can only be used within the specified namespace.

```cs
using RG.Annotations;

namespace Baz {
    [RestrictTo("Baz")]
    public class Foo { }
}

namespace Bar {
    class C : Foo { } // RG0037: Usage of 'Foo' is only allowed in namespace 'Baz'
}
```

Correct usage:
```cs
using RG.Annotations;

namespace Baz {
    [RestrictTo("Baz")]
    public class Foo { }
    
    class C : Foo { } // OK - within allowed namespace
}

namespace Baz.Sub {
    class D : Foo { } // OK - within sub-namespace
}
```

This analyzer helps enforce architectural boundaries by restricting where certain types can be used. Sub-namespaces of the restricted namespace are allowed.

### 38. Pending justification for suppressing code analysis message
```cs
using System.Diagnostics.CodeAnalysis;

class MyClass {
    [SuppressMessage("Category", "RG0001")] // RG0038: Justification is required for suppressing message 'RG0001'
    [SuppressMessage("Category", "RG0002", Justification = "")] // RG0038: Justification is required for suppressing message 'RG0002'
    [SuppressMessage("Category", "RG0003", Justification = "<Pending>")] // RG0038: Justification is required for suppressing message 'RG0003'
    public void MyMethod() {
    }
}
```

When using `[SuppressMessage]` attribute to suppress code analysis warnings, you must always provide the `Justification` parameter with a meaningful, non-empty explanation of why the warning is being suppressed. This ensures code suppressions are documented and justified. The analyzer detects missing justifications, empty justifications, and the default `"<Pending>"` placeholder generated by code fixes.

Valid usage:
```cs
using System.Diagnostics.CodeAnalysis;

class MyClass {
    [SuppressMessage("Category", "RG0001", Justification = "Performance-critical path, async not needed")]
    public void MyMethod() {
    }
}
```

### 39. Nullable reference type is not enabled
```cs
// RG0039: Nullable reference type static analysis is not enabled
namespace ConsoleApplication1
{
    public class TypeName
    {
        public string Name { get; set; }
    }
}
```

This analyzer produces an error when nullable reference type static analysis is not enabled in a C# project. Nullable reference types help prevent null reference exceptions by making the type system express whether a reference can be null or not.

To enable nullable reference types, add one of the following to your project file (`.csproj`):
```xml
<PropertyGroup>
    <Nullable>enable</Nullable>
</PropertyGroup>
```

Or use the `#nullable enable` directive at the top of your source files:
```cs
#nullable enable
namespace ConsoleApplication1
{
    public class TypeName
    {
        public string? Name { get; set; } // ? indicates nullable
    }
}
```

**Note:** If you have legacy code and don't want to enable nullable reference types project-wide, you can disable this analyzer by setting its severity to `None` in your `.editorconfig` or project configuration.

### 40. Override method must call base method

```cs
class A {
    [MustCallBase]
    protected virtual void Foo() {
    }
}

class B : A {
    protected override void Foo() { // Error: must call base method
    }
}
```

This analyzer ensures that when a virtual or abstract method is marked with the `[MustCallBase]` attribute from the `RG.Annotations` package, all overriding methods must call the base implementation. This is similar to Kotlin's `@CallSuper` annotation.

The analyzer checks the entire inheritance chain, so if a method in any ancestor class has `[MustCallBase]`, all descendants must call their immediate base implementation.

#### Correct Usage:

```cs
class A {
    [MustCallBase]
    protected virtual void Foo() {
        // Base implementation
    }
}

class B : A {
    protected override void Foo() {
        base.Foo(); // Correct: calls base implementation
        // Additional logic
    }
}

class C : B {
    protected override void Foo() {
        base.Foo(); // Also required: the attribute is inherited through the chain
        // Additional logic
    }
}
```

The base call can be placed anywhere in the method body - before, after, or in the middle of the override's logic.

### 41. Invalid use of [NeverAsync] attribute
The `[NeverAsync]` attribute from the `RG.Annotations` package indicates that a method returns a `Task` but never executes asynchronously. When a method is marked with this attribute, the RG0006 (Task.Wait) and RG0007 (Task.Result) warnings are suppressed when calling that method.

```cs
using System.Threading.Tasks;
using RG.Annotations;

class MyClass {
    [NeverAsync]
    private Task<int> Foo() => Task.FromResult(10); // OK - never actually async
    
    public void Bar() {
        int x = Foo().Result; // OK - RG0007 suppressed because Foo is marked [NeverAsync]
    }
}
```

The analyzer produces RG0041 warnings if the `[NeverAsync]` attribute is misused (e.g., on methods that don't return `Task`, or on `async` methods).

**Note:** Use this attribute sparingly and only when you're certain the method never executes asynchronously. Misuse can lead to confusion and maintenance issues.

### 42. Refactor expression-bodied property to auto-property with initializer

```cs
private const int SixSeven = 67;
private static readonly TimeSpan OneHour = TimeSpan.FromHours(1);

public int MaxSize => SixSeven; // RG0042: Property 'MaxSize' can be refactored to auto-property with initializer
public TimeSpan TTL => OneHour; // RG0042: Property 'TTL' can be refactored to auto-property with initializer
```

This analyzer detects expression-bodied properties that return constant or static readonly field values and suggests refactoring them to auto-properties with initializers for better performance and clarity.

Code fix: Refactor to auto-property with initializer

```cs
private const int SixSeven = 67;
private static readonly TimeSpan OneHour = TimeSpan.FromHours(1);

public int MaxSize { get; } = SixSeven; // Refactored
public TimeSpan TTL { get; } = OneHour; // Refactored
```

The refactoring improves performance because auto-properties with initializers are assigned once during object construction, whereas expression-bodied properties are evaluated every time the property is accessed.

## Code Refactorings

### Generate GUID in empty string literal
Place your cursor on an empty string literal `""` and invoke code actions (Ctrl+. or Cmd+.) to see the "Generate GUID" refactoring option.

```cs
string id = ""; // Offers: Generate GUID
// After applying: string id = "3fa85f64-5717-4562-b3fc-2c963f66afa6";
```

This refactoring is useful when you need to quickly generate unique identifiers in your code.

### Add missing named arguments to method call or constructor
Place your cursor on a method call or object creation expression and invoke code actions (Ctrl+. or Cmd+.) to see the "Add missing named arguments" refactoring option.

Before code fix:
```cs
var person = new Person(1, "John", "Doe");
```

After code fix:
```cs
var person = new Person(
	id: _,
	firstName: _,
	lastName: _
);
```

This also works for method invocations:
```cs
DoSomething(1, "test");
// After applying:
DoSomething(
	id: _,
	name: _
);
```

**Note:** When multiple matching overloads are found, multiple code fixes will be offered, each showing the parameter names in the title to help you choose the correct overload.
