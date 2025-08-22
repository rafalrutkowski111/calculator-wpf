using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator.Data.Entities
{
    public class Calculation
    {
        public int Id { get; set; }
        public string Expression { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
