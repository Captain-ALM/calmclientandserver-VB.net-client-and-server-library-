Imports System.Net
Imports System.Net.Sockets
Imports System.Net.NetworkInformation
Imports System.Security

'
' Created by SharpDevelop.
' User: Alfred
' Date: 19/05/2019
' Time: 15:58
' 
' To change this template use Tools | Options | Coding | Edit Standard Headers.
'
Namespace CALMNetLib
    ''' <summary>
    ''' This is a NetTCPClient socket.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class NetTCPClient
        Implements INetSocket, IDisposable, INetConfig

        Protected _sock As Socket = Nothing
        Protected _ip As IPAddress = Nothing
        Protected _port As Integer = 0
        Protected slocksend As New Object()
        Protected slockreceive As New Object()
        Protected slockman As New Object()
        Protected _hlh As Boolean = False
        ''' <summary>
        ''' Constructs a New NetTCPClient Instance connecting to the specified IP Address and port.
        ''' </summary>
        ''' <param name="ip">The IP Address to connect to</param>
        ''' <param name="port">The port to connect to</param>
        ''' <remarks></remarks>
        Public Sub New(ip As IPAddress, port As Integer)
            _sock = New Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            _sock.NoDelay = False
            _sock.ReceiveTimeout = 0
            _sock.SendTimeout = 0
            _ip = ip
            _port = port
        End Sub

        Friend Sub New(sock As Socket, lip As IPAddress, lport As Integer)
            slockman = New Object()
            SyncLock slockman
                _sock = sock
                _ip = lip
                _port = lport
            End SyncLock
        End Sub
        ''' <summary>
        ''' Opens the socket for network connection.
        ''' </summary>
        ''' <remarks></remarks>
        Public Overridable Sub open() Implements INetSocket.open
            SyncLock slockman
                Try
                    If Not _sock.Connected Then _sock.Connect(New IPEndPoint(_ip, _port))
                Catch ex As Exception When (TypeOf ex Is SocketException Or TypeOf ex Is ArgumentException Or TypeOf ex Is SecurityException Or TypeOf ex Is ObjectDisposedException Or TypeOf ex Is InvalidOperationException)
                    Throw New NetLibException(ex)
                End Try
            End SyncLock
        End Sub

        Protected Overridable Function pollConnection() As Boolean
            Try
                Return _sock.Connected
            Catch ex As Exception When (TypeOf ex Is SocketException Or TypeOf ex Is ArgumentException Or TypeOf ex Is SecurityException Or TypeOf ex Is ObjectDisposedException Or TypeOf ex Is InvalidOperationException)
            End Try
            Return False
        End Function
        ''' <summary>
        ''' Returns whether the Socket is Connected.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>Whether the Socket is Connected</returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property connected As Boolean Implements INetSocket.connected
            Get
                Return pollConnection()
            End Get
        End Property
        ''' <summary>
        ''' Returns whether the Socket is Listening.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>Whether the Socket is Listening</returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property listening As Boolean Implements INetSocket.listening
            Get
                Return False
            End Get
        End Property
        ''' <summary>
        ''' Returns whether data is ready to be read from the network.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>Whether there is data on the network</returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property hasData As Boolean Implements INetSocket.hasData
            Get
                If Not pollConnection() Then Return False
                Dim toret As Boolean = False
                SyncLock slockreceive
                    Try
                        toret = _sock.Available > 0
                    Catch ex As Exception When (TypeOf ex Is SocketException Or TypeOf ex Is ArgumentException Or TypeOf ex Is SecurityException Or TypeOf ex Is ObjectDisposedException Or TypeOf ex Is InvalidOperationException)
                        Throw New NetLibException(ex)
                    End Try
                End SyncLock
                Return toret
            End Get
        End Property
        ''' <summary>
        ''' Sends a byte array over the network.
        ''' </summary>
        ''' <param name="bytes">The byte array to send</param>
        ''' <returns>Whether the send was successful</returns>
        ''' <remarks></remarks>
        Public Overridable Function sendBytes(bytes As Byte()) As Boolean Implements INetSocket.sendBytes
            Dim ret As Integer = 0
            If Not pollConnection() Then Return False
            Dim limit As Integer = _sock.SendBufferSize
            If _hlh Then limit -= 4
            If bytes.Length > limit Then Throw New NetLibException(New ArgumentOutOfRangeException("The Byte Array is too big."))
            SyncLock slocksend
                Try
                    Dim arrlen As Integer = bytes.Length - 1
                    If _hlh Then arrlen += 4
                    Dim ts(arrlen) As Byte
                    If _hlh Then
                        Dim len As Byte() = Utilities.Int32ToBytes(bytes.Length)
                        System.Buffer.BlockCopy(len, 0, ts, 0, 4)
                        System.Buffer.BlockCopy(bytes, 0, ts, 4, bytes.Length)
                    Else
                        System.Buffer.BlockCopy(bytes, 0, ts, 0, bytes.Length)
                    End If
                    ret = _sock.Send(ts, ts.Length, SocketFlags.None)
                Catch ex As Exception When (TypeOf ex Is SocketException Or TypeOf ex Is ArgumentException Or TypeOf ex Is SecurityException Or TypeOf ex Is ObjectDisposedException Or TypeOf ex Is InvalidOperationException)
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
        Public Overridable Function receiveBytes() As Byte() Implements INetSocket.receiveBytes
            Dim bts(-1) As Byte
            If Not pollConnection() Then Return bts
            SyncLock slockreceive
                Try
                    If _hlh Then
                        Dim len(3) As Byte
                        _sock.Receive(len, 4, SocketFlags.None)
                        Dim lengthflen As Integer = Utilities.BytesToInt32(len)
                        ReDim bts(lengthflen - 1)
                        If lengthflen <= _sock.ReceiveBufferSize - 4 Then
                            _sock.Receive(bts, bts.Length, SocketFlags.None)
                        Else
                            Dim btsrem As Integer = lengthflen
                            Dim btsind As Integer = 0
                            While btsrem > 0
                                If btsrem < _sock.ReceiveBufferSize - 4 Then
                                    Dim rec(btsrem - 1) As Byte
                                    _sock.Receive(rec, btsrem, SocketFlags.None)
                                    Buffer.BlockCopy(rec, 0, bts, btsind, btsrem)
                                    btsind += btsrem
                                    btsrem -= btsrem
                                Else
                                    Dim rec(_sock.ReceiveBufferSize - 5) As Byte
                                    _sock.Receive(rec, _sock.ReceiveBufferSize - 5, SocketFlags.None)
                                    Buffer.BlockCopy(rec, 0, bts, btsind, _sock.ReceiveBufferSize - 5)
                                    btsrem -= (_sock.ReceiveBufferSize - 5)
                                    btsind += (_sock.ReceiveBufferSize - 5)
                                End If
                            End While
                        End If
                    Else
                        Dim btsb(_sock.ReceiveBufferSize - 1) As Byte
                        Dim lentr As Integer = _sock.Receive(btsb, _sock.ReceiveBufferSize - 1, SocketFlags.None)
                        ReDim bts(lentr - 1)
                        Buffer.BlockCopy(btsb, 0, bts, 0, lentr)
                    End If
                Catch ex As Exception When (TypeOf ex Is SocketException Or TypeOf ex Is ArgumentException Or TypeOf ex Is SecurityException Or TypeOf ex Is ObjectDisposedException Or TypeOf ex Is InvalidOperationException)
                End Try
            End SyncLock
            Return bts
        End Function
        ''' <summary>
        ''' Returns whether a client is waiting to connect.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>If a client is waiting to connect</returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property clientWaiting As Boolean Implements INetSocket.clientWaiting
            Get
                Throw New NetLibException(New InvalidOperationException("Not a TCP Listener."))
            End Get
        End Property
        ''' <summary>
        ''' Accepts a client that is waiting to connect.
        ''' </summary>
        ''' <returns>The Accepted Client's Socket</returns>
        ''' <remarks></remarks>
        Public Overridable Function acceptClient() As INetSocket Implements INetSocket.acceptClient
            Throw New NetLibException(New InvalidOperationException("Not a TCP Listener."))
        End Function
        ''' <summary>
        ''' Close the socket stopping network connections.
        ''' </summary>
        ''' <remarks></remarks>
        Public Overridable Sub close() Implements INetSocket.close, IDisposable.Dispose
            SyncLock slockman
                If pollConnection() Then
                    Try
                        _sock.Shutdown(SocketShutdown.Both)
                    Catch ex As Exception When (TypeOf ex Is SocketException Or TypeOf ex Is ArgumentException Or TypeOf ex Is SecurityException Or TypeOf ex Is ObjectDisposedException Or TypeOf ex Is InvalidOperationException)
                        Throw New NetLibException(ex)
                    Finally
                        Try
                            _sock.Close()
                        Catch ex As Exception When (TypeOf ex Is SocketException Or TypeOf ex Is ArgumentException Or TypeOf ex Is SecurityException Or TypeOf ex Is ObjectDisposedException Or TypeOf ex Is InvalidOperationException)
                            Throw New NetLibException(ex)
                        End Try
                    End Try
                End If
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
                    Throw New NetLibException(ex)
                Catch ex As SocketException
                    Throw New NetLibException(ex)
                Catch ex As InvalidOperationException
                    Throw New NetLibException(ex)
                End Try
                Return toret
            End Get
            Set(value As Integer)
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                Try
                    _sock.SendBufferSize = value
                Catch ex As ObjectDisposedException
                    Throw New NetLibException(ex)
                Catch ex As SocketException
                    Throw New NetLibException(ex)
                Catch ex As InvalidOperationException
                    Throw New NetLibException(ex)
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
                    Throw New NetLibException(ex)
                Catch ex As SocketException
                    Throw New NetLibException(ex)
                Catch ex As InvalidOperationException
                    Throw New NetLibException(ex)
                End Try
                Return toret
            End Get
            Set(value As Integer)
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                Try
                    _sock.ReceiveBufferSize = value
                Catch ex As ObjectDisposedException
                    Throw New NetLibException(ex)
                Catch ex As SocketException
                    Throw New NetLibException(ex)
                Catch ex As InvalidOperationException
                    Throw New NetLibException(ex)
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
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                Dim toret As Boolean = False
                Try
                    toret = _sock.NoDelay
                Catch ex As ObjectDisposedException
                    Throw New NetLibException(ex)
                Catch ex As SocketException
                    Throw New NetLibException(ex)
                Catch ex As InvalidOperationException
                    Throw New NetLibException(ex)
                End Try
                Return toret
            End Get
            Set(value As Boolean)
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                Try
                    _sock.NoDelay = value
                Catch ex As ObjectDisposedException
                    Throw New NetLibException(ex)
                Catch ex As SocketException
                    Throw New NetLibException(ex)
                Catch ex As InvalidOperationException
                    Throw New NetLibException(ex)
                End Try
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
                    Throw New NetLibException(ex)
                Catch ex As SocketException
                    Throw New NetLibException(ex)
                Catch ex As InvalidOperationException
                    Throw New NetLibException(ex)
                End Try
                Return toret
            End Get
            Set(value As Integer)
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                Try
                    _sock.ReceiveTimeout = value
                Catch ex As ObjectDisposedException
                    Throw New NetLibException(ex)
                Catch ex As SocketException
                    Throw New NetLibException(ex)
                Catch ex As InvalidOperationException
                    Throw New NetLibException(ex)
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
                    Throw New NetLibException(ex)
                Catch ex As SocketException
                    Throw New NetLibException(ex)
                Catch ex As InvalidOperationException
                    Throw New NetLibException(ex)
                End Try
                Return toret
            End Get
            Set(value As Integer)
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                Try
                    _sock.SendTimeout = value
                Catch ex As ObjectDisposedException
                    Throw New NetLibException(ex)
                Catch ex As SocketException
                    Throw New NetLibException(ex)
                Catch ex As InvalidOperationException
                    Throw New NetLibException(ex)
                End Try
            End Set
        End Property
        ''' <summary>
        ''' Returns the IP Address of the listener.
        ''' This is a local listener if the connection was accepted by a connection listener or it is a remote listener if the socket was connected to a connection listener.
        ''' </summary>
        ''' <value>String</value>
        ''' <returns>The listener IP Address</returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property listenerIPAddress As String
            Get
                Return _ip.ToString()
            End Get
        End Property
        ''' <summary>
        ''' Returns the listener IP Port.
        ''' This is a local listener if the connection was accepted by a connection listener or it is a remote listener if the socket was connected to a connection listener.
        ''' </summary>
        ''' <value>String</value>
        ''' <returns>The listener IP Port</returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property listenerPort As Integer
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
        Public Overridable ReadOnly Property remoteIPAddress As String Implements INetConfig.remoteIPAddress
            Get
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                Dim toret As String = ""
                Try
                    If _sock.RemoteEndPoint Is Nothing Then Throw New NetLibException(New InvalidOperationException("socket not open"))
                    toret = CType(_sock.RemoteEndPoint, IPEndPoint).Address.ToString()
                Catch ex As ObjectDisposedException
                    Throw New NetLibException(ex)
                Catch ex As SocketException
                    Throw New NetLibException(ex)
                Catch ex As InvalidOperationException
                    Throw New NetLibException(ex)
                Catch ex As NullReferenceException
                    Throw New NetLibException(ex)
                Catch ex As ArgumentException
                    Throw New NetLibException(ex)
                Catch ex As InvalidCastException
                    Throw New NetLibException(ex)
                End Try
                Return toret
            End Get
        End Property
        ''' <summary>
        ''' Returns the remote IP Port.
        ''' </summary>
        ''' <value>String</value>
        ''' <returns>The remote IP Port</returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property remotePort As Integer Implements INetConfig.remotePort
            Get
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                Dim toret As Integer = 0
                Try
                    If _sock.RemoteEndPoint Is Nothing Then Throw New NetLibException(New InvalidOperationException("socket not open"))
                    toret = CType(_sock.RemoteEndPoint, IPEndPoint).Port
                Catch ex As ObjectDisposedException
                    Throw New NetLibException(ex)
                Catch ex As SocketException
                    Throw New NetLibException(ex)
                Catch ex As InvalidOperationException
                    Throw New NetLibException(ex)
                Catch ex As NullReferenceException
                    Throw New NetLibException(ex)
                Catch ex As ArgumentException
                    Throw New NetLibException(ex)
                Catch ex As InvalidCastException
                    Throw New NetLibException(ex)
                End Try
                Return toret
            End Get
        End Property
        ''' <summary>
        ''' Returns the local IP Address.
        ''' </summary>
        ''' <value>String</value>
        ''' <returns>The local IP Address</returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property localIPAddress As String Implements INetConfig.localIPAddress
            Get
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                Dim toret As String = ""
                Try
                    If _sock.LocalEndPoint Is Nothing Then Throw New NetLibException(New InvalidOperationException("socket not open"))
                    toret = CType(_sock.LocalEndPoint, IPEndPoint).Address.ToString()
                Catch ex As ObjectDisposedException
                    Throw New NetLibException(ex)
                Catch ex As SocketException
                    Throw New NetLibException(ex)
                Catch ex As InvalidOperationException
                    Throw New NetLibException(ex)
                Catch ex As NullReferenceException
                    Throw New NetLibException(ex)
                Catch ex As ArgumentException
                    Throw New NetLibException(ex)
                Catch ex As InvalidCastException
                    Throw New NetLibException(ex)
                End Try
                Return toret
            End Get
        End Property
        ''' <summary>
        ''' Returns the local IP Port.
        ''' </summary>
        ''' <value>String</value>
        ''' <returns>The local IP Port</returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property localPort As Integer Implements INetConfig.localPort
            Get
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                Dim toret As Integer = 0
                Try
                    If _sock.LocalEndPoint Is Nothing Then Throw New NetLibException(New InvalidOperationException("socket not open"))
                    toret = CType(_sock.LocalEndPoint, IPEndPoint).Port
                Catch ex As ObjectDisposedException
                    Throw New NetLibException(ex)
                Catch ex As SocketException
                    Throw New NetLibException(ex)
                Catch ex As InvalidOperationException
                    Throw New NetLibException(ex)
                Catch ex As NullReferenceException
                    Throw New NetLibException(ex)
                Catch ex As ArgumentException
                    Throw New NetLibException(ex)
                Catch ex As InvalidCastException
                    Throw New NetLibException(ex)
                End Try
                Return toret
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
                    Throw New NetLibException(ex)
                Catch ex As SocketException
                    Throw New NetLibException(ex)
                Catch ex As InvalidOperationException
                    Throw New NetLibException(ex)
                End Try
                Return toret
            End Get
            Set(value As Boolean)
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                Try
                    _sock.ExclusiveAddressUse = value
                Catch ex As ObjectDisposedException
                    Throw New NetLibException(ex)
                Catch ex As SocketException
                    Throw New NetLibException(ex)
                Catch ex As InvalidOperationException
                    Throw New NetLibException(ex)
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
                Throw New NetLibException(New InvalidOperationException("Not a TCP Listener."))
            End Get
            Set(value As Integer)
                Throw New NetLibException(New InvalidOperationException("Not a TCP Listener."))
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets whether the socket sends and receives data using the length header.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>Whether the socket uses a length header</returns>
        ''' <remarks></remarks>
        Public Property hasLengthHeader As Boolean Implements INetConfig.hasLengthHeader
            Get
                Return _hlh
            End Get
            Set(value As Boolean)
                _hlh = value
            End Set
        End Property
        ''' <summary>
        ''' Allows for supported source settings to be copied to the current INetConfig instance.
        ''' </summary>
        ''' <param name="source">The source to copy</param>
        ''' <remarks></remarks>
        Public Sub copyConfigFrom(source As INetConfig) Implements INetConfig.copyConfigFrom
            If _sock Is Nothing Then Exit Sub
            Me.sendBufferSize = source.sendBufferSize
            Me.receiveBufferSize = source.receiveBufferSize
            Me.noDelay = source.noDelay
            Me.receiveTimeout = source.receiveTimeout
            Me.sendTimeout = source.sendTimeout
            If Not _sock.IsBound Then Me.exclusiveAddressUse = source.exclusiveAddressUse
            Me.hasLengthHeader = source.hasLengthHeader
        End Sub
        ''' <summary>
        ''' Allows for a safe NetSocketConfig containing duplicated supported configuration to be returned without exceptions.
        ''' </summary>
        ''' <returns>The safe NetSocketConfig with duplicated supported configuration</returns>
        ''' <remarks></remarks>
        Public Function getSafeSocketConfig() As NetSocketConfig Implements INetConfig.getSafeSocketConfig
            If _sock Is Nothing Then Return New NetSocketConfig()
            Dim toret As New NetSocketConfig()
            If Not _sock.LocalEndPoint Is Nothing Then
                toret.setLocalIPAddress(Me.localIPAddress)
                toret.setLocalPort(Me.localPort)
            End If
            If Not _sock.RemoteEndPoint Is Nothing Then
                toret.setRemoteIPAddress(Me.remoteIPAddress)
                toret.setRemotePort(Me.remotePort)
            End If
            toret.sendBufferSize = Me.sendBufferSize
            toret.receiveBufferSize = Me.receiveBufferSize
            toret.noDelay = Me.noDelay
            toret.receiveTimeout = Me.receiveTimeout
            toret.sendTimeout = Me.sendTimeout
            toret.exclusiveAddressUse = Me.exclusiveAddressUse
            toret.hasLengthHeader = Me.hasLengthHeader
            Return toret
        End Function
    End Class
End Namespace
