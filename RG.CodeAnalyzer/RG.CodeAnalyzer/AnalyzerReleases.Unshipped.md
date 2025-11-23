; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
RG0001 | Performance | Info | Do not await inside a loop
RG0002 | Reliability | Warning | Do not return Task from a method that disposes object
RG0003 | Security | Error | Identifiers declared in Internal namespace must be internal
RG0004 | Code Quality | Warning | Do not access private fields of another object directly
RG0005 | Reliability | Warning | Do not call Dispose() on static readonly fields
RG0006 | Performance | Warning | Do not call Task.Wait() to invoke a Task
RG0007 | Performance | Warning | Do not access Task<>.Result to invoke a Task
RG0008 | Code Style | Warning | Tuple element names must be in Pascal case
RG0009 | Performance | Warning | Not using overload with CancellationToken
RG0010 | Code Quality | Warning | Inferred type is obsolete
RG0011 | Code Quality | Warning | Interfaces shouldn't derive from IDisposable
RG0012 | Maintainability | Warning | Task is unresolved
RG0013 | Code Quality | Warning | 'with' shouldn't be used outside its record declaration
RG0014 | Reliability | Warning | Do not parse using Convert
RG0015 | Code Quality | Warning | Records should not contain set accessor
RG0016 | Code Quality | Warning | Records should not contain mutable field
RG0017 | Code Quality | Warning | Records should not contain mutable collection
RG0018 | Code Quality | Warning | Records should not contain reference to class or struct type
RG0019 | Code Quality | Warning | Required record property should be initialized
RG0021 | Code Quality | Warning | Local variable is readonly
RG0022 | Code Quality | Warning | Parameter is readonly
RG0023 | Usage | Warning | Ref or out parameter cannot be readonly
RG0024 | Reliability | Warning | In argument should be readonly
RG0025 | Reliability | Warning | Casting to an incompatible enum
RG0026 | Reliability | Info | Possibly casting to an incompatible enum, some names have different value
RG0027 | Reliability | Info | Possibly casting to an incompatible enum, some values have different name
RG0028 | Usage | Warning | Required protobuf property should be initialized
RG0029 | Usage | Error | Can only initialize one of properties in a OneOf case
RG0030 | Convention | Error | Argument must be locked
RG0031 | Code Quality | Error | Do not use dynamic type
RG0032 | Naming | Warning | Static classes containing extension methods should have an 'Extensions' suffix
RG0033 | Performance | Warning | Use overload without CancellationToken if default or CancellationToken.None was supplied
RG0034 | Convention | Error | Service registered with Add{Lifetime} must have corresponding lifetime attribute
RG0035 | Reliability | Error | Service with longer lifetime cannot depend on service with shorter lifetime
RG0036 | Code Quality | Warning | DI implementation classes should be internal
RG0037 | Usage | Error | Usage is restricted to a specific namespace
RG0038 | Code Quality | Warning | Pending justification for suppressing code analysis message
RG0039 | Code Quality | Error | Nullable reference type is not enabled
RG0040 | Reliability | Error | Override method must call base method
RG0041 | Code Quality | Warning | Invalid use of [NeverAsync] attribute
RG0042 | Code Style | Info | Refactor expression-bodied property to auto-property with initializer
RG0043 | Maintainability | Info | File should not contain more than 1000 lines
