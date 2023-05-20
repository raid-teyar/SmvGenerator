using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for CheckPropertyWindow.xaml
    /// </summary>
    public partial class CheckPropertyWindow : Window
    {
        public CheckPropertyWindow()
        {
            InitializeComponent();
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            string property = tbProperty.Text;

            if (File.Exists("modelp.smv"))
            {
                File.Delete("modelp.smv");
            }

            File.Copy("model.smv", "modelp.smv");
            File.AppendAllText("modelp.smv", "\n" + "LTLSPEC " + property);

            bool isFalse;
            string output = CheckProperty("modelp.smv", out isFalse);

            if (output != null)
            {
                rbResults.Text = output;
            }

            if (isFalse)
            {
                btCheck.Background = new SolidColorBrush(Colors.Red);
                
            }
            else
            {
                btCheck.Background = new SolidColorBrush(Colors.LightGreen);
            }

        }

        public string CheckProperty(string modelCode, out bool isFalse)
        {
            try
            {
                // Run nuSMV command line tool
                var processInfo = new ProcessStartInfo("cmd.exe", $"/k nuSMV -bmc {modelCode}")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                using (var process = Process.Start(processInfo))
                {
                    process.WaitForExit();

                    // Parse the output to determine if the property is satisfied or not
                    var output = process.StandardOutput.ReadToEnd();
                    var match = Regex.Match(output, @"-- specification.*?((?<=is )[A-Za-z]+)", RegexOptions.Singleline);
                    if (match.Success)
                    {
                        isFalse = match.Groups[1].Value.ToLower() == "false";
                        return output;
                    }
                    else
                    {
                        isFalse = false;
                        return output;
                    }
                }
            }
            catch (Exception ex)
            {
                isFalse = false;
                return "Failed to parse nuSMV output";
            }
            finally
            {
                File.Delete("modelp.smv");
            }
        }
    }
}
