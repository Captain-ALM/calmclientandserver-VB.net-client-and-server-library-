'
' Created by SharpDevelop.
' User: Alfred
' Date: 22/05/2019
' Time: 17:46
' 
' To change this template use Tools | Options | Coding | Edit Standard Headers.
'
Namespace CALMNetLib
    ''' <summary>
    ''' Defines the NetSocketConfig Structure.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable>
    Public Structure NetSocketConfig
        Implements INetConfig

        Private _SendBufferSize As Integer
        Private _ReceiveBufferSize As Integer
        Private _NoDelay As Boolean
        Private _receiveTimeout As Integer
        Private _sendTimeout As Integer
        Private _ExclusiveAddressUse As Boolean
        Private _bl As Integer
        Private _rip As String
        Private _rport As Integer
        Private _lip As String
        Private _lport As Integer
        Private _hlh As Boolean
        ''' <summary>
        ''' Constructs a New NetSocketConfig Structure with the Specified addresses and ports.
        ''' </summary>
        ''' <param name="local_IPAddress">The Local IP Address</param>
        ''' <param name="local_Port">The Local Port</param>
        ''' <param name="remote_IPAddress">The Remote IP Address</param>
        ''' <param name="remote_Port">The Remote Port</param>
        ''' <remarks></remarks>
        Public Sub New(local_IPAddress As String, local_Port As Integer, remote_IPAddress As String, remote_Port As Integer)
            _lip = local_IPAddress
            _lport = local_Port
            _rip = remote_IPAddress
            _rport = remote_Port
        End Sub
        ''' <summary>
        ''' Constructs a New NetSocketConfig Structure copying the configuration of another INetConfig Implementation.
        ''' This will not throw exceptions if unsupported configuration exists.
        ''' </summary>
        ''' <param name="source">The INetConfig Configuration</param>
        ''' <remarks></remarks>
        Public Sub New(source As INetConfig)
            Me.New(source, False)
        End Sub
        ''' <summary>
        ''' Constructs a New NetSocketConfig Structure copying the configuration of another INetConfig Implementation.
        ''' </summary>
        ''' <param name="source">The INetConfig Configuration</param>
        ''' <param name="throwExceptions">Whether to throw exceptions for unsupported configuration</param>
        ''' <remarks></remarks>
        Public Sub New(source As INetConfig, throwExceptions As Boolean)
            Dim asrc As NetSocketConfig
            If throwExceptions Then asrc = source Else asrc = source.getSafeSocketConfig()
            _SendBufferSize = asrc.sendBufferSize
            _ReceiveBufferSize = asrc.receiveBufferSize
            _NoDelay = asrc.noDelay
            _receiveTimeout = asrc.receiveTimeout
            _sendTimeout = asrc.sendTimeout
            _ExclusiveAddressUse = asrc.exclusiveAddressUse
            _bl = asrc.connectionBacklog
            _rip = asrc.remoteIPAddress
            _rport = asrc.remotePort
            _lip = asrc.localIPAddress
            _lport = asrc.localPort
            _hlh = asrc.hasLengthHeader
        End Sub
        ''' <summary>
        ''' Gets or Sets the size of the Send Buffer.
        ''' </summary>
        ''' <value>Integer</value>
        ''' <returns>The size of the send buffer</returns>
        ''' <remarks></remarks>
        Public Property sendBufferSize As Integer Implements INetConfig.sendBufferSize
            Get
                Return _SendBufferSize
            End Get
            Set(value As Integer)
                _SendBufferSize = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or Sets the size of the receive Buffer.
        ''' </summary>
        ''' <value>Integer</value>
        ''' <returns>The size of the receive buffer</returns>
        ''' <remarks></remarks>
        Public Property receiveBufferSize As Integer Implements INetConfig.receiveBufferSize
            Get
                Return _ReceiveBufferSize
            End Get
            Set(value As Integer)
                _ReceiveBufferSize = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or Sets the Disablement of Nagle's Algorithm.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>The Disablement of Nagle's Algorithm.</returns>
        ''' <remarks></remarks>
        Public Property noDelay As Boolean Implements INetConfig.noDelay
            Get
                Return _NoDelay
            End Get
            Set(value As Boolean)
                _NoDelay = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or Sets the receive Timeout.
        ''' </summary>
        ''' <value>Integer</value>
        ''' <returns>The receive timeout</returns>
        ''' <remarks></remarks>
        Public Property receiveTimeout As Integer Implements INetConfig.receiveTimeout
            Get
                Return _receiveTimeout
            End Get
            Set(value As Integer)
                _receiveTimeout = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or Sets the send Timeout.
        ''' </summary>
        ''' <value>Integer</value>
        ''' <returns>The send timeout</returns>
        ''' <remarks></remarks>
        Public Property sendTimeout As Integer Implements INetConfig.sendTimeout
            Get
                Return _sendTimeout
            End Get
            Set(value As Integer)
                _sendTimeout = value
            End Set
        End Property
        ''' <summary>
        ''' Returns the local IP Address.
        ''' </summary>
        ''' <value>String</value>
        ''' <returns>The local IP Address</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property localIPAddress As String Implements INetConfig.localIPAddress
            Get
                Return _lip
            End Get
        End Property
        ''' <summary>
        ''' Returns the local IP Port.
        ''' </summary>
        ''' <value>String</value>
        ''' <returns>The local IP Port</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property localPort As Integer Implements INetConfig.localPort
            Get
                Return _lport
            End Get
        End Property
        ''' <summary>
        ''' Returns the remote IP Address.
        ''' </summary>
        ''' <value>String</value>
        ''' <returns>The remote IP Address</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property remoteIPAddress As String Implements INetConfig.remoteIPAddress
            Get
                Return _rip
            End Get
        End Property
        ''' <summary>
        ''' Returns the remote IP Port.
        ''' </summary>
        ''' <value>String</value>
        ''' <returns>The remote IP Port</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property remotePort As Integer Implements INetConfig.remotePort
            Get
                Return _rport
            End Get
        End Property
        ''' <summary>
        ''' Gets or Sets whether address use is exclusive.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>Whether the address use is exclusive</returns>
        ''' <remarks></remarks>
        Public Property exclusiveAddressUse As Boolean Implements INetConfig.exclusiveAddressUse
            Get
                Return _ExclusiveAddressUse
            End Get
            Set(value As Boolean)
                _ExclusiveAddressUse = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the backlog of connections.
        ''' </summary>
        ''' <value>Integer</value>
        ''' <returns>Backlog of Connections</returns>
        ''' <remarks></remarks>
        Public Property connectionBacklog As Integer Implements INetConfig.connectionBacklog
            Get
                Return _bl
            End Get
            Set(value As Integer)
                _bl = value
            End Set
        End Property
        ''' <summary>
        ''' Sets the Local IP Address.
        ''' </summary>
        ''' <param name="local_IPAddress">The Local IP Address</param>
        ''' <remarks></remarks>
        Public Sub setLocalIPAddress(local_IPAddress As String)
            _lip = local_IPAddress
        End Sub
        ''' <summary>
        ''' Sets the Remote IP Address.
        ''' </summary>
        ''' <param name="remote_IPAddress">The Remote IP Address</param>
        ''' <remarks></remarks>
        Public Sub setRemoteIPAddress(remote_IPAddress As String)
            _rip = remote_IPAddress
        End Sub
        ''' <summary>
        ''' Sets the Local Port.
        ''' </summary>
        ''' <param name="local_Port">The Local Port</param>
        ''' <remarks></remarks>
        Public Sub setLocalPort(local_Port As Integer)
            _lport = local_Port
        End Sub
        ''' <summary>
        ''' Sets the Remote Port.
        ''' </summary>
        ''' <param name="remote_Port">The Remote Port</param>
        ''' <remarks></remarks>
        Public Sub setRemotePort(remote_Port As Integer)
            _rport = remote_Port
        End Sub
        ''' <summary>
        ''' Gets or sets whether the socket sends and receives data using the length header.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>Whether the socket uses a length header</returns>
        ''' <remarks></remarks>
        Public Property hasLengthHeader As Boolean Implements INetConfig.hasLengthHeader
            Get
                Return _hlh
            End Get
            Set(value As Boolean)
                _hlh = value
            End Set
        End Property
        ''' <summary>
        ''' Allows for supported source settings to be copied to the current INetConfig instance.
        ''' </summary>
        ''' <param name="source">The source to copy</param>
        ''' <remarks></remarks>
        Public Sub copyConfigFrom(source As INetConfig) Implements INetConfig.copyConfigFrom
            _SendBufferSize = source.sendBufferSize
            _ReceiveBufferSize = source.receiveBufferSize
            _NoDelay = source.noDelay
            _receiveTimeout = source.receiveTimeout
            _sendTimeout = source.sendTimeout
            _ExclusiveAddressUse = source.exclusiveAddressUse
            _bl = source.connectionBacklog
            _rip = source.remoteIPAddress
            _rport = source.remotePort
            _lip = source.localIPAddress
            _lport = source.localPort
            _hlh = source.hasLengthHeader
        End Sub
        ''' <summary>
        ''' Allows for a safe NetSocketConfig containing duplicated supported configuration to be returned without exceptions.
        ''' </summary>
        ''' <returns>The safe NetSocketConfig with duplicated supported configuration</returns>
        ''' <remarks></remarks>
        Public Function getSafeSocketConfig() As NetSocketConfig Implements INetConfig.getSafeSocketConfig
            Return Me
        End Function
    End Structure

End Namespace 
