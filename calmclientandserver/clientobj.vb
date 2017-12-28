Imports System.Text
Imports System.Net.Sockets
Imports System.Threading
Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
''' <summary>
''' Internal Client Object (Used By Server for Client Communication) {Internal Access Only}
''' </summary>
''' <remarks></remarks>
Friend Class clientobj
    Private tcpClient As TcpClient
    Private networkStream As NetworkStream
    Private _cdelay As Integer = 0
    Public name As String
    Private Connected As Boolean = False
    Private discon_invoked As Boolean = False 'was the disconnection invoked by the disconect func already?
    Private _packet_frame_part_dict As New Dictionary(Of Integer, packet_frame_part())

    Public Event DataReceived(ByVal cname As String, ByVal Msg As packet_frame)
    Public Event errEncounter(ByVal ex As Exception)
    Public Event lostConnection(ByVal cientname As String)
    Private listenthread As Thread = Nothing

    Public ReadOnly Property isConnected As Boolean
        Get
            Return Connected
        End Get
    End Property

    Public Property close_delay As Integer
        Get
            Return _cdelay
        End Get
        Set(value As Integer)
            _cdelay = value
        End Set
    End Property

    Public Sub New(name2 As String, tcpClient2 As TcpClient)
        tcpClient = tcpClient2
        Name = name2
        networkStream = tcpClient.GetStream()
        Connected = True
        listenthread = New Thread(New ThreadStart(AddressOf Me.Listen))
        listenthread.IsBackground = True
        listenthread.Start()
    End Sub

    Private Function tcpcon() As Boolean
        If Not tcpClient Is Nothing Then
            Try
                Dim c As Boolean = tcpClient.Connected
                Return c
            Catch ex As Exception
                Return False
            End Try
        Else
            Return False
        End If
    End Function

    Private Sub Listen()
            Try
            networkStream = tcpClient.GetStream()
            Dim cdatarr(-1) As Byte
            Dim cnumarr As New List(Of Byte)
            Dim more_dat As Boolean = False
            Dim length_left As Integer = 0
            Dim in_packet As Boolean = False
            Dim in_number As Integer = 0
            Dim c_byte As Byte = 0
            Dim c_index As Integer = 0
            Do While Connected And tcpcon()
                Try
                    Dim Bytes(tcpClient.ReceiveBufferSize) As Byte
                    networkStream.Read(Bytes, 0, tcpClient.ReceiveBufferSize)
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
                            pr_bytes(cdatarr)
                            in_packet = False
                            c_index += length_left
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
                            length_left = utils.ConvertFromAscii(cnumarr.ToArray)
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
                                pr_bytes(rr)
                                in_packet = False
                                c_index += length_left - 1 'take away one as the while loop increments it by one anyway
                            End If
                        ElseIf c_byte = 0 And Not in_packet Then
                            pr_bytes(Bytes)
                        End If
                        c_index += 1
                    End While
                    c_byte = 0

                Catch ex As Exception
                    Exit Do
                End Try
                Thread.Sleep(150)
            Loop
            Disconnect(Not discon_invoked)
            Thread.CurrentThread.Abort()
            Catch ex As ThreadAbortException
        Catch ex_12C As Exception
            RaiseEvent errEncounter(ex_12C)
        End Try
    End Sub

    Private Sub pr_bytes(ByVal bytes As Byte())
        Dim Msg As packet_frame_part = Nothing
        Try
            Msg = bytes
        Catch ex As Exception
            Msg = Nothing
        End Try
        If Not Msg.data Is Nothing Then
            If Msg.partnum = Msg.totalparts And _packet_frame_part_dict.ContainsKey(Msg.refnum) Then
                Dim arr As packet_frame_part() = _packet_frame_part_dict(Msg.refnum)
                arr(Msg.partnum - 1) = Msg
                _packet_frame_part_dict(Msg.refnum) = arr
                Dim pf As packet_frame = Nothing
                Dim sc As Boolean = False
                Try
                    pf = arr
                    sc = True
                Catch ex As Exception
                    pf = Nothing
                    sc = False
                End Try
                If sc Then
                    RaiseEvent DataReceived(name, pf)
                End If
                _packet_frame_part_dict.Remove(Msg.refnum)
            ElseIf _packet_frame_part_dict.ContainsKey(Msg.refnum) Then
                Dim arr As packet_frame_part() = _packet_frame_part_dict(Msg.refnum)
                arr(Msg.partnum - 1) = Msg
                _packet_frame_part_dict(Msg.refnum) = arr
            ElseIf Not _packet_frame_part_dict.ContainsKey(Msg.refnum) Then
                Dim arr(Msg.totalparts - 1) As packet_frame_part
                arr(0) = Msg
                If arr.Length = 1 Then
                    Dim pf As packet_frame = Nothing
                    Dim sc As Boolean = False
                    Try
                        pf = arr
                        sc = True
                    Catch ex As Exception
                        pf = Nothing
                        sc = False
                    End Try
                    If sc Then
                        RaiseEvent DataReceived(name, pf)
                    End If
                Else
                    _packet_frame_part_dict.Add(Msg.refnum, arr)
                End If
            End If
        Else
            Connected = False
        End If
    End Sub

    Public Sub SendData(ByVal Data() As Byte)
        Try
            networkStream.Write(Data, 0, Data.Length)
            networkStream.Flush()
        Catch ex As Exception
            RaiseEvent errEncounter(ex)
        End Try
    End Sub

    Public Sub Disconnect(Optional raise_event As Boolean = True)
        On Error Resume Next
        If raise_event Then Thread.Sleep(close_delay)
        Connected = False
        If Not networkStream Is Nothing Then networkStream.Close(0)
        If Not tcpClient Is Nothing Then tcpClient.Close()
        If raise_event Then RaiseEvent lostConnection(name)
        discon_invoked = True
    End Sub

    Public Sub purge_msgs()
        Try
            _packet_frame_part_dict.Clear()
        Catch ex As Exception
        End Try
    End Sub
End Class
