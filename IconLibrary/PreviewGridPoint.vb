Imports Windows.UI
Imports Windows.UI.Text

Imports System.Drawing
Imports Windows.UI.Xaml.Shapes

Public Class PreviewGridPoint

    Public Shared BlackBrush As Brush = New SolidColorBrush(Colors.Black)
    Public Shared WhiteBrush As Brush = New SolidColorBrush(Colors.White)
    Public Shared BlueBrush As Brush = New SolidColorBrush(Colors.Blue)
    Public Shared GreenBrush As Brush = New SolidColorBrush(Colors.Green)
    Public Shared RedBrush As Brush = New SolidColorBrush(Colors.Red)
    Public Shared MagentaBrush As Brush = New SolidColorBrush(Colors.Magenta)
    Public Shared GridBrush As Brush = New SolidColorBrush(Colors.Gray)

    Public Const Size As Integer = 20

    Private _x As Integer
    Public ReadOnly Property X As Integer
        Get
            Return _x
        End Get
    End Property

    Private _y As Integer
    Public ReadOnly Property Y As Integer
        Get
            Return _y
        End Get
    End Property

    Private container As Canvas
    Private triangle As Polygon
    Private grid As PreviewGrid

    Public Enum PointState
        NotSet
        IsSet
        StartPoint
        TurnPoint
        EndPoint
        LastPoint
    End Enum

    Private _state As PointState = PointState.NotSet
    Public Property State As PointState
        Get
            Return _state
        End Get
        Set(value As PointState)
            If value <> _state Then
                Select Case value
                    Case PointState.NotSet
                        container.Background = WhiteBrush
                        triangle.Fill = Nothing
                    Case PointState.IsSet
                        If _state <> PointState.NotSet Then ' Other states have priority
                            Return
                        End If
                        container.Background = BlackBrush
                    Case PointState.StartPoint
                        If _state = PointState.NotSet Then
                            container.Background = GreenBrush
                        Else
                            triangle.Fill = GreenBrush
                        End If
                    Case PointState.TurnPoint
                        If _state = PointState.NotSet Then
                            container.Background = BlueBrush
                        Else
                            triangle.Fill = BlueBrush
                        End If
                    Case PointState.EndPoint
                        If _state = PointState.NotSet Then
                            container.Background = RedBrush
                        Else
                            triangle.Fill = RedBrush
                        End If
                    Case PointState.LastPoint
                        If _state = PointState.NotSet Then
                            container.Background = MagentaBrush
                        Else
                            triangle.Fill = MagentaBrush
                        End If
                End Select
                _state = value
            End If
        End Set
    End Property

    Private Sub PointerEntered(sender As Object, e As PointerRoutedEventArgs)
        grid.CurrentX = X
        grid.CurrentY = Y
    End Sub

    Public Sub New(root As Canvas, grid As PreviewGrid, x As Integer, y As Integer)
        _x = x
        _y = y

        Me.grid = grid

        ' Define child Canvas element
        container = New Canvas With {
            .Background = WhiteBrush,
            .Height = Size,
            .Width = Size
        }
        Canvas.SetTop(container, y * Size)
        Canvas.SetLeft(container, x * Size)
        AddHandler container.PointerEntered, AddressOf PointerEntered

        Dim border As New Border
        border.BorderBrush = GridBrush
        border.BorderThickness = New Thickness(1)
        border.Width = Size
        border.Height = Size
        container.Children.Add(border)

        triangle = New Polygon
        Dim points As New PointCollection()
        points.Add(New Windows.Foundation.Point(0, Size - 1))
        points.Add(New Windows.Foundation.Point(Size - 1, Size - 1))
        points.Add(New Windows.Foundation.Point(Size - 1, 0))
        triangle.Points = points
        container.Children.Add(triangle)

        root.Children.Add(container)
    End Sub
End Class
