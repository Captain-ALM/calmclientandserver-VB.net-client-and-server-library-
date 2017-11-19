Imports System.Net
Imports System.Net.Sockets
Imports System.Net.NetworkInformation
Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Text
Imports System.Threading
''' <summary>
''' Provides a server class.
''' </summary>
''' <remarks></remarks>
Public Class server
    Private _ip As IPAddress = IPAddress.None

    Private _port As Integer = 100

    Private tcpListener As TcpListener = Nothing

    Private tcpServer As TcpClient = Nothing

    Private tcpServerNetStream As NetworkStream

    Private listening As Boolean = False

    Private _closeDelay As Integer = 100

    Private lockSend As Object = New Object()

    Private lockListen As Object = New Object()

    Private clients As List(Of clientobj) = New List(Of clientobj)

    Private serverData As List(Of String) = New List(Of String)

    Private encryptmethod As EncryptionMethod = EncryptionMethod.none

    Private password As String = ""

    Private listenthread As Thread = Nothing

    Private synclockcheckl As Boolean = False

    Private synclockchecks As Boolean = False

    ''' <summary>
    ''' Raised everytime data is received.
    ''' </summary>
    ''' <param name="Data">Packet received.</param>
    ''' <param name="clientname">The name of sender.</param>
    ''' <remarks></remarks>
    Public Event ClientMessage(ByVal clientname As String, ByVal data As packet)
    ''' <summary>
    ''' Raised when an error occurs.
    ''' </summary>
    ''' <param name="ex">The exception occured.</param>
    ''' <remarks></remarks>
    Public Event errEncounter(ByVal ex As Exception)
    ''' <summary>
    ''' Raised everytime a client connected.
    ''' </summary>
    ''' <param name="clientname">The client connected ID.</param>
    ''' <remarks></remarks>
    Public Event ClientConnect(ByVal clientname As String)
    ''' <summary>
    ''' Raised everytime a client disconnected.
    ''' </summary>
    ''' <param name="clientname">The disconnected client name.</param>
    ''' <remarks></remarks>
    Public Event ClientDisconnect(ByVal clientname As String)
    ''' <summary>
    ''' Raised when the Server Stops.
    ''' </summary>
    ''' <remarks></remarks>
    Public Event ConnectionClosed()
    ''' <summary>
    ''' Creates a new default instance of the server class.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()
        tcpListener = New TcpListener(_ip, _port)
    End Sub
    ''' <summary>
    ''' Creates a new instance of the server class with the specified IP address and port (optional).
    ''' </summary>
    ''' <param name="ipaddress">The IP address to bind to.</param>
    ''' <param name="port">The port to bind to (Optional) [Default: 100].</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal ipaddress As IPAddress, Optional ByVal port As Integer = 100)
        Try
            _ip = ipaddress
        Catch ex As Exception
            _ip = System.Net.IPAddress.None
        End Try
        Try
            _port = port
        Catch ex As Exception
            _port = 100
        End Try
        tcpListener = New TcpListener(_ip, _port)
    End Sub
    ''' <summary>
    ''' Flushes this instance of server (Cleaning).
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub flush()
        If Not listening And Not synclockcheckl And Not synclockchecks Then
            _ip = IPAddress.None

            _port = 100

            tcpListener = New TcpListener(IPAddress.None, _port)

            tcpServer = Nothing

            tcpServerNetStream = Nothing

            listening = False

            _closeDelay = 100

            lockSend = New Object()

            lockListen = New Object()

            clients = New List(Of clientobj)

            serverData = New List(Of String)

            encryptmethod = EncryptionMethod.none

            password = ""

            listenthread = Nothing

            synclockcheckl = False

            synclockchecks = False
        End If
    End Sub
    ''' <summary>
    ''' Get the IP address of the server.
    ''' </summary>
    ''' <value>IP address of the server.</value>
    ''' <returns>IP address of the server.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property ip() As IPAddress
        Get
            If _ip Is IPAddress.None Then
                Dim list As List(Of IPAddress) = New List(Of IPAddress)()
                Dim allNetworkInterfaces As NetworkInterface() = NetworkInterface.GetAllNetworkInterfaces()
                For i As Integer = 0 To allNetworkInterfaces.Length - 1
                    Dim networkInterface As NetworkInterface = allNetworkInterfaces(i)
                    If networkInterface.OperationalStatus = OperationalStatus.Up AndAlso networkInterface.NetworkInterfaceType <> NetworkInterfaceType.Loopback Then
                        For Each current As UnicastIPAddressInformation In networkInterface.GetIPProperties().UnicastAddresses
                            If current.Address.AddressFamily = AddressFamily.InterNetwork Then
                                list.Add(current.Address)
                            End If
                        Next
                    End If
                Next
                If list.Count = 0 Then
                    Return IPAddress.None
                End If
                _ip = list(0)
            End If
            Return _ip
        End Get
    End Property
    ''' <summary>
    ''' Gets the port the server listens on.
    ''' </summary>
    ''' <value>the port the server listens on.</value>
    ''' <returns>the port the server listens on.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Port() As Integer
        Get
            Return _port
        End Get
    End Property
    ''' <summary>
    ''' Gets or Sets the delay to recieve messages before the client is disconnected.
    ''' </summary>
    ''' <value>the delay to recieve messages before the client is disconnected.</value>
    ''' <returns>the delay to recieve messages before the client is disconnected.</returns>
    ''' <remarks></remarks>
    Public Property CloseDelay() As Integer
        Get
            Return _closeDelay
        End Get
        Set(value As Integer)
            _closeDelay = value
        End Set
    End Property

    ''' <summary>
    ''' The number of bytes to send.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SendBufferSize() As Integer
        Get
            Return tcpListener.Server.SendBufferSize
        End Get
        Set(ByVal value As Integer)
            tcpListener.Server.SendBufferSize = value
        End Set
    End Property
    ''' <summary>
    ''' The number of bytes to receive.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ReceiveBufferSize() As Integer
        Get
            Return tcpListener.Server.ReceiveBufferSize
        End Get
        Set(ByVal value As Integer)
            tcpListener.Server.ReceiveBufferSize = value
        End Set
    End Property
    ''' <summary>
    ''' Determines if the server should wait an amount of time so more data will be added to the send data buffer, if set to true, the server will send the data immediatly.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NoDelay() As Boolean
        Get
            Return tcpListener.Server.NoDelay
        End Get
        Set(ByVal value As Boolean)
            tcpListener.Server.NoDelay = value
        End Set
    End Property
    ''' <summary>
    ''' Gets the currently connected clients.
    ''' </summary>
    ''' <value>the currently connected clients.</value>
    ''' <returns>the currently connected clients.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property connected_clients As List(Of String)
        Get
            Return serverData
        End Get
    End Property
    ''' <summary>
    ''' Returns if the server is listening.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property isListening As Boolean
        Get
            Return listening
        End Get
    End Property
    ''' <summary>
    ''' Gets a client by name.
    ''' </summary>
    ''' <param name="name">The Client's Name to Find.</param>
    ''' <returns>The Client's Name or Nothing.</returns>
    ''' <remarks></remarks>
    Private Function GetClient(name As String) As clientobj
        For Each current As clientobj In clients
            If current.name = name Then
                Return current
            End If
        Next
        Return Nothing
    End Function
    ''' <summary>
    ''' Removes a client from the server.
    ''' </summary>
    ''' <param name="name">The client to remove.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function RemoveClient(name As String) As Boolean
        Dim client As clientobj = GetClient(name)
        If client IsNot Nothing Then
            clients.Remove(client)
            serverData.Remove(name)
            Return True
        End If
        Return False
    End Function
    ''' <summary>
    ''' Starts the server.
    ''' </summary>
    ''' <param name="password2">The server password to use (Optional if encrypt type is none or unicode only).</param>
    ''' <param name="encrypttype">The encrypt type to use none, unicode, ase and unicodease (ase and unicode ase require a password).</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Start(Optional password2 As String = "", Optional encrypttype As EncryptionMethod = EncryptionMethod.none) As Boolean
        Dim result As Boolean = False
        Try
            listening = True
            password = password2
            encryptmethod = encrypttype
            listenthread = New Thread(New ThreadStart(AddressOf Listen))
            listenthread.IsBackground = True
            listenthread.Start()
            result = True
        Catch ex As Exception
            result = False
            RaiseEvent errEncounter(ex)
        End Try
        Return result
    End Function
    ''' <summary>
    ''' Stops listening to connections and stops the server.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub [Stop]()
        On Error Resume Next
        listening = False
        If Not tcpServer Is Nothing Then tcpServer.Close()
        For Each c As clientobj In clients
            c.Disconnect(False)
        Next
        If Not tcpServerNetStream Is Nothing Then tcpServerNetStream.Close(0)
        tcpListener.Stop()
        RaiseEvent ConnectionClosed()
    End Sub
    ''' <summary>
    ''' Kill the operating threads if they are still alive.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub kill_threads()
        If Not listening And Not synclockcheckl And Not synclockchecks Then
            While listenthread.IsAlive
                Thread.Sleep(150)
                If listenthread.ThreadState = ThreadState.AbortRequested Or listenthread.ThreadState = 132 Then
                    Exit While
                ElseIf Not synclockcheckl And Not synclockchecks Then
                    listenthread.Abort()
                End If
            End While
        End If
    End Sub
    ''' <summary>
    ''' Sends data to all of the connected clients.
    ''' </summary>
    ''' <param name="packet">The packet to send.</param>
    ''' <remarks></remarks>
    Public Function broadcast(ByVal packet As packet) As Boolean
        Try
            Dim Data() As Byte = intmsg.GetBytes(New intmsg(MainEncoding.GetBytes(packet2string(packet))))
            For Each c As clientobj In clients
                c.SendData(Data)
            Next
            Return True
        Catch ex As Exception
            RaiseEvent errEncounter(ex)
            Return False
        End Try
    End Function
    ''' <summary>
    ''' Disconnect a specific user.
    ''' </summary>
    ''' <param name="clientName">The client name to disconnect.</param>
    ''' <remarks></remarks>
    Public Function Disconnect(clientName As String) As Boolean
        Dim result As Boolean = False
        Try
            GetClient(clientName).Disconnect(False)
            result = True
        Catch ex As Exception
            result = False
            RaiseEvent errEncounter(ex)
        End Try
        Return result
    End Function
    ''' <summary>
    ''' Sends a message to a client.
    ''' </summary>
    ''' <param name="clientName">The client's name.</param>
    ''' <param name="message">The packet to send.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Send(ByVal clientName As String, ByVal message As packet) As Boolean
        Dim result As Boolean = False
        SyncLock lockSend
            synclockchecks = True
            Try
                Dim client As clientobj = GetClient(clientName)
                If client Is Nothing Then
                    result = False
                ElseIf Not client.isConnected Then
                    result = False
                Else
                    Dim bytes As Byte() = intmsg.GetBytes(New intmsg(MainEncoding.GetBytes(packet2string(message))))
                    client.SendData(bytes)
                    result = True
                End If
            Catch ex As Exception
                result = False
                RaiseEvent errEncounter(ex)
            End Try
            synclockchecks = False
        End SyncLock
        Return result
    End Function

    Private Sub Listen()
        tcpListener.Start()
        Do
            Try
                tcpServer = tcpListener.AcceptTcpClient()
                tcpServerNetStream = tcpServer.GetStream()
                Dim bytes(tcpServer.ReceiveBufferSize) As Byte
                tcpServerNetStream.Read(bytes, 0, tcpServer.ReceiveBufferSize)
                Dim clnom As String = ConvertFromAscii(bytes)
                If Not GetClient(clnom) Is Nothing Then
                    Dim OriginID As String = clnom
                    Dim cnt As Integer = 1
                    clnom = OriginID & cnt
                    While Not GetClient(clnom) Is Nothing
                        cnt += 1
                        clnom = OriginID & cnt
                    End While
                End If
                Dim clobj As New clientobj(clnom, tcpServer)
                clobj.close_delay = _closeDelay
                clients.Add(clobj)
                serverData.Add(clnom)
                AddHandler clobj.DataReceived, AddressOf DataReceivedHandler 'Handle all of the data received in all clients
                AddHandler clobj.errEncounter, AddressOf errEncounterHandler 'Handle all clients errors
                AddHandler clobj.lostConnection, AddressOf lostConnectionHandler 'Handle all clients lost connections
                RaiseEvent ClientConnect(clnom)
            Catch ex As Exception
                RaiseEvent errEncounter(ex)
            End Try
            Thread.Sleep(150)
        Loop Until listening = False
    End Sub

    Private Sub clientmsgpr(client As String, message As packet)
        SyncLock lockListen
            synclockcheckl = True
            Dim clientnolst As New List(Of String)
            clientnolst.Add(client)
            If message.header.ToLower.StartsWith("system") Then
                If message.stringdata(password).ToLower = "clients" Then
                    Send(client, New packet(0, "0", clientnolst, "system:clients", New encapsulation(serverData), password, encryptmethod))
                ElseIf message.stringdata(password).ToLower.StartsWith("client:") Then
                    Dim colonindx As Integer = message.stringdata(password).ToLower.IndexOf(":")
                    Dim newname As String = message.stringdata(password).Substring(colonindx + 1)
                    Dim arex As Boolean = False
                    For i As Integer = 0 To serverData.Count - 1
                        If serverData(i) = newname Then
                            arex = True
                            Exit For
                        End If
                    Next
                    If Not (arex) Then
                        Dim arex2 As Boolean = False
                        For i As Integer = 0 To clients.Count - 1
                            If clients(i).name = client Then
                                clients(i).name = newname
                                arex2 = True
                                Exit For
                            End If
                        Next
                        If arex2 Then
                            serverData.Remove(client)
                            serverData.Add(newname)
                        End If
                    End If
                ElseIf message.stringdata(password).ToLower.StartsWith("client") Then
                    Send(client, New packet(0, "0", clientnolst, "system:name", client, password, encryptmethod))
                ElseIf message.stringdata(password).ToLower.StartsWith("disconnect") Then
                    Disconnect(client)
                ElseIf message.stringdata(password).ToLower.StartsWith("stop") Then
                    [Stop]()
                End If
            Else
                RaiseEvent ClientMessage(client, message)
            End If
            synclockcheckl = False
        End SyncLock
    End Sub

    Private Sub DecryptBytes(ByVal cname As String, ByVal Message As intmsg)
        Dim Disconnecttf As Boolean = True
        For b = 0 To Message.Data.Length - 1
            Dim Msg As intmsg = intmsg.FromBytes(Message.Data, b)
            If Not Msg.Data Is Nothing Then
                'raise the data recived event
                Dim packet As packet = string2packet(ConvertFromAscii(Msg.Data))
                clientmsgpr(cname, packet)
                Disconnecttf = False
            End If
            If b >= Message.Data.Length - 1 Then Exit For
        Next
        If Disconnecttf Then
            Disconnect(cname)
        End If
    End Sub

    Private Sub DataReceivedHandler(ByVal cname As String, ByVal Msg As intmsg)
        DecryptBytes(cname, Msg)
    End Sub

    Private Sub errEncounterHandler(ByVal ex As Exception)
        RaiseEvent errEncounter(ex)
    End Sub

    Private Sub lostConnectionHandler(ByVal name As String)
        RaiseEvent ClientDisconnect(Name)
        Dim Handler As clientobj = GetClient(name)
        RemoveHandler Handler.DataReceived, AddressOf DataReceivedHandler
        RemoveHandler Handler.errEncounter, AddressOf errEncounterHandler
        clients.Remove(Handler)
        serverData.Remove(name)
        RemoveHandler Handler.lostConnection, AddressOf lostConnectionHandler
    End Sub
End Class
