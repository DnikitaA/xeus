using System ;
using System.Collections.Generic ;
using System.IO ;
using System.Net ;
using System.Net.Sockets ;
using System.Text ;
using System.Threading ;
using System.Windows ;
using System.Windows.Controls ;
using System.Windows.Threading ;
using agsXMPP ;
using agsXMPP.Collections ;
using agsXMPP.net ;
using agsXMPP.protocol.client ;
using agsXMPP.protocol.extensions.bytestreams ;
using agsXMPP.protocol.extensions.featureneg ;
using agsXMPP.protocol.extensions.filetransfer ;
using agsXMPP.protocol.extensions.si ;
using agsXMPP.protocol.x.data ;
using agsXMPP.Xml ;
using agsXMPP.Xml.Dom ;
using Microsoft.Win32 ;
using xeus.Core ;
using xeus.Properties ;
using File=agsXMPP.protocol.extensions.filetransfer.File;
using Uri=agsXMPP.Uri;

namespace xeus.Controls
{
	/// <summary>
	/// Interaction logic for FileTransfer.xaml
	/// </summary>
	public partial class FileTransfer : UserControl
	{
		private string _proxyUrl = Settings.Default.Client_ProxyUrl ;

		/// <summary>
		/// SID of the filetransfer
		/// </summary>
		private string _sid ;

		private Jid _from ;
		private Jid _to ;
		private DateTime _started ;
		private string _fileName = null ;

		private JEP65Socket _proxySocks5Socket ;
		private JEP65Socket _p2pSocks5Socket ;

		private XmppClientConnection _xmppConnection ;
		private bool m_DescriptionChanged = false ;

		private long _fileLength ;
		private long _bytesTransmitted = 0 ;
		private FileStream _fileStream ;
		private DateTime _lastProgressUpdate ;


		private File _file ;
		private SI _si ;
		private IQ _siIq ;

		private delegate void ProgressCallback() ;

		public FileTransfer()
		{
			InitializeComponent() ;

			Unloaded += new RoutedEventHandler( FileTransfer_Unloaded ) ;
		}

		public void Transfer( XmppClientConnection XmppCon, IQ iq )
		{
			InitializeComponent() ;
			/*cmdSend.Enabled = false;
            this.Text = "Receive File from " + iq.From.ToString();*/

			_siIq = iq ;
			_si = iq.SelectSingleElement( typeof ( SI ) ) as SI ;
			// get SID for file transfer
			_sid = _si.Id ;
			_from = iq.From ;

			_file = _si.File ;

			if ( _file != null )
			{
				_fileLength = _file.Size ;
				/*
                this.lblDescription.Text    = file.Description;
                this.lblFileName.Text       = file.Name;
                this.lblFileSize.Text       = HRSize(m_lFileLength);
                this.txtDescription.Visible = false;*/
			}

			_xmppConnection = XmppCon ;

			XmppCon.OnIq += new StreamHandler( XmppCon_OnIq ) ;
		}

		public void Transfer( XmppClientConnection XmppCon, Jid to )
		{
			//this.Text = "Send File to " + to.ToString();

			_to = to ;
			_xmppConnection = XmppCon ;

			/*
            // disable commadn buttons we don't need for sending a file
            cmdAccept.Enabled = false;
            cmdRefuse.Enabled = false;
			*/

			ChooseFileToSend() ;
		}

		private void ChooseFileToSend()
		{
			OpenFileDialog of = new OpenFileDialog() ;
			bool? result = of.ShowDialog() ;

			if ( result != null && result.Value )
			{
				_fileName = of.FileName ;

				//lblFileName.Text = m_FileName;
				FileInfo fi = new FileInfo( _fileName ) ;
				/*lblFileSize.Text = this.HRSize(fi.Length);
                lblFileName.Text = fi.Name;                
                lblDescription.Visible  = false;

                cmdSend.Enabled = true;*/
			}
		}

