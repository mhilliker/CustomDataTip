﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json;

namespace CustomDataTip
{
    public static class TreeViewBuilder
    {
        // for performance purposes, we need to throttle the number of data members that we read in
        // in future iterations, we can implement an incremental loading system for this
        private const int MaxItems = 15000;
        private const int MaxLevels = 5;
        private static int _itemCount;
        private static int _itemDepth;

        /// <summary>
        /// Text color of the tree view items
        /// </summary>
        public static SolidColorBrush TextColorBrush;

        /// <summary>
        /// Creates the tree view for a given expression.
        /// </summary>
        /// <param name="expression">DTE expression for a debugger variable</param>
        /// <returns>Tree view for the adornment</returns>
        public static TreeView GetTreeView(EnvDTE.Expression expression)
        {
            TreeView treeView = new TreeView();

            _itemCount = _itemDepth = 0;

            treeView.ItemsSource = new List<TreeViewItem>() { BuildTreeView(expression) };

            _itemCount = _itemDepth = 0;
            return treeView;
        }

        /// <summary>
        /// Creates the string representation of the object.
        /// </summary>
        /// <param name="expression">DTE expression for a given variable in the debugger</param>
        /// <param name="parallel">If true, builds out the string in parallel. Useful for large objects</param>
        /// <returns>Object</returns>
        public static object GetStringRepresentation(EnvDTE.Expression expression, bool prettyPrint = false, bool parallel = false)
        {
            _itemCount = _itemDepth = 0;

            var obj =  BuildString(expression);
            _itemCount = _itemDepth = 0;

            return JsonConvert.SerializeObject(obj, prettyPrint ? Formatting.Indented : Formatting.None);
        }

        /// <summary>
        /// Creates an object representation of the given expression in order to serialize it to a string.
        /// </summary>
        /// <param name="expression">DTE expression to serialize</param>
        /// <returns>Convenient dictionary object representation to serialize</returns>
        private static object BuildString(EnvDTE.Expression expression)
        {
            Dictionary<string, object> newItem = new Dictionary<string, object>();

            if (expression.DataMembers == null || expression.DataMembers.Count == 0 || expression.Type.Contains("Function"))
            {
                return expression.Value;
            }
            else if (expression.Name == "[Methods]")
            {
                return new object[] {};
            }
            else
            {
                List<EnvDTE.Expression> dataMembers = expression.DataMembers.Cast<EnvDTE.Expression>().ToList();

                if (_itemCount < MaxItems*5 && _itemDepth < MaxLevels*2) 
                {
                    _itemDepth++;
                    foreach(var member in dataMembers)
                    {
                        newItem.Add(member.Name, BuildString(member));
                    }
                }
            }

            return newItem;
        }

        /// <summary>
        /// Creates the tree view for the adornment recursively.
        /// </summary>
        /// <param name="expression">DTE expression to parse</param>
        /// <returns>TreeViewItem representing the expression</returns>
        private static TreeViewItem BuildTreeView(EnvDTE.Expression expression)
        {
            TreeViewItem newItem = null;

            if (expression.DataMembers == null || expression.DataMembers.Count == 0)
            {
                newItem = GetTreeViewItem(expression.Name, expression.Value);
            }
            else
            {
                List<EnvDTE.Expression> dataMembers = expression.DataMembers.Cast<EnvDTE.Expression>().ToList();

                newItem = GetTreeViewItem(expression.Name, expression.Type);
                var childItems = new List<TreeViewItem>();

                if (_itemCount < MaxItems && _itemDepth < MaxLevels)
                {
                    _itemDepth++;
                    foreach (EnvDTE.Expression member in dataMembers)
                    {
                        childItems.Add(BuildTreeView(member));
                    }

                    newItem.ItemsSource = childItems;
                }
            }

            return newItem;
        }

        /// <summary>
        /// Generates a tree view item from a given name and value.
        /// </summary>
        /// <param name="name">name of variable</param>
        /// <param name="value">value of variable</param>
        /// <returns>Tree view item that will be part of the adornment</returns>
        private static TreeViewItem GetTreeViewItem(string name, string value)
        {
            TreeViewItem item = new TreeViewItem();

            item.IsExpanded = false;

            // create stack panel
            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;

            // create name
            Label lblName = new Label()
            {
                Content = name + ":",
                Foreground = TextColorBrush ?? Brushes.AntiqueWhite
            };

            // create value
            Label lblValue = new Label()
            {
                Content = value,
                Foreground = TextColorBrush ?? Brushes.AntiqueWhite
            };


            // Add into stack (change to name and value)
            stack.Children.Add(lblName);
            stack.Children.Add(lblValue);

            // assign stack to header
            item.Header = stack;

            _itemCount++;
            
            return item;
        }
    }
}
