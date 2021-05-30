Imports System.Net
Imports captainalm.CALMNetLib
Imports System.Net.Sockets
Imports System.Threading
Imports captainalm.Serialize

Namespace CALMNetMarshal
    ''' <summary>
    ''' This class provides a UDP Socket Marshal.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class NetMarshalUDP
        Inherits NetMarshalBase

        Protected _f As AddressFamily
        ''' <summary>
        ''' Constructs a new instance of NetMarshalUDP.
        ''' </summary>
        ''' <param name="iptb">The IP Address to bind to</param>
        ''' <param name="ptb">The Port to bind to</param>
        ''' <param name="bufsiz">The buffer size for the sockets</param>
        ''' <remarks></remarks>
        Public Sub New(iptb As IPAddress, ptb As Integer, Optional bufsiz As Integer = 16384)
            MyBase.New(New NetUDPClient(iptb, ptb, UDPIPPortSpecification.Local) With {.sendBufferSize = bufsiz, .receiveBufferSize = bufsiz})
            _f = iptb.AddressFamily
            _buffer = bufsiz
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
            If (Not _cl Is Nothing) AndAlso _cl.listening Then
                toret = CType(_cl, INetSocketConnectionless).sendBytesTo(msg.getData, msg.receiverIP, msg.receiverPort)
            End If
            Return toret
        End Function
        ''' <summary>
        ''' Starts the Marshal and Opens the Connection.
        ''' </summary>
        ''' <remarks></remarks>
        Public Overrides Sub start()
            If Not _cl Is Nothing Then
                If Not _cl.listening Then _cl.open()
                MyBase.start()
            End If
        End Sub
        ''' <summary>
        ''' Stops the Marshal and Closes the Connection.
        ''' </summary>
        ''' <remarks></remarks>
        Public Overrides Sub close()
            MyBase.close()
            If Not _cl Is Nothing Then
                _cl.close()
                _cl = Nothing
            End If
        End Sub
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
                If Not _cl Is Nothing Then
                    MyBase.bufferSize = value
                    CType(_cl, INetConfig).receiveBufferSize = value
                    CType(_cl, INetConfig).sendBufferSize = value
                End If
            End Set
        End Property
        Protected Overrides Sub t_exec()
            While _cl IsNot Nothing AndAlso _cl.listening
                Try
                    While _cl IsNot Nothing AndAlso _cl.listening
                        Dim ip As String = ""
                        If _f = AddressFamily.InterNetwork Then
                            ip = IPAddress.Any.ToString()
                        ElseIf _f = AddressFamily.InterNetworkV6 Then
                            ip = IPAddress.IPv6Any.ToString()
                        End If
                        Dim bts As Byte() = CType(_cl, INetSocketConnectionless).receiveBytesFrom(ip, 0)
                        If bts.Length > 0 Then
                            Dim tbeat As Beat = New Serializer().deSerializeObject(Of Beat)(bts)
                            If Not tbeat.valid Then
                                Dim tmsg As IPacket = New Serializer().deSerializeObject(Of IPacket)(bts)
                                If Not (TypeOf tmsg Is Beat) Then
                                    raiseMessageReceived(tmsg)
                                End If
                            End If
                        End If
                        Thread.Sleep(125)
                    End While
                Catch ex As NetLibException
                    raiseExceptionRaised(ex)
                End Try
                Thread.Sleep(125)
            End While
        End Sub
    End Class

End Namespace
