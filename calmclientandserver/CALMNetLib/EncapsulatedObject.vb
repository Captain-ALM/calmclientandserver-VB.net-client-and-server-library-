Imports captainalm.Serialize

Namespace CALMNetLib
    ''' <summary>
    ''' Represents a Generic Encapsulated Object.
    ''' </summary>
    ''' <typeparam name="t">The Generic Type to Hold (Should Serialize)</typeparam>
    ''' <remarks></remarks>
    Public Class EncapsulatedObject(Of t)
        Implements IEncapsulation

        'EncapsulatedObject(Of t) Part Format:
        '20 Bytes Overhead
        '[0-3] Length Header (4 Bytes) {Includes the length of all headers and checksum trailer, not only the held data}
        '[4-11] Part Sequential Identifier (8 Bytes) [Always 8 bytes]
        '[12-...] Part Data (... Bytes)
        '[...+1-...+9] Check Sum (8 Bytes) [Always 8 bytes]

        Protected intobj As Object
        Protected _ser As ISerialize
        ''' <summary>
        ''' Constructs a EncapsulatedObject with the specified object to be encapsulated.
        ''' </summary>
        ''' <param name="obj">The Object to Encapsulate</param>
        ''' <remarks></remarks>
        Public Sub New(obj As t)
            intobj = obj
            _ser = New Serializer()
        End Sub
        ''' <summary>
        ''' Constructs a EncapsulatedObject with the specified object to be encapsulated and the serializer to use.
        ''' </summary>
        ''' <param name="obj">The Object to Encapsulate</param>
        ''' <param name="ser">The Serializer to Use</param>
        ''' <remarks></remarks>
        Public Sub New(obj As t, ser As ISerialize)
            intobj = obj
            _ser = ser
        End Sub
        ''' <summary>
        ''' Combines the split parts into the Encapsulated Object.
        ''' </summary>
        ''' <param name="parts">The Array of Array of Bytes of the Encapsulated Object</param>
        ''' <remarks></remarks>
        Public Sub combineParts(parts()() As Byte) Implements IEncapsulation.combineParts
            Dim bts(-1) As Byte
            Dim pnumapind As New List(Of Tuple(Of Long, Byte()))
            Dim lengthd As Integer = 4
            For i As Integer = 0 To parts.Length - 1 Step 1
                pnumapind.Add(New Tuple(Of Long, Byte())(partID(parts(i)), processPart(parts(i))))
                If pnumapind(i).Item2.Length = 0 Then Throw New NetLibException(New NullReferenceException("An Error in the Data has caused a corrupted part, at index " & i & "."))
                lengthd += pnumapind(i).Item2.Length
            Next
            pnumapind.Sort(New ICompareTupleOfLongByteArr())
            ReDim bts(lengthd - 1)
            Dim pos As Integer = 0
            For i As Integer = 0 To pnumapind.Count - 1 Step 1
                Dim cpp As Byte() = pnumapind(i).Item2
                Buffer.BlockCopy(cpp, 0, bts, pos, cpp.Length)
                pos += cpp.Length
            Next
            intobj = _ser.deSerializeObject(Of t)(bts)
        End Sub
        ''' <summary>
        ''' Returns the data of the object.
        ''' </summary>
        ''' <value>Object</value>
        ''' <returns>Pure Encapsulated Object Data</returns>
        ''' <remarks></remarks>
        Public Overridable Property data As Object Implements IEncapsulation.data
            Get
                Return intobj
            End Get
            Set(value As Object)
                intobj = value
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
                Return intobj
            End Get
            Set(value As Object)
                intobj = value
            End Set
        End Property
        ''' <summary>
        ''' Returns the Data type of the Pure Encapsulation.
        ''' </summary>
        ''' <value>Type</value>
        ''' <returns>The data type of the pure encapsulation</returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property dataType As Type Implements IEncapsulation.dataType
            Get
                Return GetType(t)
            End Get
        End Property
        ''' <summary>
        ''' Returns the Length of the Encapsulated Object.
        ''' </summary>
        ''' <value>Long</value>
        ''' <returns>The Object Length</returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property Length As Long Implements IEncapsulation.Length
            Get
                If intobj IsNot Nothing Then
                    Return 1
                End If
                Return 0
            End Get
        End Property

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
            'Part overhead (Header and Trailer) is 20 Bytes
            Dim tlen As Integer = 20 + bytesIn.Length
            Dim len As Byte() = Utilities.Int32ToBytes(tlen)
            Dim idarr As Byte() = Utilities.Int64ToBytes(id)
            Dim chksum As Long = calculateCheckSum(bytesIn)
            Dim chksumarr As Byte() = Utilities.Int64ToBytes(chksum)
            Dim partarr(tlen - 1) As Byte
            Buffer.BlockCopy(len, 0, partarr, 0, len.Length)
            Buffer.BlockCopy(idarr, 0, partarr, 4, idarr.Length)
            Buffer.BlockCopy(bytesIn, 0, partarr, 12, bytesIn.Length)
            Buffer.BlockCopy(chksumarr, 0, partarr, 12 + bytesIn.Length, chksumarr.Length)
            Return partarr
        End Function

        Protected Overridable Function processPart(partIn As Byte()) As Byte()
            Dim len(3) As Byte
            Buffer.BlockCopy(partIn, 0, len, 0, len.Length)
            Dim datlen As Integer = Utilities.BytesToInt32(len)
            Dim chksumarr(7) As Byte
            Buffer.BlockCopy(partIn, partIn.Length - 8, chksumarr, 0, chksumarr.Length)
            Dim chksum As Long = Utilities.BytesToInt64(chksumarr)
            Dim dat(datlen - 21) As Byte
            Buffer.BlockCopy(partIn, 12, dat, 0, datlen - 20)
            Dim verify As Boolean = (calculateCheckSum(dat) = chksum)
            If verify Then
                Return dat
            End If
            Return New Byte() {}
        End Function

        Protected Overridable Function partID(partIn As Byte()) As Long
            Dim idarr(7) As Byte
            Buffer.BlockCopy(partIn, 4, idarr, 0, idarr.Length)
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
            Dim bts As Byte() = _ser.serializeObject(Of t)(CType(intobj, t))
            If (size - 20) > bts.Length Then Throw New NetLibException(New ArgumentOutOfRangeException("The size is larger than the length of the data."))
            Dim cntp As Long = bts.Length \ size
            If bts.Length Mod size <> 0 Then cntp += 1
            Dim toret(cntp - 1)() As Byte
            Dim pos As Integer = 0
            Dim btsrem As Integer = bts.Length
            For i As Integer = 0 To cntp - 1 Step 1
                Dim current(-1) As Byte
                If btsrem < (size - 20) Then
                    ReDim current(btsrem - 1)
                    Buffer.BlockCopy(bts, pos, current, 0, btsrem)
                    pos += btsrem
                    btsrem -= btsrem
                Else
                    ReDim current(size - 21)
                    Buffer.BlockCopy(bts, pos, current, 0, (size - 20))
                    pos += (size - 20)
                    btsrem -= (size - 20)
                End If
                toret(i) = createPart(i, current)
            Next
            Return toret
        End Function
        ''' <summary>
        ''' Returns whether the contained data is valid.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>The contained data is valid.</returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property valid As Boolean Implements IEncapsulation.valid
            Get
                If intobj IsNot Nothing Then
                    Return True
                End If
                Return False
            End Get
        End Property

        Protected Class ICompareTupleOfLongByteArr
            Implements IComparer(Of Tuple(Of Long, Byte()))

            Public Overridable Function compare(x As Tuple(Of Long, Byte()), y As Tuple(Of Long, Byte())) As Integer Implements IComparer(Of Tuple(Of Long, Byte())).Compare
                Return x.Item1.CompareTo(y)
            End Function
        End Class
    End Class

End Namespace
