'
' Created by SharpDevelop.
' User: Alfred
' Date: 20/05/2019
' Time: 14:34
' 
' To change this template use Tools | Options | Coding | Edit Standard Headers.
'
Namespace CALMNetLib

    Public Interface IEncapsulation
        ReadOnly Property Length As Long
        Property data As Object
        Property data(index As Long) As Object
        ReadOnly Property valid As Boolean
        Function splitParts(size As Long) As Byte()()
        Sub combineParts(parts As Byte()())
        ReadOnly Property dataType As Type
    End Interface

End Namespace
