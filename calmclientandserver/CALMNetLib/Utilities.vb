Imports System.Net
Imports System.Net.Sockets
Imports System.Net.NetworkInformation
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
    End Class

End Namespace
