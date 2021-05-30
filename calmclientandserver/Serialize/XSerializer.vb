Imports System.IO
Imports System.Xml.Serialization

Namespace Serialize
    ''' <summary>
    ''' Defines the XML Serialization Class.
    ''' </summary>
    ''' <remarks></remarks>
    Public NotInheritable Class XSerializer
        Implements ISerialize

        Private slock As Object = Nothing
        ''' <summary>
        ''' Constructs a new instance of the XSerializer Class.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            slock = New Object()
        End Sub
        ''' <summary>
        ''' Deserializes an Object from a string.
        ''' No type provided is not supported.
        ''' </summary>
        ''' <param name="str">A String</param>
        ''' <returns>The Object</returns>
        ''' <remarks></remarks>
        Public Function deSerialize(str As String) As Object Implements ISerialize.deSerialize
            If Me.disposedValue Then Throw New ObjectDisposedException("XSerializer")
            Throw New NotSupportedException("No type provided is not supported.")
        End Function
        ''' <summary>
        ''' Deserializes an Object from a string.
        ''' </summary>
        ''' <param name="str">A String</param>
        ''' <returns>The Object</returns>
        ''' <remarks></remarks>
        ''' <typeparam name="t">The Type of Object to Accept as a Return Value</typeparam>
        Public Function deSerialize(Of t)(str As String) As t Implements ISerialize.deSerialize
            If Me.disposedValue Then Throw New ObjectDisposedException("XSerializer")
            SyncLock slock
                Try
                    Dim xformatter As New XmlSerializer(GetType(t))
                    Using ms As New MemoryStream()
                        Using sw As New StreamWriter(ms)
                            sw.AutoFlush = True
                            For i As Integer = 0 To str.Length - 1 Step 1
                                sw.Write(str(i))
                            Next
                            If (Not sw.AutoFlush) Then sw.Flush()
                            sw.AutoFlush = False
                            ms.Position = 0
                            Return CType(xformatter.Deserialize(ms), t)
                        End Using
                    End Using
                Catch ex As Exception When (TypeOf ex Is System.Text.EncoderFallbackException Or TypeOf ex Is ObjectDisposedException Or TypeOf ex Is IOException Or TypeOf ex Is NotSupportedException Or TypeOf ex Is ArgumentException Or TypeOf ex Is InvalidCastException Or TypeOf ex Is FormatException Or TypeOf ex Is OverflowException)
                End Try
                Return Nothing
            End SyncLock
        End Function
        ''' <summary>
        ''' Deserializes an Object from a byte array.
        ''' No type provided is not supported.
        ''' </summary>
        ''' <param name="bts">The Byte Array</param>
        ''' <returns>The Object</returns>
        ''' <remarks></remarks>
        Public Function deSerializeObject(bts() As Byte) As Object Implements ISerialize.deSerializeObject
            If Me.disposedValue Then Throw New ObjectDisposedException("XSerializer")
            Throw New NotSupportedException("No type provided is not supported.")
        End Function
        ''' <summary>
        ''' Deserializes an Object from a byte array.
        ''' </summary>
        ''' <param name="bts">The Byte Array</param>
        ''' <returns>The Object</returns>
        ''' <remarks></remarks>
        ''' <typeparam name="t">The Type of Object to Accept as a Return Value</typeparam>
        Public Function deSerializeObject(Of t)(bts() As Byte) As t Implements ISerialize.deSerializeObject
            If Me.disposedValue Then Throw New ObjectDisposedException("XSerializer")
            SyncLock slock
                Try
                    Dim xformatter As New XmlSerializer(GetType(t))
                    Using ms As New MemoryStream(bts)
                        ms.Flush()
                        Return CType(xformatter.Deserialize(ms), t)
                    End Using
                Catch ex As Exception When (TypeOf ex Is System.Text.EncoderFallbackException Or TypeOf ex Is ObjectDisposedException Or TypeOf ex Is IOException Or TypeOf ex Is NotSupportedException Or TypeOf ex Is ArgumentException Or TypeOf ex Is InvalidCastException Or TypeOf ex Is FormatException Or TypeOf ex Is OverflowException)
                End Try
                Return Nothing
            End SyncLock
        End Function
        ''' <summary>
        ''' Serializes an Object to a String.
        ''' No type provided is not supported.
        ''' </summary>
        ''' <param name="obj">The object to serialize</param>
        ''' <returns>A string</returns>
        ''' <remarks></remarks>
        Public Function serialize(obj As Object) As String Implements ISerialize.serialize
            If Me.disposedValue Then Throw New ObjectDisposedException("XSerializer")
            Throw New NotSupportedException("No type provided is not supported.")
        End Function
        ''' <summary>
        ''' Serializes an Object to a String.
        ''' </summary>
        ''' <param name="obj">The object to serialize</param>
        ''' <returns>A string</returns>
        ''' <typeparam name="t">The Type of Object to Accept as a parameter</typeparam>
        ''' <remarks></remarks>
        Public Function serialize(Of t)(obj As t) As String Implements ISerialize.serialize
            If Me.disposedValue Then Throw New ObjectDisposedException("XSerializer")
            SyncLock slock
                Try
                    Dim xformatter As New XmlSerializer(GetType(t))
                    Using ms As New MemoryStream()
                        Using sw As New StreamWriter(ms), sr As New StreamReader(ms)
                            xformatter.Serialize(sw, obj)
                            ms.Position = 0
                            Return sr.ReadToEnd()
                        End Using
                    End Using
                Catch ex As Exception When (TypeOf ex Is ObjectDisposedException Or TypeOf ex Is IOException Or TypeOf ex Is OutOfMemoryException Or TypeOf ex Is ArgumentException)
                End Try
                Return ""
            End SyncLock
        End Function
        ''' <summary>
        ''' Serializes an Object to a Byte Array.
        ''' </summary>
        ''' <param name="obj">The object to serialize</param>
        ''' <returns>A byte array</returns>
        ''' <remarks></remarks>
        Public Function serializeObject(obj As Object) As Byte() Implements ISerialize.serializeObject
            If Me.disposedValue Then Throw New ObjectDisposedException("XSerializer")
            Throw New NotSupportedException("No type provided is not supported.")
        End Function
        ''' <summary>
        ''' Serializes an Object to a Byte Array.
        ''' No type provided is not supported.
        ''' </summary>
        ''' <param name="obj">The object to serialize</param>
        ''' <returns>A byte array</returns>
        ''' <typeparam name="t">The Type of Object to Accept as a parameter</typeparam>
        ''' <remarks></remarks>
        Public Function serializeObject(Of t)(obj As t) As Byte() Implements ISerialize.serializeObject
            If Me.disposedValue Then Throw New ObjectDisposedException("XSerializer")
            SyncLock slock
                Try
                    Dim xformatter As New XmlSerializer(GetType(t))
                    Using ms As New MemoryStream()
                        Using sw As New StreamWriter(ms)
                            xformatter.Serialize(sw, obj)
                            Return ms.ToArray()
                        End Using
                    End Using
                Catch ex As Exception When (TypeOf ex Is ObjectDisposedException Or TypeOf ex Is IOException Or TypeOf ex Is OutOfMemoryException Or TypeOf ex Is ArgumentException)
                End Try
                Return New Byte() {}
            End SyncLock
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                slock = Nothing
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
