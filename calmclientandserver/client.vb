Imports System.Net
Imports System.Net.Sockets
Imports System.Net.NetworkInformation
Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Text
Imports System.Threading
''' <summary>
''' Provide a client class.
''' </summary>
''' <remarks></remarks>
Public Class client
    Private tcpClient As TcpClient = Nothing

    Private tcpClientNetStream As NetworkStream = Nothing

    Private connected As Boolean = False

    Private _closeDelay As Integer = 100

    Private lockListen As Object = New Object()

    Private lockSend As Object = New Object()

    Private clientData As New List(Of String)

    Private packets As New List(Of packet)

    Private thisClient As String = ""

    Private encryptmethod As EncryptionMethod = EncryptionMethod.none

    Private _clientrefreshdelay As Integer = 15000

    Private password As String = ""

    Private listenthread As Thread = Nothing

    Private updatethread As Thread = Nothing

    Private synclockcheckl As Boolean = False

    Private synclockchecks As Boolean = False

    Private _ip As String = ""

    Private _port As Integer = 0

    Private _packet_frame_part_dict As New Dictionary(Of Integer, packet_frame_part())

    Private _packet_delay As Integer = 50

    Private _no_packet_splitting As Boolean = False

    Private _disconnect_on_invalid_packet As Boolean = False

    Private _buffer_size As Integer = 8192

    Private _auto_msg_pass As Boolean = True
    ''' <summary>
    ''' Raised when a connection is successful.
    ''' </summary>
    ''' <remarks></remarks>
    Public Event ServerConnectSuccess()
    ''' <summary>
    ''' Raised when a connection fails
    ''' </summary>
    ''' <param name="reason">The reason the connection failed</param>
    ''' <remarks></remarks>
    Public Event ServerConnectFailed(ByVal reason As failed_connection_reason)
    ''' <summary>
    ''' Raised when disconnected by the server.
    ''' </summary>
    ''' <remarks></remarks>
    Public Event ServerDisconnect()
    ''' <summary>
    ''' Raised a message is received.
    ''' </summary>
    ''' <param name="message">The packet received.</param>
    ''' <remarks></remarks>
    Public Event ServerMessage(ByVal message As packet)
    ''' <summary>
    ''' Raised everytime an error occurs.
    ''' </summary>
    ''' <param name="ex">The exception that occured.</param>
    ''' <remarks></remarks>
    Public Event errEncounter(ByVal ex As Exception)

    ''' <summary>
    ''' Flushes this instance of client (Cleaning).
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Flush()
        If Not connected And Not synclockcheckl And Not synclockchecks Then
            tcpClient = Nothing

            tcpClient = New TcpClient()
            tcpClient.SendBufferSize = 8192
            tcpClient.ReceiveBufferSize = 8192

            connected = False

            _closeDelay = 100

            lockListen = New Object()

            lockSend = New Object()

            clientData = New List(Of String)

            packets = New List(Of packet)

            thisClient = ""

            encryptmethod = EncryptionMethod.none

            _clientrefreshdelay = 15000

            password = ""

            listenthread = Nothing

            updatethread = Nothing

            synclockcheckl = False

            synclockchecks = False

            _ip = ""

            _port = 0

            _packet_delay = 50

            _packet_frame_part_dict = New Dictionary(Of Integer, packet_frame_part())

            _disconnect_on_invalid_packet = False

            _no_packet_splitting = False

            _buffer_size = 8192

            _auto_msg_pass = True
        End If
    End Sub

    ''' <summary>
    ''' Gets or Sets the name of the client.
    ''' </summary>
    ''' <value>the name of the client.</value>
    ''' <returns>the name of the client.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Name() As String
        Get
            Return thisClient
        End Get
    End Property
    ''' <summary>
    ''' Returns the current connection state (True if connected).
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property IsConnected() As Boolean
        Get
            Return connected
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
    ''' Gets or Sets the delay to refresh the local client list (If InternalMessagePassing is enabled).
    ''' </summary>
    ''' <value>the delay to refresh the local client list.</value>
    ''' <returns>the delay to refresh the local client list.</returns>
    ''' <remarks></remarks>
    Public Property ClientRefreshDelay() As Integer
        Get
            If _auto_msg_pass Then
                Return _clientrefreshdelay
            Else
                Throw New InvalidOperationException("ClientRefreshDelay can only be used when InternalMessagePassing is enabled")
            End If
        End Get
        Set(value As Integer)
            If _auto_msg_pass Then
                _clientrefreshdelay = value
            Else
                Throw New InvalidOperationException("ClientRefreshDelay can only be used when InternalMessagePassing is enabled")
            End If
        End Set
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
    ''' Creates a new instance of client with the specified client_constructor.
    ''' </summary>
    ''' <param name="constructor">The client_constructor to use.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal constructor As client_constructor)
        tcpClient = New TcpClient()
    End Sub
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
    ''' Determines if the server should wait an amount of time so more data will be added to the send data buffer, if set to true, the server will send the data immediatly.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property NoDelay() As Boolean
        Get
            Return tcpClient.NoDelay
        End Get
    End Property
    ''' <summary>
    ''' Is the client allowed to send and process internal messages, set it when starting the client in the ClientStart structure.
    ''' If this is disabled, you will not be able to set the client name while connected.
    ''' If this is disabled, you will not be able to get a list of clients connected to the server via the connectedclients property.
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
    ''' Connect to a server.
    ''' </summary>
    '''<param name="starter">The ClientStart Information</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Connect(ByVal starter As ClientStart) As Boolean
        Dim result As Boolean = False
        Try
            If connected Then
                result = True
            Else
                thisClient = starter.client_name
                If thisClient Is Nothing Or thisClient = "" Then
                    Throw New InvalidOperationException()
                End If
                If starter.buffer_size >= 4096 Then
                    _buffer_size = starter.buffer_size
                Else
                    _buffer_size = 4096
                End If
                tcpClient.ReceiveBufferSize = _buffer_size
                tcpClient.SendBufferSize = _buffer_size
                If Not IsNothing(starter.encrypt_param) Then
                    encryptmethod = starter.encrypt_param.encrypt_method
                    password = starter.encrypt_param.password
                Else
                    encryptmethod = EncryptionMethod.none
                    password = ""
                End If
                _auto_msg_pass = starter.internal_message_passing
                tcpClient.NoDelay = starter.no_delay
                _port = validate_port(starter.port)
                _ip = starter.ip_address.ToString
                updatethread = New Thread(New ThreadStart(AddressOf updatedata))
                updatethread.IsBackground = True
                listenthread = New Thread(New ThreadStart(AddressOf Listen))
                listenthread.IsBackground = True
                listenthread.Start()
                result = True
            End If
        Catch ex As ThreadAbortException
            Throw ex
        Catch ex As Exception
            result = False
            RaiseEvent errEncounter(ex)
        End Try
        Return result
    End Function

    Public Sub SetName(ByVal name As String)
        If connected And tcpcon() Then
            If _auto_msg_pass Then
                Dim arex As Boolean = False
                For i As Integer = 0 To clientData.Count - 1
                    If clientData(i) = name Then
                        arex = True
                        Exit For
                    End If
                Next
                If Not (arex) Then
                    clientData.Remove(thisClient)
                    clientData.Add(name)
                    Dim retyt As String = send_int(New packet(0, thisClient, New List(Of String), "system", "client:" & name, New EncryptionParameter(encryptmethod, password)))
                    If retyt.ToLower = True Then
                        thisClient = name
                    End If
                End If
            Else
                Throw New InvalidOperationException("SetName can only be used if InternalMessagePassing is enabled")
            End If
        End If
    End Sub

    Private Function tcpcon() As Boolean
        If Not tcpClient Is Nothing Then
            Try
                Dim c As Boolean = tcpClient.Connected
                Return c
            Catch ex As ThreadAbortException
                Throw ex
            Catch ex As Exception
                Return False
            End Try
        Else
            Return False
        End If
    End Function

    Private Sub Listen()
        Dim bts(-1) As Byte
        Try
            tcpClient.Connect(_ip, _port)
            If tcpClient.Connected Then
                'Send the client name
                tcpClientNetStream = tcpClient.GetStream()

                Dim p2 As New packet_frame(New packet(0, thisClient, New List(Of String), "", thisClient, New EncryptionParameter(encryptmethod, password)))
                Dim pfp As packet_frame_part() = p2.ToParts(_buffer_size, True) 'send with one part only as the name reciever only supports 1 part currently
                Dim bytes2() As Byte = pfp(0)
                Dim b_l2 As Integer = bytes2.Length
                Dim b_l_b2 As Byte() = utils.Convert2Ascii(b_l2)
                Dim data_byt(0) As Byte
                data_byt(0) = 1
                data_byt = JoinBytes(data_byt, b_l_b2)
                Dim bts2 As Byte() = JoinBytes(data_byt, bytes2)
                tcpClientNetStream.Write(bts2, 0, bts2.Length)

                Dim packet As packet = Nothing
                Dim cdatarr2(-1) As Byte
                Dim cnumarr2 As New List(Of Byte)
                Dim more_dat2 As Boolean = False
                Dim length_left2 As Integer = 0
                Dim in_packet2 As Boolean = False
                Dim in_number2 As Boolean = False
                Dim c_byte2 As Byte = 0
                Dim c_index2 As Integer = 0

                Do While tcpClient.Connected
                    Try
                        Dim Bytes(tcpClient.ReceiveBufferSize - 1) As Byte
                        tcpClientNetStream.Read(Bytes, 0, tcpClient.ReceiveBufferSize)
                        c_index2 = 0
                        If more_dat2 Then
                            more_dat2 = False
                            If c_index2 + length_left2 - 1 > Bytes.Length - 1 Then
                                Dim rr(length_left2 - 1)
                                Buffer.BlockCopy(Bytes, c_index2, rr, 0, Bytes.Length - c_index2)
                                Buffer.BlockCopy(rr, 0, cdatarr2, cdatarr2.Length - length_left2, rr.Length)
                                length_left2 -= Bytes.Length - c_index2
                                more_dat2 = True
                                c_index2 += rr.Length
                            Else
                                Dim rr(length_left2 - 1) As Byte
                                Buffer.BlockCopy(Bytes, c_index2, rr, 0, length_left2)
                                Buffer.BlockCopy(rr, 0, cdatarr2, cdatarr2.Length - length_left2, rr.Length)
                                Dim p(0) As packet_frame_part
                                p(0) = cdatarr2
                                Dim packetf As New packet_frame(p)
                                packet = packetf.data
                                in_packet2 = False
                                c_index2 += length_left2
                                ReDim bts(Bytes.Length - c_index2 - 1)
                                Buffer.BlockCopy(Bytes, c_index2, bts, 0, Bytes.Length - c_index2)
                                Exit Do
                            End If
                        End If
                        While c_index2 < Bytes.Length
                            c_byte2 = Bytes(c_index2)
                            If c_byte2 = 1 And Not in_packet2 Then
                                in_packet2 = True
                                in_number2 = True
                                cnumarr2.Clear()
                            ElseIf c_byte2 = 1 And in_packet2 Then
                                in_packet2 = False
                            ElseIf in_number2 And Not c_byte2 = 2 Then
                                cnumarr2.Add(c_byte2)
                            ElseIf in_number2 And c_byte2 = 2 Then
                                length_left2 = utils.ConvertFromAscii(cnumarr2.ToArray)
                                in_number2 = False
                                If c_index2 + length_left2 - 1 > Bytes.Length - 1 Then
                                    Dim rr(length_left2 - 1) As Byte
                                    Buffer.BlockCopy(Bytes, c_index2, rr, 0, Bytes.Length - c_index2)
                                    cdatarr2 = rr
                                    more_dat2 = True
                                    length_left2 -= Bytes.Length - c_index2
                                Else
                                    Dim rr(length_left2 - 1) As Byte
                                    Buffer.BlockCopy(Bytes, c_index2, rr, 0, length_left2)
                                    Dim p(0) As packet_frame_part
                                    p(0) = rr
                                    Dim packetf As New packet_frame(p)
                                    packet = packetf.data
                                    in_packet2 = False
                                    c_index2 += length_left2 - 1 'take away one as the while loop increments it by one anyway
                                    ReDim bts(Bytes.Length - (c_index2 + 1) - 1)
                                    Buffer.BlockCopy(Bytes, c_index2 + 1, bts, 0, Bytes.Length - (c_index2 + 1))
                                    Exit Do
                                End If
                            ElseIf c_byte2 = 0 And Not in_packet2 And c_index2 = 0 Then
                                Throw New Exception("Disconnected")
                            End If
                            c_index2 += 1
                        End While
                        c_byte2 = 0
                    Catch ex As ThreadAbortException
                        Throw ex
                    Catch ex As Exception
                        Exit Do
                    End Try
                    Thread.Sleep(150)
                Loop

                Dim sd As String = packet.stringdata(password)
                If sd = "" Then
                    thisClient = packet.receivers(0)

                    connected = True

                    If _auto_msg_pass Then
                        updatethread.Start()
                    End If

                    RaiseEvent ServerConnectSuccess()
                ElseIf sd = failed_connection_reason.name_taken Then
                    Dim SBufferSize, RBufferSize As Integer
                    SBufferSize = tcpClient.SendBufferSize
                    RBufferSize = tcpClient.ReceiveBufferSize
                    Dim NDelay As Boolean = tcpClient.NoDelay
                    If Not tcpClient Is Nothing Then tcpClient.Close()
                    tcpClient = New TcpClient()
                    tcpClient.SendBufferSize = SBufferSize
                    tcpClient.ReceiveBufferSize = RBufferSize
                    tcpClient.NoDelay = NDelay
                    RaiseEvent ServerConnectFailed(failed_connection_reason.name_taken)
                ElseIf sd = failed_connection_reason.too_many_clients Then
                    Dim SBufferSize, RBufferSize As Integer
                    SBufferSize = tcpClient.SendBufferSize
                    RBufferSize = tcpClient.ReceiveBufferSize
                    Dim NDelay As Boolean = tcpClient.NoDelay
                    If Not tcpClient Is Nothing Then tcpClient.Close()
                    tcpClient = New TcpClient()
                    tcpClient.SendBufferSize = SBufferSize
                    tcpClient.ReceiveBufferSize = RBufferSize
                    tcpClient.NoDelay = NDelay
                    RaiseEvent ServerConnectFailed(failed_connection_reason.too_many_clients)
                Else
                    Dim SBufferSize, RBufferSize As Integer
                    SBufferSize = tcpClient.SendBufferSize
                    RBufferSize = tcpClient.ReceiveBufferSize
                    Dim NDelay As Boolean = tcpClient.NoDelay
                    If Not tcpClient Is Nothing Then tcpClient.Close()
                    tcpClient = New TcpClient()
                    tcpClient.SendBufferSize = SBufferSize
                    tcpClient.ReceiveBufferSize = RBufferSize
                    tcpClient.NoDelay = NDelay
                    RaiseEvent ServerConnectFailed(failed_connection_reason.unknown)
                End If
            End If
        Catch ex As ThreadAbortException
            Throw ex
        Catch ex As Exception
            RaiseEvent errEncounter(ex)
        End Try
        Dim cdatarr(-1) As Byte
        Dim cnumarr As New List(Of Byte)
        Dim more_dat As Boolean = False
        Dim length_left As Integer = 0
        Dim in_packet As Boolean = False
        Dim in_number As Boolean = False
        Dim c_byte As Byte = 0
        Dim c_index As Integer = 0
        Try
            Dim had_ex As Boolean = False

            If connected And tcpcon() Then
                Try
                    c_index = 0
                    If more_dat Then
                        more_dat = False
                        If c_index + length_left - 1 > bts.Length - 1 Then
                            Dim rr(length_left - 1) As Byte
                            Buffer.BlockCopy(bts, c_index, rr, 0, bts.Length - c_index)
                            Buffer.BlockCopy(rr, 0, cdatarr, cdatarr.Length - length_left, rr.Length)
                            length_left -= bts.Length - c_index
                            more_dat = True
                            c_index += rr.Length
                        Else
                            Dim rr(length_left - 1) As Byte
                            Buffer.BlockCopy(bts, c_index, rr, 0, length_left)
                            Buffer.BlockCopy(rr, 0, cdatarr, cdatarr.Length - length_left, rr.Length)
                            Try
                                DecryptBytes(cdatarr)
                            Catch ex As ThreadAbortException
                                Throw ex
                            Catch ex As Exception
                            End Try
                            in_packet = False
                            c_index += length_left
                            length_left = 0
                        End If
                    End If
                    While c_index < bts.Length
                        c_byte = bts(c_index)
                        If c_byte = 1 And Not in_packet Then
                            in_packet = True
                            in_number = True
                            cnumarr.Clear()
                        ElseIf c_byte = 1 And in_packet Then
                            in_packet = False
                        ElseIf in_number And Not c_byte = 2 Then
                            cnumarr.Add(c_byte)
                        ElseIf in_number And c_byte = 2 Then
                            length_left = utils.ConvertFromAscii(cnumarr.ToArray)
                            in_number = False
                            If c_index + length_left - 1 > bts.Length - 1 Then
                                Dim rr(length_left - 1) As Byte
                                Buffer.BlockCopy(bts, c_index, rr, 0, bts.Length - c_index)
                                cdatarr = rr
                                more_dat = True
                                length_left -= bts.Length - c_index
                            Else
                                Dim rr(length_left - 1) As Byte
                                Buffer.BlockCopy(bts, c_index, rr, 0, length_left)
                                Try
                                    DecryptBytes(rr)
                                Catch ex As ThreadAbortException
                                    Throw ex
                                Catch ex As Exception
                                End Try
                                in_packet = False
                                c_index += length_left - 1 'take away one as the while loop increments it by one anyway
                                length_left = 0
                            End If
                        End If
                        c_index += 1
                    End While
                    c_byte = 0
                Catch ex As ThreadAbortException
                    Throw ex
                Catch ex As Exception
                    in_number = False
                    in_packet = False
                    c_byte = 0
                    c_index = 0
                    more_dat = False
                    length_left = 0
                    cnumarr.Clear()
                    If _disconnect_on_invalid_packet Then
                        had_ex = True
                    End If
                End Try
            End If

            If Not had_ex Then
                While connected And tcpcon()
                    Try
                        Dim bytes(tcpClient.ReceiveBufferSize - 1) As Byte
                        tcpClientNetStream.Read(bytes, 0, tcpClient.ReceiveBufferSize)
                        c_index = 0
                        If more_dat Then
                            more_dat = False
                            If c_index + length_left - 1 > bytes.Length - 1 Then
                                Dim rr(length_left - 1) As Byte
                                Buffer.BlockCopy(bytes, c_index, rr, 0, bytes.Length - c_index)
                                Buffer.BlockCopy(rr, 0, cdatarr, cdatarr.Length - length_left, rr.Length)
                                length_left -= bytes.Length - c_index
                                more_dat = True
                                c_index += rr.Length
                            Else
                                Dim rr(length_left - 1) As Byte
                                Buffer.BlockCopy(bytes, c_index, rr, 0, length_left)
                                Buffer.BlockCopy(rr, 0, cdatarr, cdatarr.Length - length_left, rr.Length)
                                Try
                                    DecryptBytes(cdatarr)
                                Catch ex As ThreadAbortException
                                    Throw ex
                                Catch ex As Exception
                                End Try
                                in_packet = False
                                c_index += length_left
                                length_left = 0
                            End If
                        End If
                        While c_index < bytes.Length
                            c_byte = bytes(c_index)
                            If c_byte = 1 And Not in_packet Then
                                in_packet = True
                                in_number = True
                                cnumarr.Clear()
                            ElseIf c_byte = 1 And in_packet Then
                                in_packet = False
                            ElseIf in_number And Not c_byte = 2 Then
                                cnumarr.Add(c_byte)
                            ElseIf in_number And c_byte = 2 Then
                                length_left = utils.ConvertFromAscii(cnumarr.ToArray)
                                in_number = False
                                If c_index + length_left - 1 > bytes.Length - 1 Then
                                    Dim rr(length_left - 1) As Byte
                                    Buffer.BlockCopy(bytes, c_index, rr, 0, bytes.Length - c_index)
                                    cdatarr = rr
                                    more_dat = True
                                    length_left -= bytes.Length - c_index
                                Else
                                    Dim rr(length_left - 1) As Byte
                                    Buffer.BlockCopy(bytes, c_index, rr, 0, length_left)
                                    Try
                                        DecryptBytes(rr)
                                    Catch ex As ThreadAbortException
                                        Throw ex
                                    Catch ex As Exception
                                    End Try
                                    in_packet = False
                                    c_index += length_left - 1 'take away one as the while loop increments it by one anyway
                                    length_left = 0
                                End If
                            ElseIf c_byte = 0 And Not in_packet And c_index = 0 Then
                                connected = False
                            End If
                            c_index += 1
                        End While
                        c_byte = 0
                    Catch ex As ThreadAbortException
                        Throw ex
                    Catch ex As Exception
                        in_number = False
                        in_packet = False
                        c_byte = 0
                        c_index = 0
                        more_dat = False
                        length_left = 0
                        cnumarr.Clear()
                        If _disconnect_on_invalid_packet Then
                            Exit While
                        End If
                    End Try
                    Thread.Sleep(150)
                End While
            End If
        Catch ex As ThreadAbortException
            Throw ex
        Catch ex As Exception
            RaiseEvent errEncounter(ex)
        End Try
        Disconnect()
    End Sub

    Private Sub DecryptBytes(ByVal Message() As Byte)
        Dim Disconnected As Boolean = True
        'For b = 0 To Message.Length - 1
        Dim Msg As packet_frame_part = Nothing
        Try
            Msg = Message
        Catch ex As ThreadAbortException
            Throw ex
        Catch ex As Exception
            Msg = Nothing
        End Try
        If Not Msg.data Is Nothing Then
            If Msg.partnum = Msg.totalparts And _packet_frame_part_dict.ContainsKey(Msg.refnum) Then
                Dim arr As packet_frame_part() = _packet_frame_part_dict(Msg.refnum) 'get the packet frame array parts
                arr(Msg.partnum - 1) = Msg 'make the last part in array the current part
                _packet_frame_part_dict(Msg.refnum) = arr 'set the dictionary value of the current part's ref number to the value of the array
                Dim pf As packet_frame = Nothing 'define pf as packet frame
                Dim sc As Boolean = False 'cast successful boolean
                Try
                    pf = arr
                    sc = True
                Catch ex As ThreadAbortException
                    Throw ex
                Catch ex As Exception
                    pf = Nothing
                    sc = False
                End Try
                If sc Then 'if cast successful
                    servermsgpr(pf.data) 'call servermsg processor for the byte array casted to packet
                End If
                _packet_frame_part_dict.Remove(Msg.refnum) 'remove the array from the dict
            ElseIf _packet_frame_part_dict.ContainsKey(Msg.refnum) Then
                Dim arr As packet_frame_part() = _packet_frame_part_dict(Msg.refnum) 'get the array
                arr(Msg.partnum - 1) = Msg 'set the partnum -1 to current msg
                _packet_frame_part_dict(Msg.refnum) = arr 'put the array back into the dict
            ElseIf Not _packet_frame_part_dict.ContainsKey(Msg.refnum) Then
                Dim arr(Msg.totalparts - 1) As packet_frame_part 'generate array
                arr(0) = Msg 'set the first index to the current msg
                If arr.Length = 1 Then 'the same as being complete
                    Dim pf As packet_frame = Nothing
                    Dim sc As Boolean = False
                    Try
                        pf = arr
                        sc = True
                    Catch ex As ThreadAbortException
                        Throw ex
                    Catch ex As Exception
                        pf = Nothing
                        sc = False
                    End Try
                    If sc Then
                        servermsgpr(pf.data)
                    End If
                Else
                    _packet_frame_part_dict.Add(Msg.refnum, arr) 'add the arr to the dict
                End If
            End If
            Disconnected = False
        End If
        If Disconnected Then connected = False
    End Sub

    Private Sub updatedata()
        While connected
            Try
                Thread.Sleep(_clientrefreshdelay)
                If tcpClient.Connected Then
                    If _auto_msg_pass Then
                        send_int(New packet(0, thisClient, New List(Of String), "system", "clients", New EncryptionParameter(encryptmethod, password)))
                        send_int(New packet(0, thisClient, New List(Of String), "system", "client", New EncryptionParameter(encryptmethod, password)))
                    End If
                End If
            Catch ex As ThreadAbortException
                Thread.CurrentThread.Abort()
                Exit While
            Catch ex As Exception
            End Try
        End While
        Thread.CurrentThread.Abort()
    End Sub
    ''' <summary>
    ''' Forces the client list to update.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub UpdateClientData()
        If connected Then
            If tcpClient.Connected Then
                If _auto_msg_pass Then
                    send_int(New packet(0, thisClient, New List(Of String), "system", "clients", New EncryptionParameter(encryptmethod, password)))
                    send_int(New packet(0, thisClient, New List(Of String), "system", "client", New EncryptionParameter(encryptmethod, password)))
                Else
                    Throw New InvalidOperationException("UpdateClientData can only be used if InternalMessagePassing is enabled")
                End If
            End If
        End If
    End Sub
    ''' <summary>
    ''' Gets the currently connected clients on the server.
    ''' Throws an InvalidOperationException if InternalMessagePasing is not enabled.
    ''' </summary>
    ''' <value>the currently connected clients on the server.</value>
    ''' <returns>the currently connected clients on the server.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property ConnectedClients As List(Of String)
        Get
            If _auto_msg_pass Then
                If clientData IsNot Nothing Then
                    Return clientData
                Else
                    Return New List(Of String)
                End If
            Else
                Throw New InvalidOperationException("ConnectedClients can only be used with InternalMessagePassing Enabled")
            End If
        End Get
    End Property
    ''' <summary>
    ''' Check if a server is up.
    ''' </summary>
    ''' <param name="ipadress">The server IP address.</param>
    ''' <param name="port">The server port.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CheckServer(ipadress As String, port As Integer) As Boolean
        Try
            Dim tcpClientc As TcpClient = New TcpClient(ipadress, validate_port(port))
            If tcpClientc.Connected Then
                Dim optionValue As LingerOption = New LingerOption(False, 0)
                tcpClientc.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, optionValue)
                tcpClientc.Client.Close(0)
                Return True
            End If
        Catch ex_75 As Exception
        End Try
        Return False
    End Function
    ''' <summary>
    ''' Disconnect from the server.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Disconnect()
        On Error Resume Next
        Thread.Sleep(_closeDelay)
        connected = False
        Dim SBufferSize, RBufferSize As Integer
        SBufferSize = tcpClient.SendBufferSize
        RBufferSize = tcpClient.ReceiveBufferSize
        Dim NDelay As Boolean = tcpClient.NoDelay
        If Not tcpClient Is Nothing Then tcpClient.Close()
        tcpClient = New TcpClient()
        tcpClient.SendBufferSize = SBufferSize
        tcpClient.ReceiveBufferSize = RBufferSize
        tcpClient.NoDelay = NDelay
        RaiseEvent ServerDisconnect()
    End Sub
    ''' <summary>
    ''' Kill the operating threads if they are still alive.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub KillThreads()
        If Not connected And Not synclockcheckl And Not synclockchecks Then
            While updatethread.IsAlive
                Thread.Sleep(150)
                If updatethread.ThreadState = ThreadState.AbortRequested Or updatethread.ThreadState = 132 Then
                    Exit While
                ElseIf Not synclockcheckl And Not synclockchecks Then
                    updatethread.Abort()
                End If
            End While
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
    ''' Cleans accumalated packet_frames (Cleaning).
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub FlushPacketFrames()
        Try
            _packet_frame_part_dict.Clear()
        Catch ex As ThreadAbortException
            Throw ex
        Catch ex As Exception
        End Try
    End Sub
    ''' <summary>
    ''' Sends a message the server.
    ''' </summary>
    ''' <param name="message">The packet to send.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Send(message As packet) As Boolean
        Dim result As Boolean = False
        If Not message.header.ToLower.StartsWith("system") Then
            result = send_int(message)
        Else
            If _auto_msg_pass Then
                result = False
            Else
                result = send_int(message)
            End If
        End If
        Return result
    End Function

    Private Function send_int(message As packet) As Boolean
        Dim result As Boolean = False
        SyncLock lockSend
            synclockchecks = True
            Try
                If Not connected OrElse Not tcpClient.Connected Then
                    result = False
                Else
                    Dim frame As New packet_frame(message)
                    Dim f_p As packet_frame_part() = frame.ToParts(tcpClient.SendBufferSize, _no_packet_splitting)
                    For i As Integer = 0 To f_p.Length - 1 Step 1
                        Dim bytes As Byte() = f_p(i)
                        Dim b_l As Integer = bytes.Length
                        Dim b_l_b As Byte() = utils.Convert2Ascii(b_l)
                        Dim data_byt(0) As Byte
                        data_byt(0) = 1
                        data_byt = JoinBytes(data_byt, b_l_b)
                        Dim bts As Byte() = JoinBytes(data_byt, bytes)
                        tcpClientNetStream.Write(bts, 0, bts.Length)
                        tcpClientNetStream.Flush()
                        Thread.Sleep(_packet_delay)
                    Next
                    result = True
                End If
            Catch ex As ThreadAbortException
                Throw ex
            Catch ex As Exception
                result = False
                RaiseEvent errEncounter(ex)
            End Try
            synclockchecks = False
        End SyncLock
        Return result
    End Function

    Private Sub servermsgpr(message As packet)
        SyncLock lockListen
            synclockcheckl = True
            If _auto_msg_pass Then
                Dim clientnolst As New List(Of String)
                If message.header.ToLower.StartsWith("system") Then
                    'If message.stringdata(password).ToLower = "disconnect" Then
                    '    RaiseEvent ServerDisconnect()
                    'ElseIf message.stringdata(password).ToLower.EndsWith(":connected") Then
                    '    Dim colonindx As Integer = message.stringdata(password).ToLower.IndexOf(":")
                    '    Dim cilname As String = message.stringdata(password).Substring(0, colonindx - 1)
                    '    clientData.Add(cilname)
                    '    send_int(New packet(0, thisClient, New List(Of String), "system", "clients", New EncryptionParameter(encryptmethod, password)))
                    '    send_int(New packet(0, thisClient, New List(Of String), "system", "client", New EncryptionParameter(encryptmethod, password)))
                    'ElseIf message.stringdata(password).ToLower.EndsWith(":disconnected") Then
                    '    Dim colonindx As Integer = message.stringdata(password).ToLower.IndexOf(":")
                    '    Dim cilname As String = message.stringdata(password).Substring(0, colonindx - 1)
                    '    clientData.Remove(cilname)
                    '    send_int(New packet(0, thisClient, New List(Of String), "system", "clients", New EncryptionParameter(encryptmethod, password)))
                    '    send_int(New packet(0, thisClient, New List(Of String), "system", "client", New EncryptionParameter(encryptmethod, password)))
                    'ElseIf message.header.ToLower.StartsWith("system:clients") Then
                    If message.header.ToLower.StartsWith("system:clients") Then
                        clientData = DirectCast(message.objectdata(password), List(Of String))
                    ElseIf message.header.ToLower.StartsWith("system:name") Then
                        thisClient = message.stringdata(password)
                    End If
                Else
                    RaiseEvent ServerMessage(message)
                End If
            Else
                RaiseEvent ServerMessage(message)
            End If
            synclockcheckl = False
        End SyncLock
    End Sub
