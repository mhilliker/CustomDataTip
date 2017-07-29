using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.Windows.Controls;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;

namespace CustomDataTip
{
    /// <summary>
    /// Controller for the custom data tip.
    /// </summary>
    class CustomDataTip
    {
        private readonly CustomDataTipControl root;
        private readonly IWpfTextView view;
        private readonly IAdornmentLayer adornmentLayer;
        private int curMax;
        private DateTime start;
        private EnvDTE.DTE dte;
        private readonly EnvDTE.Debugger debugger;
        private const int AdornmentLeftOffset = 20;
        private bool inAdornment;
        private readonly JavaScriptSerializer jsonSerializer;

        /// <summary>
        /// Constructor for the data tip controller.
        /// </summary>
        /// <param name="view">Visual Studio editor view</param>
        public CustomDataTip(IWpfTextView view)
        {
            this.view = view;
            root = new CustomDataTipControl();
            curMax = 0;
            start = DateTime.UtcNow;
            dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE;
            debugger = dte?.Debugger;
            inAdornment = false;
            jsonSerializer = new JavaScriptSerializer();
            TreeViewBuilder.TextColorBrush = root.ForeBrush;

            // Grab a reference to the adornment layer that this adornment should be added to
            adornmentLayer = view.GetAdornmentLayer("CustomDataTip");

            // Reposition the adornment whenever the editor window is resized
            this.view.ViewportHeightChanged += delegate { OnSizeChange(); };
            this.view.ViewportWidthChanged += delegate { OnSizeChange(); };
            this.view.MouseHover += UpdateAdornment;
            root.MouseEnter += _root_MouseEnter;
            root.MouseLeave += _root_MouseLeave;
        }

        /// <summary>
        /// Sets the flag when the mouse enters the adornment.
        /// </summary>
        /// <param name="sender">event source</param>
        /// <param name="e">event arguments</param>
        private void _root_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            inAdornment = true;
        }

        /// <summary>
        /// Moves the adornment off screen when the mouse leaves.
        /// </summary>
        /// <param name="sender">event source</param>
        /// <param name="e">event arguments</param>
        private void _root_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            inAdornment = false;
            if (Canvas.GetLeft(root) < 8000)
                MoveAdornment(9999, 9999);
        }

        /// <summary>
        /// Updates the size of the green speed bar based on a simple (KeysPressed/TimeElapsed) calculation
        /// DEPRECATED, BUT DON'T GET RID OF THIS METHOD
        /// </summary>
        /// <param name="typedChars">Number of keys pressed since start of session</param>
        public void UpdateBar(int typedChars)
        {
            // NOT USED
        }

        /// <summary>
        /// Reposition the adornment whenever the Editor is resized
        /// </summary>
        public void OnSizeChange()
        {
            //clear the adornment layer of previous adornments
            adornmentLayer.RemoveAdornment(root);

            ////Place the image in the top right hand corner of the Viewport
            MoveAdornment(view.ViewportRight, view.ViewportTop);

            ////add the image to the adornment layer and make it relative to the viewports
            adornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, null, null, root, null);
        }

        /// <summary>
        /// Updates the adornment with the variable information
        /// </summary>
        /// <param name="sender">event source</param>
        /// <param name="e">event arguments</param>
        public void UpdateAdornment(Object sender, MouseHoverEventArgs e)
        {
            if (debugger.CurrentMode != dbgDebugMode.dbgBreakMode || "TypeScript" != dte?.ActiveDocument?.Language)
            {
                return;
            }
            if (root.IsMouseOver)
            {
                return;
            }
            var variable = DebuggerVariable.FindUnderMousePointer(debugger, e);
            if (variable != null)
            {
                var position = GetVariablePosition(variable);
                if (position != null)
                {
                    MoveAdornment(position.Item1, position.Item2);

                    //TODO: Create string representation
                    //var dictView = TreeViewBuilder.GetStringRepresentation(variable.Expression, true);
                    //var stringView = jsonSerializer.Serialize(dictView);


                    var newTreeView = TreeViewBuilder.GetTreeView(variable.Expression);
                    root.ResultTreeView.Items.Clear();
                    foreach (var item in newTreeView.Items)
                    {
                        root.ResultTreeView.Items.Add(item);
                    }
                }
            }
            else
            {
                MoveAdornment(9999, 9999);
            }
        }

        /// <summary>
        /// Moves the adornment to the given position
        /// </summary>
        /// <param name="left">distance from the left</param>
        /// <param name="top">distance from the top</param>
        private void MoveAdornment(double left, double top)
        {
            Canvas.SetLeft(root, left);
            Canvas.SetTop(root, top);
        }

        /// <summary>
        /// Gets the position of the given debugger variable.
        /// </summary>
        /// <param name="variable">Variable to get the position of</param>
        /// <returns>(Left,Top) coordinate location of the variable</returns>
        private Tuple<double, double> GetVariablePosition(DebuggerVariable variable)
        {
            var markerGeometry = view.TextViewLines.GetMarkerGeometry(variable.Span);
            var line = view.GetTextViewLineContainingBufferPosition(variable.Span.Start);

            var left = Math.Max(0, markerGeometry.Bounds.Left - AdornmentLeftOffset);
            var top = line.Top - root.ActualHeight;

            return new Tuple<double, double>(left, top);
        }

        /// <summary>
        /// Parses the given DTE expression into a dictionary recursively.
        /// </summary>
        /// <param name="expression">DTE expression</param>
        /// <param name="recursiveLevel">Current recursive level</param>
        /// <returns>Dictionary containing part of the object</returns>
        private Dictionary<string, object> ParseDTEExpression(EnvDTE.Expression expression, int recursiveLevel)
        {
            if (expression == null || recursiveLevel >= 5)
            {
                return null;
            }

            var typeStr = expression.Type;

            if (typeStr == null || typeStr == "Null")
            {
                return null;
            }

            System.Diagnostics.Debug.Write("\nMPH Type: " + typeStr);

            var parsedObject = new Dictionary<string, object>();

            if (expression.DataMembers == null || expression.DataMembers.Count == 0 || IsTypescriptPrimitive(typeStr))
            {
                (parsedObject as IDictionary<string, object>)?.Add(expression.Name, expression.Value);
            }
            else
            {
                foreach (EnvDTE.Expression item in expression.DataMembers)
                {
                    var parsedField = ParseDTEExpression(item, recursiveLevel + 1);
                    if (parsedField != null)
                    {
                        (parsedObject as IDictionary<string, object>)?.Add(item.Name, parsedField);
                    }
                }
            }
            return parsedObject;
        }

        /// <summary>
        /// Determines if a given type is a primitive.
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>True if it is a primitive</returns>
        private bool IsTypescriptPrimitive(string type)
        {
            var myType = type?.ToLower();
            List<string> types = new List<string>() {"number", "string", "boolean", "array"};

            return !string.IsNullOrEmpty(myType) && (types.Any(e => e.Equals(myType)));
        }

        /// <summary>
        /// Adds a variable to the Visual Studio watch window.
        /// </summary>
        /// <param name="var"></param>
        private void AddToWatch(DebuggerVariable var)
        {
            throw new NotImplementedException("Not yet implemented.");
        }
    }
}