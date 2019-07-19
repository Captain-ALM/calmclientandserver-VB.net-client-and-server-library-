'
' Created by SharpDevelop.
' User: Alfred
' Date: 22/05/2019
' Time: 18:02
' 
' To change this template use Tools | Options | Coding | Edit Standard Headers.
'
Imports System
Imports System.Runtime.Serialization

Namespace CALMNetLib
	''' <summary>
	''' Desctiption of NetLibException.
	''' </summary>
	Public Class NetLibException
		Inherits Exception
		Implements ISerializable
		
		Protected _inexcnom As String = ""

		Public Sub New()
		End Sub

		Public Sub New(message As String)
			MyBase.New(message)
		End Sub

		Public Sub New(message As String, innerException As Exception)
			MyBase.New(message, innerException)
			_inexcnom = innerException.GetType().Name
		End Sub
		
		Public Sub New(innerException As Exception)
			MyBase.New(innerException.Message, innerException)
			_inexcnom = innerException.GetType().Name
		End Sub

		' This constructor is needed for serialization.
		Protected Sub New(info As SerializationInfo, context As StreamingContext)
			MyBase.New(info, context)
		End Sub
		
		Public ReadOnly Overridable Property innerExceptionName As String
			Get
				Return _inexcnom
			End Get
		End Property
	End Class
End Namespace