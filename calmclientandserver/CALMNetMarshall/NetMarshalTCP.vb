Imports System.Net
Imports captainalm.CALMNetLib
Imports System.Threading

Namespace CALMNetMarshal
    ''' <summary>
    ''' This class provides a TCP Socket Marshal.
    ''' The buffer size is 16MB.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class NetMarshalTCP
        Inherits NetMarshalBase

        Protected Const buffersize As Integer = 16777216 '65536
        Protected _clcol As New List(Of NetMarshalTCPClient)
        Protected _slockcolman As New Object()
        Protected _delay As Boolean
        ''' <summary>
        ''' This event is raised when a client connects.
        ''' </summary>
        ''' <param name="ip">The IP Address</param>
        ''' <param name="port">The Port</param>
        ''' <remarks></remarks>
        Public Event clientConnected(ip As String, port As Integer)
        ''' <summary>
        ''' This event is raised when a client disconnects.
        ''' </summary>
        ''' <param name="ip">The IP Address</param>
        ''' <param name="port">The Port</param>
        ''' <remarks></remarks>
        Public Event clientDisconnected(ip As String, port As Integer)
        ''' <summary>
        ''' Constructs a new instance of NetMarshalTCP.
        ''' </summary>
        ''' <param name="iptb">The IP Address to bind to</param>
        ''' <param name="ptb">The Port to bind to</param>
        ''' <param name="cbl">The connection backlog</param>
        ''' <param name="del">Whether nagle's algorithm is enabled</param>
        ''' <remarks></remarks>
        Public Sub New(iptb As IPAddress, ptb As Integer, Optional cbl As Integer = 1, Optional del As Boolean = False)
            MyBase.New(New NetTCPListener(iptb, ptb) With {.sendBufferSize = buffersize, .receiveBufferSize = buffersize, .noDelay = Not del, .connectionBacklog = cbl})
            _delay = del
        End Sub
        ''' <summary>
        ''' Constructs a new instance of NetMarshalTCP.
        ''' </summary>
        ''' <param name="iptb">The IP Address to bind to</param>
        ''' <param name="ptb">The Port to bind to</param>
        ''' <param name="conf">The Net Socket configuration to use</param>
        ''' <remarks></remarks>
        Public Sub New(iptb As IPAddress, ptb As Integer, conf As INetConfig)
            MyBase.New(New NetTCPListener(iptb, ptb))
            Dim confs As New NetSocketConfig(conf)
            confs.setLocalIPAddress(iptb.ToString())
            confs.setLocalPort(ptb)
            confs.setRemoteIPAddress("")
            confs.setRemotePort(0)
            confs.DuplicateConfigTo(_cl)
            _delay = Not conf.noDelay
        End Sub
        ''' <summary>
        ''' Starts the Marshal and Opens the Connection.
        ''' </summary>
        ''' <remarks></remarks>
        Public Overrides Sub start()
            If _cl Is Nothing Then Return
            _cl.open()
            MyBase.start()
        End Sub
        ''' <summary>
        ''' Stops the Marshal and Closes the Connection.
        ''' </summary>
        ''' <remarks></remarks>
        Public Overrides Sub close()
            MyBase.close()
            If _cl IsNot Nothing Then
                _cl.close()
                _cl = Nothing
            End If
            While _clcol.Count > 0
                For i As Integer = _clcol.Count - 1 To 0 Step -1
                    Try
                        Dim ct As NetMarshalTCPClient = Nothing
                        SyncLock _slockcolman
                            ct = _clcol(i)
                        End SyncLock
                        Dim rip As String = CType(ct.internalSocket, INetConfig).remoteIPAddress
                        Dim rp As Integer = CType(ct.internalSocket, INetConfig).remotePort
                        RemoveHandler ct.exceptionRaised, AddressOf raiseExceptionRaised
                        RemoveHandler ct.MessageReceived, AddressOf raiseMessageReceived
                        ct.close()
                        SyncLock _slockcolman
                            _clcol.Remove(ct)
                        End SyncLock
                        raiseClientDisconnected(rip, rp)
                        ct = Nothing
                    Catch ex As Exception When (TypeOf ex Is ArgumentOutOfRangeException Or TypeOf ex Is IndexOutOfRangeException)
                        raiseExceptionRaised(ex)
                    End Try
                Next
                Thread.Sleep(250)
            End While
            SyncLock _slockcolman
                _clcol.Clear()
                _clcol = Nothing
            End SyncLock
        End Sub
        ''' <summary>
        ''' States whether the marshal is ready.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>Whether the marshal is ready</returns>
        ''' <remarks></remarks>
        Public Overrides ReadOnly Property ready As Boolean
            Get
                Return (Not _cl Is Nothing) AndAlso _cl.listening
            End Get
        End Property
        ''' <summary>
        ''' Send a message via the marshal.
        ''' </summary>
        ''' <param name="msg">The message</param>
        ''' <returns>Whether the message sending succeeded</returns>
        ''' <remarks></remarks>
        Public Overrides Function sendMessage(msg As IPacket) As Boolean
            Dim toret As Boolean = False
            For i As Integer = _clcol.Count - 1 To 0 Step -1
                Try
                    Dim ct As NetMarshalTCPClient = Nothing
                    SyncLock _slockcolman
                        ct = _clcol(i)
                    End SyncLock
                    Dim rip As String = CType(ct.internalSocket, INetConfig).remoteIPAddress
                    Dim rp As Integer = CType(ct.internalSocket, INetConfig).remotePort
                    If rip = msg.receiverIP And rp = msg.receiverPort Then
                        toret = ct.sendMessage(msg)
                        ct = Nothing
                        Exit For
                    End If
                    ct = Nothing
                Catch ex As Exception When (TypeOf ex Is ArgumentOutOfRangeException Or TypeOf ex Is IndexOutOfRangeException)
                    raiseExceptionRaised(ex)
                End Try
            Next
            Return toret
        End Function
        ''' <summary>
        ''' Connect to a remote listener.
        ''' </summary>
        ''' <param name="lip">The remote listener IP Address</param>
        ''' <param name="lport">The remote listener Port</param>
        ''' <returns>If the remote listener accepted the connection</returns>
        ''' <remarks></remarks>
        Public Overridable Function connect(lip As String, lport As String) As Boolean
            Dim toret As Boolean = False
            If Not Me.ready(lip, lport) Then
                Try
                    Dim cs As INetSocket = New NetTCPClient(IPAddress.Parse(lip), lport) With {.receiveBufferSize = buffersize, .sendBufferSize = buffersize, .noDelay = Not _delay}
                    Dim ct As New NetMarshalTCPClient(cs)
                    AddHandler ct.exceptionRaised, AddressOf raiseExceptionRaised
                    AddHandler ct.MessageReceived, AddressOf raiseMessageReceived
                    ct.beatTimeout = _bout
                    ct.start()
                    SyncLock _slockcolman
                        _clcol.Add(ct)
                    End SyncLock
                    raiseClientConnected(CType(ct.internalSocket, INetConfig).remoteIPAddress, CType(ct.internalSocket, INetConfig).remotePort)
                    toret = True
                Catch ex As NetLibException
                    raiseExceptionRaised(ex)
                End Try
            End If
            Return toret
        End Function
        ''' <summary>
        ''' Disconnect's a Connected Client.
        ''' </summary>
        ''' <param name="rip">The remote IP Address</param>
        ''' <param name="rport">The remote Port</param>
        ''' <returns>If the client was disconnected</returns>
        ''' <remarks></remarks>
        Public Overridable Function disconnect(rip As String, rport As String) As Boolean
            Dim toret As Boolean = False
            For i As Integer = _clcol.Count - 1 To 0 Step -1
                Try
                    Dim ct As NetMarshalTCPClient = Nothing
                    SyncLock _slockcolman
                        ct = _clcol(i)
                    End SyncLock
                    Dim reip As String = CType(ct.internalSocket, INetConfig).remoteIPAddress
                    Dim rp As Integer = CType(ct.internalSocket, INetConfig).remotePort
                    If rip = reip And rport = rp Then
                        RemoveHandler ct.exceptionRaised, AddressOf raiseExceptionRaised
                        RemoveHandler ct.MessageReceived, AddressOf raiseMessageReceived
                        ct.close()
                        SyncLock _slockcolman
                            _clcol.Remove(ct)
                        End SyncLock
                        raiseClientDisconnected(reip, rp)
                        ct = Nothing
                        toret = True
                        Exit For
                    End If
                Catch ex As Exception When (TypeOf ex Is ArgumentOutOfRangeException Or TypeOf ex Is IndexOutOfRangeException)
                    raiseExceptionRaised(ex)
                End Try
            Next
            Return toret
        End Function
        ''' <summary>
        ''' Gets or sets the timeout of beat messages to test sockets.
        ''' </summary>
        ''' <value>Integer</value>
        ''' <returns>The timeout of beat messages to test sockets</returns>
        ''' <remarks></remarks>
        Public Overrides Property beatTimeout As Integer
            Get
                Return MyBase.beatTimeout
            End Get
            Set(value As Integer)
                MyBase.beatTimeout = value
                For i As Integer = _clcol.Count - 1 To 0 Step -1
                    Try
                        Dim ct As NetMarshalTCPClient = Nothing
                        SyncLock _slockcolman
                            ct = _clcol(i)
                        End SyncLock
                        ct.beatTimeout = value
                    Catch ex As Exception When (TypeOf ex Is ArgumentOutOfRangeException Or TypeOf ex Is IndexOutOfRangeException)
                        raiseExceptionRaised(ex)
                    End Try
                Next
            End Set
        End Property
        ''' <summary>
        ''' States whether a contained marshal is ready.
        ''' </summary>
        ''' <param name="rip">The Remote IP Address</param>
        ''' <param name="rport">The Remote Port</param>
        ''' <value>Boolean</value>
        ''' <returns>Whether a contained marshal is ready</returns>
        ''' <remarks></remarks>
        Public Overridable Overloads ReadOnly Property ready(rip As String, rport As Integer) As Boolean
            Get
                Dim toret As Boolean = False
                For i As Integer = _clcol.Count - 1 To 0 Step -1
                    Try
                        Dim ct As NetMarshalTCPClient = Nothing
                        SyncLock _slockcolman
                            ct = _clcol(i)
                        End SyncLock
                        Dim reip As String = CType(ct.internalSocket, INetConfig).remoteIPAddress
                        Dim rp As Integer = CType(ct.internalSocket, INetConfig).remotePort
                        If rip = reip And rport = rp Then
                            toret = ct.ready
                            Exit For
                        End If
                    Catch ex As Exception When (TypeOf ex Is ArgumentOutOfRangeException Or TypeOf ex Is IndexOutOfRangeException)
                        raiseExceptionRaised(ex)
                    End Try
                Next
                Return toret
            End Get
        End Property
        ''' <summary>
        ''' Retrieves the client specified by the remote IP Address and remote Port.
        ''' </summary>
        ''' <param name="rip">The remote IP Address</param>
        ''' <param name="rport">The remote Port</param>
        ''' <value>NetMarshalTCPClient</value>
        ''' <returns>The specified NetMarshalTCPClient</returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property client(rip As String, rport As Integer) As NetMarshalTCPClient
            Get
                Dim toret As NetMarshalTCPClient = Nothing
                For i As Integer = _clcol.Count - 1 To 0 Step -1
                    Try
                        Dim ct As NetMarshalTCPClient = Nothing
                        SyncLock _slockcolman
                            ct = _clcol(i)
                        End SyncLock
                        Dim reip As String = CType(ct.internalSocket, INetConfig).remoteIPAddress
                        Dim rp As Integer = CType(ct.internalSocket, INetConfig).remotePort
                        If rip = reip And rport = rp Then
                            toret = ct
                            Exit For
                        End If
                    Catch ex As Exception When (TypeOf ex Is ArgumentOutOfRangeException Or TypeOf ex Is IndexOutOfRangeException)
                        raiseExceptionRaised(ex)
                    End Try
                Next
                Return toret
            End Get
        End Property

        Protected Overrides Sub t_exec()
            While _cl IsNot Nothing AndAlso _cl.connected
                Try
                    While _cl IsNot Nothing AndAlso _cl.connected
                        If _cl.clientWaiting Then
                            Dim [as] As INetSocket = _cl.acceptClient()
                            Dim ct As New NetMarshalTCPClient([as])
                            AddHandler ct.exceptionRaised, AddressOf raiseExceptionRaised
                            AddHandler ct.MessageReceived, AddressOf raiseMessageReceived
                            ct.beatTimeout = _bout
                            ct.start()
                            SyncLock _slockcolman
                                _clcol.Add(ct)
                            End SyncLock
                            raiseClientConnected(CType(ct.internalSocket, INetConfig).remoteIPAddress, CType(ct.internalSocket, INetConfig).remotePort)
                        End If
                        For i As Integer = _clcol.Count - 1 To 0 Step -1
                            Try
                                Dim ct As NetMarshalTCPClient = Nothing
                                SyncLock _slockcolman
                                    ct = _clcol(i)
                                End SyncLock
                                Dim rip As String = CType(ct.internalSocket, INetConfig).remoteIPAddress
                                Dim rp As Integer = CType(ct.internalSocket, INetConfig).remotePort
                                If Not ct.ready Then
                                    RemoveHandler ct.exceptionRaised, AddressOf raiseExceptionRaised
                                    RemoveHandler ct.MessageReceived, AddressOf raiseMessageReceived
                                    ct.close()
                                    SyncLock _slockcolman
                                        _clcol.Remove(ct)
                                    End SyncLock
                                    raiseClientDisconnected(rip, rp)
                                    ct = Nothing
                                End If
                            Catch ex As Exception When (TypeOf ex Is ArgumentOutOfRangeException Or TypeOf ex Is IndexOutOfRangeException)
                                raiseExceptionRaised(ex)
                            End Try
                        Next
                        Thread.Sleep(250)
                    End While
                Catch ex As NetLibException
                    raiseExceptionRaised(ex)
                End Try
                Thread.Sleep(250)
            End While
        End Sub

        Protected Sub raiseClientConnected(ip As String, port As Integer)
            RaiseEvent clientConnected(ip, port)
        End Sub

        Protected Sub raiseClientDisconnected(ip As String, port As Integer)
            RaiseEvent clientDisconnected(ip, port)
        End Sub
    End Class

End Namespace
