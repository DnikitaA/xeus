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
			Client.Instance.Services.RegisterService( ( ( Button )sender ).DataContext as ServiceItem );
		}
	}
}