		private void SendSiIq()
		{
			/*            
            Recv: 
            <iq xmlns="jabber:client" from="gnauck@jabber.org/Psi" to="gnauck@ag-software.de/SharpIM" 
            type="set" id="aab4a"> <si xmlns="http://jabber.org/protocol/si" 
            profile="http://jabber.org/protocol/si/profile/file-transfer" id="s5b_405645b6f2b7c681"> <file 
            xmlns="http://jabber.org/protocol/si/profile/file-transfer" size="719" name="Kunden.dat"> <range /> 
            </file> <feature xmlns="http://jabber.org/protocol/feature-neg"> <x xmlns="jabber:x:data" type="form"> 
            <field type="list-single" var="stream-method"> <option> 
            <value>http://jabber.org/protocol/bytestreams</value> </option> </field> </x> </feature> </si> </iq> 

            Send: <iq xmlns="jabber:client" id="agsXMPP_5" to="gnauck@jabber.org/Psi" type="set">
             <si xmlns="http://jabber.org/protocol/si" id="af5a2e8d-58d4-4038-8732-7fb348ff767f">
             <file xmlns="http://jabber.org/protocol/si/profile/file-transfer" name="ALEX1.JPG" size="22177"><range /></file>
             <feature xmlns="http://jabber.org/protocol/feature-neg"><x xmlns="jabber:x:data" type="form">
            <field type="list-single" var="stream-method"><option>http://jabber.org/protocol/bytestreams</option></field></x></feature></si></iq>
           

            Send:
            <iq xmlns="jabber:client" id="aab4a" to="gnauck@jabber.org/Psi" type="result"><si 
            xmlns="http://jabber.org/protocol/si" id="s5b_405645b6f2b7c681"><feature 
            xmlns="http://jabber.org/protocol/feature-neg"><x xmlns="jabber:x:data" type="submit"><field 
            var="stream-
            method"><value>http://jabber.org/protocol/bytestreams</value></field></x></feature></si></iq> 


            Recv:
            <iq xmlns="jabber:client" from="gnauck@jabber.org/Psi" to="gnauck@ag-software.de/SharpIM" 
            type="set" id="aab6a"> <query xmlns="http://jabber.org/protocol/bytestreams" sid="s5b_405645b6f2b7c681" 
            mode="tcp"> <streamhost port="8010" jid="gnauck@jabber.org/Psi" host="192.168.74.142" /> <streamhost 
            port="7777" jid="proxy.ag-software.de" host="82.165.34.23"> <proxy 
            xmlns="http://affinix.com/jabber/stream" /> </streamhost> <fast xmlns="http://affinix.com/jabber/stream" 
            /> </query> </iq> 


            Send:
            <iq xmlns="jabber:client" type="result" to="gnauck@jabber.org/Psi" id="aab6a"><query 
            xmlns="http://jabber.org/protocol/bytestreams"><streamhost-used jid="gnauck@jabber.org/Psi" 
            /></query></iq>            
            */

			SIIq iq = new SIIq() ;
			iq.To = _to ;
			iq.Type = IqType.set ;

			_fileLength = new FileInfo( _fileName ).Length ;

			File afile ;
			afile = new File(
				Path.GetFileName( _fileName ), _fileLength ) ;
			/* if (m_DescriptionChanged)
                file.Description = txtDescription.Text;*/
			afile.Range = new Range() ;


			FeatureNeg fNeg = new FeatureNeg() ;

			Data data = new Data( XDataFormType.form ) ;
			Field f = new Field( FieldType.List_Single ) ;
			f.Var = "stream-method" ;
			f.AddOption().SetValue( Uri.BYTESTREAMS ) ;
			data.AddField( f ) ;

			fNeg.Data = data ;

			iq.SI.File = afile ;
			iq.SI.FeatureNeg = fNeg ;
			iq.SI.Profile = Uri.SI_FILE_TRANSFER ;

			_sid = Guid.NewGuid().ToString() ;
			iq.SI.Id = _sid ;

			_xmppConnection.IqGrabber.SendIq( iq, new IqCB( SiIqResult ), null ) ;
		}

