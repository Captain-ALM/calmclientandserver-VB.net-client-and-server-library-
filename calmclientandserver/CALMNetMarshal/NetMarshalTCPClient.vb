﻿Imports captainalm.CALMNetLib
Imports System.Threading
Imports captainalm.Serialize

Namespace CALMNetMarshal
    ''' <summary>
    ''' This class can be retrieved from the NetMarshalTCP and represents a separate client.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class NetMarshalTCPClient
        Inherits NetMarshalBase

        Protected _msgCache As New Queue(Of IPacket)
        Protected _releaseCache As Boolean = False

        Protected Friend Sub New(asock As INetSocket)
            MyBase.New(asock)
            updateDupConf()
            _buffer = CType(asock, INetConfig).receiveBufferSize
            _haslengthheader = CType(asock, INetConfig).hasLengthHeader
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
            If (Not _cl Is Nothing) AndAlso _cl.connected Then
                toret = _cl.sendBytes(msg.getData)
            End If
            Return toret
        End Function

        Protected Overrides Sub t_exec()
            While _cl IsNot Nothing AndAlso _cl.connected
                Try
                    While Not _releaseCache AndAlso _cl IsNot Nothing AndAlso _cl.connected
                        Dim bts As Byte() = _cl.receiveBytes()
                        If bts.Length > 0 Then
                            Dim tmsg As IPacket = _serializer.deSerializeObject(Of IPacket)(bts)
                            If (Not TypeOf tmsg Is Beat) And (Not tmsg Is Nothing) Then
                                _msgCache.Enqueue(tmsg)
                                throbbed(False)
                            ElseIf TypeOf tmsg Is Beat Then
                                If CType(tmsg, Beat).valid Then throbbed(CType(tmsg, Beat).isBeat) Else throbbed(False)
                            End If
                        End If
                        If _threaddelay > 0 Then Thread.Sleep(_threaddelay)
                    End While
                    While _msgCache.Count > 0
                        raiseMessageReceived(_msgCache.Dequeue())
                        throbbed(False)
                    End While
                    While _cl IsNot Nothing AndAlso _cl.connected
                        Dim bts As Byte() = _cl.receiveBytes()
                        If bts.Length > 0 Then
                            Dim tmsg As IPacket = _serializer.deSerializeObject(Of IPacket)(bts)
                            If (Not TypeOf tmsg Is Beat) And (Not tmsg Is Nothing) Then
                                raiseMessageReceived(tmsg)
                                throbbed(False)
                            ElseIf TypeOf tmsg Is Beat Then
                                If CType(tmsg, Beat).valid Then throbbed(CType(tmsg, Beat).isBeat) Else throbbed(False)
                            End If
                        End If
                        If _threaddelay > 0 Then Thread.Sleep(_threaddelay)
                    End While
                Catch ex As NetLibException
                    raiseExceptionRaised(ex)
                End Try
                If _threaddelay > 0 Then Thread.Sleep(_threaddelay)
            End While
        End Sub
        ''' <summary>
        ''' Starts the Marshal and Opens the Connection.
        ''' </summary>
        ''' <remarks></remarks>
        Public Overrides Sub start()
            If Not _cl Is Nothing Then
                If Not _cl.connected Then _cl.open()
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
                If Not _cl Is Nothing Then
                    MyBase.hasLengthHeader = value
                    CType(_cl, INetConfig).hasLengthHeader = value
                End If
            End Set
        End Property
        ''' <summary>
        ''' Releases the received message cache to the event
        ''' </summary>
        ''' <remarks></remarks>
        Public Overridable Sub releaseCache()
            _releaseCache = True
        End Sub
    End Class

End Namespace
