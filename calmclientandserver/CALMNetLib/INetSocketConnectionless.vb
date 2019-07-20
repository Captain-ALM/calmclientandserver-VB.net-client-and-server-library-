'
' Created by SharpDevelop.
' User: Alfred
' Date: 22/05/2019
' Time: 16:40
' 
' To change this template use Tools | Options | Coding | Edit Standard Headers.
'
Namespace CALMNetLib
    ''' <summary>
    ''' Defines a Connectionless Socket Interface.
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface INetSocketConnectionless
        Inherits INetSocket
        ''' <summary>
        ''' Disassociates the current dedicated connectionless connection.
        ''' </summary>
        ''' <remarks></remarks>
        Sub disconnect()
        ''' <summary>
        ''' Receives a byte array from the specified address and port on the network.
        ''' </summary>
        ''' <param name="remoteIP">The remote IP</param>
        ''' <param name="remotePort">The remote Port</param>
        ''' <returns>The Received Byte Array</returns>
        ''' <remarks></remarks>
        Function recieveBytesFrom(remoteIP As String, remotePort As Integer) As Byte()
        ''' <summary>
        ''' Reassociates a dedicated connectionless connection.
        ''' </summary>
        ''' <param name="remoteIP">The remote IP</param>
        ''' <param name="remotePort">The remote Port</param>
        ''' <remarks></remarks>
        Sub reconnect(remoteIP As String, remotePort As Integer)
        ''' <summary>
        ''' Sends a byte array over the network to the specified address and port.
        ''' </summary>
        ''' <param name="bytes">The byte array to send</param>
        ''' <param name="remoteIP">The remote IP</param>
        ''' <param name="remotePort">The remote Port</param>
        ''' <returns>Whether the send was successful</returns>
        ''' <remarks></remarks>
        Function sendBytesTo(bytes As Byte(), remoteIP As String, remotePort As Integer) As Boolean
    End Interface

End Namespace
