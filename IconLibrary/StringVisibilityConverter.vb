
' <summary>
' This class converts a string value into a Visibility value (if the value Is null Or empty returns a collapsed value).
' </summary>
Public Class StringVisibilityConverter
        Implements IValueConverter

        ' <summary>
        ' Modifies the source data before passing it to the target for display in the UI.
        ' </summary>
        ' <param name="value">The source data being passed to the target.</param>
        ' <param name="targetType">The type of the target property, as a type reference (System.Type for Microsoft .NET, a TypeName helper struct for Visual C++ component extensions (C++/CX)).</param>
        ' <param name="parameter">An optional parameter to be used in the converter logic.</param>
        ' <param name="language">The language of the conversion.</param>
        ' <returns>The value to be passed to the target dependency property.</returns>
        Public Function Convert(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.Convert

            Dim visibility As Visibility = Visibility.Visible

            If value Is Nothing OrElse String.IsNullOrEmpty(value.ToString) Then
                visibility = Visibility.Collapsed
            End If

            If parameter IsNot Nothing Then
                Dim invertResult As Boolean
                Boolean.TryParse(parameter.ToString, invertResult)
                If invertResult = True Then
                    If visibility = Visibility.Visible Then
                        visibility = Visibility.Collapsed
                    Else
                        visibility = Visibility.Visible
                    End If
                End If
            End If
            Return visibility
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.ConvertBack
            Throw New NotImplementedException()
        End Function
    End Class
