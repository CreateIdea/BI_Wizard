﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Data;
using Microsoft.CSharp.RuntimeBinder;
using Expression = System.Linq.Expressions.Expression;

namespace QuickConverter
{
	public class DynamicSingleConverter : IValueConverter
	{
		private static Type _namedObjectType = typeof(DependencyObject).Assembly.GetType("MS.Internal.NamedObject", false);

		private static ConcurrentDictionary<Type, Func<object, object>> castFunctions = new ConcurrentDictionary<Type, Func<object, object>>();

		public string ConvertExpression { get; private set; }
		public string ConvertBackExpression { get; private set; }
		public Exception LastException { get; private set; }
		public int ExceptionCount { get; private set; }

		public string ConvertExpressionDebugView { get; private set; }
		public string ConvertBackExpressionDebugView { get; private set; }

		public Type PType { get; private set; }

		public Type ValueType { get; private set; }

		public object[] Values { get { return _values; } }

		private Func<object, object[], object, object> _converter;
		private Func<object, object[], object, object> _convertBack;
		private object[] _values;
		private DataContainer[] _toDataContainers;
		private DataContainer[] _fromDataContainers;
		public DynamicSingleConverter(Func<object, object[], object, object> converter, Func<object, object[], object, object> convertBack, object[] values, string convertExp, string convertExpDebug, string convertBackExp, string convertBackExpDebug, Type pType, Type vType, DataContainer[] toDataContainers, DataContainer[] fromDataContainers)
		{
			_converter = converter;
			_convertBack = convertBack;
			_values = values;
			_toDataContainers = toDataContainers;
			_fromDataContainers = fromDataContainers;
			ConvertExpression = convertExp;
			ConvertBackExpression = convertBackExp;
			ConvertExpressionDebugView = convertExpDebug;
			ConvertBackExpressionDebugView = convertBackExpDebug;
			PType = pType;
			ValueType = vType;
		}

		private object DoConversion(object value, Type targetType, object parameter, Func<object, object[], object, object> func, bool convertingBack)
		{
			if (convertingBack)
			{
				if (ValueType != null && !ValueType.IsInstanceOfType(value))
					return System.Windows.Data.Binding.DoNothing;
			}
			else
			{
				if (value == DependencyProperty.UnsetValue || _namedObjectType.IsInstanceOfType(value) || (PType != null && !PType.IsInstanceOfType(value)))
					return DependencyProperty.UnsetValue;
			}

			object result = value;
			if (func != null)
			{
				try { result = func(result, _values, parameter); }
				catch (Exception e)
				{
					LastException = e;
					++ExceptionCount;
					if (Debugger.IsAttached)
						Console.WriteLine("QuickMultiConverter Exception (\"" + (convertingBack ? ConvertBackExpression : ConvertExpression) + "\") - " + e.Message + (e.InnerException != null ? " (Inner - " + e.InnerException.Message + ")" : ""));
					if (convertingBack)
						EquationTokenizer.ThrowQuickConverterEvent(new RuntimeSingleConvertExceptionEventArgs(ConvertBackExpression, ConvertBackExpressionDebugView, null, value, _values, parameter, this, e));
					else
						EquationTokenizer.ThrowQuickConverterEvent(new RuntimeSingleConvertExceptionEventArgs(ConvertExpression, ConvertExpressionDebugView, value, null, _values, parameter, this, e));
					return DependencyProperty.UnsetValue; 
				}
				finally
				{
					var dataContainers = convertingBack ? _fromDataContainers : _toDataContainers;
					if (dataContainers != null)
					{
						foreach (var container in dataContainers)
							container.Value = null;
					}
				}
			}

			if (result == null || result == DependencyProperty.UnsetValue || result == System.Windows.Data.Binding.DoNothing || targetType == null || targetType == typeof(object))
				return result;

			if (targetType == typeof(string))
				return result.ToString();

			Func<object, object> cast;
			if (!castFunctions.TryGetValue(targetType, out cast))
			{
				ParameterExpression par = Expression.Parameter(typeof(object));
				cast = Expression.Lambda<Func<object, object>>(Expression.Convert(Expression.Dynamic(Binder.Convert(CSharpBinderFlags.ConvertExplicit, targetType, typeof(object)), targetType, par), typeof(object)), par).Compile();
				castFunctions.TryAdd(targetType, cast);
			}
			if (cast != null)
			{
				try
				{
					result = cast(result);
				}
				catch
				{
					castFunctions[targetType] = null;
				}
			}
			return result;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return DoConversion(value, targetType, parameter, _converter, false);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return DoConversion(value, targetType, parameter, _convertBack, true);
		}
	}
}
