using Microsoft.Msagl.Core.DataStructures;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;

namespace SmvGenerator
{
    public partial class MainWindow : Window
    {
        public Parameters Parameters { get; set; }
        public List<int[]> MarkingsArray { get; set; }

        // main window constructor
        public MainWindow(Parameters parameters)
        {
            MarkingsArray = new List<int[]>();
            Parameters = parameters;

            InitializeComponent();

            Graph graph = new Graph("Petri Net");

            // adding nodes
            for (int i = 0; i < Parameters.Nodes; i++)
            {
                Node node = new Node("P" + i.ToString());
                node.Attr.Shape = Shape.Circle;
                graph.AddNode(node);
            }

            for (int i = 0; i < Parameters.Nodes; i++)
            {
                for (int j = 0; j < Parameters.Transitions; j++)
                {
                    // pre matrix
                    if (Parameters.PreMatrix?[i, j] != 0)
                    {
                        string sourceLabel = "T" + j.ToString();
                        string targetLabel = "P" + i.ToString();

                        // adding edges from transition to place
                        Edge edge = graph.AddEdge(targetLabel, sourceLabel);
                        edge.Attr.Color = Color.Black;

                        // if the edge is not 1 , add label
                        if (Parameters.PreMatrix?[i, j] != 1)
                        {
                            edge.LabelText = Parameters.PreMatrix?[i, j].ToString();
                        }
                    }

                    // post matrix
                    if (Parameters.PostMatrix?[i, j] != 0)
                    {
                        string sourceLabel = "P" + i.ToString();
                        string targetLabel = "T" + j.ToString();

                        // adding edges from place to transition
                        Edge edge = graph.AddEdge(targetLabel, sourceLabel);
                        edge.Attr.Color = Color.Red;

                        // if the edge is not 1 , add label
                        if (Parameters.PostMatrix?[i, j] != 1)
                        {
                            edge.LabelText = Parameters.PostMatrix?[i, j].ToString();
                        }
                    }
                }
            }

            // adding initial marking for nodes
            for (int i = 0; i < Parameters.InitialMarking?.Length; i++)
            {
                int marking = Parameters.InitialMarking[i];
                Node node = graph.FindNode("P" + i.ToString());
                node.LabelText = "P" + i.ToString() + " (" + marking + ")";
            }

            graph.Attr.LayerDirection = LayerDirection.TB;
            graph.Attr.OptimizeLabelPositions = true;
            graph.Attr.AspectRatio = 1.0f;
            graph.Attr.LayerSeparation = 100.0f;
            graph.Attr.NodeSeparation = 100.0f;
            GViewer viewer = new GViewer();
            viewer.ToolBarIsVisible = false;
            viewer.Graph = graph;

            WindowsFormsHost host = new WindowsFormsHost();
            host.Child = viewer;

            gMain.Children.Add(host);
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, int> places = new Dictionary<string, int>();
            List<Dictionary<string, int>> markings = new List<Dictionary<string, int>>();
            markings.Add(GetPlacesFromInitialMarking());

            List<Dictionary<string, int>[]> transitions = new List<Dictionary<string, int>[]>();

            places = GetPlacesFromInitialMarking();
            transitions = GetTransitionsFromMatrices();

            // Create reachability graph
            HashSet<string> visitedMarkings = new HashSet<string>();
            HashSet<string> visitedNodes = new HashSet<string>();
            Queue<Dictionary<string, int>> queue = new Queue<Dictionary<string, int>>();
            visitedMarkings.Add(MarkingToString(markings[0]));
            queue.Enqueue(markings[0]);

            Graph graph = new Graph("Reachability Graph");
            Node rootNode = graph.AddNode(MarkingToString(markings[0]));
            visitedNodes.Add(rootNode.Id);

            while (queue.Count > 0)
            {
                Dictionary<string, int> currentMarking = queue.Dequeue();
                Node currentNode = graph.FindNode(MarkingToString(currentMarking));
                foreach (Dictionary<string, int>[] transition in transitions)
                {
                    Dictionary<string, int> newMarking = ApplyTransition(currentMarking, transition);
                    string newMarkingString = MarkingToString(newMarking);
                    if (!visitedMarkings.Contains(newMarkingString))
                    {
                        visitedMarkings.Add(newMarkingString);
                        markings.Add(newMarking);
                        queue.Enqueue(newMarking);

                        Node newNode = graph.AddNode(newMarkingString);
                        visitedNodes.Add(newNode.Id);
                        Edge newEdge = graph.AddEdge(currentNode.Id, newNode.Id);
                        newEdge.LabelText = "T" + (transitions.IndexOf(transition));
                    }
                    else
                    {
                        Node existingNode = graph.FindNode(newMarkingString);
                        if (existingNode != null && existingNode != currentNode)
                        {

                            // The Petri net is bounded
                            // Add the edge to the graph
                            Edge existingEdge = null;
                            foreach (Edge edge in graph.Edges)
                            {
                                if (edge.Source == currentNode.Id && edge.Target == existingNode.Id)
                                {
                                    existingEdge = edge;
                                    break;
                                }
                            }
                            if (existingEdge != null)
                            {
                                existingEdge.LabelText += ",T" + (transitions.IndexOf(transition));
                            }
                            else
                            {
                                Edge newEdge = graph.AddEdge(currentNode.Id, existingNode.Id);
                                newEdge.LabelText = "T" + (transitions.IndexOf(transition));
                            }

                        }
                    }

                    // check if marking in any place has exceeded MAX_K
                    bool isBounded = true;
                    foreach (KeyValuePair<string, int> place in currentMarking)
                    {
                        if (place.Value > Parameters.K)
                        {
                            isBounded = false;
                            MessageBox.Show("The Petri net is not bounded. The marking of place " + place.Key + " has exceeded the maximum value of " + Parameters.K + ".");
                            return;
                        }
                    }
                }
            }

            MarkingsArray = ConvertMarkings(markings);

            // show graph
            GViewer viewer = new GViewer();
            viewer.ToolBarIsVisible = false;
            viewer.Graph = graph;

            WindowsFormsHost host = new WindowsFormsHost();
            host.Child = viewer;

            gMain.Children.Add(host);


            bCalculate.Visibility = Visibility.Collapsed;

            // create a list view to show the markings
            ListView listView = new ListView();
            GridView gridView = new GridView();

            // add columns to the list view
            for (int i = 0; i < Parameters.Nodes; i++)
            {
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "P" + i.ToString(),
                    DisplayMemberBinding = new Binding("[" + i.ToString() + "]")
                });
            }

