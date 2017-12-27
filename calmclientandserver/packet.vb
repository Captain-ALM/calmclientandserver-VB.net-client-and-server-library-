'Packet design padded by a /u004 character
'refnum sender recievers header [payload]
'each part seperated by a /u005 character
'Payload design padded by a /u006 character
'[payload_meta] data
'each part seperated by a /u007 character
'Playload_meta design padded by a /u008 character
'isobject isencrypted encryptionmethod
'each part seperated by a /u009 character
'###Each piece of data is serialized###
''' <summary>
''' The packet class.
''' </summary>
''' <remarks></remarks>
<Serializable()>
Public Class packet
    Private _refnumber As Integer = 0
    Private _sender As String = ""
    Private _receivers As New List(Of String)
    Private _header As String = ""
    Private _data As String = ""
    Private _isobj As Boolean = False
    Private _isencrypted As Boolean = False
    Private _encryptmethod As EncryptionMethod = EncryptionMethod.none
    Private _ispacketvalid As Boolean = False

    'useful for ping packets
    ''' <summary>
    ''' Creates a new invalid packet.
    ''' </summary>
    ''' <remarks>(Useful for ping packets)</remarks>
    Public Sub New()
        _ispacketvalid = False
    End Sub

    'string construction
    ''' <summary>
    ''' Creates a new valid packet with basic data and no encryption [string data].
    ''' </summary>
    ''' <param name="refnumber">Reference Number.</param>
    ''' <param name="sender">The Sender Name.</param>
    ''' <param name="receivers">The Name(s) of the Receivers.</param>
    ''' <param name="header">The Header Data.</param>
    ''' <param name="data">The Data to Send [String].</param>
    ''' <remarks></remarks>
    Public Sub New(refnumber As Integer, sender As String, receivers As List(Of String), header As String, data As String)
        Try
            _refnumber = refnumber
            _ispacketvalid = True
            _sender = sender
            _receivers = receivers
            _header = header
            _data = data
            _isobj = False
        Catch ex As Exception
        End Try
    End Sub

    'object construction
    ''' <summary>
    ''' Creates a new valid packet with basic data and no encryption [encapsulated data].
    ''' </summary>
    ''' <param name="refnumber">Reference Number.</param>
    ''' <param name="sender">The Sender Name.</param>
    ''' <param name="receivers">The Name(s) of the Receivers.</param>
    ''' <param name="header">The Header Data.</param>
    ''' <param name="data">The Data to Send [Encapsulation].</param>
    ''' <remarks></remarks>
    Public Sub New(refnumber As Integer, sender As String, receivers As List(Of String), header As String, data As encapsulation)
        Try
            _refnumber = refnumber
            _ispacketvalid = True
            _sender = sender
            _receivers = receivers
            _header = header
            _data = data.data
            _isobj = True
        Catch ex As Exception
        End Try
    End Sub

    'string construction with encryption
    ''' <summary>
    ''' Creates a new valid packet with basic data and encryption [string data].
    ''' </summary>
    ''' <param name="refnumber">Reference Number.</param>
    ''' <param name="sender">The Sender Name.</param>
    ''' <param name="receivers">The Name(s) of the Receivers.</param>
    ''' <param name="header">The Header Data.</param>
    ''' <param name="data">The Data to Send [String].</param>
    ''' <param name="password">The password to protect the data (Optional if using no encryption or unicode only encryption).</param>
    ''' <param name="encryptmethod">The method to encrypt the data (No and Unicode Only encryption do not require a password while Ase and Unicode and Ase encryption do require a password).</param>
    ''' <remarks></remarks>
    Public Sub New(refnumber As Integer, sender As String, receivers As List(Of String), header As String, data As String, password As String, encryptmethod As EncryptionMethod)
        Try
            _refnumber = refnumber
            _ispacketvalid = True
            _sender = sender
            _receivers = receivers
            _header = header
            _data = data
            _isobj = False
            If encryptmethod > 0 And encryptmethod < 4 Then
                _encryptmethod = encryptmethod
                _isencrypted = True
            End If
            If _isencrypted Then
                If encryptmethod = EncryptionMethod.unicode Then
                    Dim oldstring As String = _data
                    _data = ""
                    For i As Integer = 0 To oldstring.Length - 1 Step 1
                        _data = _data & AscW(oldstring.Substring(i, 1)).ToString() & " "
                    Next
                ElseIf encryptmethod = EncryptionMethod.ase Then
                    Dim oldstring As String = _data
                    _data = ""
                    _data = EncryptString(oldstring, password)
                ElseIf encryptmethod = EncryptionMethod.unicodease Then
                    Dim oldstring As String = _data
                    _data = ""
                    Dim oldstring2 As String = EncryptString(oldstring, password)
                    For i As Integer = 0 To oldstring2.Length - 1 Step 1
                        _data = _data & AscW(oldstring2.Substring(i, 1)).ToString() & " "
                    Next
                End If
            End If
        Catch ex As Exception
        End Try
    End Sub

    'object construction with encryption
    ''' <summary>
    ''' Creates a new valid packet with basic data and encryption [encapsulated data].
    ''' </summary>
    ''' <param name="refnumber">Reference Number.</param>
    ''' <param name="sender">The Sender Name.</param>
    ''' <param name="receivers">The Name(s) of the Receivers.</param>
    ''' <param name="header">The Header Data.</param>
    ''' <param name="data">The Data to Send [Encapsulation].</param>
    ''' <param name="password">The password to protect the data (Optional if using no encryption or unicode only encryption).</param>
    ''' <param name="encryptmethod">The method to encrypt the data (No and Unicode Only encryption do not require a password while Ase and Unicode and Ase encryption do require a password).</param>
    ''' <remarks></remarks>
    Public Sub New(refnumber As Integer, sender As String, receivers As List(Of String), header As String, data As encapsulation, password As String, encryptmethod As EncryptionMethod)
        Try
            _refnumber = refnumber
            _ispacketvalid = True
            _sender = sender
            _receivers = receivers
            _header = header
            _data = data.data
            _isobj = True
            If encryptmethod > 0 And encryptmethod < 4 Then
                _encryptmethod = encryptmethod
                _isencrypted = True
            End If
            If _isencrypted Then
                If encryptmethod = EncryptionMethod.unicode Then
                    Dim oldstring As String = _data
                    _data = ""
                    For i As Integer = 0 To oldstring.Length - 1 Step 1
                        _data = _data & AscW(oldstring.Substring(i, 1)).ToString() & " "
                    Next
                ElseIf encryptmethod = EncryptionMethod.ase Then
                    Dim oldstring As String = _data
                    _data = ""
                    _data = EncryptString(oldstring, password)
                ElseIf encryptmethod = EncryptionMethod.unicodease Then
                    Dim oldstring As String = _data
                    _data = ""
                    Dim oldstring2 As String = EncryptString(oldstring, password)
                    For i As Integer = 0 To oldstring2.Length - 1 Step 1
                        _data = _data & AscW(oldstring2.Substring(i, 1)).ToString() & " "
                    Next
                End If
            End If
        Catch ex As Exception
        End Try
    End Sub

    'from other packet
    ''' <summary>
    ''' Creates a packet from another packet.
    ''' </summary>
    ''' <param name="packet">The packet to duplicate.</param>
    ''' <remarks></remarks>
    Public Sub New(packet As packet)
        Try
            If packet.isvalidpacket Then
                _ispacketvalid = True
                _sender = packet.sender
                _receivers = packet.receivers
                _header = packet.header
                _data = packet.data
                If packet.encryptmethod = 0 Then
                    _isencrypted = False
                    _encryptmethod = EncryptionMethod.none
                Else
                    If packet.encryptmethod > 0 And packet.encryptmethod < 4 Then
                        _encryptmethod = packet.encryptmethod
                        _isencrypted = True
                    Else
                        _isencrypted = False
                        _encryptmethod = EncryptionMethod.none
                    End If
                End If
                _refnumber = packet.referencenumber
                _isobj = packet.hasobject
            Else
                _ispacketvalid = False
            End If
        Catch ex As Exception
        End Try
    End Sub
    ''' <summary>
    ''' Returns if the packet is valid.
    ''' </summary>
    ''' <value>Returns if the packet is valid.</value>
    ''' <returns>Returns if the packet is valid.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property isvalidpacket As Boolean
        Get
            Return _ispacketvalid
        End Get
    End Property
    ''' <summary>
    ''' Returns the sender's name.
    ''' </summary>
    ''' <value>Returns the sender's name.</value>
    ''' <returns>Returns the sender's name.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property sender As String
        Get
            Return _sender
        End Get
    End Property
    ''' <summary>
    ''' Returns the names of the receivers.
    ''' </summary>
    ''' <value>Returns the names of the receivers.</value>
    ''' <returns>Returns the names of the receivers.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property receivers As List(Of String)
        Get
            Return _receivers
        End Get
    End Property
    ''' <summary>
    ''' Gets the header of the packet.
    ''' </summary>
    ''' <value>Gets the header of the packet.</value>
    ''' <returns>Gets the header of the packet.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property header As String
        Get
            Return _header
        End Get
    End Property
    ''' <summary>
    ''' Returns the encryption method.
    ''' </summary>
    ''' <value>Returns the encryption method.</value>
    ''' <returns>Returns the encryption method.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property encryptmethod As Integer
        Get
            Return _encryptmethod
        End Get
    End Property
    ''' <summary>
    ''' Returns the packet's reference number.
    ''' </summary>
    ''' <value>Returns the packet's reference number.</value>
    ''' <returns>Returns the packet's reference number.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property referencenumber As Integer
        Get
            Return _refnumber
        End Get
    End Property
    ''' <summary>
    ''' Returns the data of the packet [string form].
    ''' </summary>
    ''' <value>Returns the data of the packet [string form].</value>
    ''' <returns>Returns the data of the packet [string form].</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property data As String
        Get
            Return _data
        End Get
    End Property
    ''' <summary>
    ''' Returns if the packet contains an object.
    ''' </summary>
    ''' <value>Returns if the packet contains an object.</value>
    ''' <returns>Returns if the packet contains an object.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property hasobject As Boolean
        Get
            Return _isobj
        End Get
    End Property
    ''' <summary>
    ''' Returns the data held by the packet as a string.
    ''' </summary>
    ''' <param name="password">The password (If the data is encrypted with ase of unicodease).</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function stringdata(Optional password As String = "") As String
        Try
            If isvalidpacket Then
                Dim data As String = ""
                If _encryptmethod = EncryptionMethod.unicode Or _encryptmethod = EncryptionMethod.unicodease Then
                    Dim chars As String = ""
                    For i As Integer = 0 To _data.Length - 1 Step 1
                        Dim cchr As String = _data.Substring(i, 1)
                        If cchr = " " Then
                            Dim conint As Integer = 0
                            Try
                                conint = CInt(chars)
                            Catch ex As Exception
                                conint = 0
                            End Try
                            data = data & ChrW(conint)
                            chars = ""
                        Else
                            chars = chars & cchr
                        End If
                    Next
                Else
                    data = _data
                End If
                Dim returned As String = ""
                If (password <> "" Or password <> Nothing) And (_encryptmethod = EncryptionMethod.ase Or _encryptmethod = EncryptionMethod.unicodease) Then
                    returned = DecryptString(data, password)
                Else
                    returned = data
                End If
                Return returned
            End If
        Catch ex As Exception
        End Try
        Return ""
    End Function
    ''' <summary>
    ''' Returns the data held by the packet as an object.
    ''' </summary>
    ''' <param name="password">The password (If the data is encrypted with ase of unicodease).</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function objectdata(Optional password As String = "") As Object
        Try
            If _isobj And isvalidpacket Then
                Dim data As String = ""
                If _encryptmethod = EncryptionMethod.unicode Or _encryptmethod = EncryptionMethod.unicodease Then
                    Dim chars As String = ""
                    For i As Integer = 0 To _data.Length - 1 Step 1
                        Dim cchr As String = _data.Substring(i, 1)
                        If cchr = " " Then
                            Dim conint As Integer = 0
                            Try
                                conint = CInt(chars)
                            Catch ex As Exception
                                conint = 0
                            End Try
                            data = data & ChrW(conint)
                            chars = ""
                        Else
                            chars = chars & cchr
                        End If
                    Next
                Else
                    data = _data
                End If
                Dim returned As String = ""
                If password <> "" Or password <> Nothing Then
                    returned = DecryptString(data, password)
                Else
                    returned = data
                End If
                Dim retobj As Object = convertstringtoobject(returned)
                Return retobj
            End If
        Catch ex As Exception
        End Try
        Return Nothing
    End Function
    ''' <summary>
    ''' Returns the data in the packet as a converted byte array
    ''' </summary>
    ''' <returns>byte array</returns>
    ''' <remarks></remarks>
    Public Function ToBytes() As Byte()
        Dim payload_meta_barr((2 + 3 + 2) - 1) As Byte

        payload_meta_barr(0) = 8
        payload_meta_barr(1) = _isobj
        payload_meta_barr(2) = 9
        payload_meta_barr(3) = _isencrypted
        payload_meta_barr(4) = 9
        payload_meta_barr(5) = _encryptmethod
        payload_meta_barr(6) = 8

        Dim ascii_data As Byte() = utils.Convert2Ascii(convertobjecttostring(_data))

        Dim payload_barr(2 + payload_meta_barr.Length + 1 + ascii_data.Length) As Byte

        payload_barr(0) = 6
        System.Buffer.BlockCopy(payload_meta_barr, 0, payload_barr, 1, payload_meta_barr.Length)
        payload_barr(payload_meta_barr.Length + 1) = 7
        System.Buffer.BlockCopy(ascii_data, 0, payload_barr, 2 + payload_meta_barr.Length, ascii_data.Length)
        payload_barr(payload_barr.Length - 1) = 6

        Dim ascii_refnum As Byte() = utils.Convert2Ascii(_refnumber)
        Dim ascii_sender As Byte() = utils.Convert2Ascii(convertobjecttostring(_sender))
        Dim ascii_recievers As Byte() = utils.Convert2Ascii(convertobjecttostring(_receivers))
        Dim ascii_header As Byte() = utils.Convert2Ascii(convertobjecttostring(_header))

        Dim barrret(2 + ascii_refnum.Length + 1 + ascii_sender.Length + 1 + ascii_recievers.Length + 1 + ascii_header.Length + 1 + payload_barr.Length) As Byte

        barrret(0) = 4
        System.Buffer.BlockCopy(ascii_refnum, 0, barrret, 1, ascii_refnum.Length)
        barrret(ascii_refnum.Length + 1) = 5
        System.Buffer.BlockCopy(ascii_sender, 0, barrret, 1 + ascii_refnum.Length + 1, ascii_sender.Length)
        barrret(ascii_refnum.Length + 1 + ascii_sender.Length + 1) = 5
        System.Buffer.BlockCopy(ascii_recievers, 0, barrret, 1 + ascii_refnum.Length + 1 + ascii_sender.Length + 1, ascii_recievers.Length)
        barrret(ascii_refnum.Length + 1 + ascii_sender.Length + 1 + ascii_recievers.Length + 1) = 5
        System.Buffer.BlockCopy(ascii_header, 0, barrret, 1 + ascii_refnum.Length + 1 + ascii_sender.Length + 1 + ascii_recievers.Length + 1, ascii_header.Length)
        barrret(ascii_refnum.Length + 1 + ascii_sender.Length + 1 + ascii_recievers.Length + 1 + ascii_header.Length + 1) = 5
        System.Buffer.BlockCopy(payload_barr, 0, barrret, 1 + ascii_refnum.Length + 1 + ascii_sender.Length + 1 + ascii_recievers.Length + 1 + ascii_header.Length + 1, payload_barr.Length)
        barrret(barrret.Length - 1) = 4

        Return barrret
    End Function

    Public Shared Narrowing Operator CType(ByVal data As Byte()) As packet
        Dim packetdat(data.Length - 3) As Byte
        System.Buffer.BlockCopy(data, 1, packetdat, 0, data.Length - 2)

        Dim c_byte As Byte = 0
        Dim c_index As Integer = 0
        Dim dat_arr_lst As New List(Of Byte)

        While ((Not c_byte = 5) Or (Not c_byte = 0)) And c_index < packetdat.Length
            c_byte = packetdat(c_index)
            If c_byte = 5 Then Exit While
            dat_arr_lst.Add(c_byte)
            c_index += 1
        End While

        Dim refnum As Integer = utils.ConvertFromAscii(dat_arr_lst.ToArray)
        dat_arr_lst.Clear()

        c_index += 1
        While ((Not c_byte = 5) Or (Not c_byte = 0)) And c_index < packetdat.Length
            c_byte = packetdat(c_index)
            If c_byte = 5 Then Exit While
            dat_arr_lst.Add(c_byte)
            c_index += 1
        End While

        Dim sender As String = convertstringtoobject(utils.ConvertFromAscii(dat_arr_lst.ToArray))
        dat_arr_lst.Clear()

        c_index += 1
        While ((Not c_byte = 5) Or (Not c_byte = 0)) And c_index < packetdat.Length
            c_byte = packetdat(c_index)
            If c_byte = 5 Then Exit While
            dat_arr_lst.Add(c_byte)
            c_index += 1
        End While

        Dim recievers As List(Of String) = convertstringtoobject(utils.ConvertFromAscii(dat_arr_lst.ToArray))
        dat_arr_lst.Clear()

        c_index += 1
        While ((Not c_byte = 5) Or (Not c_byte = 0)) And c_index < packetdat.Length
            c_byte = packetdat(c_index)
            If c_byte = 5 Then Exit While
            dat_arr_lst.Add(c_byte)
            c_index += 1
        End While

        Dim header As String = convertstringtoobject(utils.ConvertFromAscii(dat_arr_lst.ToArray))
        dat_arr_lst.Clear()

        c_index += 1
        Dim payload(packetdat.Length - c_index - 1) As Byte
        Buffer.BlockCopy(packetdat, c_index, payload, 0, packetdat.Length - c_index)
        Dim payloaddat(payload.Length - 3) As Byte
        Buffer.BlockCopy(payload, 1, payloaddat, 0, payload.Length - 2)

        c_byte = 0

        Dim isobj As Boolean = payloaddat(1)
        Dim isencry As Boolean = payloaddat(3)
        Dim encrymeth As EncryptionMethod = payloaddat(5)

        Dim data_arr(payloaddat.Length - 8 - 1) As Byte
        Buffer.BlockCopy(payloaddat, 8, data_arr, 0, payloaddat.Length - 8)
        Dim t_data As String = convertstringtoobject(utils.ConvertFromAscii(data_arr))

        Dim p_t_ret As packet = New packet()
        p_t_ret._ispacketvalid = True
        p_t_ret._data = t_data
        p_t_ret._encryptmethod = encrymeth
        p_t_ret._header = header
        p_t_ret._isencrypted = isencry
        p_t_ret._isobj = isobj
        p_t_ret._receivers = recievers
        p_t_ret._refnumber = refnum
        p_t_ret._sender = sender

        Return p_t_ret
    End Operator

    Public Shared Widening Operator CType(ByVal packt As packet) As Byte()
        Dim payload_meta_barr((2 + 3 + 2) - 1) As Byte

        payload_meta_barr(0) = 8
        payload_meta_barr(1) = packt._isobj
        payload_meta_barr(2) = 9
        payload_meta_barr(3) = packt._isencrypted
        payload_meta_barr(4) = 9
        payload_meta_barr(5) = packt._encryptmethod
        payload_meta_barr(6) = 8

        Dim ascii_data As Byte() = utils.Convert2Ascii(convertobjecttostring(packt._data))

        Dim payload_barr(2 + payload_meta_barr.Length + 1 + ascii_data.Length) As Byte

        payload_barr(0) = 6
        System.Buffer.BlockCopy(payload_meta_barr, 0, payload_barr, 1, payload_meta_barr.Length)
        payload_barr(payload_meta_barr.Length + 1) = 7
        System.Buffer.BlockCopy(ascii_data, 0, payload_barr, 2 + payload_meta_barr.Length, ascii_data.Length)
        payload_barr(payload_barr.Length - 1) = 6

        Dim ascii_refnum As Byte() = utils.Convert2Ascii(packt._refnumber)
        Dim ascii_sender As Byte() = utils.Convert2Ascii(convertobjecttostring(packt._sender))
        Dim ascii_recievers As Byte() = utils.Convert2Ascii(convertobjecttostring(packt._receivers))
        Dim ascii_header As Byte() = utils.Convert2Ascii(convertobjecttostring(packt._header))

        Dim barrret(2 + ascii_refnum.Length + 1 + ascii_sender.Length + 1 + ascii_recievers.Length + 1 + ascii_header.Length + 1 + payload_barr.Length) As Byte

        barrret(0) = 4
        System.Buffer.BlockCopy(ascii_refnum, 0, barrret, 1, ascii_refnum.Length)
        barrret(ascii_refnum.Length + 1) = 5
        System.Buffer.BlockCopy(ascii_sender, 0, barrret, 1 + ascii_refnum.Length + 1, ascii_sender.Length)
        barrret(ascii_refnum.Length + 1 + ascii_sender.Length + 1) = 5
        System.Buffer.BlockCopy(ascii_recievers, 0, barrret, 1 + ascii_refnum.Length + 1 + ascii_sender.Length + 1, ascii_recievers.Length)
        barrret(ascii_refnum.Length + 1 + ascii_sender.Length + 1 + ascii_recievers.Length + 1) = 5
        System.Buffer.BlockCopy(ascii_header, 0, barrret, 1 + ascii_refnum.Length + 1 + ascii_sender.Length + 1 + ascii_recievers.Length + 1, ascii_header.Length)
        barrret(ascii_refnum.Length + 1 + ascii_sender.Length + 1 + ascii_recievers.Length + 1 + ascii_header.Length + 1) = 5
        System.Buffer.BlockCopy(payload_barr, 0, barrret, 1 + ascii_refnum.Length + 1 + ascii_sender.Length + 1 + ascii_recievers.Length + 1 + ascii_header.Length + 1, payload_barr.Length)
        barrret(barrret.Length - 1) = 4

        Return barrret
    End Operator
End Class
''' <summary>
''' Packet Encryption Method.
''' </summary>
''' <remarks></remarks>
<Serializable()>
Public Enum EncryptionMethod As Integer
    none = 0
    unicode = 1
    ase = 2
    unicodease = 3
End Enum
