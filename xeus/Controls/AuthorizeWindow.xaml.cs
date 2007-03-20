using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using agsXMPP ;
using xeus.Core ;

namespace xeus.Controls
{
	/// <summary>
	/// Interaction logic for AddUser.xaml
	/// </summary>

	public partial class AuthorizeWindow : WindowBase
	{
		static List< AuthorizeWindow > _windows = new List< AuthorizeWindow >();

		private RosterItem _rosterItem ;
		private Jid _jid ;

		internal static void ShowWindow( RosterItem rosterItem )
		{
			AuthorizeWindow authorizeWindow = new AuthorizeWindow() ;

			authorizeWindow._rosterItem = rosterItem ;

			authorizeWindow._image.Source = rosterItem.Image ;
			authorizeWindow._titleReason.Text =
				string.Format( "Contact '{0}' ({1}) is asking you for Authorization.", rosterItem.DisplayName, rosterItem.Key ) ;
			_windows.Add( authorizeWindow );

			authorizeWindow.Show();
		}

		internal static void ShowWindow( Jid jid )
		{
			AuthorizeWindow authorizeWindow = new AuthorizeWindow() ;

			authorizeWindow._jid = jid ;

			authorizeWindow._image.Source = Storage.GetDefaultAvatar() ;
			authorizeWindow._titleReason.Text =
				string.Format( "Contact '{0}' ({1}) is asking you for Authorization.", jid.User, jid.Bare ) ;
			_windows.Add( authorizeWindow );

			authorizeWindow.Show();
		}

		public static void CloseAllWindows()
		{
			AuthorizeWindow [] authorizeWindows = new AuthorizeWindow[ _windows.Count ];
			_windows.CopyTo( authorizeWindows, 0 );

			foreach ( AuthorizeWindow authorizeWindow in authorizeWindows )
			{
				authorizeWindow.Close();
			}

			_windows.Clear();
		}

		protected override void OnClosed( EventArgs e )
		{
			base.OnClosed( e );

			_windows.Remove( this ) ;
		}

		public AuthorizeWindow()
		{
			InitializeComponent();
		}

		protected void Ok( object sender, EventArgs e )
		{
			if ( _rosterItem != null )
			{
				Client.Instance.SubscribePresence( _rosterItem.XmppRosterItem.Jid, true ) ;
			}
			else
			{
				Client.Instance.SubscribePresence( _jid, true ) ;
			}

			Close() ;
		}

		protected void OnDeny( object sender, EventArgs e )
		{
			if ( _rosterItem != null )
			{
				Client.Instance.SubscribePresence( _rosterItem.XmppRosterItem.Jid, false ) ;
			}
			else
			{
				Client.Instance.SubscribePresence( _jid, false ) ;
			}

			Close() ;
		}
	}
}