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
Public Class Server
    Implements IDisposable

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

    Private _packet_delay As Integer = 50

    Private _no_packet_splitting As Boolean = False

    Private _disconnect_on_invalid_packet As Boolean = False

    Private _buffer_size As Integer = 8192

    Private _auto_msg_pass As Boolean = True

    Private _increment_client_names As Boolean = True

    Private _client_num_limit As Integer = 0

    ''' <summary>
    ''' Raised everytime a packet is received.
    ''' </summary>
    ''' <param name="Data">Packet received.</param>
    ''' <param name="clientname">The name of sender.</param>
    ''' <remarks></remarks>
    Public Event ClientMessage(ByVal clientname As String, ByVal data As Packet)
    ''' <summary>
    ''' Raised when an error occurs.
    ''' </summary>
    ''' <param name="ex">The exception occured.</param>
    ''' <remarks></remarks>
    Public Event ErrorOccured(ByVal ex As Exception)
    ''' <summary>
    ''' Raised everytime a client connects successfully.
    ''' </summary>
    ''' <param name="clientname">The client connected name.</param>
    ''' <remarks></remarks>
    Public Event ClientConnectSuccess(ByVal clientname As String)
    ''' <summary>
    ''' Raised everytime a client does not connect successfully.
    ''' </summary>
    ''' <param name="clientname">The client connected name.</param>
    ''' <param name="reason">The reason that the connection failed.</param>
    ''' <remarks></remarks>
    Public Event ClientConnectFailed(ByVal clientname As String, ByVal reason As FailedConnectionReason)
    ''' <summary>
    ''' Raised everytime a client disconnects.
    ''' </summary>
    ''' <param name="clientname">The disconnected client name.</param>
    ''' <remarks></remarks>
    Public Event ClientDisconnect(ByVal clientname As String)
    ''' <summary>
    ''' Raised when the Server Stops.
    ''' </summary>
    ''' <remarks></remarks>
    Public Event ServerStopped()
    ''' <summary>
    ''' The count of currently stored packet fragments.
    ''' </summary>
    ''' <value>Number of stored packet fragments.</value>
    ''' <returns>Integer</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property PacketFragmentCount As Integer
        Get
            Dim toret As Integer = 0
            For Each clfpfc As clientobj In clients
                toret += clfpfc.pfc
            Next
            Return toret
        End Get
    End Property
    ''' <summary>
    ''' Creates a new instance of server with the specified ServerConstructor.
    ''' </summary>
    ''' <param name="constructor">The ServerConstructor to use.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal constructor As ServerConstructor)
        If constructor.IpAddress IsNot Nothing Then
            _ip = constructor.IpAddress
        Else
            _ip = Me.Ip
        End If
        _port = validate_port(constructor.Port)
        tcpListener = New TcpListener(_ip, _port)
    End Sub
    ''' <summary>
    ''' Is the client allowed to send and process internal messages, set it when starting the server in the ServerStart structure.
    ''' If this is disabled, you will not be able to set the client name while connected.
    ''' If this is disabled, you will not be able to get a list of clients connected to the server via the connected_clients property.
    ''' </summary>
    ''' <value>Internal Message Passing</value>
    ''' <returns>True/False</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property InternalMessagePassing As Boolean
        Get
            Return _auto_msg_pass
        End Get
    End Property
    ''' <summary>
    ''' Cleans this instance of server.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Clean()
        If Not listening And Not synclockcheckl And Not synclockchecks Then
            _ip = IPAddress.None

            _port = 100

            tcpListener = Nothing

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

            _packet_delay = 50

            _disconnect_on_invalid_packet = False

            _no_packet_splitting = False

            _buffer_size = 8192

            _auto_msg_pass = True

            _increment_client_names = True
        End If
    End Sub
    ''' <summary>
    ''' Returns if client names are incremented if a client has already taken a name.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property IncrementClientNames() As Boolean
        Get
            Return _increment_client_names
        End Get
    End Property
    ''' <summary>
    ''' Split the Packets when they are sent
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SplitPacketsOnSend() As Boolean
        Get
            Return Not _no_packet_splitting
        End Get
        Set(value As Boolean)
            _no_packet_splitting = Not value
        End Set
    End Property
    ''' <summary>
    ''' Disconnect if the packet could not be formatted correctly
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property DisconnectOnInvalidPacket() As Boolean
        Get
            Return _disconnect_on_invalid_packet
        End Get
        Set(value As Boolean)
            _disconnect_on_invalid_packet = value
        End Set
    End Property
    ''' <summary>
    ''' Get the buffer size of the client
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property BufferSize() As Integer
        Get
            Return _buffer_size
        End Get
    End Property
    ''' <summary>
    ''' Gets or Sets the delay between packet parts sent.
    ''' </summary>
    ''' <value>the delay between packet parts sent.</value>
    ''' <returns>the delay between packet parts sent.</returns>
    ''' <remarks></remarks>
    Public Property MessageSendPacketDelay() As Integer
        Get
            Return _packet_delay
        End Get
        Set(value As Integer)
            If Not value = Timeout.Infinite Then
                _packet_delay = value
            End If
        End Set
    End Property
    ''' <summary>
    ''' Get the IP address of the server.
    ''' </summary>
    ''' <value>IP address of the server.</value>
    ''' <returns>IP address of the server.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Ip() As IPAddress
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
                    _ip = IPAddress.Loopback
                    Return IPAddress.Loopback
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
    ''' Determines if the server should wait an amount of time so more data will be added to the send data buffer, if set to true, the server will send the data immediatly.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property NoDelay() As Boolean
        Get
            Return tcpListener.Server.NoDelay
        End Get
    End Property
    ''' <summary>
    ''' Gets the currently connected clients.
    ''' </summary>
    ''' <value>the currently connected clients.</value>
    ''' <returns>the currently connected clients.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property ConnectedClients As List(Of String)
        Get
            If serverData IsNot Nothing Then
                Return serverData
            Else
                Return New List(Of String)
            End If
        End Get
    End Property
    ''' <summary>
    ''' Returns if the server is listening.
    ''' </summary>
    ''' <value>if the server is listening.</value>
    ''' <returns>if the server is listening.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property IsListening As Boolean
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
    '''<param name="starter">The ServerStart Info</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Start(ByVal starter As ServerStart) As Boolean
        Dim result As Boolean = False
        If listening Then
            Return True
        End If
        Try
            listening = True
            If Not IsNothing(starter.encrypt_param) Then
                encryptmethod = starter.encrypt_param.encrypt_method
                password = starter.encrypt_param.password
            Else
                encryptmethod = EncryptionMethod.none
                password = ""
            End If
            If starter.buffer_size >= 4096 Then
                _buffer_size = starter.buffer_size
            Else
                _buffer_size = 4096
            End If
            If starter.client_limit_count < 0 Then
                _client_num_limit = 0
            Else
                _client_num_limit = starter.client_limit_count
            End If
            _auto_msg_pass = starter.internal_message_passing
            _increment_client_names = starter.allow_clients_with_the_same_name_to_connect
            tcpListener.Server.NoDelay = starter.no_delay
            tcpListener.Server.ReceiveBufferSize = _buffer_size
            tcpListener.Server.SendBufferSize = _buffer_size
            listenthread = New Thread(New ThreadStart(AddressOf Listen))
            listenthread.IsBackground = True
            listenthread.Start()
            result = True
        Catch ex As ThreadAbortException
            Throw ex
        Catch ex As Exception
            result = False
            RaiseEvent ErrorOccured(ex)
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
        RaiseEvent ServerStopped()
    End Sub
    ''' <summary>
    ''' Kill the operating threads if they are still alive.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub KillThreads()
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
    ''' Sends a message to all of the connected clients.
    ''' </summary>
    ''' <param name="message">The packet to send.</param>
    ''' <remarks></remarks>
    Public Function Broadcast(ByVal message As Packet) As Boolean
        Dim result As Boolean = False
        If Not message.Header.ToLower.StartsWith("system") Then
            result = broadcast_int(message)
        Else
            If _auto_msg_pass Then
                result = False
            Else
                result = broadcast_int(message)
            End If
        End If
        Return result
    End Function

    Private Function broadcast_int(packet As Packet) As Boolean
        Dim result As Boolean = False
        SyncLock lockSend
            synclockchecks = True
            Try
                Dim frame As New packet_frame(packet)
                Dim f_p As packet_frame_part() = frame.ToParts(tcpListener.Server.SendBufferSize, _no_packet_splitting)
                Dim blst As List(Of Byte()) = createsendablebytes(f_p)
                For Each cbm As Byte() In blst
                    For Each c As clientobj In clients
                        c.SendData(cbm)
                    Next
                    Thread.Sleep(_packet_delay)
                Next
                result = True
            Catch ex As ThreadAbortException
                Throw ex
            Catch ex As Exception
                result = False
                RaiseEvent ErrorOccured(ex)
                RaiseEvent errEncounter(ex)
            End Try
            synclockchecks = False
        End SyncLock
        Return result
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
            Dim Handler As clientobj = GetClient(clientName)
            clients.Remove(Handler)
            serverData.Remove(clientName)
            result = True
        Catch ex As ThreadAbortException
            Throw ex
        Catch ex As Exception
            result = False
            RaiseEvent ErrorOccured(ex)
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
    Public Function Send(ByVal clientName As String, ByVal message As Packet) As Boolean
        Dim result As Boolean = False
        If Not message.Header.ToLower.StartsWith("system") Then
            result = send_int(clientName, message)
        Else
            If _auto_msg_pass Then
                result = False
            Else
                result = send_int(clientName, message)
            End If
        End If
        Return result
    End Function

    Private Function send_int(clientname As String, message As Packet) As Boolean
        Dim result As Boolean = False
        SyncLock lockSend
            synclockchecks = True
            Try
                Dim client As clientobj = GetClient(clientname)
                If client Is Nothing Then
                    result = False
                ElseIf Not client.isConnected Then
                    result = False
                Else
                    Dim frame As New packet_frame(message)
                    Dim f_p As packet_frame_part() = frame.ToParts(tcpListener.Server.SendBufferSize, _no_packet_splitting)
                    Dim blst As List(Of Byte()) = createsendablebytes(f_p)
                    For Each cbm As Byte() In blst
                        client.SendData(cbm)
                        Thread.Sleep(_packet_delay)
                    Next
                    result = True
                End If
            Catch ex As ThreadAbortException
                Throw ex
            Catch ex As Exception
                result = False
                RaiseEvent ErrorOccured(ex)
                RaiseEvent errEncounter(ex)
            End Try
            synclockchecks = False
        End SyncLock
        Return result
    End Function

    Private Sub Listen()
        Try
            tcpListener.Start()
            Do
                Try
                    tcpServer = tcpListener.AcceptTcpClient()
                    tcpServerNetStream = tcpServer.GetStream()

                    Dim packet As Packet = Nothing
                    Dim cdatarr(-1) As Byte
                    Dim cnumarr As New List(Of Byte)
                    Dim more_dat As Boolean = False
                    Dim length_left As Integer = 0
                    Dim in_packet As Boolean = False
                    Dim in_number As Boolean = False
                    Dim c_byte As Byte = 0
                    Dim c_index As Integer = 0

                    Dim bts(-1) As Byte

                    Do While tcpServer.Connected
                        Try
                            Dim Bytes(tcpServer.ReceiveBufferSize - 1) As Byte
                            tcpServerNetStream.Read(Bytes, 0, tcpServer.ReceiveBufferSize)
                            c_index = 0
                            If more_dat Then
                                more_dat = False
                                If c_index + length_left - 1 > Bytes.Length - 1 Then
                                    Dim rr(length_left - 1)
                                    Buffer.BlockCopy(Bytes, c_index, rr, 0, Bytes.Length - c_index)
                                    Buffer.BlockCopy(rr, 0, cdatarr, cdatarr.Length - length_left, rr.Length)
                                    length_left -= Bytes.Length - c_index
                                    more_dat = True
                                    c_index += rr.Length
                                Else
                                    Dim rr(length_left - 1) As Byte
                                    Buffer.BlockCopy(Bytes, c_index, rr, 0, length_left)
                                    Buffer.BlockCopy(rr, 0, cdatarr, cdatarr.Length - length_left, rr.Length)
                                    Dim p(0) As packet_frame_part
                                    p(0) = cdatarr
                                    Dim packetf As New packet_frame(p)
                                    packet = packetf.data
                                    in_packet = False
                                    c_index += length_left
                                    ReDim bts(Bytes.Length - c_index - 1)
                                    Buffer.BlockCopy(Bytes, c_index, bts, 0, Bytes.Length - c_index)
                                    Exit Do
                                End If
                            End If
                            While c_index < Bytes.Length
                                c_byte = Bytes(c_index)
                                If c_byte = 1 And Not in_packet Then
                                    in_packet = True
                                    in_number = True
                                    cnumarr.Clear()
                                ElseIf c_byte = 1 And in_packet Then
                                    in_packet = False
                                ElseIf in_number And Not c_byte = 2 Then
                                    cnumarr.Add(c_byte)
                                ElseIf in_number And c_byte = 2 Then
                                    length_left = Utils.ConvertFromAscii(cnumarr.ToArray)
                                    in_number = False
                                    If c_index + length_left - 1 > Bytes.Length - 1 Then
                                        Dim rr(length_left - 1) As Byte
                                        Buffer.BlockCopy(Bytes, c_index, rr, 0, Bytes.Length - c_index)
                                        cdatarr = rr
                                        more_dat = True
                                        length_left -= Bytes.Length - c_index
                                    Else
                                        Dim rr(length_left - 1) As Byte
                                        Buffer.BlockCopy(Bytes, c_index, rr, 0, length_left)
                                        Dim p(0) As packet_frame_part
                                        p(0) = rr
                                        Dim packetf As New packet_frame(p)
                                        packet = packetf.data
                                        in_packet = False
                                        c_index += length_left - 1 'take away one as the while loop increments it by one anyway
                                        ReDim bts(Bytes.Length - (c_index + 1) - 1)
                                        Buffer.BlockCopy(Bytes, c_index + 1, bts, 0, Bytes.Length - (c_index + 1))
                                        Exit Do
                                    End If
                                ElseIf c_byte = 0 And Not in_packet And c_index = 0 Then
                                    Throw New Exception("Disconnected")
                                End If
                                c_index += 1
                            End While
                            c_byte = 0
                        Catch ex As ThreadAbortException
                            Throw ex
                        Catch ex As Exception
                            Exit Do
                        End Try
                        Thread.Sleep(150)
                    Loop
                    Dim clnom As String = packet.StringData(password)
                    Dim allow_cl As Boolean = False
                    Dim allow_cl_r As FailedConnectionReason = FailedConnectionReason.Unknown
                    Dim clclr As Boolean = clients.Count + 1 > _client_num_limit
                    If _client_num_limit = 0 Then
                        clclr = False
                    End If

                    If Not GetClient(clnom) Is Nothing And _increment_client_names And Not clclr Then
                        Dim OriginID As String = clnom
                        Dim cnt As Integer = 1
                        clnom = OriginID & cnt
                        While Not GetClient(clnom) Is Nothing
                            cnt += 1
                            clnom = OriginID & cnt
                        End While
                        allow_cl = True
                    ElseIf Not GetClient(clnom) Is Nothing And Not _increment_client_names And Not clclr Then
                        allow_cl = False
                        allow_cl_r = FailedConnectionReason.NameTaken
                    ElseIf clclr Then
                        allow_cl = False
                        allow_cl_r = FailedConnectionReason.TooManyClients
                    Else
                        allow_cl = True
                    End If

                    Dim clobj As clientobj = Nothing

                    If allow_cl Then
                        clobj = New clientobj(clnom, tcpServer, _disconnect_on_invalid_packet, bts)
                        clobj.close_delay = _closeDelay
                    End If

                    Dim r As New List(Of String)
                    r.Add(clnom)
                    Dim p2 As packet_frame = Nothing

                    If allow_cl Then
                        p2 = New packet_frame(New Packet(0, "", r, "", "", New EncryptionParameter(encryptmethod, password)))
                    Else
                        p2 = New packet_frame(New Packet(0, "", r, "", allow_cl_r, New EncryptionParameter(encryptmethod, password)))
                    End If

                    Dim pfp As packet_frame_part() = p2.ToParts(_buffer_size, True) 'send with one part only as the name reciever only supports 1 part currently

                    Dim bytes2() As Byte = pfp(0)
                    Dim b_l2 As Integer = bytes2.Length
                    Dim b_l_b2 As Byte() = Utils.Convert2Ascii(b_l2)
                    Dim data_byt(0) As Byte
                    data_byt(0) = 1
                    data_byt = JoinBytes(data_byt, b_l_b2)
                    Dim bts2 As Byte() = JoinBytes(data_byt, bytes2)
                    tcpServerNetStream.Write(bts2, 0, bts2.Length)

                    If allow_cl Then
                        clients.Add(clobj)
                        serverData.Add(clnom)
                        AddHandler clobj.DataReceived, AddressOf DataReceivedHandler 'Handle all of the data received in all clients
                        AddHandler clobj.errEncounter, AddressOf errEncounterHandler 'Handle all clients errors
                        AddHandler clobj.lostConnection, AddressOf lostConnectionHandler 'Handle all clients lost connections
                        RaiseEvent ClientConnectSuccess(clnom)
                    Else
                        RaiseEvent ClientConnectFailed(clnom, allow_cl_r)
                    End If
                Catch ex As ThreadAbortException
                    Throw ex
                Catch ex As Exception
                    RaiseEvent ErrorOccured(ex)
                    RaiseEvent errEncounter(ex)
                End Try
                Thread.Sleep(150)
            Loop Until listening = False
        Catch ex As ThreadAbortException
            listening = False
            RaiseEvent ServerStopped()
            Throw ex
        Catch ex As Exception
            listening = False
            RaiseEvent ErrorOccured(ex)
            RaiseEvent errEncounter(ex)
            RaiseEvent ServerStopped()
        End Try
    End Sub

    Private Sub clientmsgpr(client As String, message As Packet)
        SyncLock lockListen
            synclockcheckl = True
            If _auto_msg_pass Then
                Dim clientnolst As New List(Of String)
                clientnolst.Add(client)
                If message.Header.ToLower.StartsWith("system") Then
                    If message.StringData(password).ToLower = "clients" Then
                        send_int(client, New Packet(0, "0", clientnolst, "system:clients", New Encapsulation(serverData), New EncryptionParameter(encryptmethod, password)))
                    ElseIf message.StringData(password).ToLower.StartsWith("client:") Then
                        Dim colonindx As Integer = message.StringData(password).ToLower.IndexOf(":")
                        Dim newname As String = message.StringData(password).Substring(colonindx + 1)
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
                    ElseIf message.StringData(password).ToLower.StartsWith("client") Then
                        send_int(client, New Packet(0, "0", clientnolst, "system:name", client, New EncryptionParameter(encryptmethod, password)))
                    End If
                Else
                    RaiseEvent ClientMessage(client, message)
                End If
            Else
                RaiseEvent ClientMessage(client, message)
            End If
            synclockcheckl = False
        End SyncLock
    End Sub

    ''' <summary>
    ''' Cleans accumalated packet_frames.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CleanPacketFrames()
        For Each cl As clientobj In clients
            Try
                cl.purge_msgs()
            Catch ex As ThreadAbortException
                Throw ex
            Catch ex As Exception
            End Try
        Next
    End Sub

    Private Sub DecryptBytes(ByVal cname As String, ByVal Message As packet_frame)
        Dim Disconnecttf As Boolean = True
        'For b = 0 To Message.Data.Length - 1
        Dim Msg As packet_frame = Message
        If Not Msg.data Is Nothing Then
            'raise the data recived event
            Dim packet As Packet = Msg.data
            clientmsgpr(cname, packet)
            Disconnecttf = False
        End If
        'Next
        If Disconnecttf Then
            Disconnect(cname)
        End If
    End Sub

    Private Sub DataReceivedHandler(ByVal cname As String, ByVal Msg As packet_frame)
        DecryptBytes(cname, Msg)
    End Sub

    Private Sub errEncounterHandler(ByVal ex As Exception)
        RaiseEvent ErrorOccured(ex)
        RaiseEvent errEncounter(ex)
    End Sub

    Private Sub lostConnectionHandler(ByVal name As String)
        RaiseEvent ClientDisconnect(name)
        Dim Handler As clientobj = GetClient(name)
        RemoveHandler Handler.DataReceived, AddressOf DataReceivedHandler
        RemoveHandler Handler.errEncounter, AddressOf errEncounterHandler
        clients.Remove(Handler)
        serverData.Remove(name)
        RemoveHandler Handler.lostConnection, AddressOf lostConnectionHandler
    End Sub
    ''' <summary>
    ''' Creates a new instance of server with the specified server_constructor.
    ''' </summary>
    ''' <param name="constructor">The server_constructor to use.</param>
    ''' <remarks></remarks>
    <Obsolete("Use New with ServerConstructor Instead.")>
    Public Sub New(ByVal constructor As server_constructor)
        If constructor.ip_address IsNot Nothing Then
            _ip = constructor.ip_address
        Else
            _ip = Me.Ip
        End If
        _port = validate_port(constructor.port)
        tcpListener = New TcpListener(_ip, _port)
    End Sub
    ''' <summary>
    ''' Raised when an error occurs.
    ''' </summary>
    ''' <param name="ex">The exception occured.</param>
    ''' <remarks></remarks>
    <Obsolete("Use ErrorOcurred")>
    Public Event errEncounter(ByVal ex As Exception)
    ''' <summary>
    ''' Flushes this instance of server (Cleaning).
    ''' </summary>
    ''' <remarks></remarks>
    <Obsolete("Use Clean Instead.")>
    Public Sub Flush()
        If Not listening And Not synclockcheckl And Not synclockchecks Then
            _ip = IPAddress.None

            _port = 100

            tcpListener = Nothing

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

            _packet_delay = 50

            _disconnect_on_invalid_packet = False

            _no_packet_splitting = False

            _buffer_size = 8192

            _auto_msg_pass = True

            _increment_client_names = True
        End If
    End Sub
    ''' <summary>
    ''' Cleans accumalated packet_frames (Cleaning).
    ''' </summary>
    ''' <remarks></remarks>
    <Obsolete("Use CleanPacketFrames Instead.")>
    Public Sub FlushPacketFrames()
        For Each cl As clientobj In clients
            Try
                cl.purge_msgs()
            Catch ex As ThreadAbortException
                Throw ex
            Catch ex As Exception
            End Try
        Next
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' dispose managed state (managed objects).
                [Stop]()
                If Not tcpListener Is Nothing Then
                    Try
                        tcpListener.Stop()
                    Catch ex As System.Net.Sockets.SocketException
                    End Try
                End If
                If Not tcpServer Is Nothing Then
                    Try
                        tcpServer.Close()
                    Catch ex As System.Net.Sockets.SocketException
                    End Try
                End If
                If Not tcpServerNetStream Is Nothing Then
                    Try
                        tcpServerNetStream.Dispose()
                    Catch ex As System.Net.Sockets.SocketException
                    End Try
                End If
                For Each cltd As clientobj In clients
                    cltd.Dispose()
                Next
            End If

            ' free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' set large fields to null.
            tcpListener = Nothing
            tcpServer = Nothing
            tcpServerNetStream = Nothing
            clients = Nothing
            serverData = Nothing
        End If
        Me.disposedValue = True
    End Sub

    ' override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class
