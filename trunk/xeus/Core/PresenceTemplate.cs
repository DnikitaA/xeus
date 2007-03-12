using System.Windows.Controls ;
using agsXMPP.protocol.client ;

namespace xeus.Core
{
	internal static class PresenceTemplate
	{
		private static ControlTemplate _templateDnd ;
		private static ControlTemplate _templateAway ;
		private static ControlTemplate _templateFreeForChat ;
		private static ControlTemplate _templateXAway ;
		private static ControlTemplate _templateOnline ;
		private static ControlTemplate _templateOffline ;

		public static ControlTemplate GetStatusTemplate( Presence presence )
		{
			if ( presence == null || presence.Type == PresenceType.unavailable )
			{
				if ( _templateOffline == null )
				{
					_templateOffline = ( ControlTemplate ) App.Instance.FindResource( "StatusOffline" ) ;
				}

				return _templateOffline ;
			}

			switch ( presence.Show )
			{
				case ShowType.dnd:
					{
						if ( _templateDnd == null )
						{
							_templateDnd = ( ControlTemplate ) App.Instance.FindResource( "StatusDnd" ) ;
						}
						return _templateDnd ;
					}

				case ShowType.away:
					{
						if ( _templateAway == null )
						{
							_templateAway = ( ControlTemplate ) App.Instance.FindResource( "StatusAway" ) ;
						}
						return _templateAway ;
					}

				case ShowType.chat:
					{
						if ( _templateFreeForChat == null )
						{
							_templateFreeForChat = ( ControlTemplate ) App.Instance.FindResource( "StatusFreeForChat" ) ;
						}
						return _templateFreeForChat ;
					}

				case ShowType.xa:
					{
						if ( _templateXAway == null )
						{
							_templateXAway = ( ControlTemplate ) App.Instance.FindResource( "StatusXAway" ) ;
						}
						return _templateXAway ;
					}

				default:
					{
						if ( _templateOnline == null )
						{
							_templateOnline = ( ControlTemplate ) App.Instance.FindResource( "StatusOnline" ) ;
						}
						return _templateOnline ;
					}
			}
		}
	}
}