		private void SiIqResult( object sender, IQ iq, object data )
		{
			// Parse the result of the form
			if ( iq.Type == IqType.result )
			{
				// <iq xmlns="jabber:client" id="aab4a" to="gnauck@jabber.org/Psi" type="result"><si 
				// xmlns="http://jabber.org/protocol/si" id="s5b_405645b6f2b7c681"><feature 
				// xmlns="http://jabber.org/protocol/feature-neg"><x xmlns="jabber:x:data" type="submit"><field 
				// var="stream-
				// method"><value>http://jabber.org/protocol/bytestreams</value></field></x></feature></si></iq> 
				SI si = iq.SelectSingleElement( typeof ( SI ) ) as SI ;
				if ( si != null )
				{
					FeatureNeg fNeg = si.FeatureNeg ;
					if ( SelectedByteStream( fNeg ) )
					{
						SendStreamHosts() ;
					}
				}
			}
			else if ( iq.Type == IqType.error )
			{
				Error err = iq.Error ;
				if ( err != null )
				{
					switch ( ( int ) err.Code )
					{
						case 403:
							// MessageBox.Show("The file was rejected by the remote user", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
							break ;
					}
				}
			}
		}

		private void _socket_OnDisconnect( object sender )
		{
		}

		private void _socket_OnConnect( object sender )
		{
		}

		#region << Send Streamhosts >>

		private void SendStreamHosts()
		{
			/*
             Recv:
            <iq xmlns="jabber:client" from="gnauck@jabber.org/Psi" to="gnauck@ag-software.de/SharpIM" 
            type="set" id="aab6a"> <query xmlns="http://jabber.org/protocol/bytestreams" sid="s5b_405645b6f2b7c681" 
            mode="tcp"> <streamhost port="8010" jid="gnauck@jabber.org/Psi" host="192.168.74.142" />
            <streamhost port="7777" jid="proxy.ag-software.de" host="82.165.34.23">
                <proxy xmlns="http://affinix.com/jabber/stream" />
            </streamhost>
            <fast xmlns="http://affinix.com/jabber/stream" /> </query> </iq> 
            */

			ByteStreamIq bsIq = new ByteStreamIq() ;
			bsIq.To = _to ;
			bsIq.Type = IqType.set ;

			bsIq.Query.Sid = _sid ;

			string hostname = Dns.GetHostName() ;

			IPHostEntry iphe = Dns.Resolve( hostname ) ;

			for ( int i = 0; i < iphe.AddressList.Length; i++ )
			{
				Console.WriteLine( "IP address: {0}", iphe.AddressList[ i ].ToString() ) ;
				bsIq.Query.AddStreamHost( _xmppConnection.MyJID, iphe.AddressList[ i ].ToString(), 1000 ) ;
			}

			bsIq.Query.AddStreamHost( new Jid( _proxyUrl ), _proxyUrl, 7777 ) ;

			_p2pSocks5Socket = new JEP65Socket() ;
			_p2pSocks5Socket.Initiator = _xmppConnection.MyJID ;
			_p2pSocks5Socket.Target = _to ;
			_p2pSocks5Socket.SID = _sid ;
			_p2pSocks5Socket.OnConnect += new ObjectHandler( _socket_OnConnect ) ;
			_p2pSocks5Socket.OnDisconnect += new ObjectHandler( _socket_OnDisconnect ) ;
			_p2pSocks5Socket.Listen( 1000 ) ;


			_xmppConnection.IqGrabber.SendIq( bsIq, new IqCB( SendStreamHostsResult ), null ) ;
		}

		private void SendStreamHostsResult( object sender, IQ iq, object data )
		{
			//  <iq xmlns="jabber:client" type="result" to="gnauck@jabber.org/Psi" id="aab6a">
			//      <query xmlns="http://jabber.org/protocol/bytestreams">
			//          <streamhost-used jid="gnauck@jabber.org/Psi" />
			//      </query>
			//  </iq>          
			if ( iq.Type == IqType.result )
			{
				ByteStream bs = iq.Query as ByteStream ;
				if ( bs != null )
				{
					Jid sh = bs.StreamHostUsed.Jid ;
					if ( sh != null & sh.Equals( _xmppConnection.MyJID, new FullJidComparer() ) )
					{
						// direct connection
						SendFile( null ) ;
					}
					if ( sh != null & sh.Equals( new Jid( _proxyUrl ), new FullJidComparer() ) )
					{
						_p2pSocks5Socket = new JEP65Socket() ;
						_p2pSocks5Socket.Address = _proxyUrl ;
						_p2pSocks5Socket.Port = 7777 ;
						_p2pSocks5Socket.Target = _to ;
						_p2pSocks5Socket.Initiator = _xmppConnection.MyJID ;
						_p2pSocks5Socket.SID = _sid ;
						_p2pSocks5Socket.ConnectTimeout = 5000 ;
						_p2pSocks5Socket.SyncConnect() ;

						if ( _p2pSocks5Socket.Connected )
						{
							ActivateBytestream( new Jid( _proxyUrl ) ) ;
						}
					}
				}
			}
		}

