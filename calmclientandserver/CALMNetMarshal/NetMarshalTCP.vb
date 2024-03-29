﻿Imports System.Net
Imports captainalm.CALMNetLib
Imports System.Threading

Namespace CALMNetMarshal
    ''' <summary>
    ''' This class provides a TCP Socket Marshal.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class NetMarshalTCP
        Inherits NetMarshalBase

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
        ''' <param name="bufsiz">The buffer size for the sockets</param>
        ''' <param name="hLengthHeader">Whether a length header is used for message passing</param>
        ''' <remarks></remarks>
        Public Sub New(iptb As IPAddress, ptb As Integer, Optional cbl As Integer = 1, Optional del As Boolean = False, Optional bufsiz As Integer = 16777216, Optional hLengthHeader As Boolean = True)
            MyBase.New(New NetTCPListener(iptb, ptb) With {.sendBufferSize = bufsiz, .receiveBufferSize = bufsiz, .noDelay = Not del, .connectionBacklog = cbl, .ClientConfig = New NetSocketConfig() With {.sendBufferSize = bufsiz, .receiveBufferSize = bufsiz, .noDelay = Not del, .hasLengthHeader = hlengthheader}})
            _delay = del
            _buffer = bufsiz
            _haslengthheader = hLengthHeader
        End Sub
        ''' <summary>
        ''' Constructs a new instance of NetMarshalTCP.
        ''' </summary>
        ''' <param name="iptb">The IP Address to bind to</param>
        ''' <param name="ptb">The Port to bind to</param>
        ''' <param name="conf">
        ''' The Net Socket configuration to use,
        ''' the buffer size is set to the largest of the send and receive buffer sizes.
        ''' </param>
        ''' <remarks></remarks>
        Public Sub New(iptb As IPAddress, ptb As Integer, conf As INetConfig)
            MyBase.New(New NetTCPListener(iptb, ptb) With {.sendBufferSize = conf.sendBufferSize, .receiveBufferSize = conf.receiveBufferSize, .noDelay = conf.noDelay, .connectionBacklog = conf.connectionBacklog, .ClientConfig = New NetSocketConfig(conf)})
            If conf.sendBufferSize >= conf.receiveBufferSize Then CType(_cl, INetConfig).receiveBufferSize = conf.sendBufferSize Else CType(_cl, INetConfig).sendBufferSize = conf.receiveBufferSize
            _delay = Not conf.noDelay
            _buffer = CType(_cl, INetConfig).receiveBufferSize
            _haslengthheader = conf.hasLengthHeader
            CType(_cl, NetTCPListener).ClientConfig.receiveBufferSize = _buffer
            CType(_cl, NetTCPListener).ClientConfig.sendBufferSize = _buffer
        End Sub
        ''' <summary>
        ''' Starts the Marshal and Opens the Connection.
        ''' </summary>
        ''' <remarks></remarks>
        Public Overrides Sub start()
            If _cl Is Nothing Then Return
            If Not _cl.listening Then _cl.open()
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
            disconnectAll()
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
                    Dim rip As String = ct.duplicatedInternalSocketConfig.remoteIPAddress
                    Dim rp As Integer = ct.duplicatedInternalSocketConfig.remotePort
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
                    Dim cs As INetSocket = New NetTCPClient(IPAddress.Parse(lip), lport) With {.receiveBufferSize = _buffer, .sendBufferSize = _buffer, .noDelay = Not _delay, .hasLengthHeader = _haslengthheader}
                    Dim ct As New NetMarshalTCPClient(cs) With {.serializer = _serializer}
                    ct.beatTimeout = _bout
                    SyncLock _slockcolman
                        _clcol.Add(ct)
                    End SyncLock
                    AddHandler ct.exceptionRaised, AddressOf raiseExceptionRaised
                    AddHandler ct.MessageReceived, AddressOf raiseMessageReceived
                    ct.start()
                    raiseClientConnected(ct.duplicatedInternalSocketConfig.remoteIPAddress, ct.duplicatedInternalSocketConfig.remotePort)
                    ct.releaseCache()
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
                    Dim reip As String = ct.duplicatedInternalSocketConfig.remoteIPAddress
                    Dim rp As Integer = ct.duplicatedInternalSocketConfig.remotePort
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
        ''' Disconnects all currently Connected Clients.
        ''' </summary>
        ''' <remarks></remarks>
        Public Overridable Sub disconnectAll()
            While _clcol.Count > 0
                For i As Integer = _clcol.Count - 1 To 0 Step -1
                    Try
                        Dim ct As NetMarshalTCPClient = Nothing
                        SyncLock _slockcolman
                            ct = _clcol(i)
                        End SyncLock
                        Dim rip As String = ct.duplicatedInternalSocketConfig.remoteIPAddress
                        Dim rp As Integer = ct.duplicatedInternalSocketConfig.remotePort
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
                If _threaddelay > 0 Then Thread.Sleep(_threaddelay)
            End While
        End Sub
        ''' <summary>
        ''' Gets or sets the timeout of beat messages to test sockets.
        ''' A value of 0 disables beat checks.
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
                        Dim reip As String = ct.duplicatedInternalSocketConfig.remoteIPAddress
                        Dim rp As Integer = ct.duplicatedInternalSocketConfig.remotePort
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
                        Dim reip As String = ct.duplicatedInternalSocketConfig.remoteIPAddress
                        Dim rp As Integer = ct.duplicatedInternalSocketConfig.remotePort
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
        ''' <summary>
        ''' Sets the buffer size of the net marshal.
        ''' </summary>
        ''' <value>Integer</value>
        ''' <returns>The buffer size of the net marshal</returns>
        ''' <remarks></remarks>
        Public Overrides Property bufferSize As Integer
            Get
                Return MyBase.bufferSize
            End Get
            Set(value As Integer)
                MyBase.bufferSize = value
                For i As Integer = _clcol.Count - 1 To 0 Step -1
                    Try
                        Dim ct As NetMarshalTCPClient = Nothing
                        SyncLock _slockcolman
                            ct = _clcol(i)
                        End SyncLock
                        ct.bufferSize = value
                    Catch ex As Exception When (TypeOf ex Is ArgumentOutOfRangeException Or TypeOf ex Is IndexOutOfRangeException)
                        raiseExceptionRaised(ex)
                    End Try
                Next
            End Set
        End Property
        ''' <summary>
        ''' Sets if the net marshal uses length headers when passing messages
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>Whether length headers are used when passing messages</returns>
        ''' <remarks></remarks>
        Public Overrides Property hasLengthHeader As Boolean
            Get
                Return MyBase.hasLengthHeader
            End Get
            Set(value As Boolean)
                MyBase.hasLengthHeader = value
                For i As Integer = _clcol.Count - 1 To 0 Step -1
                    Try
                        Dim ct As NetMarshalTCPClient = Nothing
                        SyncLock _slockcolman
                            ct = _clcol(i)
                        End SyncLock
                        ct.hasLengthHeader = value
                    Catch ex As Exception When (TypeOf ex Is ArgumentOutOfRangeException Or TypeOf ex Is IndexOutOfRangeException)
                        raiseExceptionRaised(ex)
                    End Try
                Next
            End Set
        End Property

        Protected Overrides Sub t_exec()
            While _cl IsNot Nothing AndAlso _cl.listening
                Try
                    While _cl IsNot Nothing AndAlso _cl.listening
                        If _cl.clientWaiting Then
                            Dim [as] As INetSocket = _cl.acceptClient()
                            Dim ct As New NetMarshalTCPClient([as]) With {.serializer = _serializer}
                            ct.beatTimeout = _bout
                            SyncLock _slockcolman
                                _clcol.Add(ct)
                            End SyncLock
                            AddHandler ct.exceptionRaised, AddressOf raiseExceptionRaised
                            AddHandler ct.MessageReceived, AddressOf raiseMessageReceived
                            ct.start()
                            raiseClientConnected(ct.duplicatedInternalSocketConfig.remoteIPAddress, ct.duplicatedInternalSocketConfig.remotePort)
                            ct.releaseCache()
                        End If
                        For i As Integer = _clcol.Count - 1 To 0 Step -1
                            Try
                                Dim ct As NetMarshalTCPClient = Nothing
                                SyncLock _slockcolman
                                    ct = _clcol(i)
                                End SyncLock
                                If Not ct.ready Then
                                    Dim rip As String = ct.duplicatedInternalSocketConfig.remoteIPAddress
                                    Dim rp As Integer = ct.duplicatedInternalSocketConfig.remotePort
                                    RemoveHandler ct.exceptionRaised, AddressOf raiseExceptionRaised
                                    RemoveHandler ct.MessageReceived, AddressOf raiseMessageReceived
                                    ct.close()
                                    SyncLock _slockcolman
                                        _clcol.Remove(ct)
                                    End SyncLock
                                    If Not (rip Is Nothing Or rp = 0) Then raiseClientDisconnected(rip, rp)
                                    ct = Nothing
                                End If
                            Catch ex As Exception When (TypeOf ex Is ArgumentOutOfRangeException Or TypeOf ex Is IndexOutOfRangeException)
                                raiseExceptionRaised(ex)
                            End Try
                        Next
                        If _threaddelay > 0 Then Thread.Sleep(_threaddelay)
                    End While
                Catch ex As NetLibException
                    raiseExceptionRaised(ex)
                End Try
                If _threaddelay > 0 Then Thread.Sleep(_threaddelay)
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
