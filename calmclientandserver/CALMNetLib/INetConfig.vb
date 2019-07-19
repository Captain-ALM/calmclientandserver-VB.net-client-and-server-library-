'
' Created by SharpDevelop.
' User: Alfred
' Date: 20/05/2019
' Time: 08:56
' 
' To change this template use Tools | Options | Coding | Edit Standard Headers.
'
Namespace CALMNetLib

    Public Interface INetConfig
        Property sendBufferSize As Integer
        Property receiveBufferSize As Integer
        Property noDelay As Boolean
        Property receiveTimeout As Integer
        Property sendTimeout As Integer
        ReadOnly Property remoteIPAddress As String
        ReadOnly Property remotePort As Integer
        ReadOnly Property localIPAddress As String
        ReadOnly Property localPort As Integer
        Property exclusiveAddressUse As Boolean
        Property connectionBacklog As Integer
    End Interface

End Namespace