		#endregion

		#region << Activate ByteStream >>

		/*
            4.9 Activation of Bytestream

            In order for the bytestream to be used, it MUST first be activated by the StreamHost. If the StreamHost is the Initiator, this is straightforward and does not require any in-band protocol. However, if the StreamHost is a Proxy, the Initiator MUST send an in-band request to the StreamHost. This is done by sending an IQ-set to the Proxy, including an <activate/> element whose XML character data specifies the full JID of the Target.

            Example 17. Initiator Requests Activation of Bytestream

            <iq type='set' 
                from='initiator@host1/foo' 
                to='proxy.host3' 
                id='activate'>
              <query xmlns='http://jabber.org/protocol/bytestreams' sid='mySID'>
                <activate>target@host2/bar</activate>
              </query>
            </iq>
                

            Using this information, with the SID and from address on the packet, the Proxy is able to activate the stream by hashing the SID + Initiator JID + Target JID. This provides a reasonable level of trust that the activation request came from the Initiator.

            If the Proxy can fulfill the request, it MUST then respond to the Initiator with an IQ-result.

            Example 18. Proxy Informs Initiator of Activation

            <iq type='result' 
                from='proxy.host3' 
                to='initiator@host1/foo' 
                id='activate'/>   
         
        */

		private void ActivateBytestream( Jid streamHost )
		{
			ByteStreamIq bsIq = new ByteStreamIq() ;

			bsIq.To = streamHost ;
			bsIq.Type = IqType.set ;

			bsIq.Query.Sid = _sid ;
			bsIq.Query.Activate = new Activate( _to ) ;

			_xmppConnection.IqGrabber.SendIq( bsIq, new IqCB( ActivateBytestreamResult ), null ) ;
		}

		private void ActivateBytestreamResult( object sender, IQ iq, object dat )
		{
			if ( iq.Type == IqType.result )
			{
				SendFile( null ) ;
			}
		}

		#endregion

		/// <summary>
		/// Sends the file Async
		/// </summary>
		/// <param name="ar"></param>
		private void SendFile( IAsyncResult ar )
		{
			const int BUFFERSIZE = 1024 ;
			byte[] buffer = new byte[BUFFERSIZE] ;
			FileStream fs ;
			// AsyncResult is null when we call this function the 1st time
			if ( ar == null )
			{
				_started = DateTime.Now ;
				fs = new FileStream( _fileName, FileMode.Open ) ;
			}
			else
			{
				if ( _p2pSocks5Socket.Socket.Connected )
				{
					_p2pSocks5Socket.Socket.EndReceive( ar ) ;
				}

				fs = ar.AsyncState as FileStream ;

				// Windows Forms are not Thread Safe, we need to invoke this :(
				// We're not in the UI thread, so we need to call BeginInvoke
				// to udate the progress bar
				TimeSpan ts = DateTime.Now - _lastProgressUpdate ;
				if (ts.Seconds >= 1)
                {
					UpdateProgress() ;
				}
			}

			int len = fs.Read( buffer, 0, BUFFERSIZE ) ;
			_bytesTransmitted += len ;

			if ( len > 0 )
			{
				_p2pSocks5Socket.Socket.BeginSend( buffer, 0, len, SocketFlags.None, SendFile, fs ) ;
			}
			else
			{
				// Update Pogress when finished
				UpdateProgress() ;

				fs.Close() ;
				fs.Dispose() ;
				if ( _p2pSocks5Socket != null && _p2pSocks5Socket.Connected )
				{
					_p2pSocks5Socket.Disconnect() ;
				}
			}
		}

