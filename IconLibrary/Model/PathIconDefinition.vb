Namespace Global.IconLibrary.Model

    Public Class PathIconDefinition

        Public Property Id As Guid
        Public Property Markup As String
        Public Property Name As String

        Public Sub New()
            Id = Guid.NewGuid()
        End Sub

        Public Sub New(icon As PathIconDefinition)
            Id = icon.Id
            Name = New String(icon.Name)
            Markup = New String(icon.Markup)
        End Sub

    End Class

End Namespace
