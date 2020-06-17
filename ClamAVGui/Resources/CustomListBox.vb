Public Class CustomListBox
    Inherits System.Windows.Forms.Form
    Private label1 As Label
    Private itemId As String
    Private itemCollection As New Collection
    Private bgInColor, bgOutColor, frInColor, frOutColor As Color
    Private x, y, pad, myWidth, myHeight As Integer
    Private Event item_clicked()

    Public Property ItemList() As Collection
        Get
            Return itemCollection
        End Get
        Set(ByVal Value As Collection)
            itemCollection = Value
        End Set
    End Property

    Public Property myBackColor() As Color
        Get
            Return bgOutColor
        End Get
        Set(ByVal Value As Color)
            bgOutColor = Value
        End Set
    End Property

    Public Property backColorSelected() As Color
        Get
            Return bgInColor
        End Get
        Set(ByVal Value As Color)
            bgInColor = Value
        End Set
    End Property

    Public Property myForeColor() As Color
        Get
            Return frOutColor
        End Get
        Set(ByVal Value As Color)
            frOutColor = Value
        End Set
    End Property

    Public Property foreColorSelected() As Color
        Get
            Return frInColor
        End Get
        Set(ByVal Value As Color)
            frInColor = Value
        End Set
    End Property

    Public Property LocationX() As Integer
        Get
            Return x
        End Get
        Set(ByVal Value As Integer)
            x = Value
        End Set
    End Property

    Public Property LocationY() As Integer
        Get
            Return y
        End Get
        Set(ByVal Value As Integer)
            y = Value
        End Set
    End Property

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        '
        'CustomListBox
        '
        Me.ClientSize = New System.Drawing.Size(284, 262)
        Me.Name = "CustomListBox"
        Me.ResumeLayout(False)

    End Sub

    Private Sub CustomListBox_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Public Property InterSpace() As Integer
        Get
            Return pad
        End Get
        Set(ByVal Value As Integer)
            pad = Value
        End Set
    End Property

    Public Property SizeH() As Integer
        Get
            Return myHeight
        End Get
        Set(ByVal Value As Integer)
            myHeight = Value
        End Set
    End Property

    Public Property SizeW() As Integer
        Get
            Return myWidth
        End Get
        Set(ByVal Value As Integer)
            myWidth = Value
        End Set
    End Property

    Public Sub updateItems()
        For Each item As String In itemCollection
            label1 = New Label
            With label1
                .BackColor = (bgOutColor)
                .ForeColor = (frOutColor)
                .Width = width
                .Tag = item
                .Text = item
                .Location = New Point(x, y)
                y += .Height + pad
            End With
        Next
    End Sub

    Private Sub item_in(ByVal sender As Label, ByVal e As EventArgs)
        sender.BackColor = bgInColor
        sender.ForeColor = frInColor
        itemId = sender.Tag
    End Sub

    Private Sub item_out(ByVal sender As Label, ByVal e As EventArgs)
        sender.BackColor = bgOutColor
        sender.ForeColor = frOutColor
    End Sub

    Private Sub item_mouseDown(ByVal sender As Label, ByVal e As MouseEventArgs)
        Select Case e.Button
            Case Windows.Forms.MouseButtons.Left
                RaiseEvent item_clicked()
        End Select
    End Sub

End Class