		private void XmppCon_OnIq( object sender, Node e )
		{
			//if (InvokeRequired)
			//{
			//    // Windows Forms are not Thread Safe, we need to invoke this :(
			//    // We're not in the UI thread, so we need to call BeginInvoke				
			//    BeginInvoke(new StreamHandler(XmppCon_OnIq), new object[] { sender, e });
			//    return;
			//}

			// <iq xmlns="jabber:client" from="gnauck@jabber.org/Psi" to="gnauck@ag-software.de/SharpIM" type="set" id="aac9a">
			//  <query xmlns="http://jabber.org/protocol/bytestreams" sid="s5b_8596bde0de321957" mode="tcp">
			//   <streamhost port="8010" jid="gnauck@jabber.org/Psi" host="192.168.74.142" />
			//   <streamhost port="7777" jid="proxy.ag-software.de" host="82.165.34.23"> 
			//   <proxy xmlns="http://affinix.com/jabber/stream" /> </streamhost> 
			//   <fast xmlns="http://affinix.com/jabber/stream" /> 
			//  </query> 
			// </iq> 

			IQ iq = e as IQ ;

			if ( iq.Query != null && iq.Query.GetType() == typeof ( ByteStream ) )
			{
				ByteStream bs = iq.Query as ByteStream ;
				// check is this is for the correct file
				if ( bs.Sid == _sid )
				{
					Thread t = new Thread(
						delegate() { HandleStreamHost( bs, iq ) ; }
						) ;
					t.Name = "LoopStreamHosts" ;
					t.Start() ;
				}
			}
		}

		protected void Ok( object sender, EventArgs e )
		{
			/*cmdAccept.Enabled = false;
            cmdRefuse.Enabled = false;
		   */
			FeatureNeg fNeg = _si.FeatureNeg ;
			if ( fNeg != null )
			{
				Data data = fNeg.Data ;
				if ( data != null )
				{
					Field[] field = data.GetFields() ;
					if ( field.Length == 1 )
					{
						Dictionary< string, string > methods = new Dictionary< string, string >() ;
						foreach ( Option o in field[ 0 ].GetOptions() )
						{
							string val = o.GetValue() ;
							methods.Add( val, val ) ;
						}
						if ( methods.ContainsKey( Uri.BYTESTREAMS ) )
						{
							// supports bytestream, so choose this option
							SIIq sIq = new SIIq() ;
							sIq.Id = _siIq.Id ;
							sIq.To = _siIq.From ;
							sIq.Type = IqType.result ;

							sIq.SI.Id = _si.Id ;
							sIq.SI.FeatureNeg = new FeatureNeg() ;

							Data xdata = new Data() ;
							xdata.Type = XDataFormType.submit ;
							Field f = new Field() ;
							//f.Type = FieldType.List_Single;
							f.Var = "stream-method" ;
							f.AddValue( Uri.BYTESTREAMS ) ;
							xdata.AddField( f ) ;
							sIq.SI.FeatureNeg.Data = xdata ;

							_xmppConnection.Send( sIq ) ;
						}
					}
				}
			}
		}

		protected void OnSend( object sender, EventArgs e )
		{
		}

		protected void OnDeny( object sender, EventArgs e )
		{
			/*         
            / from PSI ??? i don't think this is correct
            <iq from="gnauck@jabber.org/Psi" type="error" id="aabaa" to="gnauck@myjabber.net/Psi" >
                <error code="403" >Declined</error>
            </iq>         
            
            <iq type='error' to='sender@jabber.org/resource' id='offer1'>
              <error code='403' type='cancel>
                <forbidden xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>
                <text xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'>Offer Declined</text>
              </error>
            </iq>
            
            Exodus
            <iq id="agsXMPP_5" to="gnauck@ag-software.de/SharpIM" type="error">
                <error code="404" type="cancel">
                    <condition xmlns="urn:ietf:params:xml:ns:xmpp-stanzas">
                        <item-not-found/>
                    </condition>
                </error>
            </iq>
            
            Spark              
            <iq xmlns="jabber:client" from="gnauck@jabber.org/Spark" to="gnauck@ag-software.de/SharpIM" type="error" id="agsXMPP_5">
                <error code="403" />
            </iq> 
            
            
            Example 8. Rejecting Stream Initiation

            <iq type='error' to='sender@jabber.org/resource' id='offer1'>
              <error code='403' type='cancel>
                <forbidden xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>
                <text xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'>Offer Declined</text>
              </error>
            </iq>
    
            */
			IQ iq = new IQ() ;

			iq.To = _siIq.From ;
			iq.Id = _siIq.Id ;
			iq.Type = IqType.error ;

			iq.Error = new Error( ErrorCondition.Forbidden ) ;
			iq.Error.Code = ErrorCode.Forbidden ;
			iq.Error.Type = ErrorType.cancel ;


			_xmppConnection.Send( iq ) ;

			// this.Close();
		}

