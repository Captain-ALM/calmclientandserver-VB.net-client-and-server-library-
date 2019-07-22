﻿Imports System.Net
Imports System.Net.Sockets
Imports System.Net.NetworkInformation
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Runtime.Serialization
Imports System.IO
Imports System.Security
Imports System.Windows.Forms

'
' Created by SharpDevelop.
' User: Alfred
' Date: 21/05/2019
' Time: 16:03
' 
' To change this template use Tools | Options | Coding | Edit Standard Headers.
'
Namespace CALMNetLib
    ''' <summary>
    ''' Defines a set of shared utility functions.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Utilities

        Protected Shared logexc As Boolean = False
        Protected Shared exclog As List(Of NetLibException)
        Protected Shared slockexclog As New Object()
        ''' <summary>
        ''' Converts a 16 Bit Integer to a byte array.
        ''' </summary>
        ''' <param name="intIn">The 16 Bit Integer</param>
        ''' <returns>A Byte Array</returns>
        ''' <remarks></remarks>
        Public Shared Function Int16ToBytes(intIn As Int16) As Byte()
            Dim bts(1) As Byte
            Dim cval As Int16 = intIn
            bts(0) = cval \ 256
            cval -= (bts(0) * 256)
            bts(1) = cval
            Return bts
        End Function
        ''' <summary>
        ''' Converts a byte array to a 16 Bit Integer.
        ''' </summary>
        ''' <param name="bytesIn">A Byte Array</param>
        ''' <returns>The 16 Bit Integer it Represents</returns>
        ''' <remarks></remarks>
        Public Shared Function BytesToInt16(bytesIn As Byte()) As Int16
            If bytesIn.Length <> 2 Then Throw New NetLibException(New ArgumentOutOfRangeException("The byte array is not 2 bytes."))
            Dim cval As Int16 = (bytesIn(0) * 256) + bytesIn(1)
            Return cval
        End Function
        ''' <summary>
        ''' Converts a 32 Bit Integer to a byte array.
        ''' </summary>
        ''' <param name="intIn">The 32 Bit Integer</param>
        ''' <returns>A Byte Array</returns>
        ''' <remarks></remarks>
        Public Shared Function Int32ToBytes(intIn As Int32) As Byte()
            Dim bts(3) As Byte
            Dim cval As Int32 = intIn
            bts(0) = cval \ 16777216
            cval -= (bts(0) * 16777216)
            bts(1) = cval \ 65536
            cval -= (bts(1) * 65536)
            bts(2) = cval \ 256
            cval -= (bts(2) * 256)
            bts(3) = cval
            Return bts
        End Function
        ''' <summary>
        ''' Converts a byte array to a 32 Bit Integer.
        ''' </summary>
        ''' <param name="bytesIn">A Byte Array</param>
        ''' <returns>The 32 Bit Integer it Represents</returns>
        ''' <remarks></remarks>
        Public Shared Function BytesToInt32(bytesIn As Byte()) As Int32
            If bytesIn.Length <> 4 Then Throw New NetLibException(New ArgumentOutOfRangeException("The byte array is not 4 bytes."))
            Dim cval As Int32 = (bytesIn(0) * 16777216) + (bytesIn(1) * 65536) + (bytesIn(2) * 256) + bytesIn(3)
            Return cval
        End Function
        ''' <summary>
        ''' Converts a 64 Bit Integer to a byte array.
        ''' </summary>
        ''' <param name="intIn">The 64 Bit Integer</param>
        ''' <returns>A Byte Array</returns>
        ''' <remarks></remarks>
        Public Shared Function Int64ToBytes(intIn As Int64) As Byte()
            Dim bts(3) As Byte
            Dim cval As Int64 = intIn
            bts(0) = cval \ 72057594037927940
            cval -= (bts(0) * 72057594037927940)
            bts(1) = cval \ 281474976710656
            cval -= (bts(1) * 281474976710656)
            bts(2) = cval \ 1099511627776
            cval -= (bts(2) * 1099511627776)
            bts(3) = cval \ 4294967296
            cval -= (bts(3) * 4294967296)
            bts(4) = cval \ 16777216
            cval -= (bts(4) * 16777216)
            bts(5) = cval \ 65536
            cval -= (bts(5) * 65536)
            bts(6) = cval \ 256
            cval -= (bts(6) * 256)
            bts(7) = cval
            Return bts
        End Function
        ''' <summary>
        ''' Converts a byte array to a 64 Bit Integer.
        ''' </summary>
        ''' <param name="bytesIn">A Byte Array</param>
        ''' <returns>The 64 Bit Integer it Represents</returns>
        ''' <remarks></remarks>
        Public Shared Function BytesToInt64(bytesIn As Byte()) As Int64
            If bytesIn.Length <> 8 Then Throw New NetLibException(New ArgumentOutOfRangeException("The byte array is not 8 bytes."))
            Dim cval As Int64 = (bytesIn(0) * 72057594037927940) + (bytesIn(1) * 281474976710656) + (bytesIn(2) * 1099511627776) + (bytesIn(3) * 4294967296) + (bytesIn(4) * 16777216) + (bytesIn(5) * 65536) + (bytesIn(6) * 256) + bytesIn(7)
            Return cval
        End Function
        ''' <summary>
        ''' Checks if a TCP Port on the specified IP Address is accepting connections.
        ''' </summary>
        ''' <param name="IPAddressIn">The IP Address to check</param>
        ''' <param name="portIn">The Port to check</param>
        ''' <returns>If the specified endpoint can be connected to</returns>
        ''' <remarks></remarks>
        Public Shared Function TCPPortOpen(IPAddressIn As IPAddress, portIn As Integer) As Boolean
            Dim sock As Socket = Nothing
            Try
                sock = New Socket(IPAddressIn.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
                sock.Connect(New IPEndPoint(IPAddressIn, portIn))
                If sock.Connected Then
                    sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, New LingerOption(False, 0))
                    sock.Close(0)
                    Return True
                End If
            Catch ex As SocketException
            Finally
                If sock IsNot Nothing Then sock.Close()
            End Try
            Return False
        End Function
        ''' <summary>
        ''' Returns the IP Addresses of all the Network Interfaces on the machine.
        ''' </summary>
        ''' <returns>An array of the IP Addresses of all the Network Interfaces</returns>
        ''' <remarks></remarks>
        Public Shared Function GetIPInterfaces() As IPAddress()
            Dim list As New List(Of IPAddress)
            Dim allNetworkInterfaces As NetworkInterface() = NetworkInterface.GetAllNetworkInterfaces()
            For i As Integer = 0 To allNetworkInterfaces.Length - 1
                Dim networkInterfacec As NetworkInterface = allNetworkInterfaces(i)
                If networkInterfacec.OperationalStatus = OperationalStatus.Up And NetworkInterface.GetIsNetworkAvailable() Then
                    For Each current As UnicastIPAddressInformation In networkInterfacec.GetIPProperties().UnicastAddresses
                        If current.Address.AddressFamily = AddressFamily.InterNetwork Or current.Address.AddressFamily = AddressFamily.InterNetworkV6 Then
                            list.Add(current.Address)
                        End If
                    Next
                End If
            Next
            Return list.ToArray()
        End Function

        ''' <summary>
        ''' Returns the IP Address and the Name of the Network Interface of all the Network Interfaces on the machine.
        ''' </summary>
        ''' <returns>An array of a Two Type (Pair) Tuple containing the Name of the Network Interface and the IP Address</returns>
        ''' <remarks></remarks>
        Public Shared Function GetIPInterfacesAndNames() As Tuple(Of String, IPAddress)()
            Dim list As New List(Of Tuple(Of String, IPAddress))
            Dim allNetworkInterfaces As NetworkInterface() = NetworkInterface.GetAllNetworkInterfaces()
            For i As Integer = 0 To allNetworkInterfaces.Length - 1
                Dim networkInterfacec As NetworkInterface = allNetworkInterfaces(i)
                If networkInterfacec.OperationalStatus = OperationalStatus.Up And NetworkInterface.GetIsNetworkAvailable() Then
                    For Each current As UnicastIPAddressInformation In networkInterfacec.GetIPProperties().UnicastAddresses
                        If current.Address.AddressFamily = AddressFamily.InterNetwork Or current.Address.AddressFamily = AddressFamily.InterNetworkV6 Then
                            list.Add(New Tuple(Of String, IPAddress)(networkInterfacec.Name, current.Address))
                        End If
                    Next
                End If
            Next
            Return list.ToArray()
        End Function
        ''' <summary>
        ''' Gets or sets whether internally handled NetLibExceptions are to be logged by this class.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Property logInternallyHandledNetLibExceptions As Boolean
            Get
                Return logexc
            End Get
            Set(value As Boolean)
                logexc = value
            End Set
        End Property
        ''' <summary>
        ''' Returns the Log array of the Internally Handled NetLibExceptions.
        ''' </summary>
        ''' <value>An array of NetLibExceptions</value>
        ''' <returns>The Log of Internally Handled NetLibExceptions</returns>
        ''' <remarks></remarks>
        Public Shared ReadOnly Property getInternallyHandledNetLibExceptions As NetLibException()
            Get
                Dim toret As NetLibException() = Nothing
                SyncLock slockexclog
                    toret = exclog.ToArray()
                End SyncLock
                Return toret
            End Get
        End Property

        Friend Shared Sub addException(ex As NetLibException)
            SyncLock slockexclog
                If logexc Then exclog.Add(ex)
            End SyncLock
        End Sub
        ''' <summary>
        ''' Clears the Internally Handled NetLibException Log.
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared Sub clearInternallyHandledNetLibExceptions()
            SyncLock slockexclog
                exclog.Clear()
            End SyncLock
        End Sub
    End Class
    ''' <summary>
    ''' Defines the Serialization Class.
    ''' </summary>
    ''' <remarks></remarks>
    Public NotInheritable Class Serializer
        Implements IDisposable

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
        Public Function serializeObject(obj As Object) As Byte()
            SyncLock slock
                Dim ms As New MemoryStream()
                Try
                    formatter.Serialize(ms, obj)
                    ms.Flush()
                    Dim bts As Byte() = ms.ToArray()
                    ms.Dispose()
                    ms = Nothing
                    Return bts
                Catch ex As Exception When (TypeOf ex Is IOException Or TypeOf ex Is SerializationException Or TypeOf ex Is ArgumentNullException Or TypeOf ex Is FormatException Or TypeOf ex Is SecurityException)
                Finally
                    If ms IsNot Nothing Then
                        ms.Dispose()
                        ms = Nothing
                    End If
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
        Public Function serializeObject(Of t)(obj As t) As Byte()
            SyncLock slock
                Dim ms As New MemoryStream()
                Try
                    formatter.Serialize(ms, obj)
                    ms.Flush()
                    Dim bts As Byte() = ms.ToArray()
                    ms.Dispose()
                    ms = Nothing
                    Return bts
                Catch ex As Exception When (TypeOf ex Is IOException Or TypeOf ex Is SerializationException Or TypeOf ex Is ArgumentNullException Or TypeOf ex Is FormatException Or TypeOf ex Is SecurityException)
                Finally
                    If ms IsNot Nothing Then
                        ms.Dispose()
                        ms = Nothing
                    End If
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
        Public Function serialize(obj As Object) As String
            SyncLock slock
                Dim ms As New MemoryStream()
                Try
                    formatter.Serialize(ms, obj)
                    ms.Flush()
                    Dim bts As Byte() = ms.ToArray()
                    ms.Dispose()
                    ms = Nothing
                    Return Convert.ToBase64String(bts)
                Catch ex As Exception When (TypeOf ex Is IOException Or TypeOf ex Is SerializationException Or TypeOf ex Is ArgumentNullException Or TypeOf ex Is FormatException Or TypeOf ex Is SecurityException)
                Finally
                    If ms IsNot Nothing Then
                        ms.Dispose()
                        ms = Nothing
                    End If
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
        Public Function serialize(Of t)(obj As t) As String
            SyncLock slock
                Dim ms As New MemoryStream()
                Try
                    formatter.Serialize(ms, obj)
                    ms.Flush()
                    Dim bts As Byte() = ms.ToArray()
                    ms.Dispose()
                    ms = Nothing
                    Return Convert.ToBase64String(bts)
                Catch ex As Exception When (TypeOf ex Is IOException Or TypeOf ex Is SerializationException Or TypeOf ex Is ArgumentNullException Or TypeOf ex Is FormatException Or TypeOf ex Is SecurityException)
                Finally
                    If ms IsNot Nothing Then
                        ms.Dispose()
                        ms = Nothing
                    End If
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
        Public Function deSerializeObject(bts As Byte()) As Object
            SyncLock slock
                Try
                    Dim ms As New MemoryStream(bts)
                    Try
                        ms.Flush()
                        Dim obj As Object = formatter.Deserialize(ms)
                        ms.Dispose()
                        ms = Nothing
                        Return obj
                    Catch ex As Exception When (TypeOf ex Is IOException Or TypeOf ex Is SerializationException Or TypeOf ex Is ArgumentNullException Or TypeOf ex Is FormatException Or TypeOf ex Is SecurityException Or TypeOf ex Is InvalidCastException)
                    Finally
                        If ms IsNot Nothing Then
                            ms.Dispose()
                            ms = Nothing
                        End If
                    End Try
                Catch ex As ArgumentNullException
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
        Public Function deSerializeObject(Of t)(bts As Byte()) As t
            SyncLock slock
                Try
                    Dim ms As New MemoryStream(bts)
                    Try
                        ms.Flush()
                        Dim obj As t = CType(formatter.Deserialize(ms), t)
                        ms.Dispose()
                        ms = Nothing
                        Return obj
                    Catch ex As Exception When (TypeOf ex Is IOException Or TypeOf ex Is SerializationException Or TypeOf ex Is ArgumentNullException Or TypeOf ex Is FormatException Or TypeOf ex Is SecurityException Or TypeOf ex Is InvalidCastException)
                    Finally
                        If ms IsNot Nothing Then
                            ms.Dispose()
                            ms = Nothing
                        End If
                    End Try
                Catch ex As ArgumentNullException
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
        Public Function deSerialize(str As String) As Object
            SyncLock slock
                Try
                    Dim ms As New MemoryStream(Convert.FromBase64String(str))
                    Try
                        ms.Flush()
                        Dim obj As Object = formatter.Deserialize(ms)
                        ms.Dispose()
                        ms = Nothing
                        Return obj
                    Catch ex As Exception When (TypeOf ex Is IOException Or TypeOf ex Is SerializationException Or TypeOf ex Is ArgumentNullException Or TypeOf ex Is FormatException Or TypeOf ex Is SecurityException Or TypeOf ex Is InvalidCastException)
                    Finally
                        If ms IsNot Nothing Then
                            ms.Dispose()
                            ms = Nothing
                        End If
                    End Try
                Catch ex As Exception When (TypeOf ex Is IOException Or TypeOf ex Is SerializationException Or TypeOf ex Is ArgumentNullException Or TypeOf ex Is FormatException Or TypeOf ex Is SecurityException Or TypeOf ex Is InvalidCastException)
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
        Public Function deSerialize(Of t)(str As String) As t
            SyncLock slock
                Try
                    Dim ms As New MemoryStream(Convert.FromBase64String(str))
                    Try
                        ms.Flush()
                        Dim obj As t = CType(formatter.Deserialize(ms), t)
                        ms.Dispose()
                        ms = Nothing
                        Return obj
                    Catch ex As Exception When (TypeOf ex Is IOException Or TypeOf ex Is SerializationException Or TypeOf ex Is ArgumentNullException Or TypeOf ex Is FormatException Or TypeOf ex Is SecurityException Or TypeOf ex Is InvalidCastException)
                    Finally
                        If ms IsNot Nothing Then
                            ms.Dispose()
                            ms = Nothing
                        End If
                    End Try
                Catch ex As Exception When (TypeOf ex Is IOException Or TypeOf ex Is SerializationException Or TypeOf ex Is ArgumentNullException Or TypeOf ex Is FormatException Or TypeOf ex Is SecurityException Or TypeOf ex Is InvalidCastException)
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
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class

End Namespace