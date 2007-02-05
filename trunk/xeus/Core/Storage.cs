using System ;
using System.IO ;
using System.Reflection ;
using System.Windows ;
using System.Windows.Media.Imaging ;
using agsXMPP.protocol.iq.vcard ;

namespace xeus.Core
{
	internal static class Storage
	{
		private static string _folder ;
		private static BitmapImage _defaultAvatar ;

		static Storage()
		{
			string path = Assembly.GetExecutingAssembly().Location ;

			FileInfo fileInfo = new FileInfo( path ) ;
			_folder = fileInfo.DirectoryName ;
		}

		private static DirectoryInfo GetAvatarCacheFolder()
		{
			DirectoryInfo directoryInfo = new DirectoryInfo( _folder + "\\AvatarCache" ) ;

			if ( !directoryInfo.Exists )
			{
				directoryInfo.Create() ;
			}

			return directoryInfo ;
		}

		public static void CacheAvatar( string jid, Photo photo )
		{
			try
			{
				DirectoryInfo directoryInfo = GetAvatarCacheFolder() ;

				using ( FileStream fileStream = new FileStream( directoryInfo.FullName + "\\" + jid,
				                                                FileMode.Create, FileAccess.Write, FileShare.None ) )
				{
					byte[] photoSource = Convert.FromBase64String( photo.GetTag( "BINVAL" ) ) ;
					fileStream.Write( photoSource, 0, photoSource.Length ) ;
				}
			}

			catch ( Exception e )
			{
				Client.Instance.Log( e.Message ) ; 
			}
		}

		public static BitmapImage GetAvatar( string jid )
		{
			DirectoryInfo directoryInfo = GetAvatarCacheFolder() ;

			try
			{
				using ( FileStream fileStream = new FileStream( directoryInfo.FullName + "\\" + jid,
				                                                FileMode.Open, FileAccess.Read, FileShare.Read ) )
				{
					BitmapImage bitmap = new BitmapImage() ;
					bitmap.BeginInit() ;
					bitmap.StreamSource = fileStream ;
					bitmap.EndInit() ;
					return bitmap ;
				}
			}

			catch ( Exception e )
			{
				Client.Instance.Log( e.Message ) ;
				
				return GetDefaultAvatar() ;
			}
		}

		public static BitmapImage GetDefaultAvatar()
		{
			if ( _defaultAvatar != null )
			{
				return _defaultAvatar ;

			}

			try
			{
				Uri uri = new Uri("pack://application:,,,/Images/Avatar.png", UriKind.Absolute);
				
				using ( Stream stream = Application.GetResourceStream( uri ).Stream )
				{
					_defaultAvatar = new BitmapImage() ;
					_defaultAvatar.BeginInit() ;
					_defaultAvatar.StreamSource = stream ;
					_defaultAvatar.EndInit() ;
				}

				return _defaultAvatar ;
			}

			catch ( Exception e )
			{
				Client.Instance.Log( e.Message ) ;
				return null ; 
			}
		}


		public static BitmapImage ImageFromPhoto( Photo photo )
		{
			try
			{
				if ( photo.HasTag( "BINVAL" ) )
				{
					byte[] pic = Convert.FromBase64String( photo.GetTag( "BINVAL" ) ) ;
					MemoryStream memoryStream = new MemoryStream( pic, 0, pic.Length ) ;
					BitmapImage bitmap = new BitmapImage() ;
					bitmap.BeginInit() ;
					bitmap.StreamSource = memoryStream ;
					bitmap.EndInit() ;
					return bitmap ;
				}
				else if ( photo.HasTag( "EXTVAL" ) )
				{
					BitmapImage bitmap = new BitmapImage() ;
					bitmap.BeginInit() ;
					bitmap.CacheOption = BitmapCacheOption.OnLoad;
					bitmap.UriSource = new Uri( photo.GetTag( "EXTVAL" ) ) ;
					bitmap.EndInit() ;

					return bitmap ;
				}
				else if ( photo.TextBase64.Length > 0 )
				{
					byte[] pic = Convert.FromBase64String( photo.Value ) ;
					MemoryStream memoryStream = new MemoryStream( pic, 0, pic.Length ) ;
					BitmapImage bitmap = new BitmapImage() ;
					bitmap.BeginInit() ;
					bitmap.StreamSource = memoryStream ;
					bitmap.EndInit() ;
					
					return bitmap ;
				}
				else
				{
					return null ;
				}
			}

			catch
			{
				return null ;
			}			
		}
	}
}