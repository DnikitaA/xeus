using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls ;
using xeus.Core ;

namespace xeus.Controls
{
	partial class ServiceResources
	{
		void OnServiceClick( object sender, EventArgs e )
		{
			ServiceItem serviceItem = ( ( Button )sender ).DataContext as ServiceItem ;

			Client.Instance.Services.RegisterService( serviceItem ) ;
			/*
			if ( !serviceItem.IsRegistered )
			{
				RegisterWindow registerWindow = new RegisterWindow() ;

				registerWindow.ShowDialog() ;

				if ( registerWindow.DialogResult.HasValue && registerWindow.DialogResult.Value )
				{
					Client.Instance.Services.RegisterService( serviceItem,
					                                          registerWindow.UserName, registerWindow.Password ) ;
				}
			}*/
		}
	}
}
