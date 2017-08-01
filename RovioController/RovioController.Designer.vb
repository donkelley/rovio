<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class RovioController
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
    Me.Button1 = New System.Windows.Forms.Button
    Me.TextBox1 = New System.Windows.Forms.TextBox
    Me.Label1 = New System.Windows.Forms.Label
    Me.Button2 = New System.Windows.Forms.Button
    Me.Button3 = New System.Windows.Forms.Button
    Me.Button4 = New System.Windows.Forms.Button
    Me.Label2 = New System.Windows.Forms.Label
    Me.PictureBox1 = New System.Windows.Forms.PictureBox
    Me.Button5 = New System.Windows.Forms.Button
    Me.Button6 = New System.Windows.Forms.Button
    Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
    Me.Button7 = New System.Windows.Forms.Button
    Me.Button8 = New System.Windows.Forms.Button
    Me.PictureBox2 = New System.Windows.Forms.PictureBox
    Me.Timer2 = New System.Windows.Forms.Timer(Me.components)
    Me.TextBox2 = New System.Windows.Forms.TextBox
    Me.Button9 = New System.Windows.Forms.Button
    Me.Button10 = New System.Windows.Forms.Button
    Me.Button11 = New System.Windows.Forms.Button
    Me.Button12 = New System.Windows.Forms.Button
    Me.CheckBox1 = New System.Windows.Forms.CheckBox
    Me.CheckBox2 = New System.Windows.Forms.CheckBox
    Me.Button13 = New System.Windows.Forms.Button
    Me.Button14 = New System.Windows.Forms.Button
    CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
    CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).BeginInit()
    Me.SuspendLayout()
    '
    'Button1
    '
    Me.Button1.Location = New System.Drawing.Point(7, 7)
    Me.Button1.Name = "Button1"
    Me.Button1.Size = New System.Drawing.Size(114, 28)
    Me.Button1.TabIndex = 0
    Me.Button1.Text = "Get Rovio Status"
    Me.Button1.UseVisualStyleBackColor = True
    '
    'TextBox1
    '
    Me.TextBox1.Location = New System.Drawing.Point(9, 76)
    Me.TextBox1.Multiline = True
    Me.TextBox1.Name = "TextBox1"
    Me.TextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both
    Me.TextBox1.Size = New System.Drawing.Size(669, 152)
    Me.TextBox1.TabIndex = 1
    '
    'Label1
    '
    Me.Label1.AutoSize = True
    Me.Label1.Location = New System.Drawing.Point(581, 8)
    Me.Label1.Name = "Label1"
    Me.Label1.Size = New System.Drawing.Size(10, 13)
    Me.Label1.TabIndex = 2
    Me.Label1.Text = "-"
    '
    'Button2
    '
    Me.Button2.Location = New System.Drawing.Point(134, 8)
    Me.Button2.Name = "Button2"
    Me.Button2.Size = New System.Drawing.Size(57, 28)
    Me.Button2.TabIndex = 3
    Me.Button2.Text = "Head up"
    Me.Button2.UseVisualStyleBackColor = True
    '
    'Button3
    '
    Me.Button3.Location = New System.Drawing.Point(201, 8)
    Me.Button3.Name = "Button3"
    Me.Button3.Size = New System.Drawing.Size(81, 28)
    Me.Button3.TabIndex = 4
    Me.Button3.Text = "Head Middle"
    Me.Button3.UseVisualStyleBackColor = True
    '
    'Button4
    '
    Me.Button4.Location = New System.Drawing.Point(288, 8)
    Me.Button4.Name = "Button4"
    Me.Button4.Size = New System.Drawing.Size(74, 28)
    Me.Button4.TabIndex = 5
    Me.Button4.Text = "Head Down"
    Me.Button4.UseVisualStyleBackColor = True
    '
    'Label2
    '
    Me.Label2.AutoSize = True
    Me.Label2.Location = New System.Drawing.Point(581, 23)
    Me.Label2.Name = "Label2"
    Me.Label2.Size = New System.Drawing.Size(10, 13)
    Me.Label2.TabIndex = 6
    Me.Label2.Text = "-"
    '
    'PictureBox1
    '
    Me.PictureBox1.Location = New System.Drawing.Point(10, 234)
    Me.PictureBox1.Name = "PictureBox1"
    Me.PictureBox1.Size = New System.Drawing.Size(272, 236)
    Me.PictureBox1.TabIndex = 7
    Me.PictureBox1.TabStop = False
    '
    'Button5
    '
    Me.Button5.Location = New System.Drawing.Point(415, 7)
    Me.Button5.Name = "Button5"
    Me.Button5.Size = New System.Drawing.Size(72, 29)
    Me.Button5.TabIndex = 8
    Me.Button5.Text = "Get Picture"
    Me.Button5.UseVisualStyleBackColor = True
    '
    'Button6
    '
    Me.Button6.Location = New System.Drawing.Point(288, 234)
    Me.Button6.Name = "Button6"
    Me.Button6.Size = New System.Drawing.Size(106, 39)
    Me.Button6.TabIndex = 9
    Me.Button6.Text = "Roam and Turn"
    Me.Button6.UseVisualStyleBackColor = True
    '
    'Timer1
    '
    Me.Timer1.Interval = 150
    '
    'Button7
    '
    Me.Button7.Location = New System.Drawing.Point(12, 476)
    Me.Button7.Name = "Button7"
    Me.Button7.Size = New System.Drawing.Size(102, 29)
    Me.Button7.TabIndex = 10
    Me.Button7.Text = "Speak Text"
    Me.Button7.UseVisualStyleBackColor = True
    '
    'Button8
    '
    Me.Button8.Location = New System.Drawing.Point(288, 318)
    Me.Button8.Name = "Button8"
    Me.Button8.Size = New System.Drawing.Size(106, 30)
    Me.Button8.TabIndex = 11
    Me.Button8.Text = "Matlab Process"
    Me.Button8.UseVisualStyleBackColor = True
    '
    'PictureBox2
    '
    Me.PictureBox2.Location = New System.Drawing.Point(400, 234)
    Me.PictureBox2.Name = "PictureBox2"
    Me.PictureBox2.Size = New System.Drawing.Size(272, 236)
    Me.PictureBox2.TabIndex = 12
    Me.PictureBox2.TabStop = False
    '
    'Timer2
    '
    Me.Timer2.Interval = 1000
    '
    'TextBox2
    '
    Me.TextBox2.Location = New System.Drawing.Point(125, 482)
    Me.TextBox2.Name = "TextBox2"
    Me.TextBox2.Size = New System.Drawing.Size(546, 20)
    Me.TextBox2.TabIndex = 13
    Me.TextBox2.Text = "Hello there, everyone!"
    '
    'Button9
    '
    Me.Button9.Location = New System.Drawing.Point(288, 279)
    Me.Button9.Name = "Button9"
    Me.Button9.Size = New System.Drawing.Size(106, 33)
    Me.Button9.TabIndex = 14
    Me.Button9.Text = "Play Audio"
    Me.Button9.UseVisualStyleBackColor = True
    '
    'Button10
    '
    Me.Button10.Location = New System.Drawing.Point(288, 354)
    Me.Button10.Name = "Button10"
    Me.Button10.Size = New System.Drawing.Size(106, 41)
    Me.Button10.TabIndex = 15
    Me.Button10.Text = "Capture Audio Stream"
    Me.Button10.UseVisualStyleBackColor = True
    '
    'Button11
    '
    Me.Button11.Location = New System.Drawing.Point(288, 401)
    Me.Button11.Name = "Button11"
    Me.Button11.Size = New System.Drawing.Size(106, 41)
    Me.Button11.TabIndex = 16
    Me.Button11.Text = "Recognize Audio Stream"
    Me.Button11.UseVisualStyleBackColor = True
    Me.Button11.Visible = False
    '
    'Button12
    '
    Me.Button12.Location = New System.Drawing.Point(492, 8)
    Me.Button12.Name = "Button12"
    Me.Button12.Size = New System.Drawing.Size(70, 51)
    Me.Button12.TabIndex = 17
    Me.Button12.Text = "Test Method Calls"
    Me.Button12.UseVisualStyleBackColor = True
    '
    'CheckBox1
    '
    Me.CheckBox1.AutoSize = True
    Me.CheckBox1.Location = New System.Drawing.Point(137, 42)
    Me.CheckBox1.Name = "CheckBox1"
    Me.CheckBox1.Size = New System.Drawing.Size(71, 17)
    Me.CheckBox1.TabIndex = 18
    Me.CheckBox1.Text = "Headlight"
    Me.CheckBox1.UseVisualStyleBackColor = True
    '
    'CheckBox2
    '
    Me.CheckBox2.AutoSize = True
    Me.CheckBox2.Location = New System.Drawing.Point(214, 42)
    Me.CheckBox2.Name = "CheckBox2"
    Me.CheckBox2.Size = New System.Drawing.Size(66, 17)
    Me.CheckBox2.TabIndex = 19
    Me.CheckBox2.Text = "Bluelight"
    Me.CheckBox2.UseVisualStyleBackColor = True
    '
    'Button13
    '
    Me.Button13.Location = New System.Drawing.Point(288, 447)
    Me.Button13.Name = "Button13"
    Me.Button13.Size = New System.Drawing.Size(105, 23)
    Me.Button13.TabIndex = 20
    Me.Button13.Text = "Recognize File"
    Me.Button13.UseVisualStyleBackColor = True
    Me.Button13.Visible = False
    '
    'Button14
    '
    Me.Button14.Location = New System.Drawing.Point(7, 41)
    Me.Button14.Name = "Button14"
    Me.Button14.Size = New System.Drawing.Size(110, 29)
    Me.Button14.TabIndex = 21
    Me.Button14.Text = "Good Dog"
    Me.Button14.UseVisualStyleBackColor = True
    '
    'RovioController
    '
    Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.ClientSize = New System.Drawing.Size(692, 516)
    Me.Controls.Add(Me.Button14)
    Me.Controls.Add(Me.Button13)
    Me.Controls.Add(Me.CheckBox2)
    Me.Controls.Add(Me.CheckBox1)
    Me.Controls.Add(Me.Button12)
    Me.Controls.Add(Me.Button11)
    Me.Controls.Add(Me.Button10)
    Me.Controls.Add(Me.Button9)
    Me.Controls.Add(Me.TextBox2)
    Me.Controls.Add(Me.PictureBox2)
    Me.Controls.Add(Me.Button8)
    Me.Controls.Add(Me.Button7)
    Me.Controls.Add(Me.Button6)
    Me.Controls.Add(Me.Button5)
    Me.Controls.Add(Me.PictureBox1)
    Me.Controls.Add(Me.Label2)
    Me.Controls.Add(Me.Button4)
    Me.Controls.Add(Me.Button3)
    Me.Controls.Add(Me.Button2)
    Me.Controls.Add(Me.Label1)
    Me.Controls.Add(Me.TextBox1)
    Me.Controls.Add(Me.Button1)
    Me.Name = "RovioController"
    Me.Text = "Rovio Control Interface"
    CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
    CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).EndInit()
    Me.ResumeLayout(False)
    Me.PerformLayout()

  End Sub
  Friend WithEvents Button1 As System.Windows.Forms.Button
  Friend WithEvents TextBox1 As System.Windows.Forms.TextBox
  Friend WithEvents Label1 As System.Windows.Forms.Label
  Friend WithEvents Button2 As System.Windows.Forms.Button
  Friend WithEvents Button3 As System.Windows.Forms.Button
  Friend WithEvents Button4 As System.Windows.Forms.Button
  Friend WithEvents Label2 As System.Windows.Forms.Label
  Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
  Friend WithEvents Button5 As System.Windows.Forms.Button
  Friend WithEvents Button6 As System.Windows.Forms.Button
  Friend WithEvents Button7 As System.Windows.Forms.Button
  Friend WithEvents Button8 As System.Windows.Forms.Button
  Friend WithEvents PictureBox2 As System.Windows.Forms.PictureBox
  Friend WithEvents Timer2 As System.Windows.Forms.Timer
  Friend WithEvents TextBox2 As System.Windows.Forms.TextBox
  Friend WithEvents Button9 As System.Windows.Forms.Button
  Friend WithEvents Button10 As System.Windows.Forms.Button
  Friend WithEvents Button11 As System.Windows.Forms.Button
  Friend WithEvents Button12 As System.Windows.Forms.Button
  Friend WithEvents CheckBox1 As System.Windows.Forms.CheckBox
  Friend WithEvents CheckBox2 As System.Windows.Forms.CheckBox
  Friend WithEvents Timer1 As System.Windows.Forms.Timer
  Friend WithEvents Button13 As System.Windows.Forms.Button
  Friend WithEvents Button14 As System.Windows.Forms.Button

End Class
