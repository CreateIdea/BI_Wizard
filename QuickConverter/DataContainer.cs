﻿using System.Threading;

namespace QuickConverter
{
	public class DataContainer
	{
		private ThreadLocal<object> _value = new ThreadLocal<object>();
		public object Value
		{
			get { return _value.Value; }
			set { _value.Value = value; }
		}
	}
}
