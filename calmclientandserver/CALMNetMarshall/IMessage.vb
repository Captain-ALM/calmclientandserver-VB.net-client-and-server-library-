Imports System.Runtime.Serialization

Namespace CALMNetMarshal
    ''' <summary>
    ''' The packet Interface.
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface IPacket
        ''' <summary>
        ''' The sender's IP Address.
        ''' </summary>
        ''' <value>String</value>
        ''' <returns>The sender's IP Address.</returns>
        ''' <remarks></remarks>
        Property senderIP As String
        ''' <summary>
        ''' The sender's Port.
        ''' </summary>
        ''' <value>Integer</value>
        ''' <returns>The sender's Port.</returns>
        ''' <remarks></remarks>
        Property senderPort As Integer
        ''' <summary>
        ''' The receiver's IP Address.
        ''' </summary>
        ''' <value>String</value>
        ''' <returns>The receiver's IP Address.</returns>
        ''' <remarks></remarks>
        Property receiverIP As String
        ''' <summary>
        ''' The receiver's Port.
        ''' </summary>
        ''' <value>Integer</value>
        ''' <returns>The receiver's Port.</returns>
        ''' <remarks></remarks>
        Property receiverPort As Integer
        ''' <summary>
        ''' Allows data to be taken from the packet interface.
        ''' </summary>
        ''' <value>Byte Array</value>
        ''' <returns>Packet Interface Data</returns>
        ''' <remarks></remarks>
        ReadOnly Property getData() As Byte()
        ''' <summary>
        ''' Allows data to be written to the packet interface.
        ''' </summary>
        ''' <value>Byte Array</value>
        ''' <remarks></remarks>
        WriteOnly Property setData() As Byte()
        ''' <summary>
        ''' Allows to get or set the internally held packet data generically.
        ''' </summary>
        ''' <value>Object</value>
        ''' <returns>Held Packet Data.</returns>
        ''' <remarks></remarks>
        Property data As Object
        ''' <summary>
        ''' Gets the Type of the internally held packet data.
        ''' </summary>
        ''' <value>Type</value>
        ''' <returns>Type Of Internal Data</returns>
        ''' <remarks></remarks>
        ReadOnly Property dataType As Type
    End Interface

End Namespace
