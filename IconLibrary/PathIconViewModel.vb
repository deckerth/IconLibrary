Imports IconLibrary.Model
Imports Windows.UI.Popups

Public Class PathIconViewModel
    Implements INotifyPropertyChanged

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Protected Sub OnPropertyChanged(ByVal PropertyName As String)
        Dim e = New PropertyChangedEventArgs(PropertyName)
        RaiseEvent PropertyChanged(Me, e)
    End Sub

    Public Property IsNew As Boolean
    Public Property IsUnnamed As Boolean

    Public ReadOnly Property IconLibrary As IconLibraryViewModel
        Get
            Return IconLibraryViewModel.Current
        End Get
    End Property

    Private _def As PathIconDefinition
    Public Property Def As PathIconDefinition
        Get
            Return _def
        End Get
        Set(value As PathIconDefinition)
            _def = value
            IconChanged = False
            OnPropertyChanged("Markup")
            OnPropertyChanged("Name")
            IsNew = False
            IsUnnamed = False
        End Set
    End Property

    Private _iconChanged As Boolean
    Public Property IconChanged As Boolean
        Get
            Return _iconChanged
        End Get
        Set(value As Boolean)
            If value <> _iconChanged Then
                _iconChanged = value
                OnPropertyChanged("IconChanged")
            End If
        End Set
    End Property

    Public Property Markup As String
        Get
            Return _def.Markup
        End Get
        Set(value As String)
            If Not _def.Markup.Equals(value) Then
                _def.Markup = value
                IconChanged = True
                OnPropertyChanged("Markup")
            End If
        End Set
    End Property

    Public Property Name As String
        Get
            Return _def.Name
        End Get
        Set(value As String)
            If Not _def.Name.Equals(value) Then
                _def.Name = value
                OnPropertyChanged("Name")
            End If
        End Set
    End Property

    Private Function GetNewIconName() As String
        Dim name As String = "NewIcon"
        Dim index = 1

        Do Until IconLibraryViewModel.Current.GetIcon(name) Is Nothing
            name = "NewIcon" + index.ToString()
            index = index + 1
        Loop

        Return name
    End Function

    Public Sub New()
        _def = New PathIconDefinition With {.Name = GetNewIconName(), .Markup = "F0"}
        IconChanged = False
        OnPropertyChanged("Markup")
        IsNew = True
        IsUnnamed = True
    End Sub

    Public Sub New(icon As PathIconDefinition)
        _def = icon
        IconChanged = False
        OnPropertyChanged("Markup")
        IsNew = False
        IsUnnamed = False
    End Sub

    Private _cancelled As Boolean
    Private _save As Boolean
    Public Async Function AddIconAsync() As Task

        If IconChanged Then
            Dim saveDialog = New MessageDialog("Save changes first?")

            ' Add commands and set their callbacks 
            saveDialog.Commands.Add(New UICommand("Yes", Sub(command) _save = True))
            saveDialog.Commands.Add(New UICommand("No", Sub(command) _save = False))
            saveDialog.Commands.Add(New UICommand("Cancel", Sub(command) _cancelled = True))

            _cancelled = False
            Await saveDialog.ShowAsync()

            If _cancelled Then
                Return
            End If

            If _save Then
                Await SaveAsync()
            End If
        End If

        Dim editDialog As New IconNameDialog
        editDialog.SetAction(IconNameDialog.Actions.NewIcon)
        Await editDialog.ShowAsync()
        If Not editDialog.DialogCancelled() Then
            Def = New PathIconDefinition With {.Name = editDialog.GetIconName(), .Markup = "F0"}
            IconChanged = True
            IsNew = True
            IsUnnamed = False
        End If

    End Function

    Public Async Function SaveAsync(Optional immediately As Boolean = False) As Task

        If Not IconChanged Then
            Return
        End If

        If Not immediately And IsUnnamed Then
            Dim dialog = New IconNameDialog
            dialog.SetAction(IconNameDialog.Actions.SaveAs)
            dialog.SetIconName(_def.Name)
            Await dialog.ShowAsync()
            If dialog.DialogCancelled() Then
                Return
            End If
            Name = dialog.GetIconName()
        End If

        If IsNew Then
            IconLibrary.AddIcon(Me)
            IsNew = False
            IsUnnamed = False
        Else
            IconLibrary.SaveIcon(Me)
        End If

        IconChanged = False
    End Function

    Public Async Function SaveAsAsync() As Task

        Dim dialog = New IconNameDialog
        dialog.SetAction(IconNameDialog.Actions.SaveAs)
        If Not IsUnnamed Then
            dialog.SetIconName(_def.Name)
        End If
        Await dialog.ShowAsync()
        If dialog.DialogCancelled() Then
            Return
        End If

        If _def.Name.Equals(dialog.GetIconName()) Then
            If IsNew Then
                IconLibrary.AddIcon(Me)
                IsNew = False
                IsUnnamed = False
            ElseIf IconChanged = True Then
                IconLibrary.SaveIcon(Me)
            End If
        Else
            Name = dialog.GetIconName()
            IconLibrary.AddIcon(Me)
        End If

        IconChanged = False
    End Function

    Public Async Function RenameIconAsync() As Task

        Dim dialog = New IconNameDialog
        dialog.SetAction(IconNameDialog.Actions.Rename)
        If Not IsUnnamed Then
            dialog.SetIconName(_def.Name)
        End If
        Await dialog.ShowAsync()
        If dialog.DialogCancelled() Then
            Return
        End If

        If _def.Name.Equals(dialog.GetIconName()) Then
            Return
        ElseIf IsNew Then
            Name = dialog.GetIconName()
            IconLibrary.AddIcon(Me)
            IsNew = False
            IsUnnamed = False
        Else
            IconLibrary.RenameIcon(Me, dialog.GetIconName())
        End If

        IconChanged = False

    End Function

End Class