''' <summary>
''' Provides parameters for server construction.
''' </summary>
''' <remarks></remarks>
Public Structure ServerConstructor
    ''' <summary>
    ''' The IP Address for the server to bind to.
    ''' </summary>
    ''' <remarks></remarks>
    Public IpAddress As IPAddress
    ''' <summary>
    ''' The port for the server to bind to.
    ''' </summary>
    ''' <remarks></remarks>
    Public Port As Integer
    ''' <summary>
    ''' Creates a new server_constructor to be used in making a new server object.
    ''' </summary>
    ''' <param name="_ipaddress">The IP Address for the server to bind to.</param>
    ''' <param name="_port">The port for the server to bind to.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal _ipaddress As IPAddress, Optional ByVal _port As Integer = 100)
        IpAddress = _ipaddress
        Port = _port
    End Sub
End Structure
''' <summary>
''' The ServerStart structure for starting a server.
''' </summary>
''' <remarks></remarks>
Public Structure ServerStart
    ''' <summary>
    ''' The Encryption Parameter to use in the client.
    ''' </summary>
    ''' <remarks></remarks>
    Public encrypt_param As EncryptionParameter
    ''' <summary>
    ''' If internal message passing is enabled (allows for clients to have a list of clients and change its name while it is connected).
    ''' </summary>
    ''' <remarks></remarks>
    Public internal_message_passing As Boolean
    ''' <summary>
    ''' The buffer size that will be used for the Tcp send and recieve buffers (Minumum Size:4096).
    ''' </summary>
    ''' <remarks></remarks>
    Public buffer_size As Integer
    ''' <summary>
    ''' If there is a delay before sending accumalated packets.
    ''' </summary>
    ''' <remarks></remarks>
    Public no_delay As Boolean
    ''' <summary>
    ''' Allow clients with the same name to connect and increment their names if a client with that name already exists.
    ''' </summary>
    ''' <remarks></remarks>
    Public allow_clients_with_the_same_name_to_connect As Boolean
    ''' <summary>
    ''' The amount of clients the server is limited to, this can be set to 0 for any number.
    ''' </summary>
    ''' <remarks></remarks>
    Public client_limit_count As Integer
    ''' <summary>
    ''' Creates a new set of server start info to start the server with.
    ''' </summary>
    ''' <param name="encryptparam">The Encryption Parameter to use in the client.</param>
    ''' <param name="buffersize">The buffer size that will be used for the Tcp send and recieve buffers (Minumum Size:4096).</param>
    ''' <param name="internalmsgpass">If internal message passing is enabled (allows for clients to have a list of clients and change its name while it is connected).</param>
    ''' <param name="_no_delay">If there is a delay before sending accumalated packets.</param>
    ''' <param name="acwtsntc">Allow clients with the same name to connect and increment their names if a client with that name already exists.</param>
    ''' <param name="clnumlmt">The amount of clients the server is limited to, this can be set to 0 for any number.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal encryptparam As EncryptionParameter, Optional ByVal buffersize As Integer = 8192, Optional ByVal internalmsgpass As Boolean = True, Optional ByVal _no_delay As Boolean = False, Optional ByVal acwtsntc As Boolean = True, Optional ByVal clnumlmt As Integer = 0)
        If Not IsNothing(encryptparam) Then
            encrypt_param = encryptparam
        Else
            encrypt_param = New EncryptionParameter(EncryptionMethod.none)
        End If
        If buffersize >= 4096 Then
            buffer_size = buffersize
        Else
            buffer_size = 4096
        End If
        If clnumlmt < 0 Then
            client_limit_count = 0
        Else
            client_limit_count = clnumlmt
        End If
        internal_message_passing = internalmsgpass
        no_delay = _no_delay
        allow_clients_with_the_same_name_to_connect = acwtsntc
    End Sub
End Structure

''' <summary>
''' Provides parameters for server construction.
''' </summary>
''' <remarks></remarks>
<Obsolete("Use ServerConstructor.")>
Public Structure server_constructor
    ''' <summary>
    ''' The IP Address for the server to bind to.
    ''' </summary>
    ''' <remarks></remarks>
    Public ip_address As IPAddress
    ''' <summary>
    ''' The port for the server to bind to.
    ''' </summary>
    ''' <remarks></remarks>
    Public port As Integer
    ''' <summary>
    ''' Creates a new server_constructor to be used in making a new server object.
    ''' </summary>
    ''' <param name="ipaddress">The IP Address for the server to bind to.</param>
    ''' <param name="_port">The port for the server to bind to.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal ipaddress As IPAddress, Optional ByVal _port As Integer = 100)
        ip_address = ipaddress
        port = _port
    End Sub
End Structure