		private void HandleStreamHost( ByteStream bs, IQ iq )
			//private void HandleStreamHost(object obj)
		{
			//IQ iq = obj as IQ;
			//ByteStream bs = iq.Query as agsXMPP.protocol.extensions.bytestreams.ByteStream;;
			//ByteStream bs = iq.Query as ByteStream;
			if ( bs != null )
			{
				_proxySocks5Socket = new JEP65Socket() ;
				_proxySocks5Socket.OnConnect += new ObjectHandler( m_s5Sock_OnConnect ) ;
				_proxySocks5Socket.OnReceive += new BaseSocket.OnSocketDataHandler( m_s5Sock_OnReceive ) ;
				_proxySocks5Socket.OnDisconnect += new ObjectHandler( m_s5Sock_OnDisconnect ) ;

				StreamHost[] streamhosts = bs.GetStreamHosts() ;
				//Scroll through the possible sock5 servers and try to connect
				//foreach (StreamHost sh in streamhosts)
				//this is done back to front in order to make sure that the proxy JID is chosen first
				//this is necessary as at this stage the application only knows how to connect to a 
				//socks 5 proxy.

				foreach ( StreamHost sHost in streamhosts )
				{
					if ( sHost.Host != null )
					{
						_proxySocks5Socket.Address = sHost.Host ;
						_proxySocks5Socket.Port = sHost.Port ;
						_proxySocks5Socket.Target = _xmppConnection.MyJID ;
						_proxySocks5Socket.Initiator = _from ;
						_proxySocks5Socket.SID = _sid ;
						_proxySocks5Socket.ConnectTimeout = 5000 ;
						_proxySocks5Socket.SyncConnect() ;
						if ( _proxySocks5Socket.Connected )
						{
							SendStreamHostUsedResponse( sHost, iq ) ;
							break ;
						}
					}
				}
			}
		}

		private void m_s5Sock_OnDisconnect( object sender )
		{
			_fileStream.Close() ;
			_fileStream.Dispose() ;

			if ( _bytesTransmitted == _fileLength )
			{
				// completed
				//tslTransmitted.Text = "completed";
				// Update Progress when complete
				UpdateProgress() ;
			}
			else
			{
				App.Instance.Window.AlertError( "File Transfer Error", "Connection lost." ) ;
			}
		}

		private void m_s5Sock_OnReceive( object sender, byte[] data, int count )
		{
			_fileStream.Write( data, 0, count ) ;

			_bytesTransmitted += count ;


			// Windows Forms are not Thread Safe, we need to invoke this :(
			// We're not in the UI thread, so we need to call BeginInvoke
			// to udate the progress bar	
			TimeSpan ts = DateTime.Now - _lastProgressUpdate ;

			if ( ts.Seconds >= 1 )
			{
				UpdateProgress() ;
			}
		}

		private void UpdateProgress()
		{
			if ( App.DispatcherThread.CheckAccess() )
			{
				_lastProgressUpdate = DateTime.Now ;
				double percent = ( double ) _bytesTransmitted / ( double ) _fileLength * 100 ;
				_progress.Value = percent ;
				/*
            tslRate.Text = GetHRByteRateString();
            tslTransmitted.Text = HRSize(m_bytesTransmitted);
            tslRemaining.Text = GetHRRemainingTime();*/
			}
			else
			{
				App.DispatcherThread.BeginInvoke( DispatcherPriority.Normal,
				                                  new ProgressCallback( UpdateProgress ) ) ;
			}
		}

