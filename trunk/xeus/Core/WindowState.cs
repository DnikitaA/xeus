using System ;
using System.ComponentModel ;
using System.Configuration ;
using System.Windows ;

namespace xeus.Core
{
	/// <summary>
	/// Persists a Window's Size, Location and WindowState to UserScopeSettings 
	/// </summary>
	public class WindowSettings
	{
		public class WindowApplicationSettings : ApplicationSettingsBase
		{
			WindowSettings _windowSettings ;

			public WindowApplicationSettings( WindowSettings windowSettings )
				: base( windowSettings.window.PersistId.ToString() )
			{
				_windowSettings = windowSettings ;
			}

			[ UserScopedSetting ]
			public Rect Location
			{
				get
				{
					if ( this[ "Location" ] != null )
					{
						return ( ( Rect ) this[ "Location" ] ) ;
					}
					return Rect.Empty ;
				}
				set
				{
					this[ "Location" ] = value ;
				}
			}

			[ UserScopedSetting ]
			public WindowState WindowState
			{
				get
				{
					if ( this[ "WindowState" ] != null )
					{
						return ( WindowState ) this[ "WindowState" ] ;
					}
					return WindowState.Normal ;
				}
				set
				{
					this[ "WindowState" ] = value ;
				}
			}
		}

		private Window window = null ;

		public WindowSettings( Window window )
		{
			this.window = window ;
		}

		public static readonly DependencyProperty SaveProperty
			= DependencyProperty.RegisterAttached( "Save", typeof ( bool ), typeof ( WindowSettings ),
			                                       new FrameworkPropertyMetadata(
			                                       	new PropertyChangedCallback( OnSaveInvalidated ) ) ) ;

		public static void SetSave( DependencyObject dependencyObject, bool enabled )
		{
			dependencyObject.SetValue( SaveProperty, enabled ) ;
		}

		/// <summary>
		/// Called when Save is changed on an object.
		/// </summary>
		private static void OnSaveInvalidated( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			Window window = dependencyObject as Window ;
			if ( window != null )
			{
				if ( ( bool ) e.NewValue )
				{
					WindowSettings settings = new WindowSettings( window ) ;
					settings.Attach() ;
				}
			}
		}

		/// <summary>
		/// Load the Window Size Location and State from the settings object
		/// </summary>
		protected virtual void LoadWindowState()
		{
			Settings.Reload() ;
			if ( Settings.Location != Rect.Empty )
			{
				window.Left = Settings.Location.Left ;
				window.Top = Settings.Location.Top ;
				window.Width = Settings.Location.Width ;
				window.Height = Settings.Location.Height ;
			}

			if ( Settings.WindowState != WindowState.Maximized )
			{
				window.WindowState = Settings.WindowState ;
			}
		}


		/// <summary>
		/// Save the Window Size, Location and State to the settings object
		/// </summary>
		protected virtual void SaveWindowState()
		{
			Settings.WindowState = window.WindowState ;
			Settings.Location = window.RestoreBounds ;
			Settings.Save() ;
		}

		private void Attach()
		{
			if ( window != null )
			{
				window.Closing += new CancelEventHandler( window_Closing ) ;
				window.Initialized += new EventHandler( window_Initialized ) ;
				window.Loaded += new RoutedEventHandler( window_Loaded ) ;
			}
		}

		private void window_Loaded( object sender, RoutedEventArgs e )
		{
			if ( Settings.WindowState == WindowState.Maximized )
			{
				window.WindowState = Settings.WindowState ;
			}
		}

		private void window_Initialized( object sender, EventArgs e )
		{
			LoadWindowState() ;
		}

		private void window_Closing( object sender, CancelEventArgs e )
		{
			SaveWindowState() ;
		}

		private WindowApplicationSettings windowApplicationSettings = null ;

		protected virtual WindowApplicationSettings CreateWindowApplicationSettingsInstance()
		{
			return new WindowApplicationSettings( this ) ;
		}

		[ Browsable( false ) ]
		public WindowApplicationSettings Settings
		{
			get
			{
				if ( windowApplicationSettings == null )
				{
					windowApplicationSettings = CreateWindowApplicationSettingsInstance() ;
				}
				return windowApplicationSettings ;
			}
		}
	}
}