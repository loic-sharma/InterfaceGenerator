using System;

namespace Test
{
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
}