End Class
''' <summary>
''' Gives a reason for a failed connection.
''' </summary>
''' <remarks></remarks>
Public Enum failed_connection_reason As Integer
    ''' <summary>
    ''' The Reason for the failed connection is not specified.
    ''' </summary>
    ''' <remarks></remarks>
    unknown = 0
    ''' <summary>
    ''' The server is unavailable.
    ''' </summary>
    ''' <remarks></remarks>
    server_unavailable = 1
    ''' <summary>
    ''' The client name is already in use on the server.
    ''' </summary>
    ''' <remarks></remarks>
    name_taken = 2
    ''' <summary>
    ''' The server has the maximum amount of clients connected to it.
    ''' </summary>
    ''' <remarks></remarks>
    too_many_clients = 3
End Enum
''' <summary>
''' Provides parameters for client construction.
''' </summary>
''' <remarks></remarks>
Public Structure client_constructor
End Structure
''' <summary>
''' The ClientStart structure for connecting to a server.
''' </summary>
''' <remarks></remarks>
Public Structure ClientStart
    ''' <summary>
    ''' Specifies the connecting Client's Name
    ''' </summary>
    ''' <remarks></remarks>
    Public client_name As String
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
    ''' Creates a new set of client start info to start the client with.
    ''' </summary>
    ''' <param name="ipaddress">The IP Address for the server to bind to.</param>
    ''' <param name="_port">The port for the server to bind to.</param>
    ''' <param name="name">Specifies the connecting Client's Name</param>
    ''' <param name="encryptparam">The Encryption Parameter to use in the client.</param>
    ''' <param name="buffersize">The buffer size that will be used for the Tcp send and recieve buffers (Minumum Size:4096).</param>
    ''' <param name="internalmsgpass">If internal message passing is enabled (allows for clients to have a list of clients and change its name while it is connected).</param>
    ''' <param name="_no_delay">If there is a delay before sending accumalated packets.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal ipaddress As IPAddress, ByVal _port As Integer, ByVal name As String, Optional ByVal encryptparam As EncryptionParameter = Nothing, Optional ByVal buffersize As Integer = 8192, Optional ByVal internalmsgpass As Boolean = True, Optional ByVal _no_delay As Boolean = False)
        ip_address = ipaddress
        port = _port
        client_name = name
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
        internal_message_passing = internalmsgpass
        no_delay = _no_delay
    End Sub
End Structure