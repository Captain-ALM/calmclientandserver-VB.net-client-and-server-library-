Imports captainalm.Serialize

Namespace CALMNetMarshal

    ''' <summary>
    ''' A Message Structure that Implements IPacket using XML serialization.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable>
    Public Structure XMessage
        Implements IPacket

        Private senderIP_ As String
        Private senderPort_ As Integer
        Private receiverIP_ As String
        Private receiverPort_ As Integer
        ''' <summary>
        ''' The header of the message.
        ''' </summary>
        ''' <remarks></remarks>
        Public header As String
        ''' <summary>
        ''' The message.
        ''' </summary>
        ''' <remarks></remarks>
        Public message As String
        ''' <summary>
        ''' Allows to get or set the internally held packet data generically.
        ''' </summary>
        ''' <value>Object</value>
        ''' <returns>Held Packet Data.</returns>
        ''' <remarks></remarks>
        Public Property data As Object Implements IPacket.data
            Get
                Return New Tuple(Of String, String)(header, message)
            End Get
            Set(value As Object)
                Dim cc As Tuple(Of String, String) = CType(value, Tuple(Of String, String))
                header = cc.Item1
                message = cc.Item2
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
                Return GetType(Tuple(Of String, String))
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
                Return New XSerializer().serializeObject(Of XMLPayload)(New XMLPayload(Me))
            End Get
        End Property
        ''' <summary>
        ''' Allows data to be written to the packet interface.
        ''' </summary>
        ''' <value>Byte Array</value>
        ''' <remarks></remarks>
        Public WriteOnly Property setData As Byte() Implements IPacket.setData
            Set(value As Byte())
                Dim msg As XMessage = New XSerializer().deSerializeObject(Of XMLPayload)(value).getXMessage()
                Me.receiverIP_ = msg.receiverIP_
                Me.receiverPort_ = msg.receiverPort_
                Me.senderIP_ = msg.senderIP_
                Me.senderPort_ = msg.senderPort_
                Me.header = msg.header
                Me.message = msg.message
                msg = Nothing
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
                Return receiverIP_
            End Get
            Set(value As String)
                receiverIP_ = value
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
                Return receiverPort_
            End Get
            Set(value As Integer)
                receiverPort_ = value
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
                Return senderIP_
            End Get
            Set(value As String)
                senderIP_ = value
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
                Return senderPort_
            End Get
            Set(value As Integer)
                senderPort_ = value
            End Set
        End Property
        ''' <summary>
        ''' The XML Serializable Payload
        ''' </summary>
        ''' <remarks></remarks>
        Public Structure XMLPayload
            Public header As String
            Public message As String
            Public senderIP As String
            Public senderPort As Integer
            Public receiverIP As String
            Public receiverPort As Integer
            ''' <summary>
            ''' Constructs an XML payload given the XMessage
            ''' </summary>
            ''' <param name="xmsgIn">The XMessage to serialize</param>
            ''' <remarks></remarks>
            Public Sub New(xmsgIn As XMessage)
                Me.header = xmsgIn.header
                Me.message = xmsgIn.message
                Me.senderIP = xmsgIn.senderIP
                Me.senderPort = xmsgIn.senderPort
                Me.receiverIP = xmsgIn.receiverIP
                Me.receiverPort = xmsgIn.receiverPort
            End Sub
            ''' <summary>
            ''' Returns the XMessage to payload holds
            ''' </summary>
            ''' <returns>The XMessage</returns>
            ''' <remarks></remarks>
            Public Function getXMessage() As XMessage
                Return New XMessage() With {.header = Me.header, .message = Me.message, .senderIP = Me.senderIP, .senderPort = Me.senderPort, .receiverIP = Me.receiverIP, .receiverPort = Me.receiverPort}
            End Function
        End Structure
    End Structure

End Namespace
