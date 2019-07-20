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
    ''' The CALMNetLib NetLibException.
	''' </summary>
	Public Class NetLibException
		Inherits Exception
		Implements ISerializable
		
		Protected _inexcnom As String = ""
        ''' <summary>
        ''' Constructs a new instance of a NetLibException.
        ''' </summary>
        ''' <remarks></remarks>
		Public Sub New()
		End Sub
        ''' <summary>
        ''' Constructs a new instance of a NetLibException with the specified message.
        ''' </summary>
        ''' <param name="message">The message to store</param>
        ''' <remarks></remarks>
		Public Sub New(message As String)
			MyBase.New(message)
		End Sub
        ''' <summary>
        ''' Constructs a new instance of a NetLibException with the specified message and inner Exception.
        ''' </summary>
        ''' <param name="message">The message to store</param>
        ''' <param name="innerException">The inner Exception to store</param>
        ''' <remarks></remarks>
		Public Sub New(message As String, innerException As Exception)
			MyBase.New(message, innerException)
			_inexcnom = innerException.GetType().Name
		End Sub
        ''' <summary>
        ''' Constructs a new instance of a NetLibException with an inner Exception.
        ''' </summary>
        ''' <param name="innerException">The inner Exception to store</param>
        ''' <remarks></remarks>
		Public Sub New(innerException As Exception)
			MyBase.New(innerException.Message, innerException)
			_inexcnom = innerException.GetType().Name
		End Sub

		' This constructor is needed for serialization.
		Protected Sub New(info As SerializationInfo, context As StreamingContext)
			MyBase.New(info, context)
		End Sub
        ''' <summary>
        ''' Returns the Name of the InnerException.
        ''' </summary>
        ''' <value>String</value>
        ''' <returns>The Name of the inner Exception</returns>
        ''' <remarks></remarks>
		Public ReadOnly Overridable Property innerExceptionName As String
			Get
				Return _inexcnom
			End Get
		End Property
	End Class
End Namespace