Imports captainalm.CALMNetLib
Imports System.Threading

Namespace CALMNetMarshal
    ''' <summary>
    ''' This class can be retrieved from the NetMarshalTCP.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class NetMarshalTCPClient
        Inherits NetMarshalBase

        Protected Friend Sub New(asock As INetSocket)
            MyBase.New(asock)
        End Sub
        ''' <summary>
        ''' States whether the marshal is ready.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>Whether the marshal is ready</returns>
        ''' <remarks></remarks>
        Public Overrides ReadOnly Property ready As Boolean
            Get
                throb()
                Return (Not _cl Is Nothing) AndAlso _cl.connected
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
            If Not (TypeOf msg Is Beat) Then throb()
            If (Not _cl Is Nothing) AndAlso _cl.connected Then
                toret = _cl.sendBytes(msg.getData)
            End If
            Return toret
        End Function

        Protected Overrides Sub t_exec()
            While _cl IsNot Nothing AndAlso _cl.connected
                Try
                    While _cl IsNot Nothing AndAlso _cl.listening
                        Dim bts As Byte() = _cl.receiveBytes()
                        If bts.Length > 0 Then
                            Dim tbeat As Beat = New Serializer().deSerializeObject(Of Beat)(bts)
                            If Not tbeat.valid Then
                                Dim tmsg As IPacket = New Serializer().deSerializeObject(Of IPacket)(bts)
                                raiseMessageReceived(tmsg)
                            Else
                                throbbed()
                            End If
                        End If
                        Thread.Sleep(250)
                    End While
                Catch ex As NetLibException
                    raiseExceptionRaised(ex)
                End Try
                Thread.Sleep(250)
            End While
        End Sub
        ''' <summary>
        ''' Starts the Marshal and Opens the Connection.
        ''' </summary>
        ''' <remarks></remarks>
        Public Overrides Sub start()
            If Not _cl Is Nothing Then
                _cl.open()
                MyBase.start()
            End If
        End Sub
        ''' <summary>
        ''' Stops the Marshal and Closes the Connection.
        ''' </summary>
        ''' <remarks></remarks>
        Public Overrides Sub close()
            If Not _cl Is Nothing Then
                MyBase.close()
                _cl.close()
                _cl = Nothing
            End If
        End Sub
    End Class

End Namespace
