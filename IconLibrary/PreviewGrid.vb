Imports Windows.UI
Imports Windows.UI.Xaml.Shapes

Public Class PreviewGrid
    Implements INotifyPropertyChanged

    Public Const Width As Integer = 40
    Public Const Height As Integer = 20
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Public Property Points As New List(Of PreviewGridPoint)

    Private LayoutRoot As Canvas

    Private _currentX As Integer
    Public Property CurrentX As Integer
        Get
            Return _currentX
        End Get
        Set(value As Integer)
            If value <> _currentX Then
                _currentX = value
                OnPropertyChanged("CurrentX")
            End If
        End Set
    End Property

    Private _currentY As Integer
    Public Property CurrentY As Integer
        Get
            Return _currentY
        End Get
        Set(value As Integer)
            If value <> _currentY Then
                _currentY = value
                OnPropertyChanged("CurrentY")
            End If
        End Set
    End Property

    Private _currentPreviewInfo As String
    Public Property CurrentPreviewInfo As String
        Get
            Return _currentPreviewInfo
        End Get
        Set(value As String)
            _currentPreviewInfo = value
            OnPropertyChanged("CurrentPreviewInfo")
        End Set
    End Property

    Protected Overridable Sub OnPropertyChanged(ByVal PropertyName As String)
        ' Raise the event, and make this procedure
        ' overridable, should someone want to inherit from
        ' this class and override this behavior:
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(PropertyName))
    End Sub

    ' (x,y): {(0,0),(1,0),..,(H-1,0),(0,1),...}

    Public Sub New(rootCanvas As Canvas)
        For x = 0 To Width - 1
            For y = 0 To Height - 1
                Points.Add(New PreviewGridPoint(rootCanvas, Me, x, y))
            Next
        Next
        LayoutRoot = rootCanvas
    End Sub

    Public Sub TrySetState(x As Double, y As Double, state As PreviewGridPoint.PointState)
        Dim p = GetPreviewGridPoint(Math.Round(x), Math.Round(y))
        If p IsNot Nothing Then
            p.State = state
        End If
    End Sub

    Public Function GetPreviewGridPoint(x As Integer, y As Integer) As PreviewGridPoint
        If x >= 0 And x < Width And y >= 0 And y < Height Then
            Dim index As Integer = x * Height + y
            If index >= 0 And index < Points.Count Then
                Return Points(index)
            Else
                Return Nothing
            End If
        Else
            Return Nothing
        End If
    End Function

    Private Function GetPreviewGridPointByCanvasCoordinate(xCoordinate As Double, yCoordinate As Double) As PreviewGridPoint

        Dim x As Integer = Math.Floor(xCoordinate / PreviewGridPoint.Size)
        Dim y As Integer = Math.Floor(yCoordinate / PreviewGridPoint.Size)

        Return GetPreviewGridPoint(x, y)

    End Function

    Public Sub Clear()
        For Each p In Points
            p.State = PreviewGridPoint.PointState.NotSet
        Next
        Dim temp As New List(Of Shapes.Path)
        For Each c In LayoutRoot.Children
            If TypeOf c Is Shapes.Path Then
                temp.Add(c)
            End If
        Next
        For Each c In temp
            LayoutRoot.Children.Remove(c)
        Next
    End Sub

    Public Sub Draw(geometry As PathGeometry)
        If geometry Is Nothing Then
            Return
        End If
        Clear()
        Dim figureIndex As Integer = 1
        For Each figure In geometry.Figures
            Dim cursor = figure.StartPoint
            Dim index As Integer = 1
            For Each segment In figure.Segments
                Dim line = TryCast(segment, LineSegment)
                If line IsNot Nothing Then
                    DrawLine(index = 1, index = figure.Segments.Count, figureIndex = geometry.Figures.Count, cursor, line.Point)
                    cursor = line.Point
                Else
                    Dim arcSegment = TryCast(segment, ArcSegment)
                    If arcSegment IsNot Nothing Then
                        DrawArc(index = 1, index = figure.Segments.Count, figureIndex = geometry.Figures.Count, cursor, arcSegment)
                        cursor = arcSegment.Point
                    Else
                        Dim bezierSegment = TryCast(segment, BezierSegment)
                        If bezierSegment IsNot Nothing Then
                            DrawBezier(index = 1, index = figure.Segments.Count, figureIndex = geometry.Figures.Count, cursor, bezierSegment)
                            cursor = bezierSegment.Point3
                        Else
                            Dim quadraticBezierSegment = TryCast(segment, QuadraticBezierSegment)
                            If bezierSegment IsNot Nothing Then
                                DrawQuadraticBezier(index = 1, index = figure.Segments.Count, figureIndex = geometry.Figures.Count, cursor, quadraticBezierSegment)
                                cursor = quadraticBezierSegment.Point2
                            End If
                        End If
                    End If
                End If
                index = index + 1
            Next
            figureIndex = figureIndex + 1
        Next
    End Sub

    Private Function GetCursorPosition(geometry As PathGeometry) As Point

        Dim cursor As Point = Nothing

        If geometry Is Nothing Then
            Return Nothing
        End If
        Clear()
        For Each figure In geometry.Figures
            cursor = figure.StartPoint
            For Each segment In figure.Segments
                Dim line = TryCast(segment, LineSegment)
                If line IsNot Nothing Then
                    cursor = line.Point
                Else
                    Dim arcSegment = TryCast(segment, ArcSegment)
                    If arcSegment IsNot Nothing Then
                        cursor = arcSegment.Point
                    Else
                        Dim bezierSegment = TryCast(segment, BezierSegment)
                        If bezierSegment IsNot Nothing Then
                            cursor = bezierSegment.Point3
                        Else
                            Dim quadraticBezierSegment = TryCast(segment, QuadraticBezierSegment)
                            If bezierSegment IsNot Nothing Then
                                cursor = quadraticBezierSegment.Point2
                            End If
                        End If
                    End If
                End If
            Next
        Next

        Return cursor
    End Function

    Public Function CreateLineMarkup(geometry As PathGeometry, preview As Line) As String

        Dim cursor = GetCursorPosition(geometry)
        Dim result As String = ""

        Dim startPoint As PreviewGridPoint = GetPreviewGridPointByCanvasCoordinate(preview.X1, preview.Y1)
        Dim endPoint As PreviewGridPoint = GetPreviewGridPointByCanvasCoordinate(preview.X2, preview.Y2)

        If startPoint IsNot Nothing And endPoint IsNot Nothing Then
            Dim deltaX As Integer = endPoint.X - startPoint.X
            Dim deltaY As Integer = endPoint.Y - startPoint.Y
            If deltaX = 0 And deltaY = 0 Then
                Return result
            End If
            If cursor.X <> startPoint.X Or cursor.Y <> startPoint.Y Then
                result = "M" + startPoint.X.ToString + "," + startPoint.Y.ToString + " "
            End If
            If deltaX = 0 Then
                result = result + "v" + deltaY.ToString + " "
            ElseIf deltaY = 0 Then
                result = result + "h" + deltaX.ToString + " "
            Else
                result = result + "l" + deltaX.ToString + "," + deltaY.ToString + " "
            End If
        End If

        Return result

    End Function

    Public Function CreateRectangleMarkup(geometry As PathGeometry, preview As Rectangle)

        Dim cursor = GetCursorPosition(geometry)
        Dim result As String = ""

        Dim upperLeftCoordinate As New Point(Canvas.GetLeft(preview), Canvas.GetTop(preview))
        Dim lowerRightCoordinate As New Point(upperLeftCoordinate.X + preview.Width, upperLeftCoordinate.Y + preview.Height)

        Dim upperLeft As PreviewGridPoint = GetPreviewGridPointByCanvasCoordinate(upperLeftCoordinate.X, upperLeftCoordinate.Y)
        Dim lowerRight As PreviewGridPoint = GetPreviewGridPointByCanvasCoordinate(lowerRightCoordinate.X, lowerRightCoordinate.Y)

        Dim startPoint As PreviewGridPoint
        Dim endPoint As PreviewGridPoint

        If upperLeft IsNot Nothing AndAlso lowerRight IsNot Nothing Then
            If (cursor.X <> upperLeft.X Or cursor.Y <> upperLeft.Y) And (cursor.X <> lowerRight.X Or cursor.Y <> lowerRight.Y) Then
                startPoint = upperLeft
                endPoint = lowerRight
                result = "M" + startPoint.X.ToString + "," + startPoint.Y.ToString + " "
            ElseIf cursor.X = upperLeft.X And cursor.Y = upperLeft.Y Then
                startPoint = upperLeft
                endPoint = lowerRight
            Else
                startPoint = lowerRight
                endPoint = upperLeft
            End If
            Dim deltaX As Integer = endPoint.X - startPoint.X
            Dim deltaY As Integer = endPoint.Y - startPoint.Y
            If deltaX = 0 And deltaY = 0 Then
                Return ""
            End If

            result = result + "h" + deltaX.ToString + " v" + deltaY.ToString + " h" + (-deltaX).ToString + " v" + (-deltaY).ToString
        End If

        Return result

    End Function

    Public Function CreateEllipseMarkup(geometry As PathGeometry, preview As Ellipse)

        Dim cursor = GetCursorPosition(geometry)
        Dim result As String = ""

        Dim upperLeftCoordinate As New Point(Canvas.GetLeft(preview), Canvas.GetTop(preview))
        Dim lowerRightCoordinate As New Point(upperLeftCoordinate.X + preview.Width, upperLeftCoordinate.Y + preview.Height)
        Dim upperMidCoordinate As New Point(upperLeftCoordinate.X + (lowerRightCoordinate.X - upperLeftCoordinate.X) / 2, upperLeftCoordinate.Y)
        Dim lowerMidCoordinate As New Point(upperLeftCoordinate.X + (lowerRightCoordinate.X - upperLeftCoordinate.X) / 2, lowerRightCoordinate.Y)

        Dim upperLeft As PreviewGridPoint = GetPreviewGridPointByCanvasCoordinate(upperLeftCoordinate.X, upperLeftCoordinate.Y)
        Dim lowerRight As PreviewGridPoint = GetPreviewGridPointByCanvasCoordinate(lowerRightCoordinate.X, lowerRightCoordinate.Y)
        Dim upperMid As PreviewGridPoint = GetPreviewGridPointByCanvasCoordinate(upperMidCoordinate.X, upperMidCoordinate.Y)
        Dim lowerMid As PreviewGridPoint = GetPreviewGridPointByCanvasCoordinate(lowerMidCoordinate.X, lowerMidCoordinate.Y)

        Dim startPoint As PreviewGridPoint
        Dim sweep As Integer

        If upperLeft IsNot Nothing AndAlso lowerRight IsNot Nothing AndAlso upperMid IsNot Nothing AndAlso lowerMid IsNot Nothing Then
            If (cursor.X <> upperMid.X Or cursor.Y <> upperMid.Y) And (cursor.X <> lowerMid.X Or cursor.Y <> lowerMid.Y) Then
                startPoint = upperMid
                sweep = 1
                result = "M" + startPoint.X.ToString + "," + startPoint.Y.ToString + " "
            ElseIf cursor.X = upperMid.X And cursor.Y = upperMid.Y Then
                startPoint = upperMid
                sweep = 1
            Else
                startPoint = lowerMid
                sweep = 0
            End If
            Dim size As New Size(lowerRight.X - upperLeft.X, lowerRight.Y - upperLeft.Y)
            If size.Height = 0 Or size.Width = 0 Then
                Return ""
            End If

            result = result + "a" + (size.Width / 2).ToString.Replace(",", ".") + "," + (size.Height / 2).ToString.Replace(",", ".") + ",0,1," + sweep.ToString + ",-1,0 "
        End If

        Return result

    End Function

    Public Sub SetPreviewInfo(preview As Line)

        Dim startPoint As PreviewGridPoint = GetPreviewGridPointByCanvasCoordinate(preview.X1, preview.Y1)
        Dim endPoint As PreviewGridPoint = GetPreviewGridPointByCanvasCoordinate(preview.X2, preview.Y2)

        If startPoint IsNot Nothing And endPoint IsNot Nothing Then
            CurrentPreviewInfo = "Line: (" + startPoint.X.ToString + ", " + startPoint.Y.ToString + ") - (" + endPoint.X.ToString + ", " + endPoint.Y.ToString + ")"
        Else
            CurrentPreviewInfo = ""
        End If

    End Sub

    Private Sub SetPreviewInfoFromWidthHeight(upperLeft As PreviewGridPoint, lowerRight As PreviewGridPoint)

        If upperLeft IsNot Nothing AndAlso lowerRight IsNot Nothing Then
            CurrentPreviewInfo = "Width: " + (lowerRight.X - upperLeft.X).ToString + "  Height" + (lowerRight.Y - upperLeft.Y).ToString
        Else
            CurrentPreviewInfo = ""
        End If

    End Sub

    Public Sub SetPreviewInfo(preview As Rectangle)

        Dim upperLeftCoordinate As New Point(Canvas.GetLeft(preview), Canvas.GetTop(preview))
        Dim lowerRightCoordinate As New Point(upperLeftCoordinate.X + preview.Width, upperLeftCoordinate.Y + preview.Height)

        Dim upperLeft As PreviewGridPoint = GetPreviewGridPointByCanvasCoordinate(upperLeftCoordinate.X, upperLeftCoordinate.Y)
        Dim lowerRight As PreviewGridPoint = GetPreviewGridPointByCanvasCoordinate(lowerRightCoordinate.X, lowerRightCoordinate.Y)

        SetPreviewInfoFromWidthHeight(upperLeft, lowerRight)
    End Sub

    Public Sub SetPreviewInfo(preview As Ellipse)

        Dim upperLeftCoordinate As New Point(Canvas.GetLeft(preview), Canvas.GetTop(preview))
        Dim lowerRightCoordinate As New Point(upperLeftCoordinate.X + preview.Width, upperLeftCoordinate.Y + preview.Height)

        Dim upperLeft As PreviewGridPoint = GetPreviewGridPointByCanvasCoordinate(upperLeftCoordinate.X, upperLeftCoordinate.Y)
        Dim lowerRight As PreviewGridPoint = GetPreviewGridPointByCanvasCoordinate(lowerRightCoordinate.X, lowerRightCoordinate.Y)

        SetPreviewInfoFromWidthHeight(upperLeft, lowerRight)

    End Sub

    Private Sub DrawLine(firstLine As Boolean, lastLine As Boolean, lastFigure As Boolean, startPoint As Point, endPoint As Point)

        Dim xDist As Double = endPoint.X - startPoint.X
        Dim yDist As Double = endPoint.Y - startPoint.Y
        Dim xIncrement As Double = 0
        Dim yIncrement As Double = 0

        'X
        If yDist = 0 Then
            If xDist < 0 Then
                xIncrement = -1
            ElseIf xDist > 0 Then
                xIncrement = 1
            End If
        ElseIf xDist <> 0 Then
            ' xIncrement depends on xDist and yDist: Example: xDist = 1, yDist = 2 => xIncrement = 1 / 2
            xIncrement = xDist / Math.Abs(yDist)
            xIncrement = Math.Sign(xIncrement) * Math.Min(Math.Abs(xIncrement), 1)
        End If

        If xDist = 0 Then
            If yDist < 0 Then
                yIncrement = -1
            ElseIf yDist > 0 Then
                yIncrement = 1
            End If
        Else
            yIncrement = yDist / Math.Abs(xDist)
            yIncrement = Math.Sign(yIncrement) * Math.Min(Math.Abs(yIncrement), 1)
        End If

        Dim cursor As Point = startPoint

        If firstLine Then
            TrySetState(cursor.X, cursor.Y, PreviewGridPoint.PointState.StartPoint)
        Else
            TrySetState(cursor.X, cursor.Y, PreviewGridPoint.PointState.TurnPoint)
        End If

        Do Until cursor = endPoint
            If xIncrement < 0 And cursor.X > endPoint.X Or xIncrement > 0 And cursor.X < endPoint.X Then
                cursor.X = cursor.X + xIncrement
            Else
                cursor.X = endPoint.X
            End If
            If yIncrement < 0 And cursor.Y > endPoint.Y Or yIncrement > 0 And cursor.Y < endPoint.Y Then
                cursor.Y = cursor.Y + yIncrement
            Else
                cursor.Y = endPoint.Y
            End If
            If cursor = endPoint Then
                If lastLine Then
                    If lastFigure Then
                        TrySetState(cursor.X, cursor.Y, PreviewGridPoint.PointState.LastPoint)
                    Else
                        TrySetState(cursor.X, cursor.Y, PreviewGridPoint.PointState.EndPoint)
                    End If
                Else
                    TrySetState(cursor.X, cursor.Y, PreviewGridPoint.PointState.TurnPoint)
                End If
            Else
                TrySetState(cursor.X, cursor.Y, PreviewGridPoint.PointState.IsSet)
            End If
        Loop

        Dim lineStartPoint As New Point
        Dim lineEndPoint As New Point
        lineStartPoint.X = (startPoint.X + 0.5) * PreviewGridPoint.Size  ' x*s + 0,5*s = (x+0,5) * s
        lineStartPoint.Y = (startPoint.Y + 0.5) * PreviewGridPoint.Size
        lineEndPoint.X = (endPoint.X + 0.5) * PreviewGridPoint.Size
        lineEndPoint.Y = (endPoint.Y + 0.5) * PreviewGridPoint.Size
        LayoutRoot.Children.Add(LineArrow(lineStartPoint, lineEndPoint))

    End Sub

    Private Sub DrawArc(firstLine As Boolean, lastLine As Boolean, lastFigure As Boolean, startPoint As Point, arc As ArcSegment)

        Dim result As New Shapes.Path

        If firstLine Then
            TrySetState(startPoint.X, startPoint.Y, PreviewGridPoint.PointState.StartPoint)
        Else
            TrySetState(startPoint.X, startPoint.Y, PreviewGridPoint.PointState.TurnPoint)
        End If

        If lastLine Then
            If lastFigure Then
                TrySetState(arc.Point.X, arc.Point.Y, PreviewGridPoint.PointState.LastPoint)
            Else
                TrySetState(arc.Point.X, arc.Point.Y, PreviewGridPoint.PointState.EndPoint)
            End If
        Else
            TrySetState(arc.Point.X, arc.Point.Y, PreviewGridPoint.PointState.TurnPoint)
        End If

        Dim lineStartPoint As New Point
        Dim lineEndPoint As New Point
        lineStartPoint.X = (startPoint.X + 0.5) * PreviewGridPoint.Size  ' x*s + 0,5*s = (x+0,5) * s
        lineStartPoint.Y = (startPoint.Y + 0.5) * PreviewGridPoint.Size
        lineEndPoint.X = (arc.Point.X + 0.5) * PreviewGridPoint.Size
        lineEndPoint.Y = (arc.Point.Y + 0.5) * PreviewGridPoint.Size

        Dim arcGeometry As New PathGeometry()
        Dim figures As New PathFigureCollection()
        Dim figure As New PathFigure With {
            .StartPoint = lineStartPoint}
        Dim segments As New PathSegmentCollection()
        Dim arc1 As New ArcSegment With {
            .Point = lineEndPoint,
            .Size = New Size(arc.Size.Width * PreviewGridPoint.Size, arc.Size.Height * PreviewGridPoint.Size),
            .RotationAngle = arc.RotationAngle,
            .IsLargeArc = arc.IsLargeArc,
            .SweepDirection = arc.SweepDirection
        }

        segments.Add(arc1)
        figure.Segments = segments
        figures.Add(figure)
        arcGeometry.Figures = figures
        result.Data = arcGeometry
        result.Stroke = PreviewGridPoint.BlackBrush
        result.StrokeThickness = 10

        LayoutRoot.Children.Add(result)

    End Sub

    Private Sub DrawBezier(firstLine As Boolean, lastLine As Boolean, lastFigure As Boolean, startPoint As Point, bezier As BezierSegment)

        Dim result As New Shapes.Path

        If firstLine Then
            TrySetState(startPoint.X, startPoint.Y, PreviewGridPoint.PointState.StartPoint)
        Else
            TrySetState(startPoint.X, startPoint.Y, PreviewGridPoint.PointState.TurnPoint)
        End If

        If lastLine Then
            If lastFigure Then
                TrySetState(bezier.Point3.X, bezier.Point3.Y, PreviewGridPoint.PointState.LastPoint)
            Else
                TrySetState(bezier.Point3.X, bezier.Point3.Y, PreviewGridPoint.PointState.EndPoint)
            End If
        Else
            TrySetState(bezier.Point3.X, bezier.Point3.Y, PreviewGridPoint.PointState.TurnPoint)
        End If

        Dim lineStartPoint As New Point
        lineStartPoint.X = (startPoint.X + 0.5) * PreviewGridPoint.Size  ' x*s + 0,5*s = (x+0,5) * s
        lineStartPoint.Y = (startPoint.Y + 0.5) * PreviewGridPoint.Size

        Dim bezierGeometry As New PathGeometry()
        Dim figures As New PathFigureCollection()
        Dim figure As New PathFigure With {
            .StartPoint = lineStartPoint}
        Dim segments As New PathSegmentCollection()
        Dim bezier1 As New BezierSegment With {
            .Point1 = New Point((bezier.Point1.X + 0.5) * PreviewGridPoint.Size, (bezier.Point1.Y + 0.5) * PreviewGridPoint.Size),
            .Point2 = New Point((bezier.Point2.X + 0.5) * PreviewGridPoint.Size, (bezier.Point2.Y + 0.5) * PreviewGridPoint.Size),
            .Point3 = New Point((bezier.Point3.X + 0.5) * PreviewGridPoint.Size, (bezier.Point3.Y + 0.5) * PreviewGridPoint.Size)
        }

        segments.Add(bezier1)
        figure.Segments = segments
        figures.Add(figure)
        bezierGeometry.Figures = figures
        result.Data = bezierGeometry
        result.Stroke = PreviewGridPoint.BlackBrush
        result.StrokeThickness = 10

        LayoutRoot.Children.Add(result)

    End Sub

    Private Sub DrawQuadraticBezier(firstLine As Boolean, lastLine As Boolean, lastFigure As Boolean, startPoint As Point, bezier As QuadraticBezierSegment)

        Dim result As New Shapes.Path

        If firstLine Then
            TrySetState(startPoint.X, startPoint.Y, PreviewGridPoint.PointState.StartPoint)
        Else
            TrySetState(startPoint.X, startPoint.Y, PreviewGridPoint.PointState.TurnPoint)
        End If

        If lastLine Then
            If lastFigure Then
                TrySetState(bezier.Point2.X, bezier.Point2.Y, PreviewGridPoint.PointState.LastPoint)
            Else
                TrySetState(bezier.Point2.X, bezier.Point2.Y, PreviewGridPoint.PointState.EndPoint)
            End If
        Else
            TrySetState(bezier.Point2.X, bezier.Point2.Y, PreviewGridPoint.PointState.TurnPoint)
        End If

        Dim lineStartPoint As New Point
        lineStartPoint.X = (startPoint.X + 0.5) * PreviewGridPoint.Size  ' x*s + 0,5*s = (x+0,5) * s
        lineStartPoint.Y = (startPoint.Y + 0.5) * PreviewGridPoint.Size

        Dim bezierGeometry As New PathGeometry()
        Dim figures As New PathFigureCollection()
        Dim figure As New PathFigure With {
            .StartPoint = lineStartPoint}
        Dim segments As New PathSegmentCollection()
        Dim bezier1 As New QuadraticBezierSegment With {
            .Point1 = New Point((bezier.Point1.X + 0.5) * PreviewGridPoint.Size, (bezier.Point1.Y + 0.5) * PreviewGridPoint.Size),
            .Point2 = New Point((bezier.Point2.X + 0.5) * PreviewGridPoint.Size, (bezier.Point2.Y + 0.5) * PreviewGridPoint.Size)
        }

        segments.Add(bezier1)
        figure.Segments = segments
        figures.Add(figure)
        bezierGeometry.Figures = figures
        result.Data = bezierGeometry
        result.Stroke = PreviewGridPoint.BlackBrush
        result.StrokeThickness = 10

        LayoutRoot.Children.Add(result)

    End Sub

    Private Function LineArrow(startPoint As Point, endPoint As Point) As Shapes.Path

        Dim result As New Shapes.Path
        Dim geometryGroup = New GeometryGroup()

        ' line
        Dim line = New LineGeometry With {
            .StartPoint = startPoint
        }
        Dim length As Double = Math.Sqrt(Math.Abs(startPoint.X - endPoint.X) * Math.Abs(startPoint.X - endPoint.X) +
                Math.Abs(startPoint.Y - endPoint.Y) * Math.Abs(startPoint.Y - endPoint.Y))
        Dim lineEndPoint As Point = New Point(startPoint.X + length, startPoint.Y)
        line.EndPoint = New Point(lineEndPoint.X - 20, lineEndPoint.Y)

        'triangle
        Dim triangle As New PathGeometry()
        Dim figures As New PathFigureCollection()

        Dim figure As New PathFigure With {
            .StartPoint = New Point(lineEndPoint.X - 20, lineEndPoint.Y - 10)
        }
        Dim segemnt1 As New PathSegmentCollection()
        Dim line1 As New LineSegment With {
            .Point = New Point(lineEndPoint.X - 20, lineEndPoint.Y + 10)
        }
        segemnt1.Add(line1)
        figure.Segments = segemnt1
        figures.Add(figure)

        Dim figure2 As New PathFigure With {
            .StartPoint = New Point(lineEndPoint.X - 20, lineEndPoint.Y + 10)
        }
        Dim segemnt2 As New PathSegmentCollection()
        Dim line2 As New LineSegment With {
            .Point = New Point(lineEndPoint.X, lineEndPoint.Y)
        }
        segemnt2.Add(line2)
        figure2.Segments = segemnt2
        figures.Add(figure2)

        Dim figure3 As New PathFigure With {
            .StartPoint = New Point(lineEndPoint.X, lineEndPoint.Y)
        }
        Dim segemnt3 As New PathSegmentCollection()
        Dim line3 As New LineSegment With {
            .Point = New Point(lineEndPoint.X - 20, lineEndPoint.Y - 10)
        }
        segemnt3.Add(line3)
        figure3.Segments = segemnt3
        figures.Add(figure3)

        triangle.Figures = figures

        geometryGroup.Children.Add(line)
        geometryGroup.Children.Add(triangle)
        result.Data = geometryGroup

        'rotate
        Dim form As New RotateTransform With {
            .CenterX = startPoint.X,
            .CenterY = startPoint.Y
        }

        'calculate the angle 
        Dim angle As Double = Math.Asin(Math.Abs(startPoint.Y - endPoint.Y) / length)
        Dim angle2 As Double = 180 / Math.PI * angle

        'orientation
        If endPoint.Y > startPoint.Y Then
            If endPoint.X <= startPoint.X Then
                angle2 = 180 - angle2
            End If
        ElseIf endPoint.X > startPoint.X Then
            angle2 = -angle2
        Else
            angle2 = -(180 - angle2)
        End If
        form.Angle = angle2
        result.RenderTransform = form
        result.Stroke = New SolidColorBrush(Colors.Yellow)
        result.StrokeThickness = 2
        Return result
    End Function

End Class
