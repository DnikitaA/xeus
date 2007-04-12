using System ;
using System.ComponentModel ;
using System.Diagnostics ;
using System.Windows ;
using System.Windows.Documents ;

namespace xeus.Core
{
	public class XeusHyperlink : Hyperlink
	{
		public XeusHyperlink( Inline inline ) : base( inline )
		{
		}

		protected override void OnClick()
		{
			string target = NavigateUri.AbsoluteUri ;

			try
			{
				Process.Start( target ) ;
			}

			catch
			{
			}
		}
	}
}