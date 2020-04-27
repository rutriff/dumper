using System;

namespace Dumper
{
    class Program
    {
        static void Main(string[] args)
        {
            var dumper = new Serializer();

            var target = new Fixture
            {
                Today = DateTime.Now,
                Some = new[]
                {
                    "test",
                    "not a test",
                    "string"
                },
                Out = 123,
                IsIt = true
            };

            var code = dumper.Code(target);
            Console.WriteLine(code);
            Console.ReadLine();
        }
    }
}

    
