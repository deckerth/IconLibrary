Imports Windows.Storage

Namespace Global.IconLibrary.Model

    Public Class IconRepository

        Public Shared Current As IconRepository

        Public Sub New()
            Current = Me
        End Sub

        Const NameField As String = "Name"
        Const MarkupField As String = "Markup"
        Const IconsContainerName As String = "IconList"

#Region "AccessIcons"
        Public Function GetIcons() As List(Of PathIconDefinition)
            Dim Icons As New List(Of PathIconDefinition)
            Dim localSettings = Windows.Storage.ApplicationData.Current.LocalSettings
            Dim iconList = localSettings.CreateContainer(IconsContainerName, Windows.Storage.ApplicationDataCreateDisposition.Always)

            Icons.Clear()

            For Each item In iconList.Values
                Dim newIcon As New PathIconDefinition
                Dim iconComposite As ApplicationDataCompositeValue = item.Value

                Try
                    newIcon.Name = iconComposite(NameField)
                    newIcon.Markup = iconComposite(MarkupField)
                Catch ex As Exception
                    Continue For
                End Try
                If newIcon.Name Is Nothing Then
                    Continue For
                End If

                Icons.Add(newIcon)
            Next

            Return Icons
        End Function

        Public Sub AddIcon(newIcon As PathIconDefinition)

            'Dim newInst As New Installation With {.Name = newName}

            If newIcon Is Nothing OrElse String.IsNullOrEmpty(newIcon.Name) Then
                Return
            End If

            Dim localSettings = ApplicationData.Current.LocalSettings
            Dim iconList = localSettings.CreateContainer(IconsContainerName, ApplicationDataCreateDisposition.Always)
            Dim iconComposite = New ApplicationDataCompositeValue()
            iconComposite(NameField) = newIcon.Name
            iconComposite(MarkupField) = newIcon.Markup

            iconList.Values(Guid.NewGuid().ToString()) = iconComposite

        End Sub

        Public Sub DeleteIcon(name As String)

            Dim localSettings = ApplicationData.Current.LocalSettings
            Dim iconList = localSettings.CreateContainer(IconsContainerName, ApplicationDataCreateDisposition.Always)
            Dim index As String
            For Each item In iconList.Values
                Dim iconComposite As ApplicationDataCompositeValue = item.Value
                If iconComposite(NameField).Equals(name) Then
                    index = item.Key
                    Exit For
                End If
            Next
            If index IsNot Nothing Then
                iconList.Values.Remove(index)
            End If
        End Sub

        Public Sub UpdateIcon(oldName As String,
                                   Optional newName As String = Nothing,
                                   Optional newMarkup As String = Nothing)

            Dim localSettings = ApplicationData.Current.LocalSettings
            Dim iconList = localSettings.CreateContainer(IconsContainerName, ApplicationDataCreateDisposition.Always)

            For Each item In iconList.Values
                Dim iconComposite As ApplicationDataCompositeValue = item.Value
                Dim currentName As String
                Try
                    currentName = iconComposite(NameField)
                Catch ex As Exception
                    Continue For
                End Try
                If currentName.Equals(oldName) Then
                    Dim newIconComposite As ApplicationDataCompositeValue = New ApplicationDataCompositeValue()

                    If newName Is Nothing Then
                        newIconComposite(NameField) = oldName
                    Else
                        newIconComposite(NameField) = newName
                    End If

                    If newMarkup Is Nothing Then
                        newIconComposite(MarkupField) = iconComposite(MarkupField)
                    Else
                        newIconComposite(MarkupField) = newMarkup
                    End If

                    iconList.Values.Remove(item.Key)
                    iconList.Values(item.Key) = newIconComposite
                    Exit For
                End If
            Next

        End Sub
#End Region

    End Class

End Namespace
