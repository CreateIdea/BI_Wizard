﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace QuickConverter.Tokens
{
	public class ThrowToken : TokenBase
	{
		internal ThrowToken()
		{
		}

		public override Type ReturnType { get { return typeof(object); } }

		public override TokenBase[] Children { get { return new[] { Exception }; } }

		public TokenBase Exception { get; private set; }

		internal override bool TryGetToken(ref string text, out TokenBase token)
		{
			token = null;
			if (!text.StartsWith("throw"))
				return false;
			string temp = text.Substring(5).TrimStart();

			TokenBase valToken = null;
			if (!EquationTokenizer.TryGetValueToken(ref temp, out valToken))
				return false;
			
			text = temp;
			token = new ThrowToken() { Exception = valToken };
			return true;
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, List<DataContainer> dataContainers, Type dynamicContext, LabelTarget label)
		{
			return Expression.Throw(Exception.GetExpression(parameters, locals, dataContainers, dynamicContext, label), typeof(object));
		}
	}
}
