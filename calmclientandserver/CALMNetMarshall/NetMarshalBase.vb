Imports captainalm.CALMNetLib
Imports System.Threading

Namespace CALMNetMarshal
    ''' <summary>
    ''' The Net Marshal Base Class.
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class NetMarshalBase
        Protected _cl As INetSocket
        Protected _t As Thread = New Thread(AddressOf t_exec)
        Protected _bout As Integer = 0
        Protected _beated As Boolean = False
        Protected _awaitbeat As Boolean = False
        Protected _buffer As Integer = 0
        ''' <summary>
        ''' This event is raised when an exception is thrown.
        ''' </summary>
        ''' <param name="ex">The exception thrown</param>
        ''' <remarks></remarks>
        Public Event exceptionRaised(ex As Exception)
        ''' <summary>
        ''' This event is raised when a message is received.
        ''' </summary>
        ''' <param name="msg">The message received</param>
        ''' <remarks></remarks>
        Public Event MessageReceived(msg As IPacket)
        ''' <summary>
        ''' This event is raised when the beat times out.
        ''' </summary>
        ''' <remarks></remarks>
        Public Event beatTimedOut()
        ''' <summary>
        ''' Public constructor to create the base class.
        ''' </summary>
        ''' <param name="cl">The INetSocket to use</param>
        ''' <remarks></remarks>
        Public Sub New(cl As INetSocket)
            _cl = cl
        End Sub
        ''' <summary>
        ''' Starts the Marshal's main Thread.
        ''' </summary>
        ''' <remarks></remarks>
        Public Overridable Sub start()
            If _t IsNot Nothing Then
                If _t.ThreadState = ThreadState.Unstarted Then
                    _t.IsBackground = True
                    _t.Start()
                End If
            End If
        End Sub
        ''' <summary>
        ''' Stops the Marshal's main Thread.
        ''' </summary>
        ''' <remarks></remarks>
        Public Overridable Sub close()
            If _t IsNot Nothing Then
                If _t.IsAlive Then
                    _t.Join(500)
                End If
                If _t.IsAlive Then
                    _t.Abort()
                End If
                _t = Nothing
            End If
        End Sub
        ''' <summary>
        ''' States whether the marshal is ready.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>Whether the marshal is ready</returns>
        ''' <remarks></remarks>
        Public MustOverride ReadOnly Property ready As Boolean
        ''' <summary>
        ''' Gets or sets the timeout of beat messages to test sockets.
        ''' </summary>
        ''' <value>Integer</value>
        ''' <returns>The timeout of beat messages to test sockets</returns>
        ''' <remarks></remarks>
        Public Overridable Property beatTimeout As Integer
            Get
                Return _bout
            End Get
            Set(value As Integer)
                _bout = value
            End Set
        End Property
        ''' <summary>
        ''' Gets the internal socket.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>The internal socket</returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property internalSocket As INetSocket
            Get
                Return _cl
            End Get
        End Property
        ''' <summary>
        ''' Send a message via the marshal.
        ''' </summary>
        ''' <param name="msg">The message</param>
        ''' <returns>Whether the message sending succeeded</returns>
        ''' <remarks></remarks>
        Public MustOverride Function sendMessage(msg As IPacket) As Boolean
        ''' <summary>
        ''' Sets the buffer size of the net marshal.
        ''' </summary>
        ''' <value>Integer</value>
        ''' <returns>The buffer size of the net marshal</returns>
        ''' <remarks></remarks>
        Public Overridable Property bufferSize As Integer
            Get
                Return _buffer
            End Get
            Set(value As Integer)
                _buffer = value
            End Set
        End Property

        Protected MustOverride Sub t_exec()

        Protected _slockthrob As New Object()
        Protected Overridable Function throb() As Boolean
            Dim toret As Boolean = False
            SyncLock _slockthrob
                If (Not _cl Is Nothing) And (_bout > 0) Then
                    _awaitbeat = True
                    Dim b As New Beat
                    'WARNING: - This calls sendMessage, If you use throb in sendMessage, make sure you do not call throb if the Message is Beat.
                    toret = Me.sendMessage(b)
                    b = Nothing
                    If toret Then
                        Thread.Sleep(_bout)
                        _awaitbeat = False
                        toret = _beated
                        If _beated Then _beated = False Else raiseBeatTimedOut()
                    Else
                        raiseBeatTimedOut()
                    End If
                Else
                    toret = True
                End If
            End SyncLock
            Return toret
        End Function
        Protected Overridable Sub throbbed()
            SyncLock _slockthrob
                If _awaitbeat Then
                    _beated = True
                    _awaitbeat = False
                Else
                    Dim b As New Beat
                    'WARNING: - This calls sendMessage, If you use throb in sendMessage, make sure you do not call throb if the Message is Beat.
                    Me.sendMessage(b)
                    b = Nothing
                End If
            End SyncLock
        End Sub

        Protected Sub raiseExceptionRaised(ex As Exception)
            RaiseEvent exceptionRaised(ex)
        End Sub

        Protected Sub raiseMessageReceived(msg As IPacket)
            RaiseEvent MessageReceived(msg)
        End Sub

        Protected Sub raiseBeatTimedOut()
            RaiseEvent beatTimedOut()
        End Sub

        Protected Structure Beat
            Implements IPacket

            Public valid As Boolean

            Public Property data As Object Implements IPacket.data
                Get
                    Return valid
                End Get
                Set(value As Object)
                    valid = value
                End Set
            End Property

            Public ReadOnly Property dataType As Type Implements IPacket.dataType
                Get
                    Return GetType(Boolean)
                End Get
            End Property

            Public ReadOnly Property getData As Byte() Implements IPacket.getData
                Get
                    Return New Serializer().serializeObject(Of Beat)(Me)
                End Get
            End Property

            Public WriteOnly Property setData As Byte() Implements IPacket.setData
                Set(value As Byte())
                    Dim b As Beat = New Serializer().deSerializeObject(Of Beat)(value)
                    Me.valid = b.valid
                    b = Nothing
                End Set
            End Property

            Public Property receiverIP As String Implements IPacket.receiverIP
                Get
                    Return Nothing
                End Get
                Set(value As String)
                End Set
            End Property

            Public Property receiverPort As Integer Implements IPacket.receiverPort
                Get
                    Return Nothing
                End Get
                Set(value As Integer)
                End Set
            End Property

            Public Property senderIP As String Implements IPacket.senderIP
                Get
                    Return Nothing
                End Get
                Set(value As String)
                End Set
            End Property

            Public Property senderPort As Integer Implements IPacket.senderPort
                Get
                    Return Nothing
                End Get
                Set(value As Integer)
                End Set
            End Property
        End Structure
    End Class

End Namespace
