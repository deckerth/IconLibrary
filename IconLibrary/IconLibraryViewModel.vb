Imports IconLibrary
Imports IconLibrary.Model
Imports Windows.Storage
Imports Windows.Storage.Pickers
Imports Windows.Storage.Provider
Imports Windows.UI.Popups

Public Class IconLibraryViewModel

    Public Property Icons As New ObservableCollection(Of PathIconViewModel)

    Private Shared _current As IconLibraryViewModel = Nothing
    Public Shared ReadOnly Property Current As IconLibraryViewModel
        Get
            If _current Is Nothing Then
                _current = New IconLibraryViewModel
            End If
            Return _current
        End Get
    End Property

    Public Sub New()
        For Each item In IconRepository.Current.GetIcons()
            Icons.Add(New PathIconViewModel(item))
        Next
    End Sub

    Public Function GetIcon(theName As String) As PathIconViewModel

        Dim matches = Icons.Where(Function(other) other.Name.Equals(theName))
        If matches.Count() = 1 Then
            Return matches.First()
        Else
            Return Nothing
        End If

    End Function

    Public Function GetIcon(theId As Guid) As PathIconViewModel

        Dim matches = Icons.Where(Function(other) other.Def.Id.Equals(theId))
        If matches.Count() = 1 Then
            Return matches.First()
        Else
            Return Nothing
        End If

    End Function

    Public Sub AddIcon(newIcon As PathIconViewModel)

        If newIcon Is Nothing OrElse String.IsNullOrEmpty(newIcon.Name) Then
            Return
        End If

        Icons.Add(newIcon)

        IconRepository.Current.AddIcon(newIcon.Def)
    End Sub

    Public Sub DeleteIcon(theId As Guid)

        Dim toDelete As PathIconViewModel = GetIcon(theId)

        If toDelete IsNot Nothing Then
            Icons.Remove(toDelete)
        End If

        IconRepository.Current.DeleteIcon(toDelete.Name)
    End Sub

    Public Sub RenameIcon(theIcon As PathIconViewModel, newName As String)

        Dim oldName = theIcon.Name

        theIcon.Name = newName

        Dim icon = GetIcon(theIcon.Def.Id)
        If icon IsNot Nothing Then
            icon.Name = theIcon.Name
            icon.Markup = theIcon.Markup
        End If

        IconRepository.Current.UpdateIcon(oldName, newName, theIcon.Markup)

    End Sub

    Public Sub SaveIcon(theIcon As PathIconViewModel)

        Dim icon = GetIcon(theIcon.Def.Id)
        If icon IsNot Nothing Then
            icon.Markup = theIcon.Markup
        End If

        IconRepository.Current.UpdateIcon(theIcon.Name, newMarkup:=theIcon.Markup)

    End Sub

    Private Const RootNode As String = "IconLibrary"
    Private Const IconNode As String = "Icon"
    Private Const NameAttr As String = "Name"
    Private Const MarkupAttr As String = "Markup"

    Private Async Function ExportIconsToXML(xmlFile As StorageFile) As Task(Of Boolean)

        Try
            Dim xmlDocument As New Windows.Data.Xml.Dom.XmlDocument

            Dim rootElement = xmlDocument.CreateElement(RootNode)
            Dim root = xmlDocument.AppendChild(rootElement)

            For Each icon In Icons
                Dim element = xmlDocument.CreateElement(IconNode)
                element.SetAttribute(NameAttr, icon.Name)
                element.SetAttribute(MarkupAttr, icon.Markup)
                root.AppendChild(element)
            Next

            Await xmlDocument.SaveToFileAsync(xmlFile)
        Catch ex As Exception
            Return False
        End Try
        Return True

    End Function

    Public Enum CollisionOptions
        Undefined
        Overwrite
        OverwriteAll
        Skip
        SkipAll
    End Enum

    Private collisionOption As CollisionOptions

    Private Async Function ImportIconsFromXML(xmlFile As StorageFile) As Task

        collisionOption = CollisionOptions.Undefined

        Try
            Dim xmlDoc = Await Windows.Data.Xml.Dom.XmlDocument.LoadFromFileAsync(xmlFile)

            For Each topLevelNode In xmlDoc.ChildNodes
                If topLevelNode.NodeName.Equals(RootNode) Then
                    For Each element In topLevelNode.ChildNodes
                        Dim newIcon As New PathIconViewModel
                        Select Case element.NodeName
                            Case IconNode
                                For Each attribute In element.Attributes
                                    Select Case attribute.NodeName
                                        Case NameAttr
                                            newIcon.Name = attribute.NodeValue
                                        Case MarkupAttr
                                            newIcon.Markup = attribute.NodeValue
                                    End Select
                                Next
                                If Not String.IsNullOrEmpty(newIcon.Name) Then
                                    Dim existingIcon = GetIcon(newIcon.Name)
                                    If existingIcon IsNot Nothing Then
                                        Await AskForOverwrite(newIcon.Name)
                                        If collisionOption = CollisionOptions.Overwrite OrElse collisionOption = CollisionOptions.OverwriteAll Then
                                            existingIcon.Markup = newIcon.Markup
                                            IconRepository.Current.UpdateIcon(newIcon.Name, newMarkup:=newIcon.Markup)
                                        End If
                                    Else
                                        Icons.Add(newIcon)
                                        IconRepository.Current.AddIcon(newIcon.Def)
                                    End If
                                End If
                        End Select
                    Next
                End If
            Next
        Catch ex As Exception
        End Try

    End Function

    Private Async Function AskForOverwrite(name As String) As Task

        If collisionOption = CollisionOptions.OverwriteAll OrElse collisionOption = CollisionOptions.SkipAll Then
            Return
        End If

        Dim dialog = New OverwriteQuestion
        dialog.SetIconName(name)
        Await dialog.ShowAsync()
        collisionOption = dialog.GetDecision()

    End Function

    Public Async Function ExportIcons() As Task
        Dim savepicker As New FileSavePicker With {
            .SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        }

        ' Dropdown of file types the user can save the file as
        savepicker.FileTypeChoices.Add("XML File", New List(Of String) From {".xml"})
        ' Default file name if the user does Not type one in Or select a file to replace
        savepicker.SuggestedFileName = "MyIcons"

        Dim File As StorageFile = Await savepicker.PickSaveFileAsync()
        If File IsNot Nothing Then
            ' Prevent updates to the remote version of the file until we finish making changes And call CompleteUpdatesAsync.
            CachedFileManager.DeferUpdates(File)
            Dim successfullySaved As Boolean = Await ExportIconsToXML(File)

            Dim status As FileUpdateStatus = Await CachedFileManager.CompleteUpdatesAsync(File)
            If status = FileUpdateStatus.Complete And successfullySaved Then
                Await New MessageDialog("The icons have been successfully exported").ShowAsync()
            Else
                Await New MessageDialog("The export failed").ShowAsync()
            End If
        End If

    End Function

    Public Async Function ImportIcons() As Task

        Dim openPicker As FileOpenPicker = New FileOpenPicker()
        openPicker.ViewMode = PickerViewMode.Thumbnail
        openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        openPicker.FileTypeFilter.Add(".xml")
        Dim File As StorageFile = Await openPicker.PickSingleFileAsync()

        If File IsNot Nothing Then
            Await ImportIconsFromXML(File)
        End If

    End Function

End Class
