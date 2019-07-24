'
' Created by SharpDevelop.
' User: Alfred
' Date: 21/05/2019
' Time: 16:49
' 
' To change this template use Tools | Options | Coding | Edit Standard Headers.
'
Namespace CALMNetLib
    ''' <summary>
    ''' 16-Bit Indexed Byte Array Encapsulation.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ByteInt16ArrayEncapsulation
        Implements IEncapsulation

        'ByteInt16ArrayEncapsulation Format:
        '2 Bytes Overhead
        '[0-1] Length Header (2 Bytes) {Includes the length of all headers, not only the held data}
        '[2-...] Data (... Bytes)
        'ByteInt16ArrayEncapsulation Part Format:
        '18 Bytes Overhead
        '[0-1] Length Header (2 Bytes) {Includes the length of all headers and checksum trailer, not only the held data}
        '[2-9] Part Sequential Identifier (8 Bytes) [Always 8 bytes]
        '[10-...] Part Data (... Bytes)
        '[...+1-...+9] Check Sum (8 Bytes) [Always 8 bytes]

        Protected _arr(-1) As Byte
        ''' <summary>
        ''' Constructs an Empty ByteInt16ArrayEncapsualtion.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            ReDim _arr(1)
        End Sub
        ''' <summary>
        ''' Constructs an Empty ByteInt16ArrayEncapsualtion with the specified capacity.
        ''' <param name="length">Capacity</param>
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New(length As Integer)
            ReDim _arr(length + 1)
        End Sub
        ''' <summary>
        ''' Constructs a ByteInt16ArrayEncapsualtion with the specified byte array.
        ''' <param name="bytes">Inital Byte Array</param>
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New(ByVal bytes As Byte())
            _arr = bytes
        End Sub
        ''' <summary>
        ''' Returns the length of the Encapsulated Array.
        ''' </summary>
        ''' <value>Long</value>
        ''' <returns>The Length of The Array</returns>
        ''' <remarks></remarks>
        Public ReadOnly Overridable Property Length As Long Implements IEncapsulation.Length
            Get
                Return _arr.Length - 2
            End Get
        End Property
        ''' <summary>
        ''' Returns the Data type of the Pure Encapsulation.
        ''' </summary>
        ''' <value>Type</value>
        ''' <returns>The data type of the pure encapsulation</returns>
        ''' <remarks></remarks>
        Public ReadOnly Overridable Property dataType As Type Implements IEncapsulation.dataType
            Get
                Return GetType(Byte())
            End Get
        End Property
        ''' <summary>
        ''' Returns the data of the object.
        ''' </summary>
        ''' <value>Object</value>
        ''' <returns>Pure Encapsulated Object Data</returns>
        ''' <remarks></remarks>
        Public Overridable Property data As Object Implements IEncapsulation.data
            Get
                Return removeDataLength(_arr)
            End Get
            Set(value As Object)
                _arr = addDataLength(_arr)
            End Set
        End Property
        ''' <summary>
        ''' Returns the data of the object at a specified index.
        ''' </summary>
        ''' <param name="index">The Index as a Long Value</param>
        ''' <value>Object</value>
        ''' <returns>Pure Encapsulated Object data at the specified index</returns>
        ''' <remarks></remarks>
        Public Overridable Property data(index As Long) As Object Implements IEncapsulation.data
            Get
                Return _arr(index + 2)
            End Get
            Set(value As Object)
                _arr(index + 2) = value
            End Set
        End Property
        ''' <summary>
        ''' Returns whether the contained data is valid.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>The contained data is valid.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Overridable Property valid As Boolean Implements IEncapsulation.valid
            Get
                Return checkDataLength()
            End Get
        End Property

        Protected Overridable Function addDataLength(dataIn As Byte()) As Byte()
            Dim narr(dataIn.Length + 1) As Byte
            'Length includes the length header
            Dim len As Byte() = Utilities.Int16ToBytes(narr.Length)
            Buffer.BlockCopy(len, 0, narr, 0, len.Length)
            Buffer.BlockCopy(dataIn, 0, narr, 2, dataIn.Length)
            Return narr
        End Function

        Protected Overridable Function removeDataLength(dataWLenIn As Byte()) As Byte()
            Dim len(1) As Byte
            Buffer.BlockCopy(dataWLenIn, 0, len, 0, len.Length)
            Dim lengthd As Short = Utilities.BytesToInt16(len)
            Dim rarr(lengthd - 1) As Byte
            Buffer.BlockCopy(dataWLenIn, 2, rarr, 0, rarr.Length)
            Return rarr
        End Function

        Protected Overridable Function checkDataLength() As Boolean
            Dim verify As Boolean = True
            'Length includes the length header
            Dim len As Byte() = Utilities.Int16ToBytes(_arr.Length)
            verify = verify And (len(0) = _arr(0))
            verify = verify And (len(1) = _arr(1))
            Return verify
        End Function

        Protected Overridable Function calculateCheckSum(bytes As Byte()) As Long
            Dim sum As Long = 0
            For i As Integer = 0 To bytes.Length - 1 Step 1
                sum += bytes(i)
            Next
            Dim overflow As Long = sum \ Long.MaxValue
            Dim modulo As Long = sum Mod Long.MaxValue
            sum = Not ((overflow + modulo) Mod Long.MaxValue)
            Return sum
        End Function

        Protected Overridable Function createPart(id As Long, bytesIn As Byte()) As Byte()
            'Part overhead (Header and Trailer) is 18 Bytes
            Dim tlen As Integer = 18 + bytesIn.Length
            Dim len As Byte() = Utilities.Int16ToBytes(tlen)
            Dim idarr As Byte() = Utilities.Int64ToBytes(id)
            Dim chksum As Long = calculateCheckSum(bytesIn)
            Dim chksumarr As Byte() = Utilities.Int64ToBytes(chksum)
            Dim partarr(tlen - 1) As Byte
            Buffer.BlockCopy(len, 0, partarr, 0, len.Length)
            Buffer.BlockCopy(idarr, 0, partarr, 2, idarr.Length)
            Buffer.BlockCopy(bytesIn, 0, partarr, 10, bytesIn.Length)
            Buffer.BlockCopy(chksumarr, 0, partarr, 10 + bytesIn.Length, chksumarr.Length)
            Return partarr
        End Function

        Protected Overridable Function processPart(partIn As Byte()) As Byte()
            Dim len(1) As Byte
            Buffer.BlockCopy(partIn, 0, len, 0, len.Length)
            Dim datlen As Short = Utilities.BytesToInt16(len)
            Dim chksumarr(7) As Byte
            Buffer.BlockCopy(partIn, partIn.Length - 8, chksumarr, 0, chksumarr.Length)
            Dim chksum As Long = Utilities.BytesToInt64(chksumarr)
            Dim dat(datlen - 19) As Byte
            Buffer.BlockCopy(partIn, 10, dat, 0, datlen - 18)
            Dim verify As Boolean = (calculateCheckSum(dat) = chksum)
            If verify Then
                Return dat
            End If
            Return New Byte() {}
        End Function

        Protected Overridable Function partID(partIn As Byte()) As Long
            Dim idarr(7) As Byte
            Buffer.BlockCopy(partIn, 2, idarr, 0, idarr.Length)
            Dim id As Long = Utilities.BytesToInt64(idarr)
            Return id
        End Function
        ''' <summary>
        ''' Splits the Encapsulated Object into an array of an array of bytes.
        ''' </summary>
        ''' <param name="size">The size of each split array</param>
        ''' <returns>The Array of Array of Bytes of the Encapsulated Object</returns>
        ''' <remarks></remarks>
        Public Overridable Function splitParts(size As Long) As Byte()() Implements IEncapsulation.splitParts
            If (size - 18) > _arr.Length Then Throw New NetLibException( New ArgumentOutOfRangeException("The size is larger than the length of the data."))
            Dim cntp As Long = _arr.Length \ size
            If _arr.Length Mod size <> 0 Then cntp += 1
            Dim toret(cntp - 1)() As Byte
            Dim pos As Integer = 0
            Dim btsrem As Integer = _arr.Length
            For i As Integer = 0 To cntp - 1 Step 1
                Dim current(-1) As Byte
                If btsrem < (size - 18) Then
                    ReDim current(btsrem - 1)
                    Buffer.BlockCopy(_arr, pos, current, 0, btsrem)
                    pos += btsrem
                    btsrem -= btsrem
                Else
                    ReDim current(size - 19)
                    Buffer.BlockCopy(_arr, pos, current, 0, (size - 18))
                    pos += (size - 18)
                    btsrem -= (size - 18)
                End If
                toret(i) = createPart(i, current)
            Next
            Return toret
        End Function
        ''' <summary>
        ''' Combines the split parts into the Encapsulated Object.
        ''' </summary>
        ''' <param name="parts">The Array of Array of Bytes of the Encapsulated Object</param>
        ''' <remarks></remarks>
        Public Overridable Sub combineParts(parts As Byte()()) Implements IEncapsulation.combineParts
            Dim pnumapind As New List(Of Tuple(Of Long, Byte()))
            Dim lengthd As Integer = 4
            For i As Integer = 0 To parts.Length - 1 Step 1
                pnumapind.Add(New Tuple(Of Long, Byte())(partID(parts(i)), processPart(parts(i))))
                If pnumapind(i).Item2.Length = 0 Then Throw New NetLibException( New NullReferenceException("An Error in the Data has caused a corrupted part, at index " & i & "."))
                lengthd += pnumapind(i).Item2.Length
            Next
            pnumapind.Sort(New ICompareTupleOfLongByteArr())
            ReDim _arr(lengthd - 1)
            Dim pos As Integer = 0
            For i As Integer = 0 To pnumapind.Count - 1 Step 1
                Dim cpp As Byte() = pnumapind(i).Item2
                Buffer.BlockCopy(cpp, 0, _arr, pos, cpp.Length)
                pos += cpp.Length
            Next
        End Sub

        Protected Class ICompareTupleOfLongByteArr
            Implements IComparer(Of Tuple(Of Long, Byte()))

            Public Overridable Function compare(x As Tuple(Of Long, Byte()), y As Tuple(Of Long, Byte())) As Integer Implements IComparer(Of Tuple(Of Long, Byte())).Compare
                Return x.Item1.CompareTo(y)
            End Function
        End Class
    End Class

End Namespace
