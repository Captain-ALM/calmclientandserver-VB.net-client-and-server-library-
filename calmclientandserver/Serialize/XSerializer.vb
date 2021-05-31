Imports System.IO
Imports System.Xml
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
        ''' </summary>
        ''' <param name="str">A String</param>
        ''' <returns>The Object</returns>
        ''' <remarks></remarks>
        Public Function deSerialize(str As String) As Object Implements ISerialize.deSerialize
            If Me.disposedValue Then Throw New ObjectDisposedException("XSerializer")
            SyncLock slock
                Try
                    Dim xformatter As New XmlSerializer(getTypeDefiningAttribute(stringToXMLDoc(str)))
                    Using ms As New MemoryStream()
                        Using sw As New StreamWriter(ms)
                            sw.AutoFlush = True
                            For i As Integer = 0 To str.Length - 1 Step 1
                                sw.Write(str(i))
                            Next
                            If (Not sw.AutoFlush) Then sw.Flush()
                            sw.AutoFlush = False
                            ms.Position = 0
                            Return xformatter.Deserialize(ms)
                        End Using
                    End Using
                Catch ex As Exception When (TypeOf ex Is System.Text.EncoderFallbackException Or TypeOf ex Is ObjectDisposedException Or TypeOf ex Is IOException Or TypeOf ex Is NotSupportedException Or TypeOf ex Is ArgumentException Or TypeOf ex Is InvalidCastException Or TypeOf ex Is FormatException Or TypeOf ex Is OverflowException Or TypeOf ex Is InvalidOperationException Or TypeOf ex Is XmlException)
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
            If Me.disposedValue Then Throw New ObjectDisposedException("XSerializer")
            SyncLock slock
                Try
                    Dim xformatter As New XmlSerializer(getTypeDefiningAttribute(stringToXMLDoc(str)))
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
                Catch ex As Exception When (TypeOf ex Is System.Text.EncoderFallbackException Or TypeOf ex Is ObjectDisposedException Or TypeOf ex Is IOException Or TypeOf ex Is NotSupportedException Or TypeOf ex Is ArgumentException Or TypeOf ex Is InvalidCastException Or TypeOf ex Is FormatException Or TypeOf ex Is OverflowException Or TypeOf ex Is InvalidOperationException Or TypeOf ex Is XmlException)
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
        Public Function deSerializeObject(bts() As Byte) As Object Implements ISerialize.deSerializeObject
            If Me.disposedValue Then Throw New ObjectDisposedException("XSerializer")
            SyncLock slock
                Try
                    'Using xms As New MemoryStream(bts)
                    'Dim xformatter As New XmlSerializer(getTypeDefiningAttribute(streamToXMLDoc(xms)))
                    Using ms As New MemoryStream(bts)
                        Dim xformatter As New XmlSerializer(getTypeDefiningAttribute(streamToXMLDoc(ms)))
                        ms.Position = 0
                        Return xformatter.Deserialize(ms)
                    End Using
                    'End Using
                Catch ex As Exception When (TypeOf ex Is System.Text.EncoderFallbackException Or TypeOf ex Is ObjectDisposedException Or TypeOf ex Is IOException Or TypeOf ex Is NotSupportedException Or TypeOf ex Is ArgumentException Or TypeOf ex Is InvalidCastException Or TypeOf ex Is FormatException Or TypeOf ex Is OverflowException Or TypeOf ex Is InvalidOperationException Or TypeOf ex Is XmlException)
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
        Public Function deSerializeObject(Of t)(bts() As Byte) As t Implements ISerialize.deSerializeObject
            If Me.disposedValue Then Throw New ObjectDisposedException("XSerializer")
            SyncLock slock
                Try
                    'Using xms As New MemoryStream(bts)
                    '    Dim xformatter As New XmlSerializer(getTypeDefiningAttribute(streamToXMLDoc(xms)))
                    Using ms As New MemoryStream(bts)
                        Dim xformatter As New XmlSerializer(getTypeDefiningAttribute(streamToXMLDoc(ms)))
                        ms.Position = 0
                        Return CType(xformatter.Deserialize(ms), t)
                    End Using
                    'End Using
                Catch ex As Exception When (TypeOf ex Is System.Text.EncoderFallbackException Or TypeOf ex Is ObjectDisposedException Or TypeOf ex Is IOException Or TypeOf ex Is NotSupportedException Or TypeOf ex Is ArgumentException Or TypeOf ex Is InvalidCastException Or TypeOf ex Is FormatException Or TypeOf ex Is OverflowException Or TypeOf ex Is InvalidOperationException Or TypeOf ex Is XmlException)
                End Try
                Return Nothing
            End SyncLock
        End Function
        ''' <summary>
        ''' Serializes an Object to a String.
        ''' </summary>
        ''' <param name="obj">The object to serialize</param>
        ''' <returns>A string</returns>
        ''' <remarks></remarks>
        Public Function serialize(obj As Object) As String Implements ISerialize.serialize
            If Me.disposedValue Then Throw New ObjectDisposedException("XSerializer")
            SyncLock slock
                Try
                    Dim xformatter As New XmlSerializer(obj.GetType())
                    Using ms As New MemoryStream()
                        Using sw As New StreamWriter(ms), sr As New StreamReader(ms)
                            xformatter.Serialize(sw, obj)
                            ms.Position = 0
                            Return XMLDocToString(addTypeDefiningAttribute(stringToXMLDoc(sr.ReadToEnd()), obj.GetType()))
                        End Using
                    End Using
                Catch ex As Exception When (TypeOf ex Is ObjectDisposedException Or TypeOf ex Is IOException Or TypeOf ex Is OutOfMemoryException Or TypeOf ex Is ArgumentException Or TypeOf ex Is InvalidOperationException Or TypeOf ex Is XmlException)
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
            If Me.disposedValue Then Throw New ObjectDisposedException("XSerializer")
            SyncLock slock
                Try
                    Dim xformatter As New XmlSerializer(obj.GetType())
                    Using ms As New MemoryStream()
                        Using sw As New StreamWriter(ms), sr As New StreamReader(ms)
                            xformatter.Serialize(sw, obj)
                            ms.Position = 0
                            Return XMLDocToString(addTypeDefiningAttribute(stringToXMLDoc(sr.ReadToEnd()), obj.GetType()))
                        End Using
                    End Using
                Catch ex As Exception When (TypeOf ex Is ObjectDisposedException Or TypeOf ex Is IOException Or TypeOf ex Is OutOfMemoryException Or TypeOf ex Is ArgumentException Or TypeOf ex Is InvalidOperationException Or TypeOf ex Is XmlException)
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
            SyncLock slock
                Try
                    Dim xformatter As New XmlSerializer(obj.GetType())
                    Using ms As New MemoryStream()
                        Using sw As New StreamWriter(ms)
                            xformatter.Serialize(sw, obj)
                            ms.Position = 0
                            Using rms As New MemoryStream()
                                XMLDocToStream(addTypeDefiningAttribute(streamToXMLDoc(ms), obj.GetType()), rms)
                                Return rms.ToArray()
                            End Using
                        End Using
                    End Using
                Catch ex As Exception When (TypeOf ex Is ObjectDisposedException Or TypeOf ex Is IOException Or TypeOf ex Is OutOfMemoryException Or TypeOf ex Is ArgumentException Or TypeOf ex Is InvalidOperationException Or TypeOf ex Is XmlException)
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
            If Me.disposedValue Then Throw New ObjectDisposedException("XSerializer")
            SyncLock slock
                Try
                    Dim xformatter As New XmlSerializer(obj.GetType())
                    Using ms As New MemoryStream()
                        Using sw As New StreamWriter(ms)
                            xformatter.Serialize(sw, obj)
                            ms.Position = 0
                            Using rms As New MemoryStream()
                                XMLDocToStream(addTypeDefiningAttribute(streamToXMLDoc(ms), obj.GetType()), rms)
                                Return rms.ToArray()
                            End Using
                        End Using
                    End Using
                Catch ex As Exception When (TypeOf ex Is ObjectDisposedException Or TypeOf ex Is IOException Or TypeOf ex Is OutOfMemoryException Or TypeOf ex Is ArgumentException Or TypeOf ex Is InvalidOperationException Or TypeOf ex Is XmlException)
                End Try
                Return New Byte() {}
            End SyncLock
        End Function

        Private Function stringToXMLDoc(xmlstr As String) As XmlDocument
            Dim xdoc As New XmlDocument()
            xdoc.LoadXml(xmlstr)
            Return xdoc
        End Function

        Private Function streamToXMLDoc(strm As Stream) As XmlDocument
            Dim xdoc As New XmlDocument()
            xdoc.Load(strm)
            Return xdoc
        End Function

        Private Function XMLDocToString(xd As XmlDocument) As String
            Return xd.OuterXml
        End Function

        Private Sub XMLDocToStream(xd As XmlDocument, strm As Stream)
            xd.Save(strm)
        End Sub

        Private Function addTypeDefiningAttribute(xd As XmlDocument, typeIn As Type) As XmlDocument
            Dim xel As XmlElement = xd.DocumentElement
            If Not xel.HasAttribute("XSerializerType") Then xel.SetAttribute("XSerializerType", typeIn.FullName & ", " & typeIn.Assembly.GetName().Name)
            Return xd
        End Function

        Private Function getTypeDefiningAttribute(xd As XmlDocument) As Type
            Dim xel As XmlElement = xd.DocumentElement
            Try
                If xel.HasAttribute("XSerializerType") Then Return Type.GetType(xel.GetAttribute("XSerializerType"))
            Catch ex As Exception When (TypeOf ex Is Reflection.TargetInvocationException Or TypeOf ex Is ArgumentException Or TypeOf ex Is TypeLoadException Or TypeOf ex Is IO.FileLoadException Or TypeOf ex Is BadImageFormatException)
            End Try
            Return GetType(Object)
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
