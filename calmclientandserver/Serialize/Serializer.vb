Imports System.Runtime.Serialization.Formatters.Binary
Imports System.IO
Imports System.Runtime.Serialization
Imports System.Security

Namespace Serialize

    ''' <summary>
    ''' Defines the Serialization Class.
    ''' </summary>
    ''' <remarks></remarks>
    Public NotInheritable Class Serializer
        Implements ISerialize

        Private formatter As BinaryFormatter = Nothing
        Private slock As Object = Nothing
        ''' <summary>
        ''' Constructs a new instance of the Serializer Class.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            slock = New Object()
            formatter = New BinaryFormatter()
        End Sub
        ''' <summary>
        ''' Constructs a new instance of the Serializer Class, with a Surrogate Selector and streaming context.
        ''' </summary>
        ''' <param name="selector">The Surrogate Selector</param>
        ''' <param name="context">The Streaming Context</param>
        ''' <remarks></remarks>
        Public Sub New(selector As ISurrogateSelector, context As StreamingContext)
            slock = New Object()
            formatter = New BinaryFormatter(selector, context)
        End Sub
        ''' <summary>
        ''' Constructs a new instance of the Serializer Class, with a Binary Formatter Instance.
        ''' </summary>
        ''' <param name="format"></param>
        ''' <remarks></remarks>
        Public Sub New(format As BinaryFormatter)
            slock = New Object()
            formatter = format
        End Sub
        ''' <summary>
        ''' Serializes an Object to a Byte Array.
        ''' </summary>
        ''' <param name="obj">The object to serialize</param>
        ''' <returns>A byte array</returns>
        ''' <remarks></remarks>
        Public Function serializeObject(obj As Object) As Byte() Implements ISerialize.serializeObject
            If Me.disposedValue Then Throw New ObjectDisposedException("Serializer")
            SyncLock slock
                Try
                    Using ms As New MemoryStream()
                        formatter.Serialize(ms, obj)
                        ms.Flush()
                        Return ms.ToArray()
                    End Using
                Catch ex As Exception When (TypeOf ex Is IOException Or TypeOf ex Is SerializationException Or TypeOf ex Is ArgumentException Or TypeOf ex Is FormatException Or TypeOf ex Is SecurityException)
                End Try
                Return New Byte() {}
            End SyncLock
        End Function
        ''' <summary>
        ''' Serializes an Object to a Byte Array.
        ''' </summary>
        ''' <param name="obj">The object to serialize</param>
        ''' <returns>A byte array</returns>
        ''' <typeparam name="t">The Type of Object to Accept as a parameter</typeparam>
        ''' <remarks></remarks>
        Public Function serializeObject(Of t)(obj As t) As Byte() Implements ISerialize.serializeObject
            If Me.disposedValue Then Throw New ObjectDisposedException("Serializer")
            SyncLock slock
                Try
                    Using ms As New MemoryStream()
                        formatter.Serialize(ms, obj)
                        ms.Flush()
                        Return ms.ToArray()
                    End Using
                Catch ex As Exception When (TypeOf ex Is IOException Or TypeOf ex Is SerializationException Or TypeOf ex Is ArgumentException Or TypeOf ex Is FormatException Or TypeOf ex Is SecurityException)
                End Try
                Return New Byte() {}
            End SyncLock
        End Function
        ''' <summary>
        ''' Serializes an Object to a String.
        ''' </summary>
        ''' <param name="obj">The object to serialize</param>
        ''' <returns>A string</returns>
        ''' <remarks></remarks>
        Public Function serialize(obj As Object) As String Implements ISerialize.serialize
            If Me.disposedValue Then Throw New ObjectDisposedException("Serializer")
            SyncLock slock
                Try
                    Using ms As New MemoryStream()
                        formatter.Serialize(ms, obj)
                        ms.Flush()
                        Return Convert.ToBase64String(ms.ToArray())
                    End Using
                Catch ex As Exception When (TypeOf ex Is IOException Or TypeOf ex Is SerializationException Or TypeOf ex Is ArgumentException Or TypeOf ex Is FormatException Or TypeOf ex Is SecurityException)
                End Try
                Return ""
            End SyncLock
        End Function
        ''' <summary>
        ''' Serializes an Object to a String.
        ''' </summary>
        ''' <param name="obj">The object to serialize</param>
        ''' <returns>A string</returns>
        ''' <typeparam name="t">The Type of Object to Accept as a parameter</typeparam>
        ''' <remarks></remarks>
        Public Function serialize(Of t)(obj As t) As String Implements ISerialize.serialize
            If Me.disposedValue Then Throw New ObjectDisposedException("Serializer")
            SyncLock slock
                Try
                    Using ms As New MemoryStream()
                        formatter.Serialize(ms, obj)
                        ms.Flush()
                        Return Convert.ToBase64String(ms.ToArray())
                    End Using
                Catch ex As Exception When (TypeOf ex Is IOException Or TypeOf ex Is SerializationException Or TypeOf ex Is ArgumentException Or TypeOf ex Is FormatException Or TypeOf ex Is SecurityException)
                End Try
                Return ""
            End SyncLock
        End Function
        ''' <summary>
        ''' Deserializes an Object from a byte array.
        ''' </summary>
        ''' <param name="bts">The Byte Array</param>
        ''' <returns>The Object</returns>
        ''' <remarks></remarks>
        Public Function deSerializeObject(bts As Byte()) As Object Implements ISerialize.deSerializeObject
            If Me.disposedValue Then Throw New ObjectDisposedException("Serializer")
            SyncLock slock
                Try
                    Using ms As New MemoryStream(bts)
                        ms.Flush()
                        Return formatter.Deserialize(ms)
                    End Using
                Catch ex As Exception When (TypeOf ex Is IOException Or TypeOf ex Is SerializationException Or TypeOf ex Is ArgumentException Or TypeOf ex Is FormatException Or TypeOf ex Is SecurityException Or TypeOf ex Is InvalidCastException Or TypeOf ex Is OverflowException)
                End Try
                Return Nothing
            End SyncLock
        End Function
        ''' <summary>
        ''' Deserializes an Object from a byte array.
        ''' </summary>
        ''' <param name="bts">The Byte Array</param>
        ''' <returns>The Object</returns>
        ''' <remarks></remarks>
        ''' <typeparam name="t">The Type of Object to Accept as a Return Value</typeparam>
        Public Function deSerializeObject(Of t)(bts As Byte()) As t Implements ISerialize.deSerializeObject
            If Me.disposedValue Then Throw New ObjectDisposedException("Serializer")
            SyncLock slock
                Try
                    Using ms As New MemoryStream(bts)
                        ms.Flush()
                        Return CType(formatter.Deserialize(ms), t)
                    End Using
                Catch ex As Exception When (TypeOf ex Is IOException Or TypeOf ex Is SerializationException Or TypeOf ex Is ArgumentException Or TypeOf ex Is FormatException Or TypeOf ex Is SecurityException Or TypeOf ex Is InvalidCastException Or TypeOf ex Is OverflowException)
                End Try
                Return Nothing
            End SyncLock
        End Function
        ''' <summary>
        ''' Deserializes an Object from a string.
        ''' </summary>
        ''' <param name="str">A String</param>
        ''' <returns>The Object</returns>
        ''' <remarks></remarks>
        Public Function deSerialize(str As String) As Object Implements ISerialize.deSerialize
            If Me.disposedValue Then Throw New ObjectDisposedException("Serializer")
            SyncLock slock
                Try
                    Using ms As New MemoryStream(Convert.FromBase64String(str))
                        ms.Flush()
                        Return formatter.Deserialize(ms)
                    End Using
                Catch ex As Exception When (TypeOf ex Is IOException Or TypeOf ex Is SerializationException Or TypeOf ex Is ArgumentException Or TypeOf ex Is FormatException Or TypeOf ex Is SecurityException Or TypeOf ex Is InvalidCastException Or TypeOf ex Is OverflowException)
                End Try
                Return Nothing
            End SyncLock
        End Function
        ''' <summary>
        ''' Deserializes an Object from a string.
        ''' </summary>
        ''' <param name="str">A String</param>
        ''' <returns>The Object</returns>
        ''' <remarks></remarks>
        ''' <typeparam name="t">The Type of Object to Accept as a Return Value</typeparam>
        Public Function deSerialize(Of t)(str As String) As t Implements ISerialize.deSerialize
            If Me.disposedValue Then Throw New ObjectDisposedException("Serializer")
            SyncLock slock
                Try
                    Using ms As New MemoryStream(Convert.FromBase64String(str))
                        ms.Flush()
                        Return CType(formatter.Deserialize(ms), t)
                    End Using
                Catch ex As Exception When (TypeOf ex Is IOException Or TypeOf ex Is SerializationException Or TypeOf ex Is ArgumentException Or TypeOf ex Is FormatException Or TypeOf ex Is SecurityException Or TypeOf ex Is InvalidCastException Or TypeOf ex Is OverflowException)
                End Try
                Return Nothing
            End SyncLock
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                slock = Nothing
                formatter = Nothing
            End If
            Me.disposedValue = True
        End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        ''' <summary>
        ''' Clears class instance resources.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
        End Sub
#End Region

    End Class

End Namespace