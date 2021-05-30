Namespace Serialize

    ''' <summary>
    ''' Provides a serialization interface
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface ISerialize
        Inherits IDisposable
        ''' <summary>
        ''' Serializes an Object to a Byte Array.
        ''' </summary>
        ''' <param name="obj">The object to serialize</param>
        ''' <returns>A byte array</returns>
        ''' <remarks></remarks>
        Function serializeObject(obj As Object) As Byte()
        ''' <summary>
        ''' Serializes an Object to a Byte Array.
        ''' </summary>
        ''' <param name="obj">The object to serialize</param>
        ''' <returns>A byte array</returns>
        ''' <typeparam name="t">The Type of Object to Accept as a parameter</typeparam>
        ''' <remarks></remarks>
        Function serializeObject(Of t)(obj As t) As Byte()
        ''' <summary>
        ''' Serializes an Object to a String.
        ''' </summary>
        ''' <param name="obj">The object to serialize</param>
        ''' <returns>A string</returns>
        ''' <remarks></remarks>
        Function serialize(obj As Object) As String
        ''' <summary>
        ''' Serializes an Object to a String.
        ''' </summary>
        ''' <param name="obj">The object to serialize</param>
        ''' <returns>A string</returns>
        ''' <typeparam name="t">The Type of Object to Accept as a parameter</typeparam>
        ''' <remarks></remarks>
        Function serialize(Of t)(obj As t) As String
        ''' <summary>
        ''' Deserializes an Object from a byte array.
        ''' </summary>
        ''' <param name="bts">The Byte Array</param>
        ''' <returns>The Object</returns>
        ''' <remarks></remarks>
        Function deSerializeObject(bts As Byte()) As Object
        ''' <summary>
        ''' Deserializes an Object from a byte array.
        ''' </summary>
        ''' <param name="bts">The Byte Array</param>
        ''' <returns>The Object</returns>
        ''' <remarks></remarks>
        ''' <typeparam name="t">The Type of Object to Accept as a Return Value</typeparam>
        Function deSerializeObject(Of t)(bts As Byte()) As t
        ''' <summary>
        ''' Deserializes an Object from a string.
        ''' </summary>
        ''' <param name="str">A String</param>
        ''' <returns>The Object</returns>
        ''' <remarks></remarks>
        Function deSerialize(str As String) As Object
        ''' <summary>
        ''' Deserializes an Object from a string.
        ''' </summary>
        ''' <param name="str">A String</param>
        ''' <returns>The Object</returns>
        ''' <remarks></remarks>
        ''' <typeparam name="t">The Type of Object to Accept as a Return Value</typeparam>
        Function deSerialize(Of t)(str As String) As t
    End Interface

End Namespace
