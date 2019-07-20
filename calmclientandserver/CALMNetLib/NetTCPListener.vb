Imports System.Net
Imports System.Net.Sockets
'
' Created by SharpDevelop.
' User: Alfred
' Date: 20/05/2019
' Time: 09:31
' 
' To change this template use Tools | Options | Coding | Edit Standard Headers.
'
Namespace CALMNetLib
    ''' <summary>
    ''' This is a NetTCPListener socket.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class NetTCPListener
        Implements INetSocket, INetConfig, IDisposable

        Protected _sock As Socket = Nothing
        Protected _ip As IPAddress = Nothing
        Protected _port As Integer = 0
        Protected _bl As Integer = 1
        Protected _l As Boolean = False
        Protected slockaccept As New Object()
        ''' <summary>
        ''' Constructs a New NetTCPListener Instance bound to the specified IP Address interface and port.
        ''' </summary>
        ''' <param name="ip">The IP Address to bind to</param>
        ''' <param name="port">The port to bind to</param>
        ''' <remarks></remarks>
        Public Sub New(ip As IPAddress, port As Integer)
            _sock = New Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            _sock.NoDelay = False
            _sock.ReceiveTimeout = 0
            _sock.SendTimeout = 0
            _ip = ip
            _port = port
        End Sub
        ''' <summary>
        ''' Opens the socket for network connection.
        ''' </summary>
        ''' <remarks></remarks>
        Public Overridable Sub open() Implements INetSocket.open
            Try
                If Not _sock.IsBound Then
                    _sock.Bind(New IPEndPoint(_ip, _port))
                    _sock.Listen(_bl)
                    _l = True
                End If
            Catch ex As ObjectDisposedException
                Utilities.addException(New NetLibException(ex))
            Catch ex As SocketException
                Utilities.addException(New NetLibException(ex))
            End Try
        End Sub
        ''' <summary>
        ''' Returns whether the Socket is Connected.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>Whether the Socket is Connected</returns>
        ''' <remarks></remarks>
        Public ReadOnly Overridable Property connected As Boolean Implements INetSocket.connected
            Get
                Return False
            End Get
        End Property
        ''' <summary>
        ''' Returns whether the Socket is Listening.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>Whether the Socket is Listening</returns>
        ''' <remarks></remarks>
        Public ReadOnly Overridable Property listening As Boolean Implements INetSocket.listening
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
        Public ReadOnly Overridable Property hasData As Boolean Implements INetSocket.hasData
            Get
                Throw New NetLibException( New InvalidOperationException("Not a TCP Client."))
            End Get
        End Property
        ''' <summary>
        ''' Sends a byte array over the network.
        ''' </summary>
        ''' <param name="bytes">The byte array to send</param>
        ''' <returns>Whether the send was successful</returns>
        ''' <remarks></remarks>
        Public Overridable Function sendBytes(bytes As Byte()) As Boolean Implements INetSocket.sendBytes
            Throw New NetLibException( New InvalidOperationException("Not a TCP Client."))
        End Function
        ''' <summary>
        ''' Receives a byte array from the network.
        ''' </summary>
        ''' <returns>The Received Byte Array</returns>
        ''' <remarks></remarks>
        Public Overridable Function recieveBytes() As Byte() Implements INetSocket.recieveBytes
            Throw New NetLibException( New InvalidOperationException("Not a TCP Client."))
        End Function
        ''' <summary>
        ''' Returns whether a client is waiting to connect.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>If a client is waiting to connect</returns>
        ''' <remarks></remarks>
        Public ReadOnly Overridable Property clientWaiting As Boolean Implements INetSocket.clientWaiting
            Get
                Dim toret As Boolean = False
                SyncLock slockaccept
                    Try
                        toret = _sock.Poll(((_sock.ReceiveTimeout + 1) * 1000), SelectMode.SelectRead)
                    Catch ex As SocketException
                        Utilities.addException(New NetLibException(ex))
                    End Try
                End SyncLock
                Return toret
            End Get
        End Property
        ''' <summary>
        ''' Accepts a client that is waiting to connect.
        ''' </summary>
        ''' <returns>The Accepted Client's Socket</returns>
        ''' <remarks></remarks>
        Public Overridable Function acceptClient() As INetSocket Implements INetSocket.acceptClient
            Dim toret As INetSocket = Nothing
            SyncLock slockaccept
                Try
                    toret = New NetTCPClient(_sock.Accept())
                Catch ex As SocketException
                    Utilities.addException(New NetLibException(ex))
                End Try
            End SyncLock
            Return toret
        End Function
        ''' <summary>
        ''' Close the socket stopping network connections.
        ''' </summary>
        ''' <remarks></remarks>
        Public Overridable Sub close() Implements INetSocket.close, IDisposable.Dispose
            Try
                _l = False
                _sock.Close()
            Catch ex As ObjectDisposedException
                Utilities.addException(New NetLibException(ex))
            Catch ex As SocketException
                Utilities.addException(New NetLibException(ex))
            End Try
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
                Return _sock.SendBufferSize
            End Get
            Set(value As Integer)
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                _sock.SendBufferSize = value
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
                Return _sock.ReceiveBufferSize
            End Get
            Set(value As Integer)
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                _sock.ReceiveBufferSize = value
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
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                Return _sock.NoDelay
            End Get
            Set(value As Boolean)
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                _sock.NoDelay = value
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
                Return _sock.ReceiveTimeout
            End Get
            Set(value As Integer)
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                _sock.ReceiveTimeout = value
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
                Return _sock.SendTimeout
            End Get
            Set(value As Integer)
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                _sock.SendTimeout = value
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
                Return _ip.ToString()
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
                Return _port
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
                Throw New NetLibException( New InvalidOperationException("Not a TCP or UDP Client."))
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
                Throw New NetLibException( New InvalidOperationException("Not a TCP or UDP Client."))
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
                Return _sock.ExclusiveAddressUse
            End Get
            Set(value As Boolean)
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                _sock.ExclusiveAddressUse = value
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
                Return _bl
            End Get
            Set(value As Integer)
            	If _l Then Throw New NetLibException( New InvalidOperationException("The TCP Listener is listening."))
                _bl = value
            End Set
        End Property
    End Class

End Namespace
