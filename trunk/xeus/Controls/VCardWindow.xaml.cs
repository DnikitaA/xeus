using System ;
using System.Collections.Generic ;
using System.IO ;
using System.Windows ;
using System.Windows.Controls ;
using System.Windows.Media ;
using System.Windows.Media.Imaging ;
using xeus.Core ;

namespace xeus.Controls
{
	/// <summary>
	/// Interaction logic for VCardWindow.xaml
	/// </summary>
	public partial class VCardWindow
	{
		public enum FileType
		{
			Video,
			Image,
			NotSupported
		}

		private static List< VCardWindow > _windows = new List< VCardWindow >() ;

		public static void CloseAllWindows()
		{
			VCardWindow[] vCardWindows = new VCardWindow[_windows.Count] ;
			_windows.CopyTo( vCardWindows, 0 ) ;

			foreach ( VCardWindow vCardWindow in vCardWindows )
			{
				vCardWindow.Close() ;
			}

			_windows.Clear() ;
		}

		public VCardWindow()
		{
			InitializeComponent() ;
		}

		internal static void ShowWindow( RosterItem rosterItem )
		{
			foreach ( VCardWindow vCardWindow in _windows )
			{
				if ( ( ( RosterItem ) vCardWindow.DataContext ).Key == rosterItem.Key )
				{
					vCardWindow.Activate() ;
					return ;
				}
			}

			VCardWindow vcardWindow = new VCardWindow() ;

			vcardWindow.DataContext = rosterItem ;

			vcardWindow._image.Source = rosterItem.Image ;
			_windows.Add( vcardWindow ) ;

			vcardWindow.Show() ;
		}

		protected override void OnClosed( EventArgs e )
		{
			base.OnClosed( e ) ;

			_windows.Remove( this ) ;
		}

		protected void OnPublish( object sender, EventArgs e )
		{
			RosterItem rosterItem = ( RosterItem ) DataContext ;
			
			rosterItem.RemoveTemporaryImage = false ;
			
			if ( string.IsNullOrEmpty( rosterItem.ImageFileName )
					&& rosterItem.Image != null )
			{
				rosterItem.ImageFileName = Storage.FlushImage( rosterItem.Key ) ;
				rosterItem.RemoveTemporaryImage = true ;
			}

			Client.Instance.Roster.PublishMyVCard() ;
			Close() ;
		}

		protected void OnDropFile( object sender, DragEventArgs e )
		{
			string[] fileNames = e.Data.GetData( DataFormats.FileDrop, true ) as string[] ;

			if ( fileNames.Length > 0 )
			{
				string fileName = fileNames[ 0 ] ;

				FileType type = GetFileType( fileName ) ;

				// Handles image files
				if ( type == FileType.Image )
				{
					// Open a Uri and decode a JPG image
					Uri uri = new Uri( fileName, UriKind.RelativeOrAbsolute ) ;

					BitmapImage bitmapImage = new BitmapImage( uri );

					_image.Source = bitmapImage ;

					RosterItem rosterItem = ( RosterItem ) DataContext ;
					rosterItem.Image = bitmapImage ;
					rosterItem.ImageFileName = fileName ;
				}
			}

			// Mark the event as handled, so the control's native Drop handler is not called.
			e.Handled = true ;
		}

		protected void OnDragOver( object sender, DragEventArgs e )
		{
			e.Effects = DragDropEffects.None ;

			string[] fileNames = e.Data.GetData( DataFormats.FileDrop, true ) as string[] ;

			foreach ( string fileName in fileNames )
			{
				FileType type = GetFileType( fileName ) ;

				// Only Image files are supported
				if ( type == FileType.Image )
				{
					e.Effects = DragDropEffects.Copy ;
				}
			}

			// Mark the event as handled, so control's native DragOver handler is not called.
			e.Handled = true ;
		}

		public FileType GetFileType( string fileName )
		{
			string extension = Path.GetExtension( fileName ).ToLower() ;

			switch ( extension.ToUpper() )
			{
				case ".JPG":
				case ".JPEG":
				case ".GIF":
				case ".PNG":
					{
						return FileType.Image ;
					}
			}

			return FileType.NotSupported ;
		}

		protected void OnClose( object sender, EventArgs e )
		{
			Close() ;
		}
	}
}