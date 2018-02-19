using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TLKMODELS
{
    public class TLKENTRY
    {
        public short Type { get; set; }
        public ulong ResourceName { get; set; }
        public int Volume { get; set; }
        public int Pitch { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }

        public TLKENTRY()
        {

        }
    }
}
