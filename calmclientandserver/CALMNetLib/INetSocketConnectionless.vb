'
' Created by SharpDevelop.
' User: Alfred
' Date: 22/05/2019
' Time: 16:40
' 
' To change this template use Tools | Options | Coding | Edit Standard Headers.
'
Namespace CALMNetLib

    Public Interface INetSocketConnectionless
        Inherits INetSocket

        Sub disconnect()
        Function recieveBytesFrom(remoteIP As String, remotePort As Integer) As Byte()
        Sub reconnect(remoteIP As String, remotePort As Integer)
        Function sendBytesTo(bytes As Byte(), remoteIP As String, remotePort As Integer) As Boolean
    End Interface

End Namespace
