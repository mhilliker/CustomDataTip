using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;

namespace CustomDataTip
{
    public class ObjectNode
    {
        #region Private Properties
        private string _name;
        private object _value;
        private Type _type;
        #endregion

        #region Constructor
        public ObjectNode(object value)
        {
            ParseObjectTree("root", value, value.GetType());
        }

        public ObjectNode(string name, object value)
        {
            ParseObjectTree(name, value, value.GetType());
        }

        public ObjectNode(object value, Type t)
        {
            ParseObjectTree("root", value, t);
        }

        public ObjectNode(string name, object value, Type t)
        {
            ParseObjectTree(name, value, t);
        }
        #endregion

        #region Public Properties
        public string Name
        {
            get { return _name; }
        }

        public object Value
        {
            get { return _value; }
        }

        public Type Type
        {
            get { return _type; }
        }

        public ObservableCollection<ObjectNode> Children { get; set; }
        #endregion

        #region Private Methods
        private void ParseObjectTree(string name, object value, Type type)
        {
            Children = new ObservableCollection<ObjectNode>();

            _type = type;
            _name = name;

            if (value != null)
            {
                if (value is string && type != typeof(object))
                {
                    if (value != null)
                    {
                        _value = "\"" + value + "\"";
                    }
                }
                else if (value is double || value is bool || value is int || value is float || value is long || value is decimal)
                {
                    _value = value;
                }
                else
                {
                    _value = "{" + value.ToString() + "}";
                }
            }

            PropertyInfo[] props = type.GetProperties();

            if (props.Length == 0 && type.IsClass && value is IEnumerable && !(value is string))
            {
                IEnumerable arr = value as IEnumerable;

                if (arr != null)
                {
                    int i = 0;
                    foreach (object element in arr)
                    {
                        Children.Add(new ObjectNode("[" + i + "]", element, element.GetType()));
                        i++;
                    }
                }
            }

            foreach (PropertyInfo p in props)
            {
                if (p.PropertyType.IsPublic)
                {
                    if (p.PropertyType.IsClass || p.PropertyType.IsArray || p.PropertyType.IsInterface || p.PropertyType.ToString().ToLower() != "string")
                    {
                        if (p.PropertyType.IsArray)
                        {
                            try
                            {
                                object v = p.GetValue(value, null);
                                IEnumerable arr = v as IEnumerable;

                                ObjectNode arrayNode = new ObjectNode(p.Name, arr.ToString(), typeof(object));

                                if (arr != null)
                                {
                                    int i = 0, k = 0;
                                    ObjectNode arrayNode2;

                                    foreach (object element in arr)
                                    {
                                        //Handle 2D arrays
                                        if (element is IEnumerable && !(element is string))
                                        {
                                            arrayNode2 = new ObjectNode("[" + i + "]", element.ToString(), typeof(object));

                                            IEnumerable arr2 = element as IEnumerable;
                                            k = 0;

                                            foreach (object e in arr2)
                                            {
                                                arrayNode2.Children.Add(new ObjectNode("[" + k + "]", e, e.GetType()));
                                                k++;
                                            }

                                            arrayNode.Children.Add(arrayNode2);
                                        }
                                        else
                                        {
                                            arrayNode.Children.Add(new ObjectNode("[" + i + "]", element, element.GetType()));
                                        }
                                        i++;
                                    }

                                }

                                Children.Add(arrayNode);
                            }
                            catch { }
                        }
                        else
                        {
                            object v = p.GetValue(value, null);

                            if (v != null)
                            {
                                Children.Add(new ObjectNode(p.Name, v, p.PropertyType));
                            }
                        }
                    }
                    else if (p.PropertyType.IsValueType && !(value is string))
                    {
                        try
                        {
                            object v = p.GetValue(value, null);

                            if (v != null)
                            {
                                Children.Add(new ObjectNode(p.Name, v, p.PropertyType));
                            }
                        }
                        catch { }
                    }
                }
            }
        }

        #endregion
    }
}
