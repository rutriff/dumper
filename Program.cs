using System;
using System.Collections.Generic;

namespace Dumper
{
    class Program
    {
        static void Main(string[] args)
        {
            var dumper = new Serializer();

            var target = new Fixture
            {
                Id = Guid.NewGuid(),
                Today = DateTime.Now,
                Some = new[]
                {
                    "test",
                    "not a test",
                    "string"
                },
                Time = DateTime.Now.TimeOfDay,
                Internal = new List<Fixture>
                {
                    new Fixture
                    {
                        Id = Guid.NewGuid(),
                        Today = DateTime.Now.Add(TimeSpan.Parse("12:32:11")),
                        IsIt = true,
                        Some = new []
                        {
                            "zxc",
                            "123"
                        },
                        Decimal = 3123.463M
                    }
                },
                Enum = System.DateTimeKind.Utc,
                Out = 123,
                IsIt = true
            };

            var code = dumper.Code(target);
            Console.WriteLine(code);
        }
        
        
    }
}

    
