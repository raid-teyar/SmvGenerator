using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SmvGenerator
{
    /// <summary>
    /// Interaction logic for ParametersWindow.xaml
    /// </summary>
    public partial class ParametersWindow : Window
    {


        public Parameters Parameters { get; set; }

        public ParametersWindow()
        {
            InitializeComponent();
            Parameters = new Parameters();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            Parameters.Nodes = Convert.ToInt32(tbNodes.Text);
            Parameters.Transitions = Convert.ToInt32(tbTransitions.Text);

            for (int i = 0; i < Parameters.Transitions; i++)
            {
                ColumnDefinition col = new ColumnDefinition();
                col.Width = new GridLength(1, GridUnitType.Star);
                gIncidenceMatrix.ColumnDefinitions.Add(col);
            }

            for (int i = 0; i < Parameters.Nodes; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(1, GridUnitType.Star);
                gIncidenceMatrix.RowDefinitions.Add(row);
            }

            TextBlock textBlock = new TextBlock();
            textBlock.Text = "Incidence Matrix (Pre, Post)";
            textBlock.FontSize = 20;
            textBlock.FontWeight = FontWeights.Bold;
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetRow(textBlock, 2);
            Grid.SetColumn(textBlock, 0);

            // set column span to the number of columns
            Grid.SetColumnSpan(textBlock, Parameters.Transitions + 1);

            gMain.Children.Add(textBlock);


            // fill the grid with textboxes
            for (int i = 0; i < Parameters.Nodes; i++)
            {
                for (int j = 0; j < Parameters.Transitions; j++)
                {
                    TextBox tb = new TextBox();
                    tb.Name = "tb" + i + j;
                    tb.Text = "0";
                    tb.Margin = new Thickness(5);
                    tb.HorizontalAlignment = HorizontalAlignment.Stretch;
                    tb.VerticalAlignment = VerticalAlignment.Stretch;
                    tb.TextAlignment = TextAlignment.Center;
                    tb.FontSize = 20;
                    tb.FontWeight = FontWeights.Bold;
                    Grid.SetRow(tb, i);
                    Grid.SetColumn(tb, j);
                    gIncidenceMatrix.Children.Add(tb);
                }
            }

            bNext.Visibility = Visibility.Visible;
        }

        private void Next1Button_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Parameters.Nodes; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(1, GridUnitType.Star);
                gInitialMarking.RowDefinitions.Add(row);
            }

            TextBlock textBlock = new TextBlock();
            textBlock.Text = "Initial Marking";
            textBlock.FontSize = 20;
            textBlock.FontWeight = FontWeights.Bold;
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetRow(textBlock, 5);
            Grid.SetColumn(textBlock, 0);

            // set column span to the number of columns
            Grid.SetColumnSpan(textBlock, 2);

            gMain.Children.Add(textBlock);

            for (int i = 0; i < Parameters.Nodes; i++)
            {
                TextBox tb = new TextBox();
                tb.Name = "tb" + i;
                tb.Text = "0";
                tb.Margin = new Thickness(5);
                tb.HorizontalAlignment = HorizontalAlignment.Stretch;
                tb.VerticalAlignment = VerticalAlignment.Stretch;
                tb.TextAlignment = TextAlignment.Center;
                tb.FontSize = 20;
                tb.FontWeight = FontWeights.Bold;
                Grid.SetRow(tb, i);
                gInitialMarking.Children.Add(tb);
            }

            bGenerate.Visibility = Visibility.Visible;
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            int[] initiaMarking = new int[Parameters.Nodes];


            for (int i = 0; i < Parameters.Nodes; i++)
            {
                TextBox tb = (TextBox)gInitialMarking.Children[i];

                initiaMarking[i] = Convert.ToInt32(tb.Text);
            }

            int[,] post = new int[Parameters.Nodes, Parameters.Transitions];
            int[,] pre = new int[Parameters.Nodes, Parameters.Transitions];

            for (int i = 0; i < Parameters.Nodes; i++)
            {
                for (int j = 0; j < Parameters.Transitions; j++)
                {
                    TextBox tb = (TextBox)gIncidenceMatrix.Children[i * Parameters.Transitions + j];

                    if (tb.Text == "0")
                        break;

                    pre[i, j] = Convert.ToInt32(tb.Text.Split(',')[0]);
                    post[i, j] = Convert.ToInt32(tb.Text.Split(',')[1]);
                }
            }

            Parameters.InitialMarking = initiaMarking;
            //Parameters.PostMatrix = post;
            //Parameters.PreMatrix = pre;

            Parameters.PreMatrix = new int[,]{
                {1,0 },
                {2,0 },
                {0,1 },
                };

            Parameters.PostMatrix = new int[,]
            {
                {1,0 },
                {0,1 },
                {2,0 },
                };

            MainWindow mw = new MainWindow(Parameters);
            mw.Show();
            this.Close();
        }
    }
}
