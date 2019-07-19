Namespace calmclientandserver

    ''' <summary>
    ''' Packet Frame {Internal Access Only}
    ''' </summary>
    ''' <remarks></remarks>
    <Obsolete("This is deprecated, use the calmnetlib or calmnetmarshal namespaces instead.")>
    Friend Structure packet_frame
        Dim data As Byte()
        Dim refnum As Integer

        Public Sub New(ByVal pckt As Packet)
            data = pckt.ToBytes
            refnum = pckt.ReferenceNumber
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
                cindex += packetfps(i).data.Length
            Next
            refnum = refn
            data = dat
        End Sub

        Public Function ToParts(Optional ByVal buffer_size As Integer = 4096, Optional ByVal one_part_only As Boolean = False) As packet_frame_part()
            If one_part_only Then
                Dim frame_parts(0) As packet_frame_part
                frame_parts(0) = New packet_frame_part(refnum, 1, 1, data)
                Return frame_parts
            Else
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
            End If
        End Function

        Public Shared Narrowing Operator CType(ByVal packetfps As packet_frame_part()) As packet_frame
            Dim dat As Byte() = packetfps(0).data
            Dim refn As Integer = packetfps(0).refnum
            Dim cindex As Integer = packetfps(0).data.Length
            For i As Integer = 1 To packetfps.Length - 1 Step 1
                ReDim Preserve dat((cindex - 1) + packetfps(i).data.Length)
                Buffer.BlockCopy(packetfps(i).data, 0, dat, cindex, packetfps(i).data.Length)
                cindex += packetfps(i).data.Length
            Next
            Dim p_f As New packet_frame(refn, dat)
            Return p_f
        End Operator
    End Structure


    'packet_frame_part design padded by a /u002 character
    'refnum partnum totalparts data
    'each part seperated by a /u003 character
    ''' <summary>
    ''' Part of a Packet Frame {Internal Access Only}
    ''' </summary>
    ''' <remarks></remarks>
    <Obsolete("This is deprecated, use the calmnetlib or calmnetmarshal namespaces instead.")>
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

        Public Sub New(ByVal dat As Byte())
            Dim packetdat(dat.Length - 3) As Byte
            System.Buffer.BlockCopy(dat, 1, packetdat, 0, dat.Length - 2)

            Dim c_byte As Byte = 1
            Dim c_index As Integer = 0
            Dim dat_arr_lst As New List(Of Byte)

            While ((Not c_byte = 3) Or (Not c_byte = 0)) And c_index < packetdat.Length
                c_byte = packetdat(c_index)
                If c_byte = 3 Then Exit While
                dat_arr_lst.Add(c_byte)
                c_index += 1
            End While

            refnum = Utils.ConvertFromAscii(dat_arr_lst.ToArray)
            dat_arr_lst.Clear()

            c_byte = 1
            c_index += 1
            While ((Not c_byte = 3) Or (Not c_byte = 0)) And c_index < packetdat.Length
                c_byte = packetdat(c_index)
                If c_byte = 3 Then Exit While
                dat_arr_lst.Add(c_byte)
                c_index += 1
            End While

            partnum = Utils.ConvertFromAscii(dat_arr_lst.ToArray)
            dat_arr_lst.Clear()

            c_byte = 1
            c_index += 1
            While ((Not c_byte = 3) Or (Not c_byte = 0)) And c_index < packetdat.Length
                c_byte = packetdat(c_index)
                If c_byte = 3 Then Exit While
                dat_arr_lst.Add(c_byte)
                c_index += 1
            End While

            totalparts = Utils.ConvertFromAscii(dat_arr_lst.ToArray)
            dat_arr_lst.Clear()

            c_byte = 1
            c_index += 1
            Dim pldat(packetdat.Length - c_index - 1) As Byte
            Buffer.BlockCopy(packetdat, c_index, pldat, 0, packetdat.Length - c_index)
            data = pldat
        End Sub

        Public Function ToBytes() As Byte()
            Dim ascii_refnum As Byte() = Utils.Convert2Ascii(refnum)
            Dim ascii_partnum As Byte() = Utils.Convert2Ascii(partnum)
            Dim ascii_totalpnum As Byte() = Utils.Convert2Ascii(totalparts)

            Dim byte_arr(2 + ascii_refnum.Length + 1 + ascii_partnum.Length + 1 + ascii_totalpnum.Length + 1 + data.Length - 1) As Byte

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
            Dim ascii_refnum As Byte() = Utils.Convert2Ascii(pfp.refnum)
            Dim ascii_partnum As Byte() = Utils.Convert2Ascii(pfp.partnum)
            Dim ascii_totalpnum As Byte() = Utils.Convert2Ascii(pfp.totalparts)

            Dim byte_arr(2 + ascii_refnum.Length + 1 + ascii_partnum.Length + 1 + ascii_totalpnum.Length + 1 + pfp.data.Length - 1) As Byte

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

        Public Shared Narrowing Operator CType(ByVal dat As Byte()) As packet_frame_part
            Dim packetdat(dat.Length - 3) As Byte
            System.Buffer.BlockCopy(dat, 1, packetdat, 0, dat.Length - 2)

            Dim c_byte As Byte = 1
            Dim c_index As Integer = 0
            Dim dat_arr_lst As New List(Of Byte)

            While ((Not c_byte = 3) Or (Not c_byte = 0)) And c_index < packetdat.Length
                c_byte = packetdat(c_index)
                If c_byte = 3 Then Exit While
                dat_arr_lst.Add(c_byte)
                c_index += 1
            End While

            Dim refnum As Integer = Utils.ConvertFromAscii(dat_arr_lst.ToArray)
            dat_arr_lst.Clear()

            c_byte = 1
            c_index += 1
            While ((Not c_byte = 3) Or (Not c_byte = 0)) And c_index < packetdat.Length
                c_byte = packetdat(c_index)
                If c_byte = 3 Then Exit While
                dat_arr_lst.Add(c_byte)
                c_index += 1
            End While

            Dim partnum As Integer = Utils.ConvertFromAscii(dat_arr_lst.ToArray)
            dat_arr_lst.Clear()

            c_byte = 1
            c_index += 1
            While ((Not c_byte = 3) Or (Not c_byte = 0)) And c_index < packetdat.Length
                c_byte = packetdat(c_index)
                If c_byte = 3 Then Exit While
                dat_arr_lst.Add(c_byte)
                c_index += 1
            End While

            Dim totalpnum As Integer = Utils.ConvertFromAscii(dat_arr_lst.ToArray)
            dat_arr_lst.Clear()

            c_byte = 1
            c_index += 1
            Dim pldat(packetdat.Length - c_index - 1) As Byte
            Buffer.BlockCopy(packetdat, c_index, pldat, 0, packetdat.Length - c_index)

            Return New packet_frame_part(refnum, partnum, totalpnum, pldat)
        End Operator
    End Structure

End Namespace