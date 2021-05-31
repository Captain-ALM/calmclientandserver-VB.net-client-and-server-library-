Imports captainalm.CALMNetLib
Imports System.Threading
Imports captainalm.Serialize
Imports System.Xml.Serialization

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
        Protected _buffer As Integer = 0
        Protected _configdup As NetSocketConfig
        Protected _serializer As ISerialize = New Serializer()
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
            updateDupConf()
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
                    _t.Join(250)
                End If
                If _t.IsAlive Then
                    _t.Abort()
                End If
                _t = Nothing
            End If
        End Sub
        ''' <summary>
        ''' Provides the serializer for the packets to use
        ''' </summary>
        ''' <value>ISerialize</value>
        ''' <returns>The Serializer in use</returns>
        ''' <remarks></remarks>
        Public Overridable Property serializer As ISerialize
            Get
                Return _serializer
            End Get
            Set(value As ISerialize)
                If value Is Nothing Then Return
                _serializer = value
            End Set
        End Property
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
        ''' <value>INetSocket</value>
        ''' <returns>The internal socket</returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property internalSocket As INetSocket
            Get
                Return _cl
            End Get
        End Property
        ''' <summary>
        ''' Gets the internal socket's duplicated static configuration.
        ''' </summary>
        ''' <value>NetSocketConfig</value>
        ''' <returns>The duplicated static configuration</returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property duplicatedInternalSocketConfig As NetSocketConfig
            Get
                updateDupConf()
                Return _configdup
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

        Protected Overridable Sub updateDupConf()
            If Not _cl Is Nothing Then
                If TypeOf _cl Is INetConfig Then
                    _configdup = New NetSocketConfig(CType(_cl, INetConfig))
                End If
            End If
        End Sub

        Protected MustOverride Sub t_exec()

        Protected _slockbeat As New Object()
        Protected Overridable Function throb() As Boolean
            SyncLock _slockbeat
                If (Not _cl Is Nothing) And (_bout > 0) Then
                    Dim toret As Boolean = False
                    toret = Me.sendMessage(New Beat(1, _serializer))
                    If toret Then
                        Dim blft As Integer = _bout
                        While blft > 0
                            Thread.Sleep(125)
                            blft -= 125
                        End While
                        toret = _beated
                        If _beated Then _beated = False Else raiseBeatTimedOut()
                    Else
                        raiseBeatTimedOut()
                    End If
                    Return toret
                ElseIf Not _cl Is Nothing Then
                    Return True
                End If
                Return False
            End SyncLock
        End Function

        Protected Overridable Sub throbbed(sendback As Boolean)
            If sendback Then Me.sendMessage(New Beat(2, _serializer)) Else _beated = True
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
        ''' <summary>
        ''' Provides the beat message structure
        ''' </summary>
        ''' <remarks></remarks>
        <Serializable>
        Public Structure Beat
            Implements IPacket

            Public valid As Integer
            Private _serializer As ISerialize
            ''' <summary>
            ''' Constructs a new beat packet with a value and the serializer to use
            ''' </summary>
            ''' <param name="val">The value to store</param>
            ''' <param name="ser">The serializer to use</param>
            ''' <remarks></remarks>
            Public Sub New(val As Integer, ser As ISerialize)
                valid = val
                _serializer = ser
            End Sub
            ''' <summary>
            ''' Allows to get or set the internally held packet data generically.
            ''' </summary>
            ''' <value>Object</value>
            ''' <returns>Held Packet Data.</returns>
            ''' <remarks></remarks>
            <XmlIgnore>
            Public Property data As Object Implements IPacket.data
                Get
                    Return valid
                End Get
                Set(value As Object)
                    valid = value
                End Set
            End Property
            ''' <summary>
            ''' Provides the serializer for the packet to use
            ''' </summary>
            ''' <value>ISerialize</value>
            ''' <returns>The Serializer in use</returns>
            ''' <remarks></remarks>
            <XmlIgnore>
            Public Property serializer As ISerialize
                Get
                    Return _serializer
                End Get
                Set(value As ISerialize)
                    If value Is Nothing Then Return
                    _serializer = value
                End Set
            End Property
            ''' <summary>
            ''' Gets the Type of the internally held packet data.
            ''' </summary>
            ''' <value>Type</value>
            ''' <returns>Type Of Internal Data</returns>
            ''' <remarks></remarks>
            Public ReadOnly Property dataType As Type Implements IPacket.dataType
                Get
                    Return GetType(Integer)
                End Get
            End Property
            ''' <summary>
            ''' Allows data to be taken from the packet interface.
            ''' </summary>
            ''' <value>Byte Array</value>
            ''' <returns>Packet Interface Data</returns>
            ''' <remarks></remarks>
            Public ReadOnly Property getData As Byte() Implements IPacket.getData
                Get
                    If _serializer Is Nothing Then _serializer = New Serializer()
                    Return _serializer.serializeObject(Of Beat)(Me)
                End Get
            End Property
            ''' <summary>
            ''' Allows data to be written to the packet interface.
            ''' </summary>
            ''' <value>Byte Array</value>
            ''' <remarks></remarks>
            Public WriteOnly Property setData As Byte() Implements IPacket.setData
                Set(value As Byte())
                    If _serializer Is Nothing Then _serializer = New Serializer()
                    Dim b As Beat = _serializer.deSerializeObject(Of Beat)(value)
                    Me.valid = b.valid
                    b = Nothing
                End Set
            End Property
            ''' <summary>
            ''' The receiver's IP Address.
            ''' </summary>
            ''' <value>String</value>
            ''' <returns>The receiver's IP Address.</returns>
            ''' <remarks></remarks>
            Public Property receiverIP As String Implements IPacket.receiverIP
                Get
                    Return " "
                End Get
                Set(value As String)
                End Set
            End Property
            ''' <summary>
            ''' The receiver's Port.
            ''' </summary>
            ''' <value>Integer</value>
            ''' <returns>The receiver's Port.</returns>
            ''' <remarks></remarks>
            Public Property receiverPort As Integer Implements IPacket.receiverPort
                Get
                    Return 0
                End Get
                Set(value As Integer)
                End Set
            End Property
            ''' <summary>
            ''' The sender's IP Address.
            ''' </summary>
            ''' <value>String</value>
            ''' <returns>The sender's IP Address.</returns>
            ''' <remarks></remarks>
            Public Property senderIP As String Implements IPacket.senderIP
                Get
                    Return " "
                End Get
                Set(value As String)
                End Set
            End Property
            ''' <summary>
            ''' The sender's Port.
            ''' </summary>
            ''' <value>Integer</value>
            ''' <returns>The sender's Port.</returns>
            ''' <remarks></remarks>
            Public Property senderPort As Integer Implements IPacket.senderPort
                Get
                    Return 0
                End Get
                Set(value As Integer)
                End Set
            End Property
            ''' <summary>
            ''' Returns whether this is a beat
            ''' </summary>
            ''' <value>Boolean</value>
            ''' <returns>If this is a beat</returns>
            ''' <remarks></remarks>
            Public ReadOnly Property isBeat() As Boolean
                Get
                    Return Me.valid = 1
                End Get
            End Property
            ''' <summary>
            ''' Returns whether this is a throb
            ''' </summary>
            ''' <value>Boolean</value>
            ''' <returns>If this is a throb</returns>
            ''' <remarks></remarks>
            Public ReadOnly Property isThrob() As Boolean
                Get
                    Return Me.valid = 2
                End Get
            End Property
        End Structure
    End Class

End Namespace
