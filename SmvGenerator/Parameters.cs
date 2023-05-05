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
        public List<Dictionary<string, int>>? Markings { get; set; }


        private int[,]? _incidenceMatrix;

        public int[,]? IncidenceMatrix
        {
            get
            {
                // If the incidence matrix is not set, calculate it from the pre and post matrices
                if (_incidenceMatrix == null || _incidenceMatrix.Length == 0)
                {
                    IncidenceMatrix = new int[Nodes, Transitions];
                    for (int i = 0; i < Nodes; i++)
                    {
                        for (int j = 0; j < Transitions; j++)
                        {
                            IncidenceMatrix[i, j] = PostMatrix[i, j] - PreMatrix[i, j];
                        }
                    }
                }
                return _incidenceMatrix;

            }
            set { _incidenceMatrix = value; }
        }

    }
}
