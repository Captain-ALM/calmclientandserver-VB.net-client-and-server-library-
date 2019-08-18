Imports System.Net
Imports System.Net.Sockets
'
' Created by SharpDevelop.
' User: Alfred
' Date: 20/05/2019
' Time: 10:28
' 
' To change this template use Tools | Options | Coding | Edit Standard Headers.
'
Namespace CALMNetLib
    ''' <summary>
    ''' This is a NetUDPClient socket.
    ''' </summary>
    ''' <remarks></remarks>
	Public Class NetUDPClient
		Implements INetSocketConnectionless, INetConfig, IDisposable
		
		Protected _sock As Socket = Nothing
		Protected _rip As IPAddress = Nothing
		Protected _rport As Integer = 0
		Protected _lip As IPAddress = Nothing
		Protected _lport As Integer = 0
		Protected _l As Boolean = False
		Protected _uc As Boolean = False
		Protected _c As Boolean = False
		Protected slocksend As New Object()
		Protected slockreceive As New Object()
		Protected slocksockman As New Object()
        ''' <summary>
        ''' Constructs a new NetUDPClient Instance on the specified IP Address, port and specification.
        ''' </summary>
        ''' <param name="IP">The IP Address</param>
        ''' <param name="Port">The Port</param>
        ''' <param name="specification">The Specification Mode</param>
        ''' <remarks></remarks>
		Public Sub New(IP As IPAddress, Port As Integer, specification As UDPIPPortSpecification)
			_sock = New Socket(IP.AddressFamily, SocketType.Dgram, ProtocolType.Udp)
			_sock.ReceiveTimeout = 0
			_sock.SendTimeout = 0
			If specification = UDPIPPortSpecification.Local Then
				_lip = IP
				_lport = Port
				_rip = IPAddress.Any
				_rport = 0
				_uc = False
			ElseIf specification = UDPIPPortSpecification.Remote Then
				_rip = IP
				_rport = Port
				_lip = IPAddress.Any
				_lport = 0
				_uc = True
			Else
				Throw New NetLibException( New ArgumentException("You must specify a UDPIPPortSpecification."))
			End If
		End Sub
        ''' <summary>
        ''' Constructs a new NetUDPClient Instance with the specified remote and local IP Addresses and Ports.
        ''' </summary>
        ''' <param name="localIP">The Local IP Address of a Network Interface</param>
        ''' <param name="localPort">The Local Port</param>
        ''' <param name="remoteIP">The Remote IP to dedicate a connection to</param>
        ''' <param name="remotePort">The Remote Port to dedicate a connection to</param>
        ''' <remarks></remarks>
		Public Sub New(localIP As IPAddress, localPort As Integer, remoteIP As IPAddress, remotePort As Integer)
			_sock = New Socket(localIP.AddressFamily, SocketType.Dgram, ProtocolType.Udp)
			_sock.ReceiveTimeout = 0
			_sock.SendTimeout = 0
			_rip = remoteIP
			_rport = remotePort
			_lip = localIP
			_lport = localPort
			_uc = True
		End Sub
        ''' <summary>
        ''' Opens the socket for network connection.
        ''' </summary>
        ''' <remarks></remarks>
		Public Overridable Sub open() Implements INetSocketConnectionless.open
			SyncLock slocksockman
				Try
					If Not _sock.IsBound Then
						_sock.Bind(New IPEndPoint(_lip, _lport))
						_l = True
						If _uc Then
							_sock.Connect(New IPEndPoint(_rip, _rport))
							_c = True
						End If
					End If
                Catch ex As ObjectDisposedException
                    Utilities.addException(New NetLibException(ex))
                Catch ex As SocketException
                    Utilities.addException(New NetLibException(ex))
                End Try
			End SyncLock
		End Sub
        ''' <summary>
        ''' Returns whether the Socket is Connected.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>Whether the Socket is Connected</returns>
        ''' <remarks></remarks>
		Public ReadOnly Overridable Property connected As Boolean Implements INetSocketConnectionless.connected
			Get
				Return _c
			End Get
		End Property
        ''' <summary>
        ''' Returns whether the Socket is Listening.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>Whether the Socket is Listening</returns>
        ''' <remarks></remarks>
		Public ReadOnly Overridable Property listening As Boolean Implements INetSocketConnectionless.listening
			Get
				Return _l
			End Get
		End Property
        ''' <summary>
        ''' Returns whether data is ready to be read from the network.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>Whether there is data on the network</returns>
        ''' <remarks></remarks>
		Public ReadOnly Overridable Property hasData As Boolean Implements INetSocketConnectionless.hasData
			Get
				Throw New NetLibException( New InvalidOperationException("Not a TCP Client."))
			End Get
		End Property
        ''' <summary>
        ''' Returns whether a client is waiting to connect.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>If a client is waiting to connect</returns>
        ''' <remarks></remarks>
		Public ReadOnly Overridable Property clientWaiting As Boolean Implements INetSocketConnectionless.clientWaiting
			Get
				Throw New NetLibException( New InvalidOperationException("Not a TCP Listener."))
			End Get
		End Property
        ''' <summary>
        ''' Accepts a client that is waiting to connect.
        ''' </summary>
        ''' <returns>The Accepted Client's Socket</returns>
        ''' <remarks></remarks>
		Public Overridable Function acceptClient() As INetSocket Implements INetSocketConnectionless.acceptClient
			Throw New NetLibException( New InvalidOperationException("Not a TCP Listener."))
		End Function
        ''' <summary>
        ''' Close the socket stopping network connections.
        ''' </summary>
        ''' <remarks></remarks>
        Public Overridable Sub close() Implements INetSocketConnectionless.close, IDisposable.Dispose
            Try
                SyncLock slocksockman
                    _c = False
                    _l = False
                    _sock.Close()
                End SyncLock
            Catch ex As ObjectDisposedException
                Utilities.addException(New NetLibException(ex))
            Catch ex As SocketException
                Utilities.addException(New NetLibException(ex))
            End Try
        End Sub
        ''' <summary>
        ''' Sends a byte array over the network.
        ''' </summary>
        ''' <param name="bytes">The byte array to send</param>
        ''' <returns>Whether the send was successful</returns>
        ''' <remarks></remarks>
		Public Overridable Function sendBytes(bytes As Byte()) As Boolean Implements INetSocketConnectionless.sendBytes
			If Not _uc Then Throw New NetLibException( New InvalidOperationException("This UDP Client does not have a dedicated connection."))
			Dim ret As Integer = 0
			If bytes.Length > _sock.SendBufferSize - 4 Then Throw New NetLibException( New ArgumentOutOfRangeException("The Byte Array is too big."))
			SyncLock slocksend
				Try
					Dim len As Byte() = Utilities.Int32ToBytes(bytes.Length)
                    Dim ts(_sock.SendBufferSize - 1) As Byte
					System.Buffer.BlockCopy(len, 0, ts, 0, 4)
					System.Buffer.BlockCopy(bytes, 0, ts, 4, bytes.Length)
					ret = _sock.Send(ts, ts.Length, SocketFlags.None)
                Catch ex As SocketException
                    Utilities.addException(New NetLibException(ex))
                    Return False
				End Try
			End SyncLock
			Return ret > 0
		End Function
        ''' <summary>
        ''' Receives a byte array from the network.
        ''' </summary>
        ''' <returns>The Received Byte Array</returns>
        ''' <remarks></remarks>
		Public Overridable Function receiveBytes() As Byte() Implements INetSocketConnectionless.receiveBytes
			If Not _uc Then Throw New NetLibException( New InvalidOperationException("This UDP Client does not have a dedicated connection."))
            Dim bts(_sock.ReceiveBufferSize - 1) As Byte
            Dim btstr(-1) As Byte
            SyncLock slockreceive
                Try
                    Dim len(3) As Byte
                    _sock.Receive(bts, bts.Length, SocketFlags.None)
                    Buffer.BlockCopy(bts, 0, len, 0, 4)
                    Dim lengthflen As Integer = Utilities.BytesToInt32(len)
                    ReDim btstr(lengthflen - 1)
                    If lengthflen <= _sock.ReceiveBufferSize - 4 Then
                        Buffer.BlockCopy(bts, 4, btstr, 0, lengthflen)
                    Else
                        Buffer.BlockCopy(bts, 4, btstr, 0, _sock.ReceiveBufferSize - 4)
                    End If
                Catch ex As SocketException
                    Utilities.addException(New NetLibException(ex))
                End Try
            End SyncLock
            Return btstr
		End Function
        ''' <summary>
        ''' Sends a byte array over the network to the specified address and port.
        ''' </summary>
        ''' <param name="bytes">The byte array to send</param>
        ''' <param name="remoteIP">The remote IP</param>
        ''' <param name="remotePort">The remote Port</param>
        ''' <returns>Whether the send was successful</returns>
        ''' <remarks></remarks>
		Public Overridable Function sendBytesTo(bytes As Byte(), remoteIP As String, remotePort As Integer) As Boolean Implements INetSocketConnectionless.sendBytesTo
			If _uc Then Throw New NetLibException( New InvalidOperationException("This UDP Client has a dedicated connection."))
			Dim remote_IP As IPAddress = IPAddress.Parse(remoteIP)
			Dim ret As Integer = 0
			If bytes.Length > _sock.SendBufferSize - 4 Then Throw New NetLibException( New ArgumentOutOfRangeException("The Byte Array is too big."))
			SyncLock slocksend
				Try
					Dim len As Byte() = Utilities.Int32ToBytes(bytes.Length)
                    Dim ts(_sock.SendBufferSize - 1) As Byte
					System.Buffer.BlockCopy(len, 0, ts, 0, 4)
					System.Buffer.BlockCopy(bytes, 0, ts, 4, bytes.Length)
					ret = _sock.SendTo(ts, ts.Length, SocketFlags.None, New IPEndPoint(remote_IP, remotePort))
                Catch ex As SocketException
                    Utilities.addException(New NetLibException(ex))
                    Return False
				End Try
			End SyncLock
			Return ret > 0
		End Function
        ''' <summary>
        ''' Receives a byte array from the specified address and port on the network.
        ''' </summary>
        ''' <param name="remoteIP">The remote IP</param>
        ''' <param name="remotePort">The remote Port</param>
        ''' <returns>The Received Byte Array</returns>
        ''' <remarks></remarks>
		Public Overridable Function receiveBytesFrom(remoteIP As String, remotePort As Integer) As Byte() Implements INetSocketConnectionless.receiveBytesFrom
			If _uc Then Throw New NetLibException( New InvalidOperationException("This UDP Client has a dedicated connection."))
			Dim remote_IP As IPAddress = IPAddress.Parse(remoteIP)
            Dim bts(_sock.ReceiveBufferSize - 1) As Byte
            Dim btstr(-1) As Byte
            SyncLock slockreceive
                Try
                    Dim len(3) As Byte
                    _sock.ReceiveFrom(bts, bts.Length, SocketFlags.None, New IPEndPoint(remote_IP, remotePort))
                    Buffer.BlockCopy(bts, 0, len, 0, 4)
                    Dim lengthflen As Integer = Utilities.BytesToInt32(len)
                    ReDim btstr(lengthflen - 1)
                    If lengthflen <= _sock.ReceiveBufferSize - 4 Then
                        Buffer.BlockCopy(bts, 4, btstr, 0, lengthflen)
                    Else
                        Buffer.BlockCopy(bts, 4, btstr, 0, _sock.ReceiveBufferSize - 4)
                    End If
                Catch ex As SocketException
                    Utilities.addException(New NetLibException(ex))
                End Try
            End SyncLock
            Return btstr
		End Function
        ''' <summary>
        ''' Reassociates a dedicated connectionless connection.
        ''' </summary>
        ''' <param name="remoteIP">The remote IP</param>
        ''' <param name="remotePort">The remote Port</param>
        ''' <remarks></remarks>
		Public Overridable Sub reconnect(remoteIP As String, remotePort As Integer) Implements INetSocketConnectionless.reconnect
            SyncLock slocksockman
                Try
                    Dim sconf As NetSocketConfig = New NetSocketConfig(Me)
                    sconf.setRemoteIPAddress(remoteIP)
                    sconf.setRemotePort(remotePort)
                    _sock.Close()
                    _sock = Nothing
                    _sock = New Socket(IPAddress.Parse(sconf.localIPAddress).AddressFamily, SocketType.Dgram, ProtocolType.Udp)
                    _sock.ReceiveTimeout = 0
                    _sock.SendTimeout = 0
                    sconf.DuplicateConfigTo(Me)
                    _rip = IPAddress.Parse(sconf.remoteIPAddress)
                    _rport = sconf.remotePort
                    _uc = True
                    If Not _sock.IsBound Then
                        _sock.Bind(New IPEndPoint(_lip, _lport))
                        _l = True
                        If _uc Then
                            _sock.Connect(New IPEndPoint(_rip, _rport))
                            _c = True
                        End If
                    End If
                Catch ex As ObjectDisposedException
                    Utilities.addException(New NetLibException(ex))
                Catch ex As SocketException
                    Utilities.addException(New NetLibException(ex))
                End Try
            End SyncLock
		End Sub
        ''' <summary>
        ''' Disassociates the current dedicated connectionless connection.
        ''' </summary>
        ''' <remarks></remarks>
		Public Overridable Sub disconnect() Implements INetSocketConnectionless.disconnect
            SyncLock slocksockman
                Try
                    Dim sconf As NetSocketConfig = New NetSocketConfig(Me)
                    sconf.setRemoteIPAddress(IPAddress.Any.ToString())
                    sconf.setRemotePort(0)
                    _sock.Close()
                    _sock = Nothing
                    _sock = New Socket(IPAddress.Parse(sconf.localIPAddress).AddressFamily, SocketType.Dgram, ProtocolType.Udp)
                    _sock.ReceiveTimeout = 0
                    _sock.SendTimeout = 0
                    sconf.DuplicateConfigTo(Me)
                    _rip = IPAddress.Any
                    _rport = 0
                    _uc = False
                    _c = False
                    If Not _sock.IsBound Then
                        _sock.Bind(New IPEndPoint(_lip, _lport))
                        _l = True
                    End If
                Catch ex As ObjectDisposedException
                    Utilities.addException(New NetLibException(ex))
                Catch ex As SocketException
                    Utilities.addException(New NetLibException(ex))
                End Try
            End SyncLock
		End Sub
        ''' <summary>
        ''' Gets or Sets the size of the Send Buffer.
        ''' </summary>
        ''' <value>Integer</value>
        ''' <returns>The size of the send buffer</returns>
        ''' <remarks></remarks>
        Public Overridable Property sendBufferSize As Integer Implements INetConfig.sendBufferSize
            Get
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                Dim toret As Integer = 0
                Try
                    toret = _sock.SendBufferSize
                Catch ex As ObjectDisposedException
                    Utilities.addException(New NetLibException(ex))
                Catch ex As SocketException
                    Utilities.addException(New NetLibException(ex))
                Catch ex As InvalidOperationException
                    Utilities.addException(New NetLibException(ex))
                End Try
                Return toret
            End Get
            Set(value As Integer)
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                Try
                    _sock.SendBufferSize = value
                Catch ex As ObjectDisposedException
                    Utilities.addException(New NetLibException(ex))
                Catch ex As SocketException
                    Utilities.addException(New NetLibException(ex))
                Catch ex As InvalidOperationException
                    Utilities.addException(New NetLibException(ex))
                End Try
            End Set
        End Property
        ''' <summary>
        ''' Gets or Sets the size of the receive Buffer.
        ''' </summary>
        ''' <value>Integer</value>
        ''' <returns>The size of the receive buffer</returns>
        ''' <remarks></remarks>
        Public Overridable Property receiveBufferSize As Integer Implements INetConfig.receiveBufferSize
            Get
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                Dim toret As Integer = 0
                Try
                    toret = _sock.ReceiveBufferSize
                Catch ex As ObjectDisposedException
                    Utilities.addException(New NetLibException(ex))
                Catch ex As SocketException
                    Utilities.addException(New NetLibException(ex))
                Catch ex As InvalidOperationException
                    Utilities.addException(New NetLibException(ex))
                End Try
                Return toret
            End Get
            Set(value As Integer)
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                Try
                    _sock.ReceiveBufferSize = value
                Catch ex As ObjectDisposedException
                    Utilities.addException(New NetLibException(ex))
                Catch ex As SocketException
                    Utilities.addException(New NetLibException(ex))
                Catch ex As InvalidOperationException
                    Utilities.addException(New NetLibException(ex))
                End Try
            End Set
        End Property
        ''' <summary>
        ''' Gets or Sets the Disablement of Nagle's Algorithm.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>The Disablement of Nagle's Algorithm.</returns>
        ''' <remarks></remarks>
		Public Overridable Property noDelay As Boolean Implements INetConfig.noDelay
            Get
                Throw New NetLibException( New InvalidOperationException("Not a TCP Listener Or TCP Client."))
            End Get
            Set(value As Boolean)
                Throw New NetLibException( New InvalidOperationException("Not a TCP Listener Or TCP Client."))
            End Set
		End Property
        ''' <summary>
        ''' Gets or Sets the receive Timeout.
        ''' </summary>
        ''' <value>Integer</value>
        ''' <returns>The receive timeout</returns>
        ''' <remarks></remarks>
        Public Overridable Property receiveTimeout As Integer Implements INetConfig.receiveTimeout
            Get
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                Dim toret As Integer = 0
                Try
                    toret = _sock.ReceiveTimeout
                Catch ex As ObjectDisposedException
                    Utilities.addException(New NetLibException(ex))
                Catch ex As SocketException
                    Utilities.addException(New NetLibException(ex))
                Catch ex As InvalidOperationException
                    Utilities.addException(New NetLibException(ex))
                End Try
                Return toret
            End Get
            Set(value As Integer)
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                Try
                    _sock.ReceiveTimeout = value
                Catch ex As ObjectDisposedException
                    Utilities.addException(New NetLibException(ex))
                Catch ex As SocketException
                    Utilities.addException(New NetLibException(ex))
                Catch ex As InvalidOperationException
                    Utilities.addException(New NetLibException(ex))
                End Try
            End Set
        End Property
        ''' <summary>
        ''' Gets or Sets the send Timeout.
        ''' </summary>
        ''' <value>Integer</value>
        ''' <returns>The send timeout</returns>
        ''' <remarks></remarks>
        Public Overridable Property sendTimeout As Integer Implements INetConfig.sendTimeout
            Get
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                Dim toret As Integer = 0
                Try
                    toret = _sock.SendTimeout
                Catch ex As ObjectDisposedException
                    Utilities.addException(New NetLibException(ex))
                Catch ex As SocketException
                    Utilities.addException(New NetLibException(ex))
                Catch ex As InvalidOperationException
                    Utilities.addException(New NetLibException(ex))
                End Try
                Return toret
            End Get
            Set(value As Integer)
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                Try
                    _sock.SendTimeout = value
                Catch ex As ObjectDisposedException
                    Utilities.addException(New NetLibException(ex))
                Catch ex As SocketException
                    Utilities.addException(New NetLibException(ex))
                Catch ex As InvalidOperationException
                    Utilities.addException(New NetLibException(ex))
                End Try
            End Set
        End Property
        ''' <summary>
        ''' Returns the local IP Address.
        ''' </summary>
        ''' <value>String</value>
        ''' <returns>The local IP Address</returns>
        ''' <remarks></remarks>
		Public ReadOnly Overridable Property localIPAddress As String Implements INetConfig.localIPAddress
			Get
				Return _lip.ToString()
			End Get
		End Property
        ''' <summary>
        ''' Returns the local IP Port.
        ''' </summary>
        ''' <value>String</value>
        ''' <returns>The local IP Port</returns>
        ''' <remarks></remarks>
		Public ReadOnly Overridable Property localPort As Integer Implements INetConfig.localPort
			Get
				Return _lport
			End Get
		End Property
        ''' <summary>
        ''' Returns the remote IP Address.
        ''' </summary>
        ''' <value>String</value>
        ''' <returns>The remote IP Address</returns>
        ''' <remarks></remarks>
		Public ReadOnly Overridable Property remoteIPAddress As String Implements INetConfig.remoteIPAddress
			Get
				Return _rip.ToString()
			End Get
		End Property
        ''' <summary>
        ''' Returns the remote IP Port.
        ''' </summary>
        ''' <value>String</value>
        ''' <returns>The remote IP Port</returns>
        ''' <remarks></remarks>
		Public ReadOnly Overridable Property remotePort As Integer Implements INetConfig.remotePort
			Get
				Return _rport
			End Get
		End Property
        ''' <summary>
        ''' Gets or Sets whether address use is exclusive.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>Whether the address use is exclusive</returns>
        ''' <remarks></remarks>
        Public Overridable Property exclusiveAddressUse As Boolean Implements INetConfig.exclusiveAddressUse
            Get
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                Dim toret As Boolean = False
                Try
                    toret = _sock.ExclusiveAddressUse
                Catch ex As ObjectDisposedException
                    Utilities.addException(New NetLibException(ex))
                Catch ex As SocketException
                    Utilities.addException(New NetLibException(ex))
                Catch ex As InvalidOperationException
                    Utilities.addException(New NetLibException(ex))
                End Try
                Return toret
            End Get
            Set(value As Boolean)
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                Try
                    _sock.ExclusiveAddressUse = value
                Catch ex As ObjectDisposedException
                    Utilities.addException(New NetLibException(ex))
                Catch ex As SocketException
                    Utilities.addException(New NetLibException(ex))
                Catch ex As InvalidOperationException
                    Utilities.addException(New NetLibException(ex))
                End Try
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the backlog of connections.
        ''' </summary>
        ''' <value>Integer</value>
        ''' <returns>Backlog of Connections</returns>
        ''' <remarks></remarks>
		Public Overridable Property connectionBacklog As Integer Implements INetConfig.connectionBacklog
			Get
				Throw New NetLibException( New InvalidOperationException("Not a TCP Listener."))
			End Get
			Set(value As Integer)
				Throw New NetLibException( New InvalidOperationException("Not a TCP Listener."))
			End Set
		End Property
	End Class
    ''' <summary>
    ''' Specifies the Selector for UDP Address and Port specification.
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum UDPIPPortSpecification As Integer
        ''' <summary>
        ''' Specifies None
        ''' </summary>
        ''' <remarks></remarks>
        None = 0
        ''' <summary>
        ''' Specifies Local Specification
        ''' </summary>
        ''' <remarks></remarks>
        Local = 1
        ''' <summary>
        ''' Specifies Remote Specification
        ''' </summary>
        ''' <remarks></remarks>
        Remote = 2
    End Enum
	
End Namespace


