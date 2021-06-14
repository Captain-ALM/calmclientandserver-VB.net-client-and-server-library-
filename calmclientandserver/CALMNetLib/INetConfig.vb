'
' Created by SharpDevelop.
' User: Alfred
' Date: 20/05/2019
' Time: 08:56
' 
' To change this template use Tools | Options | Coding | Edit Standard Headers.
'
Namespace CALMNetLib
    ''' <summary>
    ''' Defines The Socket Configuration Interface.
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface INetConfig
        ''' <summary>
        ''' Gets or Sets the size of the Send Buffer.
        ''' </summary>
        ''' <value>Integer</value>
        ''' <returns>The size of the send buffer</returns>
        ''' <remarks></remarks>
        Property sendBufferSize As Integer
        ''' <summary>
        ''' Gets or Sets the size of the receive Buffer.
        ''' </summary>
        ''' <value>Integer</value>
        ''' <returns>The size of the receive buffer</returns>
        ''' <remarks></remarks>
        Property receiveBufferSize As Integer
        ''' <summary>
        ''' Gets or Sets the Disablement of Nagle's Algorithm.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>The Disablement of Nagle's Algorithm.</returns>
        ''' <remarks></remarks>
        Property noDelay As Boolean
        ''' <summary>
        ''' Gets or Sets the receive Timeout.
        ''' </summary>
        ''' <value>Integer</value>
        ''' <returns>The receive timeout</returns>
        ''' <remarks></remarks>
        Property receiveTimeout As Integer
        ''' <summary>
        ''' Gets or Sets the send Timeout.
        ''' </summary>
        ''' <value>Integer</value>
        ''' <returns>The send timeout</returns>
        ''' <remarks></remarks>
        Property sendTimeout As Integer
        ''' <summary>
        ''' Returns the remote IP Address.
        ''' </summary>
        ''' <value>String</value>
        ''' <returns>The remote IP Address</returns>
        ''' <remarks></remarks>
        ReadOnly Property remoteIPAddress As String
        ''' <summary>
        ''' Returns the remote IP Port.
        ''' </summary>
        ''' <value>String</value>
        ''' <returns>The remote IP Port</returns>
        ''' <remarks></remarks>
        ReadOnly Property remotePort As Integer
        ''' <summary>
        ''' Returns the local IP Address.
        ''' </summary>
        ''' <value>String</value>
        ''' <returns>The local IP Address</returns>
        ''' <remarks></remarks>
        ReadOnly Property localIPAddress As String
        ''' <summary>
        ''' Returns the local IP Port.
        ''' </summary>
        ''' <value>String</value>
        ''' <returns>The local IP Port</returns>
        ''' <remarks></remarks>
        ReadOnly Property localPort As Integer
        ''' <summary>
        ''' Gets or Sets whether address use is exclusive.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>Whether the address use is exclusive</returns>
        ''' <remarks></remarks>
        Property exclusiveAddressUse As Boolean
        ''' <summary>
        ''' Gets or sets the backlog of connections.
        ''' </summary>
        ''' <value>Integer</value>
        ''' <returns>Backlog of Connections</returns>
        ''' <remarks></remarks>
        Property connectionBacklog As Integer
        ''' <summary>
        ''' Gets or sets whether the socket sends and receives data using the length header.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>Whether the socket uses a length header</returns>
        ''' <remarks></remarks>
        Property hasLengthHeader As Boolean
        ''' <summary>
        ''' Allows for supported source settings to be copied to the current INetConfig instance.
        ''' </summary>
        ''' <param name="source">The source to copy</param>
        ''' <remarks></remarks>
        Sub copyConfigFrom(source As INetConfig)
        ''' <summary>
        ''' Allows for a safe NetSocketConfig containing duplicated supported configuration to be returned without exceptions.
        ''' </summary>
        ''' <returns>The safe NetSocketConfig with duplicated supported configuration</returns>
        ''' <remarks></remarks>
        Function getSafeSocketConfig() As NetSocketConfig
    End Interface

End Namespace
