using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmvGenerator
{
    public class Parameters
    {
        public int Nodes { get; set; }
        public int Transitions { get; set; }
        public int[]? InitialMarking { get; set; }
        public int[,]? PreMatrix { get; set; }
        public int[,]? PostMatrix { get; set; }
        public List<int[]>? Markings { get; set; }
    }
}
