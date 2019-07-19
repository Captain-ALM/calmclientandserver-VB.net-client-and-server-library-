'
' Created by SharpDevelop.
' User: Alfred
' Date: 19/05/2019
' Time: 15:47
' 
' To change this template use Tools | Options | Coding | Edit Standard Headers.
'
Namespace CALMNetLib

    Public Interface INetSocket
        ReadOnly Property connected As Boolean
        ReadOnly Property listening As Boolean
        Function sendBytes(bytes As Byte()) As Boolean
        Function recieveBytes() As Byte()
        ReadOnly Property clientWaiting As Boolean
        Function acceptClient() As INetSocket
        Sub open()
        Sub close()
        ReadOnly Property hasData As Boolean
    End Interface

End Namespace
