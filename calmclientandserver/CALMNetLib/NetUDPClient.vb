﻿Imports System.Net
Imports System.Net.Sockets
Imports System.Security

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
        Implements INetSocketConnectionless, IDisposable, INetConfig

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
        Protected _hlh As Boolean = False
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
                Throw New NetLibException(New ArgumentException("You must specify a UDPIPPortSpecification."))
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
                Catch ex As Exception When (TypeOf ex Is SocketException Or TypeOf ex Is ArgumentException Or TypeOf ex Is SecurityException Or TypeOf ex Is ObjectDisposedException Or TypeOf ex Is InvalidOperationException)
                    Throw New NetLibException(ex)
                End Try
            End SyncLock
        End Sub
        ''' <summary>
        ''' Returns whether the Socket is Connected.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>Whether the Socket is Connected</returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property connected As Boolean Implements INetSocketConnectionless.connected
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
        Public Overridable ReadOnly Property listening As Boolean Implements INetSocketConnectionless.listening
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
        Public Overridable ReadOnly Property hasData As Boolean Implements INetSocketConnectionless.hasData
            Get
                Throw New NetLibException(New InvalidOperationException("Not a TCP Client."))
            End Get
        End Property
        ''' <summary>
        ''' Returns whether a client is waiting to connect.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>If a client is waiting to connect</returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property clientWaiting As Boolean Implements INetSocketConnectionless.clientWaiting
            Get
                Throw New NetLibException(New InvalidOperationException("Not a TCP Listener."))
            End Get
        End Property
        ''' <summary>
        ''' Accepts a client that is waiting to connect.
        ''' </summary>
        ''' <returns>The Accepted Client's Socket</returns>
        ''' <remarks></remarks>
        Public Overridable Function acceptClient() As INetSocket Implements INetSocketConnectionless.acceptClient
            Throw New NetLibException(New InvalidOperationException("Not a TCP Listener."))
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
            Catch ex As Exception When (TypeOf ex Is SocketException Or TypeOf ex Is ArgumentException Or TypeOf ex Is SecurityException Or TypeOf ex Is ObjectDisposedException Or TypeOf ex Is InvalidOperationException)
                Throw New NetLibException(ex)
            End Try
        End Sub
        ''' <summary>
        ''' Sends a byte array over the network.
        ''' </summary>
        ''' <param name="bytes">The byte array to send</param>
        ''' <returns>Whether the send was successful</returns>
        ''' <remarks></remarks>
        Public Overridable Function sendBytes(bytes As Byte()) As Boolean Implements INetSocketConnectionless.sendBytes
            If Not _uc Then Throw New NetLibException(New InvalidOperationException("This UDP Client does not have a dedicated connection."))
            Dim ret As Integer = 0
            Dim limit As Integer = _sock.SendBufferSize
            If _hlh Then limit -= 4
            If bytes.Length > limit Then Throw New NetLibException(New ArgumentOutOfRangeException("The Byte Array is too big."))
            SyncLock slocksend
                Try
                    If _hlh Then
                        Dim len As Byte() = Utilities.Int32ToBytes(bytes.Length)
                        ret = _sock.Send(len, len.Length, SocketFlags.None)
                        If ret = len.Length Then ret = _sock.Send(bytes, bytes.Length, SocketFlags.None) Else ret = 0
                    Else
                        ret = _sock.Send(bytes, bytes.Length, SocketFlags.None)
                    End If
                Catch ex As Exception When (TypeOf ex Is SocketException Or TypeOf ex Is ArgumentException Or TypeOf ex Is SecurityException Or TypeOf ex Is ObjectDisposedException Or TypeOf ex Is InvalidOperationException)
                    Return False
                End Try
            End SyncLock
            Return ret > 0 And ret = bytes.Length
        End Function
        ''' <summary>
        ''' Receives a byte array from the network.
        ''' </summary>
        ''' <returns>The Received Byte Array</returns>
        ''' <remarks></remarks>
        Public Overridable Function receiveBytes() As Byte() Implements INetSocketConnectionless.receiveBytes
            If Not _uc Then Throw New NetLibException(New InvalidOperationException("This UDP Client does not have a dedicated connection."))
            Dim bts(-1) As Byte
            SyncLock slockreceive
                Try
                    If _hlh Then
                        Dim len(3) As Byte
                        _sock.Receive(len, 4, SocketFlags.None)
                        Dim lengthflen As Integer = Utilities.BytesToInt32(len)
                        If lengthflen > _sock.ReceiveBufferSize - 4 Then Return bts
                        ReDim bts(lengthflen - 1)
                        Dim btsrem As Integer = lengthflen
                        Dim btsind As Integer = 0
                        While btsrem > 0
                            Dim rec(btsrem - 1) As Byte
                            Dim ls As Integer = _sock.Receive(rec, btsrem, SocketFlags.None)
                            Buffer.BlockCopy(rec, 0, bts, btsind, ls)
                            btsind += ls
                            btsrem -= ls
                        End While
                    Else
                        Dim btsb(_sock.ReceiveBufferSize - 1) As Byte
                        Dim lentr As Integer = _sock.Receive(btsb, _sock.ReceiveBufferSize, SocketFlags.None)
                        If lentr < 1 Then Return bts
                        ReDim bts(lentr - 1)
                        Buffer.BlockCopy(btsb, 0, bts, 0, lentr)
                    End If
                Catch ex As Exception When (TypeOf ex Is SocketException Or TypeOf ex Is ArgumentException Or TypeOf ex Is SecurityException Or TypeOf ex Is ObjectDisposedException Or TypeOf ex Is InvalidOperationException)
                End Try
            End SyncLock
            Return bts
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
            If _uc Then Throw New NetLibException(New InvalidOperationException("This UDP Client has a dedicated connection."))
            Dim remote_IP As IPAddress = IPAddress.Parse(remoteIP)
            Dim ret As Integer = 0
            Dim limit As Integer = _sock.SendBufferSize
            If _hlh Then limit -= 4
            If bytes.Length > limit Then Throw New NetLibException(New ArgumentOutOfRangeException("The Byte Array is too big."))
            SyncLock slocksend
                Try
                    If _hlh Then
                        Dim len As Byte() = Utilities.Int32ToBytes(bytes.Length)
                        ret = _sock.SendTo(len, len.Length, SocketFlags.None, New IPEndPoint(remote_IP, remotePort))
                        If ret = len.Length Then ret = _sock.SendTo(bytes, bytes.Length, SocketFlags.None, New IPEndPoint(remote_IP, remotePort)) Else ret = 0
                    Else
                        ret = _sock.SendTo(bytes, bytes.Length, SocketFlags.None, New IPEndPoint(remote_IP, remotePort))
                    End If
                Catch ex As Exception When (TypeOf ex Is SocketException Or TypeOf ex Is ArgumentException Or TypeOf ex Is SecurityException Or TypeOf ex Is ObjectDisposedException Or TypeOf ex Is InvalidOperationException)
                    Return False
                End Try
            End SyncLock
            Return ret > 0 And ret = bytes.Length
        End Function
        ''' <summary>
        ''' Receives a byte array from the specified address and port on the network.
        ''' </summary>
        ''' <param name="remoteIP">The remote IP</param>
        ''' <param name="remotePort">The remote Port</param>
        ''' <returns>The Received Byte Array</returns>
        ''' <remarks></remarks>
        Public Overridable Function receiveBytesFrom(remoteIP As String, remotePort As Integer) As Byte() Implements INetSocketConnectionless.receiveBytesFrom
            If _uc Then Throw New NetLibException(New InvalidOperationException("This UDP Client has a dedicated connection."))
            Dim remote_IP As IPAddress = IPAddress.Parse(remoteIP)
            Dim bts(-1) As Byte
            SyncLock slockreceive
                Try
                    If _hlh Then
                        Dim len(3) As Byte
                        _sock.ReceiveFrom(len, 4, SocketFlags.None, New IPEndPoint(remote_IP, remotePort))
                        Dim lengthflen As Integer = Utilities.BytesToInt32(len)
                        If lengthflen > _sock.ReceiveBufferSize - 4 Then Return bts
                        ReDim bts(lengthflen - 1)
                        Dim btsrem As Integer = lengthflen
                        Dim btsind As Integer = 0
                        While btsrem > 0
                            Dim rec(btsrem - 1) As Byte
                            Dim ls As Integer = _sock.ReceiveFrom(rec, btsrem, SocketFlags.None, New IPEndPoint(remote_IP, remotePort))
                            Buffer.BlockCopy(rec, 0, bts, btsind, ls)
                            btsind += ls
                            btsrem -= ls
                        End While
                    Else
                        Dim btsb(_sock.ReceiveBufferSize - 1) As Byte
                        Dim lentr As Integer = _sock.ReceiveFrom(btsb, _sock.ReceiveBufferSize, SocketFlags.None, New IPEndPoint(remote_IP, remotePort))
                        If lentr < 1 Then Return bts
                        ReDim bts(lentr - 1)
                        Buffer.BlockCopy(btsb, 0, bts, 0, lentr)
                    End If
                Catch ex As Exception When (TypeOf ex Is SocketException Or TypeOf ex Is ArgumentException Or TypeOf ex Is SecurityException Or TypeOf ex Is ObjectDisposedException Or TypeOf ex Is InvalidOperationException)
                End Try
            End SyncLock
            Return bts
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
                    Me.copyConfigFrom(sconf)
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
                Catch ex As Exception When (TypeOf ex Is SocketException Or TypeOf ex Is ArgumentException Or TypeOf ex Is SecurityException Or TypeOf ex Is ObjectDisposedException Or TypeOf ex Is InvalidOperationException)
                    Throw New NetLibException(ex)
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
                    Me.copyConfigFrom(sconf)
                    _rip = IPAddress.Any
                    _rport = 0
                    _uc = False
                    _c = False
                    If Not _sock.IsBound Then
                        _sock.Bind(New IPEndPoint(_lip, _lport))
                        _l = True
                    End If
                Catch ex As Exception When (TypeOf ex Is SocketException Or TypeOf ex Is ArgumentException Or TypeOf ex Is SecurityException Or TypeOf ex Is ObjectDisposedException Or TypeOf ex Is InvalidOperationException)
                    Throw New NetLibException(ex)
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
                Throw New NetLibException(New InvalidOperationException("Not a TCP Listener Or TCP Client."))
            End Get
            Set(value As Boolean)
                Throw New NetLibException(New InvalidOperationException("Not a TCP Listener Or TCP Client."))
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
        ''' Returns the local IP Address.
        ''' </summary>
        ''' <value>String</value>
        ''' <returns>The local IP Address</returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property localIPAddress As String Implements INetConfig.localIPAddress
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
        Public Overridable ReadOnly Property localPort As Integer Implements INetConfig.localPort
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
        Public Overridable ReadOnly Property remoteIPAddress As String Implements INetConfig.remoteIPAddress
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
        Public Overridable ReadOnly Property remotePort As Integer Implements INetConfig.remotePort
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
            Dim toret As New NetSocketConfig(Me.localIPAddress, Me.localPort, Me.remoteIPAddress, Me.remotePort)
            toret.sendBufferSize = Me.sendBufferSize
            toret.receiveBufferSize = Me.receiveBufferSize
            toret.receiveTimeout = Me.receiveTimeout
            toret.sendTimeout = Me.sendTimeout
            toret.exclusiveAddressUse = Me.exclusiveAddressUse
            toret.hasLengthHeader = Me.hasLengthHeader
            Return toret
        End Function
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


