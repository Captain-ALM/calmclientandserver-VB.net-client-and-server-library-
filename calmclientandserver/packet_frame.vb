
Friend Structure packet_frame
    Dim data As Byte()
    Dim refnum As Integer

    Public Sub New(ByVal pckt As packet)
        data = pckt.ToBytes
        refnum = pckt.referencenumber
    End Sub

    Public Sub New(ByVal rn As Integer, ByVal dat As Byte())
        data = dat
        refnum = rn
    End Sub

    Public Sub New(ByVal packetfps As packet_frame_part())
        Dim dat As Byte() = packetfps(0).data
        Dim refn As Integer = packetfps(0).refnum
        Dim cindex As Integer = packetfps(0).data.Length
        For i As Integer = 1 To packetfps.Length - 1 Step 1
            ReDim Preserve dat((cindex - 1) + packetfps(i).data.Length)
            Buffer.BlockCopy(packetfps(i).data, 0, dat, cindex, packetfps(i).data.Length)
        Next
        refnum = refn
        data = dat
    End Sub

    Public Function ToParts(Optional ByVal buffer_size As Integer = 4096) As packet_frame_part()
        Dim split_size As Integer = buffer_size / 4
        Dim arrsize As Double = data.Length / split_size
        Dim arrsiz As Integer = Int(Math.Ceiling(arrsize))
        Dim frame_parts(arrsiz - 1) As packet_frame_part
        Dim cindex As Integer = 0

        For i As Integer = 0 To arrsiz - 1 Step 1
            If cindex + split_size > data.Length Then
                Dim dat(data.Length - cindex - 1) As Byte
                Buffer.BlockCopy(data, cindex, dat, 0, data.Length - cindex)
                frame_parts(i) = New packet_frame_part(refnum, i + 1, arrsiz, dat)
            Else
                Dim dat(split_size - 1) As Byte
                Buffer.BlockCopy(data, cindex, dat, 0, split_size)
                frame_parts(i) = New packet_frame_part(refnum, i + 1, arrsiz, dat)
            End If
            cindex += split_size
        Next

        Return frame_parts
    End Function

    Public Shared Narrowing Operator CType(ByVal packetfps As packet_frame_part()) As packet_frame
        Dim dat As Byte() = packetfps(0).data
        Dim refn As Integer = packetfps(0).refnum
        Dim cindex As Integer = packetfps(0).data.Length
        For i As Integer = 1 To packetfps.Length - 1 Step 1
            ReDim Preserve dat((cindex - 1) + packetfps(i).data.Length)
            Buffer.BlockCopy(packetfps(i).data, 0, dat, cindex, packetfps(i).data.Length)
        Next
        Dim p_f As New packet_frame(refn, dat)
        Return p_f
    End Operator
End Structure

Friend Structure packet_frame_part
    Dim data As Byte()
    Dim refnum As Integer
    Dim partnum As Integer
    Dim totalparts As Integer

    Public Sub New(ByVal rn As Integer, ByVal pn As Integer, ByVal tp As Integer, ByVal dat As Byte())
        refnum = rn
        partnum = pn
        totalparts = tp
        data = dat
    End Sub
End Structure