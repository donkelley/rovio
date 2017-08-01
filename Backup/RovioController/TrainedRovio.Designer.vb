<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class TrainedRovio
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
    Me.components = New System.ComponentModel.Container
    Me.Label2 = New System.Windows.Forms.Label
    Me.lblStatus = New System.Windows.Forms.Label
    Me.Label1 = New System.Windows.Forms.Label
    Me.txtHistory = New System.Windows.Forms.TextBox
    Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
    Me.SuspendLayout()
    '
    'Label2
    '
    Me.Label2.AutoSize = True
    Me.Label2.Location = New System.Drawing.Point(12, 28)
    Me.Label2.Name = "Label2"
    Me.Label2.Size = New System.Drawing.Size(68, 13)
    Me.Label2.TabIndex = 9
    Me.Label2.Text = "State history:"
    '
    'lblStatus
    '
    Me.lblStatus.AutoSize = True
    Me.lblStatus.Location = New System.Drawing.Point(101, 9)
    Me.lblStatus.Name = "lblStatus"
    Me.lblStatus.Size = New System.Drawing.Size(65, 13)
    Me.lblStatus.TabIndex = 8
    Me.lblStatus.Text = "Not listening"
    '
    'Label1
    '
    Me.Label1.AutoSize = True
    Me.Label1.Location = New System.Drawing.Point(12, 9)
    Me.Label1.Name = "Label1"
    Me.Label1.Size = New System.Drawing.Size(83, 13)
    Me.Label1.TabIndex = 7
    Me.Label1.Text = "Attention status:"
    '
    'txtHistory
    '
    Me.txtHistory.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.txtHistory.Location = New System.Drawing.Point(14, 44)
    Me.txtHistory.Multiline = True
    Me.txtHistory.Name = "txtHistory"
    Me.txtHistory.ScrollBars = System.Windows.Forms.ScrollBars.Both
    Me.txtHistory.Size = New System.Drawing.Size(646, 355)
    Me.txtHistory.TabIndex = 10
    Me.txtHistory.WordWrap = False
    '
    'Timer1
    '
    Me.Timer1.Interval = 150
    '
    'TrainedRovio
    '
    Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.ClientSize = New System.Drawing.Size(669, 406)
    Me.Controls.Add(Me.txtHistory)
    Me.Controls.Add(Me.Label2)
    Me.Controls.Add(Me.lblStatus)
    Me.Controls.Add(Me.Label1)
    Me.Name = "TrainedRovio"
    Me.Text = "Trained Rovio"
    Me.ResumeLayout(False)
    Me.PerformLayout()

  End Sub
  Friend WithEvents Label2 As System.Windows.Forms.Label
  Friend WithEvents lblStatus As System.Windows.Forms.Label
  Friend WithEvents Label1 As System.Windows.Forms.Label
  Friend WithEvents txtHistory As System.Windows.Forms.TextBox
  Friend WithEvents Timer1 As System.Windows.Forms.Timer
End Class
