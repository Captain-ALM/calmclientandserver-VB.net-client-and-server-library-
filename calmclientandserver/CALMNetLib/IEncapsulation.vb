'
' Created by SharpDevelop.
' User: Alfred
' Date: 20/05/2019
' Time: 14:34
' 
' To change this template use Tools | Options | Coding | Edit Standard Headers.
'
Namespace CALMNetLib
    ''' <summary>
    ''' Defines an Encapsulation Interface.
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface IEncapsulation
        ''' <summary>
        ''' Returns the Length of the Encapsulated Object.
        ''' </summary>
        ''' <value>Long</value>
        ''' <returns>The Object Length</returns>
        ''' <remarks></remarks>
        ReadOnly Property Length As Long
        ''' <summary>
        ''' Returns the data of the object.
        ''' </summary>
        ''' <value>Object</value>
        ''' <returns>Pure Encapsulated Object Data</returns>
        ''' <remarks></remarks>
        Property data As Object
        ''' <summary>
        ''' Returns the data of the object at a specified index.
        ''' </summary>
        ''' <param name="index">The Index as a Long Value</param>
        ''' <value>Object</value>
        ''' <returns>Pure Encapsulated Object data at the specified index</returns>
        ''' <remarks></remarks>
        Property data(index As Long) As Object
        ''' <summary>
        ''' Returns whether the contained data is valid.
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns>The contained data is valid.</returns>
        ''' <remarks></remarks>
        ReadOnly Property valid As Boolean
        ''' <summary>
        ''' Splits the Encapsulated Object into an array of an array of bytes.
        ''' </summary>
        ''' <param name="size">The size of each split array</param>
        ''' <returns>The Array of Array of Bytes of the Encapsulated Object</returns>
        ''' <remarks></remarks>
        Function splitParts(size As Long) As Byte()()
        ''' <summary>
        ''' Combines the split parts into the Encapsulated Object.
        ''' </summary>
        ''' <param name="parts">The Array of Array of Bytes of the Encapsulated Object</param>
        ''' <remarks></remarks>
        Sub combineParts(parts As Byte()())
        ''' <summary>
        ''' Returns the Data type of the Pure Encapsulation.
        ''' </summary>
        ''' <value>Type</value>
        ''' <returns>The data type of the pure encapsulation</returns>
        ''' <remarks></remarks>
        ReadOnly Property dataType As Type
    End Interface

End Namespace
