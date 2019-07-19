Namespace calmclientandserver

    ''' <summary>
    ''' Object Encapsulation Class.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()>
    <Obsolete("This is deprecated, use the calmnetlib or calmnetmarshal namespaces instead.")>
    Public Class Encapsulation
        Private _data As String = ""
        ''' <summary>
        ''' Creates a new Encapsulation Instance with a New Empty Object.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            _data = ConvertObjectToString(New Object())
        End Sub
        ''' <summary>
        ''' Creates a new Encapsulation Instance with the passed object.
        ''' </summary>
        ''' <param name="obj">The passed object to encapsulate.</param>
        ''' <remarks></remarks>
        Public Sub New(obj As Object)
            _data = ConvertObjectToString(obj)
        End Sub
        ''' <summary>
        ''' Returns the data of the encapsulation object.
        ''' </summary>
        ''' <value>The data of the encapsulation object.</value>
        ''' <returns>The data of the encapsulation object.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Data As String
            Get
                Return _data
            End Get
        End Property
        ''' <summary>
        ''' Gets the object held by the encapsulation object.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObject() As Object
            Return ConvertStringToObject(_data)
        End Function
    End Class

End Namespace
