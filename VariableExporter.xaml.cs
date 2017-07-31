using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CustomDataTip
{
    /// <summary>
    /// Interaction logic for VariableExporter.xaml
    /// </summary>
    public partial class VariableExporter : UserControl
    {
        /// <summary>
        /// Expression that will be converted into a string
        /// </summary>
        public EnvDTE.Expression Expression { get; set; }

        /// <summary>
        /// Construct control
        /// </summary>
        public VariableExporter()
        {
            InitializeComponent();

            // fix depth at 10 for now
            DepthBox.Text = "10";
            DepthBox.IsReadOnly = true;
        }

        /// <summary>
        /// Valdiator for the depth text box. Only allows positive integers.
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event args</param>
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        /// <summary>
        /// Click handler to generate the string representation and places it in the textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            if (Expression == null)
            {
                GeneratedText.Text = "Variable cannot be null.";
            }
            else
            {
                bool prettyPrintMode = PrettyPrintCheckbox.IsChecked ?? false;
                GeneratedText.Text = TreeViewBuilder.GetStringRepresentation(Expression, prettyPrintMode) as string;
            }
            
        }

        #region Parser




        #endregion



    }
}
