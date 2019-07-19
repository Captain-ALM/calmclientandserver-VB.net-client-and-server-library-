Imports System.Net
Imports System.Net.Sockets
Imports System.Net.NetworkInformation

'
' Created by SharpDevelop.
' User: Alfred
' Date: 19/05/2019
' Time: 15:58
' 
' To change this template use Tools | Options | Coding | Edit Standard Headers.
'
Namespace CALMNetLib

    Public Class NetTCPClient
        Implements INetSocket, INetConfig, IDisposable

        Protected _sock As Socket = Nothing
        Protected _ip As IPAddress = Nothing
        Protected _port As Integer = 0
        Protected slocksend As New Object()
        Protected slockreceive As New Object()

        Public Sub New(ip As IPAddress, port As Integer)
            _sock = New Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            _sock.NoDelay = False
            _sock.ReceiveTimeout = 0
            _sock.SendTimeout = 0
            _ip = ip
            _port = port
        End Sub

        Friend Sub New(sock As Socket)
            _sock = sock
            _ip = CType(sock.RemoteEndPoint, IPEndPoint).Address
            _port = CType(sock.RemoteEndPoint, IPEndPoint).Port
        End Sub

        Public Overridable Sub open() Implements INetSocket.open
            Try
                If Not _sock.Connected Then _sock.Connect(New IPEndPoint(_ip, _port))
            Catch ex As ObjectDisposedException
                Utilities.addException(New NetLibException(ex))
            Catch ex As SocketException
                Utilities.addException(New NetLibException(ex))
            End Try
        End Sub

        Protected Overridable Function pollConnection() As Boolean
            Try
                If _sock.SendTimeout > 0 And _sock.ReceiveTimeout > 0 Then
                    Try
                        Dim toret As Boolean = False
                        SyncLock slocksend
                            _sock.Send(New Byte() {0}, 0, SocketFlags.None)
                            toret = _sock.Poll((1000 * (_sock.SendTimeout + 1)), SelectMode.SelectWrite) And _sock.Connected And Not ((_sock.Available = 0 And _sock.Poll((1000 * (_sock.ReceiveTimeout + 1)), SelectMode.SelectRead)))
                            Dim ce As Boolean = False
                            Dim tcpConnections As TcpConnectionInformation() = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections()
                            For Each c As TcpConnectionInformation In tcpConnections
                                Dim stateOfConnection As TcpState = c.State
                                If c.LocalEndPoint.Equals(_sock.LocalEndPoint) AndAlso c.RemoteEndPoint.Equals(_sock.RemoteEndPoint) Then
                                    ce = True
                                    If stateOfConnection = TcpState.Established Then
                                        toret = True
                                    Else
                                        toret = False
                                    End If
                                End If
                            Next
                            If Not ce Then toret = False
                        End SyncLock
                        Return toret
                    Catch ex As NetworkInformationException
                        Utilities.addException(New NetLibException(ex))
                    Catch ex As ObjectDisposedException
                        Utilities.addException(New NetLibException(ex))
                    Catch ex As SocketException
                        Utilities.addException(New NetLibException(ex))
                    End Try
                Else
                    Return _sock.Connected
                End If
            Catch ex As ObjectDisposedException
                Utilities.addException(New NetLibException(ex))
            End Try
            Return False
        End Function

        Public Overridable ReadOnly Property connected As Boolean Implements INetSocket.connected
            Get
                Return pollConnection()
            End Get
        End Property

        Public Overridable ReadOnly Property listening As Boolean Implements INetSocket.listening
            Get
                Return False
            End Get
        End Property

        Public Overridable ReadOnly Property hasData As Boolean Implements INetSocket.hasData
            Get
                If Not pollConnection() Then Return False
                Dim toret As Boolean = False
                SyncLock slockreceive
                    Try
                        toret = _sock.Available > 0
                    Catch ex As SocketException
                        Utilities.addException(New NetLibException(ex))
                    End Try
                End SyncLock
                Return toret
            End Get
        End Property

        Public Overridable Function sendBytes(bytes As Byte()) As Boolean Implements INetSocket.sendBytes
            Dim ret As Integer = 0
            If Not pollConnection() Then Return False
            If bytes.Length > _sock.SendBufferSize - 4 Then Throw New NetLibException(New ArgumentOutOfRangeException("The Byte Array is too big."))
            SyncLock slocksend
                Try
                    Dim len As Byte() = Utilities.Int32ToBytes(bytes.Length)
                    Dim ts((3 + bytes.Length)) As Byte
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

        Public Overridable Function recieveBytes() As Byte() Implements INetSocket.recieveBytes
            Dim bts(-1) As Byte
            If Not pollConnection() Then Return bts
            SyncLock slockreceive
                Try
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
                Catch ex As SocketException
                    Utilities.addException(New NetLibException(ex))
                End Try
            End SyncLock
            Return bts
        End Function

        Public Overridable ReadOnly Property clientWaiting As Boolean Implements INetSocket.clientWaiting
            Get
                Throw New NetLibException(New InvalidOperationException("Not a TCP Listener."))
            End Get
        End Property

        Public Overridable Function acceptClient() As INetSocket Implements INetSocket.acceptClient
            Throw New NetLibException(New InvalidOperationException("Not a TCP Listener."))
        End Function

        Public Overridable Sub close() Implements INetSocket.close, IDisposable.Dispose
            If pollConnection() Then
                Try
                    _sock.Shutdown(SocketShutdown.Both)
                Catch ex As ObjectDisposedException
                    Utilities.addException(New NetLibException(ex))
                Catch ex As SocketException
                    Utilities.addException(New NetLibException(ex))
                End Try
                Try
                    _sock.Close()
                Catch ex As ObjectDisposedException
                    Utilities.addException(New NetLibException(ex))
                Catch ex As SocketException
                    Utilities.addException(New NetLibException(ex))
                End Try
            End If
        End Sub

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

        Public Overridable ReadOnly Property remoteIPAddress As String Implements INetConfig.remoteIPAddress
            Get
                Return _ip.ToString()
            End Get
        End Property

        Public Overridable ReadOnly Property remotePort As Integer Implements INetConfig.remotePort
            Get
                Return _port
            End Get
        End Property

        Public Overridable ReadOnly Property localIPAddress As String Implements INetConfig.localIPAddress
            Get
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                If _sock.LocalEndPoint Is Nothing Then Throw New NetLibException(New InvalidOperationException("socket not open"))
                Return CType(_sock.LocalEndPoint, IPEndPoint).Address.ToString()
            End Get
        End Property

        Public Overridable ReadOnly Property localPort As Integer Implements INetConfig.localPort
            Get
                If _sock Is Nothing Then Throw New NetLibException(New NullReferenceException("socket is null"))
                If _sock.LocalEndPoint Is Nothing Then Throw New NetLibException(New InvalidOperationException("socket not open"))
                Return CType(_sock.LocalEndPoint, IPEndPoint).Port
            End Get
        End Property

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

        Public Overridable Property connectionBacklog As Integer Implements INetConfig.connectionBacklog
            Get
                Throw New NetLibException(New InvalidOperationException("Not a TCP Listener."))
            End Get
            Set(value As Integer)
                Throw New NetLibException(New InvalidOperationException("Not a TCP Listener."))
            End Set
        End Property
    End Class
End Namespace
