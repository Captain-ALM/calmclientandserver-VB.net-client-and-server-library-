'
' Created by SharpDevelop.
' User: Alfred
' Date: 22/05/2019
' Time: 17:46
' 
' To change this template use Tools | Options | Coding | Edit Standard Headers.
'
Namespace CALMNetLib
	
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
		
		Public Sub New(local_IPAddress As String, local_Port As Integer, remote_IPAddress As String, remote_Port As Integer)
			_lip = local_IPAddress
			_lport = local_Port
			_rip = remote_IPAddress
			_rport = remote_Port
		End Sub
		
		Public Sub New(conf As INetConfig)
			Me.New(conf, True)
		End Sub
		
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
		
		Public Property sendBufferSize As Integer Implements INetConfig.sendBufferSize
			Get
				Return _SendBufferSize
			End Get
			Set(value As Integer)
				_SendBufferSize = value
			End Set
		End Property
		
		Public Property receiveBufferSize As Integer Implements INetConfig.receiveBufferSize
			Get
				Return _ReceiveBufferSize
			End Get
			Set(value As Integer)
				_ReceiveBufferSize = value
			End Set
		End Property
		
		Public Property noDelay As Boolean Implements INetConfig.noDelay
			Get
				Return _NoDelay
			End Get
			Set(value As Boolean)
				_NoDelay = value
			End Set
		End Property
		
		Public Property receiveTimeout As Integer Implements INetConfig.receiveTimeout
			Get
				Return _ReceiveTimeout
			End Get
			Set(value As Integer)
				_ReceiveTimeout = value
			End Set
		End Property
		
		Public Property sendTimeout As Integer Implements INetConfig.sendTimeout
			Get
				Return _SendTimeout
			End Get
			Set(value As Integer)
				_SendTimeout = value
			End Set
		End Property
		
		Public ReadOnly Property localIPAddress As String Implements INetConfig.localIPAddress
			Get
				Return _lip
			End Get
		End Property
		
		Public ReadOnly Property localPort As Integer Implements INetConfig.localPort
			Get
				Return _lport
			End Get
		End Property
		
		Public ReadOnly Property remoteIPAddress As String Implements INetConfig.remoteIPAddress
			Get
				Return _rip
			End Get
		End Property
		
		Public ReadOnly Property remotePort As Integer Implements INetConfig.remotePort
			Get
				Return _rport
			End Get
		End Property
		
		Public Property exclusiveAddressUse As Boolean Implements INetConfig.exclusiveAddressUse
			Get
				Return _ExclusiveAddressUse
			End Get
			Set(value As Boolean)
				_ExclusiveAddressUse = value
			End Set
		End Property
		
		Public Property connectionBacklog As Integer Implements INetConfig.connectionBacklog
			Get
				Return _bl
			End Get
			Set(value As Integer)
				_bl = value
			End Set
		End Property
		
		Public Sub DuplicateConfigTo(ByRef conf As INetConfig)
			DuplicateConfigTo(conf, True)
		End Sub
		
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
		
		Public Sub setLocalIPAddress(local_IPAddress As String)
			_lip = local_IPAddress
		End Sub
		
		Public Sub setRemoteIPAddress(remote_IPAddress As String)
			_rip = remote_IPAddress
		End Sub
		
		Public Sub setLocalPort(local_Port As Integer)
			_lport = local_Port
		End Sub
		
		Public Sub setRemotePort(remote_Port As Integer)
			_rport = remote_Port
		End Sub
	End Structure
	
End Namespace 
