''' <summary>
''' Internal Msg {Internal Access Only}
''' </summary>
''' <remarks></remarks>
<Obsolete("Superceded by packet_frame and packet_frame_part", False)>
Friend Structure intmsg
    Dim Data() As Byte
    Public Sub New(ByVal DataByte() As Byte)
        Data = DataByte
    End Sub
    Public Shared Function GetBytes(ByVal c As intmsg) As Byte()
        Dim EncryptedString As String = "!" & c.Data.Length & "!"
        Return JoinBytes(MainEncoding.GetBytes(EncryptedString), c.Data)
    End Function
    Public Shared Function FromBytes(ByVal bytes() As Byte, Optional ByRef Start As Integer = 0) As intmsg
        Try
            Dim Msg As intmsg
            Dim Length_Start As Integer = Start 'The first "!" character is already the previous one
            Dim Length_End As Integer = Length_Start + 1 'Gets the data length within the "!" characters
            Do Until bytes(Length_End) = 33
                Length_End += 1
                If Length_End >= bytes.Length - 1 Then Return New intmsg(Nothing)
                If bytes(Length_End) = 0 Then Return New intmsg(Nothing) 'make sure to detect "null" bytes and return a blank message if one (stop useless cpu churning)
            Loop
            Dim DataLength As Integer
            DataLength = MainEncoding.GetString(ChopBytes(bytes, Length_Start + 1, Length_End - Length_Start - 1))
            Msg.Data = ChopBytes(bytes, Length_End + 1, DataLength)
            Start = Length_End + DataLength
            Return Msg
        Catch ex As Exception
            Return New intmsg(Nothing)
        End Try
    End Function
End Structure