            // add all of the markings to the list view
            foreach (Dictionary<string, int> marking in markings)
            {
                listView.Items.Add(marking.Values.ToArray());
            }


            listView.View = gridView;

            // add the list view to the grid to the 2nd column
            Grid.SetColumn(listView, 1);
            gMain.Children.Add(listView);

            Button button = new Button();
            button.VerticalAlignment = VerticalAlignment.Bottom;
            button.Margin = new Thickness(10);
            button.Padding = new Thickness(20, 10, 20, 10);
            button.FontSize = 20;
            button.FontWeight = FontWeights.Bold;
            button.Content = "Generate nuSMV";
            button.Click += Button_Click;

            // add the button to the grid to the 3rd column
            Grid.SetColumn(button, 1);
            gMain.Children.Add(button);


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string smvCode = GenerateSMV(Parameters);
            string fileName = "model.smv";

            // delete the file if it exists
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            File.WriteAllText(fileName, smvCode);

            // open the file
            Process.Start(new ProcessStartInfo(fileName) { UseShellExecute = true });

            CheckPropertyWindow checkPropertyWindow = new CheckPropertyWindow();
            checkPropertyWindow.Show();
        }

        // petri net to SMV algorithm
        private string GenerateSMV(Parameters parameters)
        {
            // 1: add MODULE main statement
            string smvCode = "MODULE main\n\n";

            // 2: add VAR keyword
            smvCode += "VAR\n";

            // 3: add states from the markings
            smvCode += "  s : {";
            for (int i = 0; i < MarkingsArray?.Count; i++)
            {
                smvCode += "s" + i.ToString();
                if (i < MarkingsArray.Count - 1)
                {
                    smvCode += ", ";
                }
            }
            smvCode += "};\n";

            // 6: add Boolean and bounded integer variables for each place
            for (int i = 0; i < parameters.Nodes; i++)
            {
                if (IsPlaceBoolean(i))
                {
                    smvCode += "  p" + i.ToString() + " : boolean;\n";
                }
                else
                {
                    smvCode += "  p" + i.ToString() + " : 0.." + MarkingsArray?.Max(marking => marking[i]) + ";\n";
                }
            }

            // 13: add ASSIGN keyword
            smvCode += "\nASSIGN\n";

            // 14: init s variable
            smvCode += "  init(s) := s0;\n";

            // 15: open transition relation switch statement
            smvCode += "  next(s) := case\n";


            // 16: add transition relation cases for each state
            for (int i = 0; i < MarkingsArray?.Count; i++)
            {
                string transitionStates = "";
                // 17: add case s = si
                transitionStates += "\ts = s" + i + ": {";
                List<int> successors = GetSuccessors(i);
                for (int j = 0; j < successors.Count; j++)
                {
                    transitionStates += "s" + successors[j];
                    if (j < successors.Count - 1)
                    {
                        transitionStates += ", ";
                    }
                }
                transitionStates += "};\n";

                if (successors.Count > 0)
                {
                    smvCode += transitionStates;
                }
            }

            // 24: close transition relation switch statement
            smvCode += "\tTRUE: s; \n\tesac;\n";

            // 25: for all P
            for (int i = 0; i < parameters.Nodes; i++)
            {
                // 26: open labelling function switch statement . pi := case
                smvCode += "\n  p" + i + " := case\n";

                // 27: for all si
                for (int j = 0; j < MarkingsArray?.Count; j++)
                {
                    //29: assign m to case s = sj, where m = Mj (pi) . s = sj : m;
                    if (MarkingsArray[j][i] > 0)
                    {
                        smvCode += "\ts = s" + j + ": " + MarkingsArray[j][i] + ";\n";
                    }
                }

                // 32: Set default value based on variable type
                if (IsPlaceBoolean(i))
                {
                    // 33: set default value to FALSE
                    smvCode += "\tTRUE: FALSE;\n";
                }
                else
                {
                    // 35: set default value to 0 
                    smvCode += "\tTRUE: 0;\n";
                }

                // 37: close labelling function switch statement
                smvCode += "\tesac;\n";
            }

            return smvCode;

            #region local_functions
            // local functions used in the algorithm
            bool IsPlaceBoolean(int placeIndex)
            {
                if (MarkingsArray.All(marking => marking[placeIndex] <= 1))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            List<int> GetSuccessors(int markingIndex)
            {
                List<int> successors = new List<int>();
                for (int i = 0; i < parameters.Transitions; i++)
                {
                    bool enabled = true;
                    for (int j = 0; j < parameters.Nodes; j++)
                    {
                        if (Parameters.PreMatrix?[j, i] > 0 && MarkingsArray[markingIndex][j] < Parameters.PreMatrix[j, i])
                        {
                            enabled = false;
                            break;
                        }
                    }
                    // If the input places are satisfied, enable transition
                    if (enabled)
                    {
                        // calculate new marking
                        int[] newMarking = (int[])MarkingsArray[markingIndex].Clone();

                        for (int j = 0; j < Parameters.Nodes; j++)
                        {
                            newMarking[j] -= Parameters.PreMatrix[j, i];
                            newMarking[j] += Parameters.PostMatrix[j, i];
                        }

                        // find the index of the new marking
                        int newMarkingIndex = MarkingsArray.FindIndex(marking => marking.SequenceEqual(newMarking));
                        // add the index to the list
                        successors.Add(newMarkingIndex);
                    }
                }
                return successors;
            }
            #endregion
        }

        // convert pre, post to a list of transitions
        private List<Dictionary<string, int>[]> GetTransitionsFromMatrices()
        {
            int numRows = Parameters.Nodes;
            int numCols = Parameters.Transitions;
            List<Dictionary<string, int>[]> transitions = new List<Dictionary<string, int>[]>();

            for (int col = 0; col < numCols; col++)
            {
                Dictionary<string, int>[] transition = new Dictionary<string, int>[2];
                transition[0] = new Dictionary<string, int>();
                transition[1] = new Dictionary<string, int>();

                for (int row = 0; row < numRows; row++)
                {
                    string place = "P" + row.ToString();
                    int preValue = Parameters.PreMatrix![row, col];
                    int postValue = Parameters.PostMatrix![row, col];

                    if (preValue != 0)
                    {
                        transition[0].Add(place, preValue);
                    }

                    if (postValue != 0)
                    {
                        transition[1].Add(place, postValue);
                    }
                }

                transitions.Add(transition);
            }
            return transitions;
        }

        // convert initial marking to a list of places
        private Dictionary<string, int> GetPlacesFromInitialMarking()
        {
            int numRows = Parameters.Nodes;
            Dictionary<string, int> places = new Dictionary<string, int>();
            for (int row = 0; row < numRows; row++)
            {
                string place = "P" + row.ToString();
                int value = Parameters.InitialMarking![row];
                places.Add(place, value);
            }
            return places;
        }

        private Dictionary<string, int> ApplyTransition(Dictionary<string, int> marking, Dictionary<string, int>[] transition)
        {
            Dictionary<string, int> newMarking = new Dictionary<string, int>(marking);
            Dictionary<string, int> preArcs = transition[0];
            Dictionary<string, int> postArcs = transition[1];

            foreach (KeyValuePair<string, int> entry in preArcs)
            {
                string place = entry.Key;
                int weight = entry.Value;
                int tokens = newMarking[place];

                if (tokens < weight)
                {
                    // The transition cannot fire because there are not enough tokens, so return the original marking
                    return marking;
                }
                else
                {
                    newMarking[place] -= weight;
                }
            }

            foreach (KeyValuePair<string, int> entry in postArcs)
            {
                string place = entry.Key;
                int weight = entry.Value;
                newMarking[place] += weight;
            }

            return newMarking;
        }

        private string MarkingToString(Dictionary<string, int> marking)
        {
            string[] tokens = new string[marking.Count];
            int i = 0;
            foreach (KeyValuePair<string, int> entry in marking)
            {
                tokens[i] = Convert.ToString(entry.Value);
                i++;
            }
            return string.Join(",", tokens);
        }

        // Convert markings from List<Dictionary<string, int>> to List<int[]>
        public List<int[]> ConvertMarkings(List<Dictionary<string, int>> markings)
        {
            List<int[]> convertedMarkings = new List<int[]>();
            foreach (Dictionary<string, int> marking in markings)
            {
                int[] convertedMarking = new int[Parameters.Nodes];
                for (int i = 0; i < Parameters.Nodes; i++)
                {
                    string place = "P" + i.ToString();
                    if (marking.ContainsKey(place))
                    {
                        convertedMarking[i] = marking[place];
                    }
                    else
                    {
                        convertedMarking[i] = 0;
                    }
                }
                convertedMarkings.Add(convertedMarking);
            }
            return convertedMarkings;
        }

        // Convert markings from List<int[]> to List<Dictionary<string, int>>
        public List<Dictionary<string, int>> ConvertMarkings(List<int[]> markings)
        {
            List<Dictionary<string, int>> convertedMarkings = new List<Dictionary<string, int>>();
            foreach (int[] marking in markings)
            {
                Dictionary<string, int> convertedMarking = new Dictionary<string, int>();
                for (int i = 0; i < marking.Length; i++)
                {
                    string place = "P" + i.ToString();
                    int value = marking[i];
                    if (value != 0)
                    {
                        convertedMarking.Add(place, value);
                    }
                }
                convertedMarkings.Add(convertedMarking);
            }
            return convertedMarkings;

        }

        private bool isGraphUnBounded(int method = 0)
        {
            switch (method)
            {
                case 0:
                    return getRank(Parameters.IncidenceMatrix) > Parameters.Nodes - Parameters.Transitions;
                case 1:
                    return DetermineMaxKSafe() == -1;
                default:
                    return false;
            }
        }

        // identify if graph is bounded or not
        // using the rank of the incidence matrix, if rank < nodes - transitions, then the graph is unbounded
        private int getRank(int[,] incidenceMatrix)
        {
            int rows = incidenceMatrix.GetLength(0);
            int cols = incidenceMatrix.GetLength(1);
            int rank = 0;

            for (int j = 0; j < cols; j++)
            {
                // Find a non-zero entry in column j
                int i;
                for (i = rank; i < rows; i++)
                {
                    if (incidenceMatrix[i, j] != 0)
                    {
                        break;
                    }
                }

                // If a non-zero entry is found, swap the rows
                if (i < rows)
                {
                    for (int k = 0; k < cols; k++)
                    {
                        int temp = incidenceMatrix[rank, k];
                        incidenceMatrix[rank, k] = incidenceMatrix[i, k];
                        incidenceMatrix[i, k] = temp;
                    }

                    // Eliminate the entries below row rank in column j
                    for (int k = rank + 1; k < rows; k++)
                    {
                        int factor = incidenceMatrix[k, j] / incidenceMatrix[rank, j];
                        for (int l = j; l < cols; l++)
                        {
                            incidenceMatrix[k, l] -= factor * incidenceMatrix[rank, l];
                        }
                    }

                    rank++;
                }
            }

            return rank;
        }

        // using k-safety
        public int DetermineMaxKSafe()
        {
            int k = 0;
            bool isKSafe = false;
            int[,] incidenceMatrix = Parameters.IncidenceMatrix;
            int[] initialMarking = Parameters.InitialMarking;

            while (!isKSafe)
            {
                k++;

                if (k > Parameters.K)
                {
                    // The Petri net is not k-safe for any k
                    return -1;
                }

                // Check each place in the Petri net
                for (int i = 0; i < incidenceMatrix.GetLength(0); i++)
                {
                    int maxInput = 0;
                    int maxOutput = 0;

                    // Check incoming arcs to the current place
                    for (int j = 0; j < incidenceMatrix.GetLength(1); j++)
                    {
                        if (incidenceMatrix[i, j] < 0)
                        {
                            // This is an incoming arc to the current place
                            int inputTokens = initialMarking[j] - incidenceMatrix[i, j];

                            if (inputTokens > maxInput)
                            {
                                maxInput = inputTokens;
                            }
                        }
                    }

                    // Check outgoing arcs from the current place
                    for (int j = 0; j < incidenceMatrix.GetLength(1); j++)
                    {
                        if (incidenceMatrix[i, j] > 0)
                        {
                            // This is an outgoing arc from the current place
                            int outputTokens = initialMarking[j] + incidenceMatrix[i, j];

                            if (outputTokens > maxOutput)
                            {
                                maxOutput = outputTokens;
                            }
                        }
                    }

                    // Check if the current place is k-bounded
                    if (maxInput > k || maxOutput > k)
                    {
                        break; // Current k value is not safe
                    }
                    else if (i == incidenceMatrix.GetLength(0) - 1)
                    {
                        isKSafe = true; // All places are k-bounded
                    }
                }
            }

            return k;
        }
    }
}
