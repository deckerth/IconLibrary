' Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

Imports IconLibrary.Model
Imports Windows.UI
Imports Windows.UI.Xaml.Shapes
''' <summary>
''' Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
''' </summary>
Public NotInheritable Class MainPage
    Inherits Page
    Implements INotifyPropertyChanged

    Public Property ViewModel As New PathIconViewModel

    Public Property Preview As PreviewGrid

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Protected Sub OnPropertyChanged(ByVal PropertyName As String)
        Dim e = New PropertyChangedEventArgs(PropertyName)
        RaiseEvent PropertyChanged(Me, e)
    End Sub

    Private _errorMessage As String = ""
    Public Property ErrorMessage As String
        Get
            Return _errorMessage
        End Get
        Set(value As String)
            _errorMessage = value
            OnPropertyChanged("ErrorMessage")
        End Set
    End Property

    Private _libraryPaneOpen As Boolean
    Public Property LibraryPaneOpen As Boolean
        Get
            Return _libraryPaneOpen
        End Get
        Set(value As Boolean)
            If value <> _libraryPaneOpen Then
                _libraryPaneOpen = value
                OnPropertyChanged("LibraryPaneOpen")
            End If
        End Set
    End Property

    Public Sub New()
        InitializeComponent()

        Preview = New PreviewGrid(PreviewCanvas)

        DataContext = ViewModel

        '' Import
        'IconLibrary.Add(New PathIconViewModel With {.Markup = "F0 M 1,1 h10 v4 h-1 v-3 h-8 v13 h8 v-3 h1 v4 h-10 v-15 M 14,7 h-8 v2 h8 v-2 M 6,7 l2,-2 l-1,-1 l-4,4 l4,4 l1,-1 l-2,-2"})

        '' Hub view 
        'IconLibrary.Add(New PathIconViewModel With {.Markup = "M2,3L14,3 14,6 2,6 2,3 M2,9L14,9 14,12 2,12 2,9 M2,15.0000009536743L14,15.0000009536743 14,18 2,18 2,15.0000009536743 M18,3L30,3 30,6 18,6 18,3 M18,9L30,9 30,12 18,12 18,9"})

        '' Circle
        'IconLibrary.Add(New PathIconViewModel With {.Markup = "F0 M10,0 a5,5,0,1,1,-1,0 M10,1 a4,4,0,1,1,-1,0"})

        '' Polygon
        'IconLibrary.Add(New PathIconViewModel With {.Markup = "F0 M0,0 l1,10 l10,1 l-1,-10 l-10,-1"})

    End Sub

    Private Sub RefreshDisplay()
        Dim geo = (New StringToGeometryConverter).Convert(ViewModel.Markup, GetType(Geometry), Nothing, Nothing)
        ErrorMessage = StringToGeometryConverter.LastError
        If geo IsNot Nothing Then
            Preview.Draw(TryCast(geo, PathGeometry))
        End If
    End Sub

    Private Sub Refresh_Click(sender As Object, e As RoutedEventArgs)
        RefreshDisplay()
    End Sub

    Private Sub PointerEntered(sender As Object, e As PointerRoutedEventArgs)

    End Sub

    Private Sub CopyToClipboad_Click(sender As Object, e As RoutedEventArgs)
        Dim selectedId As Guid = DirectCast(sender, MenuFlyoutItem).CommandParameter
        Dim pathIcon As PathIconViewModel = ViewModel.IconLibrary.GetIcon(selectedId)
        If pathIcon IsNot Nothing Then
            Dim markup As New DataTransfer.DataPackage
            markup.SetText(pathIcon.Markup)
            DataTransfer.Clipboard.SetContent(markup)
        End If
    End Sub

    Private Sub EditIcon_Click(sender As Object, e As RoutedEventArgs)
        Dim selectedId As Guid = DirectCast(sender, MenuFlyoutItem).CommandParameter
        Dim pathIcon As PathIconViewModel = ViewModel.IconLibrary.GetIcon(selectedId)
        If pathIcon IsNot Nothing Then
            ViewModel.Def = New PathIconDefinition(pathIcon.Def)
            LibraryPaneOpen = False
            RefreshDisplay()
        End If
    End Sub

    Private Sub DeleteIcon_Click(sender As Object, e As RoutedEventArgs)
        Dim selectedId As Guid = DirectCast(sender, MenuFlyoutItem).CommandParameter
        Dim pathIcon As PathIconViewModel = ViewModel.IconLibrary.GetIcon(selectedId)
        If pathIcon IsNot Nothing Then
            ViewModel.IconLibrary.DeleteIcon(pathIcon.Def.Id)
            If ViewModel.Name.Equals(pathIcon.Name) Then
                ViewModel.IconChanged = True
            End If
        End If
    End Sub

    Private Sub LibraryToggleButton_Clicked(sender As Object, e As RoutedEventArgs)
        LibraryPaneOpen = Not _libraryPaneOpen
    End Sub

    Private StartPoint As Point
    Private PreviewLine As Line
    Private PreviewRectangle As Rectangle
    Private PreviewEllipse As Ellipse

    Private Sub PreviewGrid_PointerPressed(sender As Object, e As PointerRoutedEventArgs) Handles PreviewCanvas.PointerPressed
        Dim ptr As Pointer = e.Pointer
        If ptr.PointerDeviceType = Windows.Devices.Input.PointerDeviceType.Mouse Then
            Dim ptrPt As Windows.UI.Input.PointerPoint = e.GetCurrentPoint(PreviewCanvas)
            If ptrPt.Properties.IsLeftButtonPressed Then
                StartPoint = ptrPt.Position
                If CreateLine.IsChecked Then
                    PreviewLine = New Line With {
                    .X1 = StartPoint.X,
                    .Y1 = StartPoint.Y,
                    .X2 = StartPoint.X,
                    .Y2 = StartPoint.Y,
                    .Stroke = New SolidColorBrush(Colors.DarkOrange),
                    .StrokeThickness = 4}
                    PreviewCanvas.Children.Add(PreviewLine)
                ElseIf CreateRectangle.IsChecked Then
                    PreviewRectangle = New Rectangle With {
                    .Width = 0,
                    .Height = 0,
                    .Stroke = New SolidColorBrush(Colors.DarkOrange),
                    .StrokeThickness = 4}
                    Canvas.SetTop(PreviewRectangle, StartPoint.Y)
                    Canvas.SetLeft(PreviewRectangle, StartPoint.X)
                    PreviewCanvas.Children.Add(PreviewRectangle)
                ElseIf CreateCircle.IsChecked Then
                    PreviewEllipse = New Ellipse With {
                    .Width = 0,
                    .Height = 0,
                    .Stroke = New SolidColorBrush(Colors.DarkOrange),
                    .StrokeThickness = 4}
                    Canvas.SetTop(PreviewEllipse, StartPoint.Y)
                    Canvas.SetLeft(PreviewEllipse, StartPoint.X)
                    PreviewCanvas.Children.Add(PreviewEllipse)
                End If
            End If
        End If
    End Sub

    Private Sub ClearPreviewElements()
        If PreviewLine IsNot Nothing Then
            PreviewCanvas.Children.Remove(PreviewLine)
            PreviewLine = Nothing
        End If
        If PreviewRectangle IsNot Nothing Then
            PreviewCanvas.Children.Remove(PreviewRectangle)
            PreviewRectangle = Nothing
        End If
        If PreviewEllipse IsNot Nothing Then
            PreviewCanvas.Children.Remove(PreviewEllipse)
            PreviewEllipse = Nothing
        End If
        Preview.CurrentPreviewInfo = ""
    End Sub

    Private Sub PreviewGrid_PointerReleased(sender As Object, e As PointerRoutedEventArgs) Handles PreviewCanvas.PointerReleased
        Dim ptr As Pointer = e.Pointer
        If ptr.PointerDeviceType = Windows.Devices.Input.PointerDeviceType.Mouse Then
            Dim ptrPt As Windows.UI.Input.PointerPoint = e.GetCurrentPoint(PreviewCanvas)
            If ptrPt.Properties.PointerUpdateKind = Input.PointerUpdateKind.LeftButtonReleased Then
                Dim markup As String = ""
                If String.IsNullOrEmpty(ViewModel.Markup) Then
                    ViewModel.Markup = "F0"
                End If
                If PreviewLine IsNot Nothing Then
                    Dim geo = (New StringToGeometryConverter).Convert(ViewModel.Markup, GetType(Geometry), Nothing, Nothing)
                    ErrorMessage = StringToGeometryConverter.LastError
                    If geo IsNot Nothing Then
                        markup = Preview.CreateLineMarkup(geo, PreviewLine)
                    End If
                End If
                If PreviewRectangle IsNot Nothing Then
                    Dim geo = (New StringToGeometryConverter).Convert(ViewModel.Markup, GetType(Geometry), Nothing, Nothing)
                    ErrorMessage = StringToGeometryConverter.LastError
                    If geo IsNot Nothing Then
                        markup = Preview.CreateRectangleMarkup(geo, PreviewRectangle)
                    End If
                End If
                If PreviewEllipse IsNot Nothing Then
                    Dim geo = (New StringToGeometryConverter).Convert(ViewModel.Markup, GetType(Geometry), Nothing, Nothing)
                    ErrorMessage = StringToGeometryConverter.LastError
                    If geo IsNot Nothing Then
                        markup = Preview.CreateEllipseMarkup(geo, PreviewEllipse)
                    End If
                End If
                ClearPreviewElements()
                If Not String.IsNullOrEmpty(markup) Then
                    ViewModel.Markup = ViewModel.Markup + vbCrLf + markup
                End If
                RefreshDisplay()
            End If
        End If
    End Sub

    Private Sub PreviewGrid_PointerMoved(sender As Object, e As PointerRoutedEventArgs) Handles PreviewCanvas.PointerMoved
        Dim ptr As Pointer = e.Pointer
        If ptr.PointerDeviceType = Windows.Devices.Input.PointerDeviceType.Mouse Then
            Dim ptrPt As Windows.UI.Input.PointerPoint = e.GetCurrentPoint(PreviewCanvas)
            If ptrPt.Properties.IsLeftButtonPressed Then
                If PreviewLine IsNot Nothing Then
                    PreviewLine.X2 = ptrPt.Position.X
                    PreviewLine.Y2 = ptrPt.Position.Y
                    Preview.SetPreviewInfo(PreviewLine)
                ElseIf PreviewRectangle IsNot Nothing Then
                    If ptrPt.Position.X >= StartPoint.X Then
                        PreviewRectangle.Width = ptrPt.Position.X - StartPoint.X
                    Else
                        Canvas.SetLeft(PreviewRectangle, ptrPt.Position.X)
                        PreviewRectangle.Width = StartPoint.X - ptrPt.Position.X
                    End If
                    If ptrPt.Position.Y >= StartPoint.Y Then
                        PreviewRectangle.Height = ptrPt.Position.Y - StartPoint.Y
                    Else
                        Canvas.SetTop(PreviewRectangle, ptrPt.Position.Y)
                        PreviewRectangle.Height = StartPoint.Y - ptrPt.Position.Y
                    End If
                    Preview.SetPreviewInfo(PreviewRectangle)
                ElseIf PreviewEllipse IsNot Nothing Then
                    If ptrPt.Position.X >= StartPoint.X Then
                        PreviewEllipse.Width = ptrPt.Position.X - StartPoint.X
                    Else
                        Canvas.SetLeft(PreviewEllipse, ptrPt.Position.X)
                        PreviewEllipse.Width = StartPoint.X - ptrPt.Position.X
                    End If
                    If ptrPt.Position.Y >= StartPoint.Y Then
                        PreviewEllipse.Height = ptrPt.Position.Y - StartPoint.Y
                    Else
                        Canvas.SetTop(PreviewEllipse, ptrPt.Position.Y)
                        PreviewEllipse.Height = StartPoint.Y - ptrPt.Position.Y
                    End If
                    Preview.SetPreviewInfo(PreviewEllipse)
                End If
            End If
        End If
    End Sub

    Private Sub PreviewGrid_PointerExited(sender As Object, e As PointerRoutedEventArgs) Handles PreviewCanvas.PointerExited
        Dim ptr As Pointer = e.Pointer
        If ptr.PointerDeviceType = Windows.Devices.Input.PointerDeviceType.Mouse Then
            Dim ptrPt As Windows.UI.Input.PointerPoint = e.GetCurrentPoint(PreviewCanvas)
            If ptrPt.Properties.IsLeftButtonPressed Then
                ClearPreviewElements()
            End If
        End If
    End Sub

    Private Sub CreateLine_Click(sender As Object, e As RoutedEventArgs)
        CreateRectangle.IsChecked = False
        CreateCircle.IsChecked = False
    End Sub

    Private Sub CreateRectangle_CLick(sender As Object, e As RoutedEventArgs)
        CreateLine.IsChecked = False
        CreateCircle.IsChecked = False
    End Sub

    Private Sub CreateCircle_CLick(sender As Object, e As RoutedEventArgs)
        CreateLine.IsChecked = False
        CreateRectangle.IsChecked = False
    End Sub

    Private Sub ClearMarkup_Click(sender As Object, e As RoutedEventArgs)
        ViewModel.Markup = "F0"
        Preview.Clear()
    End Sub

    Private Async Function AddIcon_Click(sender As Object, e As RoutedEventArgs) As Task
        Await ViewModel.AddIconAsync()
        RefreshDisplay()
    End Function

    Private Async Sub SaveIcon_Click(sender As Object, e As RoutedEventArgs)
        Await ViewModel.SaveAsync()
    End Sub

    Private Async Sub RenameIcon_Click(sender As Object, e As RoutedEventArgs)
        Await ViewModel.RenameIconAsync()
    End Sub

    Private Async Sub SaveIconAs_Click(sender As Object, e As RoutedEventArgs)
        Await ViewModel.SaveAsAsync()
    End Sub

    Private Async Sub ExportIcons_Click(sender As Object, e As RoutedEventArgs)
        Await ViewModel.IconLibrary.ExportIcons()
    End Sub

    Private Async Sub ImportIcons_Click(sender As Object, e As RoutedEventArgs)
        Await ViewModel.IconLibrary.ImportIcons()
    End Sub

End Class
