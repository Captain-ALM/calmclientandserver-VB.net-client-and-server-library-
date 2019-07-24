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
        ''' </summary>
        ''' <param name="conf">The INetConfig Configuration</param>
        ''' <remarks></remarks>
		Public Sub New(conf As INetConfig)
			Me.New(conf, True)
		End Sub
        ''' <summary>
        ''' Constructs a New NetSocketConfig Structure copying the configuration of another INetConfig Implementation and whether thrown NetLibExceptions are to be caught.
        ''' </summary>
        ''' <param name="conf">The INetConfig Configuration</param>
        ''' <param name="catchNetLibExceptions">Whether NetLibExceptions are to be caught</param>
        ''' <remarks></remarks>
		Public Sub New(conf As INetConfig, catchNetLibExceptions As Boolean)
			If catchNetLibExceptions Then
				Try
					_SendBufferSize = conf.sendBufferSize
				Catch ex As NetLibException
					Utilities.addException(ex)
				End Try
				Try
					_ReceiveBufferSize = conf.receiveBufferSize
				Catch ex As NetLibException
					Utilities.addException(ex)
				End Try
				Try
					_NoDelay = conf.noDelay
				Catch ex As NetLibException
					Utilities.addException(ex)
				End Try
				Try
					_receiveTimeout = conf.receiveTimeout
				Catch ex As NetLibException
					Utilities.addException(ex)
				End Try
				Try
					_sendTimeout = conf.sendTimeout
				Catch ex As NetLibException
					Utilities.addException(ex)
				End Try
				Try
					_ExclusiveAddressUse = conf.exclusiveAddressUse
				Catch ex As NetLibException
					Utilities.addException(ex)
				End Try
				Try
					_bl = conf.connectionBacklog
				Catch ex As NetLibException
					Utilities.addException(ex)
				End Try
				Try
					_rip = conf.remoteIPAddress
				Catch ex As NetLibException
					Utilities.addException(ex)
				End Try
				Try
					_rport = conf.remotePort
				Catch ex As NetLibException
					Utilities.addException(ex)
				End Try
				Try
					_lip = conf.localIPAddress
				Catch ex As NetLibException
					Utilities.addException(ex)
				End Try
				Try
					_lport = conf.localPort
				Catch ex As NetLibException
					Utilities.addException(ex)
				End Try
			Else
				_SendBufferSize = conf.sendBufferSize
				_ReceiveBufferSize = conf.receiveBufferSize
				_NoDelay = conf.noDelay
				_receiveTimeout = conf.receiveTimeout
				_sendTimeout = conf.sendTimeout
				_ExclusiveAddressUse = conf.exclusiveAddressUse
				_bl = conf.connectionBacklog
				_rip = conf.remoteIPAddress
				_rport = conf.remotePort
				_lip = conf.localIPAddress
				_lport = conf.localPort
			End If
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
				Return _ReceiveTimeout
			End Get
			Set(value As Integer)
				_ReceiveTimeout = value
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
				Return _SendTimeout
			End Get
			Set(value As Integer)
				_SendTimeout = value
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
        ''' Duplicates the Structure's Configuration to another INetConfig interface implementation.
        ''' </summary>
        ''' <param name="conf">The INetConfig configuration implementation</param>
        ''' <remarks></remarks>
		Public Sub DuplicateConfigTo(ByRef conf As INetConfig)
			DuplicateConfigTo(conf, True)
		End Sub
        ''' <summary>
        ''' Duplicates the Structure's Configuration to another INetConfig interface implementation and whether thrown NetLibExceptions are to be caught.
        ''' </summary>
        ''' <param name="conf">The INetConfig configuration implementation</param>
        ''' <param name="catchNetLibExceptions">Whether NetLibExceptions are to be thrown</param>
        ''' <remarks></remarks>
		Public Sub DuplicateConfigTo(ByRef conf As INetConfig, catchNetLibExceptions As Boolean)
			If catchNetLibExceptions Then
				Try
					conf.sendBufferSize = _SendBufferSize
				Catch ex As NetLibException
					Utilities.addException(ex)
				End Try
				Try
					conf.receiveBufferSize = _ReceiveBufferSize
				Catch ex As NetLibException
					Utilities.addException(ex)
				End Try
				Try
					conf.noDelay = _NoDelay
				Catch ex As NetLibException
					Utilities.addException(ex)
				End Try
				Try
					conf.receiveTimeout = _receiveTimeout
				Catch ex As NetLibException
					Utilities.addException(ex)
				End Try
				Try
					conf.sendTimeout = _sendTimeout
				Catch ex As NetLibException
					Utilities.addException(ex)
				End Try
				Try
					conf.exclusiveAddressUse = _ExclusiveAddressUse
				Catch ex As NetLibException
					Utilities.addException(ex)
				End Try
				Try
					conf.connectionBacklog = _bl
				Catch ex As NetLibException
					Utilities.addException(ex)
				End Try
			Else
				conf.sendBufferSize = _SendBufferSize
				conf.receiveBufferSize = _ReceiveBufferSize
				conf.noDelay = _NoDelay
				conf.receiveTimeout = _receiveTimeout
				conf.sendTimeout = _sendTimeout
				conf.exclusiveAddressUse = _ExclusiveAddressUse
				conf.connectionBacklog = _bl
			End If
		End Sub
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
	End Structure
	
End Namespace 
