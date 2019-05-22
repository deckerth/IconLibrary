' Die Elementvorlage "Inhaltsdialogfeld" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

Public NotInheritable Class OverwriteQuestion
    Inherits ContentDialog

    Public Sub SetIconName(name As String)
        OverwriteQuestion.Text = "The icon " + name + " does already exist. Do you want to overwrite?"
    End Sub

    Private decision As IconLibraryViewModel.CollisionOptions = IconLibraryViewModel.CollisionOptions.Undefined

    Public Function GetDecision() As IconLibraryViewModel.CollisionOptions
        Return decision
    End Function

    Private Sub ContentDialog_PrimaryButtonClick(sender As ContentDialog, args As ContentDialogButtonClickEventArgs)
        If RememberDecision.IsChecked Then
            decision = IconLibraryViewModel.CollisionOptions.OverwriteAll
        Else
            decision = IconLibraryViewModel.CollisionOptions.Overwrite
        End If
    End Sub

    Private Sub ContentDialog_SecondaryButtonClick(sender As ContentDialog, args As ContentDialogButtonClickEventArgs)
        If RememberDecision.IsChecked Then
            decision = IconLibraryViewModel.CollisionOptions.SkipAll
        Else
            decision = IconLibraryViewModel.CollisionOptions.Skip
        End If
    End Sub
End Class
