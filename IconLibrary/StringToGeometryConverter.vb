Imports Windows.UI.Xaml.Markup

Public Class StringToGeometryConverter
    Implements IValueConverter

    Public Shared LastError As String = ""

    Public Function Convert(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.Convert

        LastError = ""

        If value Is Nothing Then
            Return Nothing
        End If

        Dim xaml As String = "<Path " + "xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" + "<Path.Data>" + DirectCast(value, String) + "</Path.Data></Path>"
        Try
            Dim path = XamlReader.Load(xaml)
            Dim geometry As Geometry = path.Data
            ' Detach the PathGeometry from the Path (important!)
            path.Data = Nothing
            Return geometry
        Catch ex As Exception
            LastError = ex.Message
            Return Nothing
        End Try

    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class
