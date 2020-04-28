using System;
using System.Collections.Generic;

namespace Dumper
{
    internal class Fixture
    {
        public int DefaultInt { get; set; }
        public bool DefaultBool { get; set; }
        public DateTime Today { get; set; }
        public string[] Some { get; set; }
        public int Out { get; set; }
        public bool IsIt { get; set; }
        public List<Fixture> Internal { get; set; }
        public decimal Decimal { get; set; }
        public TimeSpan Time { get; set; }
        public DateTimeKind Enum { get; set; }
        public Guid Id { get; set; }
    }
}