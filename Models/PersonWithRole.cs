using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sobs.Models
{
    public class PersonWithRole
    {
        public string Name { get; set; } = "";
        public string Role { get; set; } = "";

        public override string ToString()
        {
            return $"[{Role}] {Name}";
        }
    }
}