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
            Do While Connected And tcpcon()
                Try
                    Dim GetBytes(tcpClient.ReceiveBufferSize) As Byte
                    networkStream.Read(GetBytes, 0, tcpClient.ReceiveBufferSize)
                    Dim Msg As packet_frame_part = Nothing
                    Try
                        Msg = GetBytes
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