		private void m_s5Sock_OnConnect( object sender )
		{
			_started = DateTime.Now ;

			string path = Storage.GetRecievedFolder().FullName ;
			Directory.CreateDirectory( path ) ;

			_fileStream = new FileStream( Path.Combine( path, _file.Name ), FileMode.Create ) ;

			//throw new Exception("The method or operation is not implemented.");
		}

		private void SendStreamHostUsedResponse( StreamHost sh, IQ iq )
		{
			ByteStreamIq bsIQ = new ByteStreamIq( IqType.result, _from ) ;
			bsIQ.Id = iq.Id ;

			bsIQ.Query.StreamHostUsed = new StreamHostUsed( sh.Jid ) ;
			_xmppConnection.Send( bsIQ ) ;
		}

		private bool SelectedByteStream( FeatureNeg fn )
		{
			if ( fn != null )
			{
				Data data = fn.Data ;
				if ( data != null )
				{
					foreach ( Field field in data.GetFields() )
					{
						if ( field != null && field.Var == "stream-method" )
						{
							if ( field.GetValue() == Uri.BYTESTREAMS )
							{
								return true ;
							}
						}
					}
				}
			}
			return false ;
		}

		private void FileTransfer_Unloaded( object sender, RoutedEventArgs e )
		{

			_xmppConnection.OnIq -= new StreamHandler( XmppCon_OnIq ) ;
		}

		#region << Helper Function >>

		/// <summary>
		/// Converts the Numer of bytes to a human readable string
		/// </summary>
		/// <param name="lBytes"></param>
		/// <returns></returns>
		public string HRSize( long lBytes )
		{
			StringBuilder sb = new StringBuilder() ;
			string strUnits = "Bytes" ;
			float fAdjusted = 0.0F ;

			if ( lBytes > 1024 )
			{
				if ( lBytes < 1024 * 1024 )
				{
					strUnits = "KB" ;
					fAdjusted = Convert.ToSingle( lBytes ) / 1024 ;
				}
				else
				{
					strUnits = "MB" ;
					fAdjusted = Convert.ToSingle( lBytes ) / 1048576 ;
				}
				sb.AppendFormat( "{0:0.0} {1}", fAdjusted, strUnits ) ;
			}
			else
			{
				fAdjusted = Convert.ToSingle( lBytes ) ;
				sb.AppendFormat( "{0:0} {1}", fAdjusted, strUnits ) ;
			}

			return sb.ToString() ;
		}

		private long GetBytePerSecond()
		{
			TimeSpan ts = DateTime.Now - _started ;
			double dBytesPerSecond = _bytesTransmitted / ts.TotalSeconds ;

			return ( long ) dBytesPerSecond ;
		}

		private string GetHRByteRateString()
		{
			TimeSpan ts = DateTime.Now - _started ;

			if ( ts.TotalSeconds != 0 )
			{
				double dBytesPerSecond = _bytesTransmitted / ts.TotalSeconds ;
				long lBytesPerSecond = Convert.ToInt64( dBytesPerSecond ) ;
				return HRSize( lBytesPerSecond ) + "/s" ;
			}
			else
			{
				// to fast to calculate a bitrate (0 seconds)
				return HRSize( 0 ) + "/s" ;
			}
		}

		private string GetHRRemainingTime()
		{
			float fRemaingTime = 0 ;
			float fTotalNumberOfBytes = _fileLength ;
			float fPartialNumberOfBytes = _bytesTransmitted ;
			float fBytesPerSecond = GetBytePerSecond() ;

			StringBuilder sb = new StringBuilder() ;

			if ( fBytesPerSecond != 0 )
			{
				fRemaingTime = ( fTotalNumberOfBytes - fPartialNumberOfBytes ) / fBytesPerSecond ;
			}

			TimeSpan ts = TimeSpan.FromSeconds( fRemaingTime ) ;

			return String.Format( "{0:00}h {1:00}m {2:00}s",
			                      ts.Hours, ts.Minutes, ts.Seconds ) ;
		}

		#endregion

		private void cmdSend_Click( object sender, EventArgs e )
		{
			SendSiIq() ;
			// Disable the Send button, because we can send this file only once
			// cmdSend.Enabled = false;
		}

		private void txtDescription_TextChanged( object sender, EventArgs e )
		{
			m_DescriptionChanged = true ;
		}
	}
}