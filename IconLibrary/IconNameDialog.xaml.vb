Imports IconLibrary.Model

Public NotInheritable Class IconNameDialog
    Inherits ContentDialog

    Class StringContainer
        Public content As New String("")
    End Class

    Public Enum Actions
        SaveAs
        Rename
        NewIcon
    End Enum

    Private oldName As String
    Private cancelled As Boolean
    Private intendedAction As Actions

    Public Sub SetIconName(ByRef TheName As String)
        IconName.Text = TheName
        oldName = IconName.Text
        CheckInput()
    End Sub

    Public Sub SetAction(ByRef action As Actions)
        intendedAction = action

        Select Case intendedAction
            Case Actions.NewIcon
                IconNameEditor.Title = "New icon"
                IconNameEditor.PrimaryButtonText = "Create"
            Case Actions.SaveAs
                IconNameEditor.Title = "Save as"
                IconNameEditor.PrimaryButtonText = "Save"
            Case Actions.Rename
                IconNameEditor.Title = "Rename icon"
                IconNameEditor.PrimaryButtonText = "Rename"
        End Select
    End Sub

    Public Function DialogCancelled() As Boolean
        Return cancelled
    End Function

    Public Function GetIconName() As String
        Return IconName.Text.Trim()
    End Function

    Private Function NameIsValid(Optional ByRef errorMessage As StringContainer = Nothing) As Boolean

        If IconName.Text Is Nothing OrElse IconName.Text.Trim().Equals("") Then
            If errorMessage IsNot Nothing Then
                errorMessage.content = "Specify an icon name"
            End If
            Return False
        End If

        Dim name As String = IconName.Text.Trim()

        If intendedAction = Actions.NewIcon OrElse Not oldName.Equals(name) Then
            ' The existence check is skipped when renaming, and if the name has not been changed
            If IconLibraryViewModel.Current.GetIcon(name) IsNot Nothing Then
                If errorMessage IsNot Nothing Then
                    errorMessage.content = "Choose a new icon name"
                End If
                Return False
            End If
        End If

        If errorMessage IsNot Nothing Then
            errorMessage.content = ""
        End If
        Return True

    End Function

    Private Sub CheckInput()

        Dim errorMessage As New StringContainer()

        If NameIsValid(errorMessage) Then
            IconName.SetValue(BorderBrushProperty, New SolidColorBrush(Windows.UI.Colors.Black))
            IconNameEditor.IsPrimaryButtonEnabled = True
        Else
            IconName.SetValue(BorderBrushProperty, New SolidColorBrush(Windows.UI.Colors.Red))
            IconNameEditor.IsPrimaryButtonEnabled = False
        End If

        ErrorMessageDisplay.Text = errorMessage.content

    End Sub


    Private Sub ContentDialog_PrimaryButtonClick(sender As ContentDialog, args As ContentDialogButtonClickEventArgs)

        IconNameEditor.Hide()
        cancelled = False

    End Sub

    Private Sub ContentDialog_SecondaryButtonClick(sender As ContentDialog, args As ContentDialogButtonClickEventArgs)

        IconNameEditor.Hide()
        cancelled = True

    End Sub

    Private Sub IconName_TextChanged(sender As Object, e As TextChangedEventArgs) Handles IconName.TextChanged
        CheckInput()
    End Sub
End Class
