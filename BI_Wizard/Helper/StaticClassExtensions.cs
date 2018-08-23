using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Expression = System.Linq.Expressions.Expression;

namespace BI_Wizard.Helper
{
    public static class PropertyHelper<T>
    {

        public static string GetPropertyName2<T>(Expression<Func<T>> exp)
        {
            MemberExpression memberExpression = (MemberExpression) exp.Body;
            return memberExpression.Member.Name;
        }
        public static string GetPropertyName<TValue>(Expression<Func<T, TValue>> selector)
        {
            Expression body = selector;
            if (body is LambdaExpression)
            {
                body = ((LambdaExpression) body).Body;
            }
            switch (body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return ((PropertyInfo) ((MemberExpression) body).Member).Name;
                default:
                    return null;
            }
        }


        //class Foo
        //{
        //    public string Bar
        //    {
        //        get; set;
        //    }
        //}
        //static class Program
        //{
        //    static void Main()
        //    {
        //        PropertyInfo prop = PropertyHelper<Foo>.GetProperty(x => x.Bar);
        //    }
        //}

        public static PropertyInfo GetProperty<TValue>(Expression<Func<T, TValue>> selector)
        {
            Expression body = selector;
            if (body is LambdaExpression)
            {
                body = ((LambdaExpression) body).Body;
            }
            switch (body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return (PropertyInfo) ((MemberExpression) body).Member;
                default:
                    return null;
            }
        }
    }
    public static class ObjectReflexionExtension
    {
        //propertyName could be property1.property2
        public static Object GetPropValue(this Object obj, String propertyName)
        {
            foreach (String part in propertyName.Split('.'))
            {
                if (obj == null)
                {
                    return null;
                }

                Type type = obj.GetType();
                PropertyInfo info = type.GetProperty(part);
                if (info == null)
                {
                    return null;
                }

                obj = info.GetValue(obj, null);
            }
            return obj;
        }


        public static bool HasProperty(this Type obj, string propertyName)
        {
            //return obj.GetProperty(propertyName) != null;

            foreach (String part in propertyName.Split('.'))
            {
                if (obj == null)
                {
                    return false;
                }

                PropertyInfo info = obj.GetProperty(part);
                if (info == null)
                {
                    return false;
                }

                obj = info.GetType();
            }
            return obj != null;
        }

        //type safe version of GetPropValue if you know the type
        public static T GetPropValue<T>(this Object obj, String propertyName)
        {
            Object retval = GetPropValue(obj, propertyName);
            if (retval == null)
            {
                return default(T);
            }

            // throws InvalidCastException if types are incompatible
            return (T) retval;
        }

        ///http://stackoverflow.com/questions/1196991/get-property-value-from-string-using-reflection-in-c-sharp
        ///propertyName could be property1.property2[X].property3:
        public static object GetPropertyValueCollection(object srcobj, string propertyName)
        {
            if (srcobj == null)
                return null;

            object obj = srcobj;

            // Split property name to parts (propertyName could be hierarchical, like obj.subobj.subobj.property
            string[] propertyNameParts = propertyName.Split('.');

            foreach (string propertyNamePart in propertyNameParts)
            {
                if (obj == null)
                    return null;

                // propertyNamePart could contain reference to specific 
                // element (by index) inside a collection
                if (!propertyNamePart.Contains("["))
                {
                    PropertyInfo pi = obj.GetType().GetProperty(propertyNamePart);
                    if (pi == null)
                        return null;
                    obj = pi.GetValue(obj, null);
                }
                else
                {   // propertyNamePart is areference to specific element 
                    // (by index) inside a collection
                    // like AggregatedCollection[123]
                    //   get collection name and element index
                    int indexStart = propertyNamePart.IndexOf("[") + 1;
                    string collectionPropertyName = propertyNamePart.Substring(0, indexStart - 1);
                    int collectionElementIndex = Int32.Parse(propertyNamePart.Substring(indexStart, propertyNamePart.Length - indexStart - 1));
                    //   get collection object
                    PropertyInfo pi = obj.GetType().GetProperty(collectionPropertyName);
                    if (pi == null)
                        return null;
                    object unknownCollection = pi.GetValue(obj, null);
                    //   try to process the collection as array
                    if (unknownCollection.GetType().IsArray)
                    {
                        object[] collectionAsArray = unknownCollection as Array[];
                        obj = collectionAsArray[collectionElementIndex];
                    }
                    else
                    {
                        //   try to process the collection as IList
                        System.Collections.IList collectionAsList = unknownCollection as System.Collections.IList;
                        if (collectionAsList != null)
                        {
                            obj = collectionAsList[collectionElementIndex];
                        }
                        else
                        {
                            // ??? Unsupported collection type
                        }
                    }
                }
            }

            return obj;
        }
    }
    public static class ObservableCollectionExtensions
    {
        public static int RemoveAll<T>(this ObservableCollection<T> collection,
                                                           Func<T, bool> condition)
        {
            int aNrRemoved=0;
            for (int i = collection.Count - 1; i >= 0; i--)
            {
                if (condition(collection[i]))
                {
                    collection.RemoveAt(i);
                    aNrRemoved = aNrRemoved + 1;
                }
            }
            return aNrRemoved;
        }
    }
    public static class StaticClassExtensions
    {
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null)
                return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        public static T FindLogicalParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = LogicalTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null)
                return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindLogicalParent<T>(parentObject);
        }

        public static T FindFirstChild<T>(FrameworkElement element) where T : FrameworkElement
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(element);
            var children = new FrameworkElement[childrenCount];

            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(element, i) as FrameworkElement;
                children[i] = child;
                if (child is T)
                    return (T) child;
            }

            for (int i = 0; i < childrenCount; i++)
            {
                if (children[i] != null)
                {
                    var subChild = FindFirstChild<T>(children[i]);
                    if (subChild != null)
                        return subChild;
                }
            }
            return null;
        }
    }
}
