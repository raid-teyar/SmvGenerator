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

        // main window constructor
        public MainWindow(Parameters parameters)
        {
            Parameters = parameters;
            Parameters.Markings = new List<int[]>();
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
            int[] currentMarking = (int[])Parameters.InitialMarking!.Clone();

            List<int[]> markings = new List<int[]>();
            markings.Add(currentMarking);

            while (true)
            {
                bool transitionEnabled = false;

                // check if there is a transition enabled
                for (int i = 0; i < Parameters.Transitions; i++)
                {
                    bool enabled = true;
                    for (int j = 0; j < Parameters.Nodes; j++)
                    {
                        if (Parameters.PreMatrix?[j, i] > 0 && currentMarking[j] < Parameters.PreMatrix[j, i])
                        {
                            enabled = false;
                            break;
                        }
                    }

                    // If the input places are satisfied, enable transition
                    if (enabled)
                    {
                        transitionEnabled = true;

                        // calculate new marking
                        int[] newMarking = (int[])currentMarking.Clone();

                        for (int j = 0; j < Parameters.Nodes; j++)
                        {
                            newMarking[j] -= Parameters.PreMatrix[j, i];
                            newMarking[j] += Parameters.PostMatrix[j, i];
                        }

                        currentMarking = newMarking;

                        // add new marking to the list
                        markings.Add(currentMarking);
                    }
                }

                // If no transition is enabled, marking graph is complete
                if (!transitionEnabled)
                {
                    break;
                }
            }

            Parameters.Markings = markings;

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
            foreach (int[] marking in markings)
            {
                listView.Items.Add(marking);
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
            for (int i = 0; i < parameters.Markings?.Count; i++)
            {
                smvCode += "s" + i.ToString();
                if (i < parameters.Markings.Count - 1)
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
                    smvCode += "  p" + i.ToString() + " : 0.." + parameters.Markings?.Max(marking => marking[i]) + ";\n";
                }
            }

            // 13: add ASSIGN keyword
            smvCode += "\nASSIGN\n";

            // 14: init s variable
            smvCode += "  init(s) := s0;\n";

            // 15: open transition relation switch statement
            smvCode += "  next(s) := case\n";

            // 16: add transition relation cases for each state
            for (int i = 0; i < parameters.Markings?.Count; i++)
            {
                // 17: add case s = si
                smvCode += "\ts = s" + i + ": {";
                List<int> successors = GetSuccessors(i);
                for (int j = 0; j < successors.Count; j++)
                {
                    smvCode += "s" + successors[j];
                    if (j < successors.Count - 1)
                    {
                        smvCode += ", ";
                    }
                }
                smvCode += "};\n";
            }

            // 24: close transition relation switch statement
            smvCode += "\tesac;\n";

            // 25: for all P
            for (int i = 0; i < parameters.Nodes; i++)
            {
                // 26: open labelling function switch statement . pi := case
                smvCode += "\n  p" + i + " := case\n";

                // 27: for all si
                for (int j = 0; j < parameters.Markings?.Count; j++)
                {
                    //29: assign m to case s = sj, where m = Mj (pi) . s = sj : m;
                    if (parameters.Markings[j][i] > 0)
                    {
                        smvCode += "\ts = s" + j + ": " + parameters.Markings[j][i] + ";\n";
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
                if (parameters.Markings.All(marking => marking[placeIndex] <= 1))
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
                        if (Parameters.PreMatrix?[j, i] > 0 && parameters.Markings[markingIndex][j] < Parameters.PreMatrix[j, i])
                        {
                            enabled = false;
                            break;
                        }
                    }
                    // If the input places are satisfied, enable transition
                    if (enabled)
                    {
                        // calculate new marking
                        int[] newMarking = (int[])parameters.Markings[markingIndex].Clone();

                        for (int j = 0; j < Parameters.Nodes; j++)
                        {
                            newMarking[j] -= Parameters.PreMatrix[j, i];
                            newMarking[j] += Parameters.PostMatrix[j, i];
                        }

                        // find the index of the new marking
                        int newMarkingIndex = parameters.Markings.FindIndex(marking => marking.SequenceEqual(newMarking));
                        // add the index to the list
                        successors.Add(newMarkingIndex);
                    }
                }
                return successors;
            }
            #endregion
        }
    }
}
