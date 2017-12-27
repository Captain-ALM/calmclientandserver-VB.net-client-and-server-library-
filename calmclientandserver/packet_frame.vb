
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


'packet_frame_part design padded by a /u002 character
'refnum partnum totalparts data
'each part seperated by a /u003 character
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

    Public Function ToBytes() As Byte()
        Dim ascii_refnum As Byte() = utils.Convert2Ascii(refnum)
        Dim ascii_partnum As Byte() = utils.Convert2Ascii(partnum)
        Dim ascii_totalpnum As Byte() = utils.Convert2Ascii(totalparts)

        Dim byte_arr(2 + ascii_refnum.Length + 1 + ascii_partnum.Length + 1 + ascii_totalpnum.Length + 1 + data.Length) As Byte

        byte_arr(0) = 2
        System.Buffer.BlockCopy(ascii_refnum, 0, byte_arr, 1, ascii_refnum.Length)
        byte_arr(ascii_refnum.Length + 1) = 3
        System.Buffer.BlockCopy(ascii_partnum, 0, byte_arr, 1 + ascii_refnum.Length + 1, ascii_partnum.Length)
        byte_arr(ascii_refnum.Length + 1 + ascii_partnum.Length + 1) = 3
        System.Buffer.BlockCopy(ascii_totalpnum, 0, byte_arr, 1 + ascii_refnum.Length + 1 + ascii_partnum.Length + 1, ascii_totalpnum.Length)
        byte_arr(ascii_refnum.Length + 1 + ascii_partnum.Length + 1 + ascii_totalpnum.Length + 1) = 3
        System.Buffer.BlockCopy(data, 0, byte_arr, 1 + ascii_refnum.Length + 1 + ascii_partnum.Length + 1 + ascii_totalpnum.Length + 1, data.Length)
        byte_arr(byte_arr.Length - 1) = 2

        Return byte_arr
    End Function

    Public Shared Widening Operator CType(ByVal pfp As packet_frame_part) As Byte()
        Dim ascii_refnum As Byte() = utils.Convert2Ascii(pfp.refnum)
        Dim ascii_partnum As Byte() = utils.Convert2Ascii(pfp.partnum)
        Dim ascii_totalpnum As Byte() = utils.Convert2Ascii(pfp.totalparts)

        Dim byte_arr(2 + ascii_refnum.Length + 1 + ascii_partnum.Length + 1 + ascii_totalpnum.Length + 1 + pfp.data.Length) As Byte

        byte_arr(0) = 2
        System.Buffer.BlockCopy(ascii_refnum, 0, byte_arr, 1, ascii_refnum.Length)
        byte_arr(ascii_refnum.Length + 1) = 3
        System.Buffer.BlockCopy(ascii_partnum, 0, byte_arr, 1 + ascii_refnum.Length + 1, ascii_partnum.Length)
        byte_arr(ascii_refnum.Length + 1 + ascii_partnum.Length + 1) = 3
        System.Buffer.BlockCopy(ascii_totalpnum, 0, byte_arr, 1 + ascii_refnum.Length + 1 + ascii_partnum.Length + 1, ascii_totalpnum.Length)
        byte_arr(ascii_refnum.Length + 1 + ascii_partnum.Length + 1 + ascii_totalpnum.Length + 1) = 3
        System.Buffer.BlockCopy(pfp.data, 0, byte_arr, 1 + ascii_refnum.Length + 1 + ascii_partnum.Length + 1 + ascii_totalpnum.Length + 1, pfp.data.Length)
        byte_arr(byte_arr.Length - 1) = 2

        Return byte_arr
    End Operator
End Structure