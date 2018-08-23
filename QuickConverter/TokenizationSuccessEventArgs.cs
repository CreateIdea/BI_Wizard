using QuickConverter.Tokens;

namespace QuickConverter
{
	public class TokenizationSuccessEventArgs : QuickConverterEventArgs
	{
		public override QuickConverterEventType Type { get { return QuickConverterEventType.TokenizationSuccess; } }

		public TokenBase Root { get; set; }

		internal TokenizationSuccessEventArgs(string expression, TokenBase root)
			: base(expression)
		{
			Root = root;
		}
	}
}
