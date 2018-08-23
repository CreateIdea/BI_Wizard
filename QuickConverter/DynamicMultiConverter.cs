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
	public class DynamicMultiConverter : IMultiValueConverter
	{
		private static Type _namedObjectType = typeof(DependencyObject).Assembly.GetType("MS.Internal.NamedObject", false);

		private static ConcurrentDictionary<Type, Func<object, object>> castFunctions = new ConcurrentDictionary<Type, Func<object, object>>();

		public string ConvertExpression { get; private set; }
		public string[] ConvertBackExpression { get; private set; }
		public Exception LastException { get; private set; }
		public int ExceptionCount { get; private set; }

		public string ConvertExpressionDebugView { get; private set; }
		public string[] ConvertBackExpressionDebugView { get; private set; }

		public Type[] PTypes { get; private set; }

		public Type ValueType { get; private set; }

		public object[] Values { get { return _values; } }

		private int[] pIndices;

		private Func<object[], object[], object> _converter;
		private Func<object, object[], object>[] _convertBack;
		private object[] _values;
		private DataContainer[] _toDataContainers;
		private DataContainer[] _fromDataContainers;
		public DynamicMultiConverter(Func<object[], object[], object> converter, Func<object, object[], object>[] convertBack, object[] values, string convertExp, string convertExpDebug, string[] convertBackExp, string[] convertBackExpDebug, Type[] pTypes, int[] pIndices, Type vType, DataContainer[] toDataContainers, DataContainer[] fromDataContainers)
		{
			_converter = converter;
			_convertBack = convertBack;
			_values = values;
			_toDataContainers = toDataContainers;
			_fromDataContainers = fromDataContainers;
			ConvertExpression = convertExp;
			ConvertBackExpression = convertBackExp;
			PTypes = pTypes;
			this.pIndices = pIndices;
			ValueType = vType;
		}

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			for (int i = 0; i < values.Length; ++i)
			{
				if (values[i] == DependencyProperty.UnsetValue || _namedObjectType.IsInstanceOfType(values[i]) || (PTypes[i] != null && !PTypes[i].IsInstanceOfType(values[i])))
					return DependencyProperty.UnsetValue;
			}

			object result;
			try { result = _converter(values, _values); }
			catch (Exception e)
			{
				LastException = e;
				++ExceptionCount;
				if (Debugger.IsAttached)
					Console.WriteLine("QuickMultiConverter Exception (\"" + ConvertExpression + "\") - " + e.Message + (e.InnerException != null ? " (Inner - " + e.InnerException.Message + ")" : ""));
				EquationTokenizer.ThrowQuickConverterEvent(new RuntimeMultiConvertExceptionEventArgs(ConvertExpression, ConvertExpressionDebugView, values, pIndices, null, _values, parameter, this, e));
				return DependencyProperty.UnsetValue;
			}
			finally
			{
				if (_toDataContainers != null)
				{
					foreach (var container in _toDataContainers)
						container.Value = null;
				}
			}

			result = CastResult(result, targetType);
			return result;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			object[] ret = new object[_convertBack.Length];

			if (ValueType != null && !ValueType.IsInstanceOfType(value))
			{
				for (int i = 0; i < ret.Length; ++i)
					ret[i] = System.Windows.Data.Binding.DoNothing;
				return ret;
			}

			for (int i = 0; i < _convertBack.Length; ++i)
			{
				try { ret[i] = _convertBack[i](value, _values); }
				catch (Exception e)
				{
					LastException = e;
					++ExceptionCount;
					if (Debugger.IsAttached)
						Console.WriteLine("QuickMultiConverter Exception (\"" + ConvertBackExpression[i] + "\") - " + e.Message + (e.InnerException != null ? " (Inner - " + e.InnerException.Message + ")" : ""));
					EquationTokenizer.ThrowQuickConverterEvent(new RuntimeMultiConvertExceptionEventArgs(ConvertBackExpression[i], ConvertBackExpressionDebugView[i], null, pIndices, value, _values, parameter, this, e));
					ret[i] = DependencyProperty.UnsetValue;
				}
				ret[i] = CastResult(ret[i], targetTypes[i]);
			}
			if (_fromDataContainers != null)
			{
				foreach (var container in _fromDataContainers)
					container.Value = null;
			}
			return ret;
		}

		private object CastResult(object result, Type targetType)
		{
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
	}
}
