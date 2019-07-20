'
' Created by SharpDevelop.
' User: Alfred
' Date: 19/05/2019
' Time: 15:47
' 
' To change this template use Tools | Options | Coding | Edit Standard Headers.
'
Namespace CALMNetLib
    ''' <summary>
    ''' Defines a Socket Interface.
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface INetSocket
        ''' <summary>
        ''' Returns whether the Socket is Connected.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>Whether the Socket is Connected</returns>
        ''' <remarks></remarks>
        ReadOnly Property connected As Boolean
        ''' <summary>
        ''' Returns whether the Socket is Listening.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>Whether the Socket is Connected</returns>
        ''' <remarks></remarks>
        ReadOnly Property listening As Boolean
        ''' <summary>
        ''' Sends a byte array over the network.
        ''' </summary>
        ''' <param name="bytes">The byte array to send</param>
        ''' <returns>Whether the send was successful</returns>
        ''' <remarks></remarks>
        Function sendBytes(bytes As Byte()) As Boolean
        ''' <summary>
        ''' Receives a byte array from the network.
        ''' </summary>
        ''' <returns>The Received Byte Array</returns>
        ''' <remarks></remarks>
        Function recieveBytes() As Byte()
        ''' <summary>
        ''' Returns whether a client is waiting to connect.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>If a client is waiting to connect</returns>
        ''' <remarks></remarks>
        ReadOnly Property clientWaiting As Boolean
        ''' <summary>
        ''' Accepts a client that is waiting to connect.
        ''' </summary>
        ''' <returns>The Accepted Client's Socket</returns>
        ''' <remarks></remarks>
        Function acceptClient() As INetSocket
        ''' <summary>
        ''' Opens the socket for network connection.
        ''' </summary>
        ''' <remarks></remarks>
        Sub open()
        ''' <summary>
        ''' Close the socket stopping network connections.
        ''' </summary>
        ''' <remarks></remarks>
        Sub close()
        ''' <summary>
        ''' Returns whether data is ready to be read from the network.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>Whether there is data on the network</returns>
        ''' <remarks></remarks>
        ReadOnly Property hasData As Boolean
    End Interface

End Namespace
