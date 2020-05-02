# Interface Generator

This is an toy project that uses [C# Source Generators](https://devblogs.microsoft.com/dotnet/introducing-c-source-generators/) to automatically generate the interface for a class. For example:

```csharp
class Program
{
    static void Main(string[] args)
    {
        IFoo x = new Foo();

        x.Test("test");
    }
}

public class Foo : IFoo
{
    public void Test(string name)
    {
        Console.WriteLine($"Hello {name}");
    }
}
```

The generator will automatically create `IFoo`:

```csharp
public interface IFoo
{
    void Bar(string test);
}
```