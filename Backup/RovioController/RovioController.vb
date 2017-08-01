'****************************************************************************************
' Testing Form for RovioWrap
' RovioController
'   This is the testing, debug, and example window for the RovioWrap Object.
'
' Written by : Scott Settembre
' Forum name : Sidekick
' Created on : Jan 2009
' Project page        : http://www.codeplex.com/RovioWrap
' Academic home page  : http://www.cse.buffalo.edu/~ss424/
' Company website     : http://www.esprogramming.com
'
' License: Free to use, Free to learn from, Free to extend, just give me some credit somewhere.
'           If used for commercial use, also send me an email and link to your product so I
'           drool and wish I had a free copy! :)
'
' Acknowledgements inline to: "RoboSapienPet", "mt"
'
' Visit Robosapienpet's cause for opensource Rovio code at:
' http://www.robocommunity.com/forum/thread/14178/Rovio-uses-eCos-WowWee-owes-us-some-open-source-code/;jsessionid=F885CB9D87BB3399D3E27CA1048EFA71
'
'****************************************************************************************
Public Class RovioController

  Private WithEvents RO As New RovioWrap
  Dim Matlab As Object

  Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
    ' Rovio, Want Doggie Bone?!!
    ' Note: Call GetStatus before accessing any READ ONLY properties except AudioFormat and FirmwareVersion
    RO.GetStatus()
    TextBox1.AppendText(RO.LastStatusString)
    Label1.Text = "Battery=" & RO.BatteryState
    Label2.Text = IIf(RO.ObstacleFlag, "OBSTACLE", "No obstacle")
  End Sub

  Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
    ' Rovio, Up Boy!
    RO.HeadElevationState = 2
  End Sub

  Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
    ' Rovio, Steady Boy!
    RO.HeadElevationState = 1
  End Sub

  Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
    ' Rovio, Down Boy!
    RO.HeadElevationState = 0
  End Sub

  Private Sub Button5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.Click
    ' Rovio, Take a Pic!
    PictureBox1.SizeMode = PictureBoxSizeMode.StretchImage
    PictureBox1.Image = RO.CurrentImage
  End Sub

  Private Sub Button6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button6.Click
    ' Rovio, Wander and Avoid Obstacles!
    ' Turn on the timers that move, avoid, and process vision.
    Timer1.Enabled = Not Timer1.Enabled
    Timer2.Enabled = Not Timer2.Enabled
  End Sub

  Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
    ' Rovio, Wander!
    ' Timer to move the Rovio with some simple obstacle avoidance code every .15 seconds.
    MoveRovio()
  End Sub

  Public Sub MoveRovio()
    ' Rovio, Come here!

    ' Get status needs to be called each time you wish to update the object
    ' But I am requiring that the user does it at the moment.
    RO.GetStatus()
    If RO.ObstacleFlag Then
      'Full stop
      RO.Move(RovioWrap.MovementParameter.moveStop, 1)
      'Turn 20 degrees
      RO.Move(RovioWrap.MovementParameter.rotateRight20Degrees, 1)
    Else
      'Move forward
      RO.Move(RovioWrap.MovementParameter.moveForward, 1)
    End If
  End Sub

  Private Sub Button8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button8.Click

    ' Matlab code works nicely, but commenting it out not to confuse people
    ' However if you get the student or trial version and uncomment out this then
    ' you will have a great way to process images

    PictureBox1.SizeMode = PictureBoxSizeMode.StretchImage
    PictureBox1.Image = RO.CurrentImage
    Exit Sub  ' comment this line out to access Matlab functioning

    PictureBox1.Image.Save("c:\rovio\ImageIn.jpg")

    ' This works perfectly
    ' Create an out of process COM object of Matlab type
    ' Execute the DetectEdges script
    ' Grab the outuput of the script and put it in the box
    If Matlab Is Nothing Then
      Matlab = CreateObject("Matlab.Application")
      Debug.Print("Created MATLAB object")
    End If

    Matlab.Execute("cd c:\rovio\;")
    Matlab.Execute("DetectEdges;")

    PictureBox2.SizeMode = PictureBoxSizeMode.StretchImage
    PictureBox2.ImageLocation = "c:\rovio\ImageOut.jpg"
    PictureBox2.Load()

  End Sub


  Private Sub Timer2_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer2.Tick
    ' Captures images and processes them, Matlab code commented out though
    Button8.PerformClick()
  End Sub

  Private Sub Button7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button7.Click
    ' Rovio, Speak!
    RO.Say(TextBox2.Text.ToString)
  End Sub

  Private Sub Button9_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button9.Click
    ' Rovio, Play!
    'RO.PlayAudioFile("c:\rovio\silence.pcm")
    RO.PlayAudioFile("c:\rovio\Scott_HelloThere.pcm")
    'RO.PlayAudioFile("c:\rovio\Scott_HelloThere.wav")
    'RO.PlayAudioFile("c:\rovio\rawwav.wav")
  End Sub

  Private Sub Button10_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button10.Click
    ' I'm close to getting this to work, not quite yet though
    ' This will allow speech recognition
    ' Disabling button and commenting this out
    CheckForIllegalCrossThreadCalls = False
    RO.RecordWAVFile(2, "c:\rovio\rawwav.wav")
  End Sub

  Private Sub Button11_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button11.Click
    ' I'm close to getting this to work, not quite yet though
    ' This will allow speech recognition
    ' Disabling button and commenting this out
    RO.SpeechRecognize(5)
  End Sub

  Private Sub RO_AudioFunctionComplete(ByVal TheCurrentAudioFunction As Integer) Handles RO.AudioFunctionComplete
    Debug.Print(Now & " : Finished audio function: " & TheCurrentAudioFunction)
  End Sub

  Private Sub RO_RecognizedPhrase(ByVal ThePhrase As String) Handles RO.RecognizedPhrase
    ' I'm close to getting this to work, not quite yet though
    ' This will allow speech recognition
    Debug.Print(Now & " : Recognized """ & ThePhrase & """")
    TextBox2.Text = ThePhrase
    TextBox2.Refresh()
  End Sub

  Private Sub Button12_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button12.Click
    RO.GetStatus()
    Debug.Print("AudioFormat=" & RO.AudioFormat)
    Debug.Print("BatteryState=" & RO.BatteryState)
    Debug.Print("CurrentlyCharging=" & RO.CurrentlyCharging)
    Debug.Print("FirmwareVersion=" & RO.FirmwareVersion)
    Debug.Print("HeadElevationState=" & RO.HeadElevationState)
    Debug.Print("ImageBrightness=" & RO.ImageBrightness)
    Debug.Print("ImageResolution=" & RO.ImageResolution)
    Debug.Print("MicrophoneVolume=" & RO.MicrophoneVolume)
    Debug.Print("ObstacleFlag=" & RO.ObstacleFlag)
    Debug.Print("Position: X=" & RO.Position.X & ", Y=" & RO.Position.Y & ", Theta=" & RO.Position.Theta)
    Debug.Print("SpeakerVolume=" & RO.SpeakerVolume)
    Debug.Print("WiFiSignalStrength=" & RO.WiFiSignalStrength)
  End Sub

  Private Sub CheckBox1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox1.CheckedChanged
    ' Rovio, Light!
    RO.Headlight = CheckBox1.Checked
  End Sub

  Private Sub CheckBox2_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox2.CheckedChanged
    'BlueLight call does not work for my Rovio version
    'RO.BlueLight = CheckBox2.Checked
  End Sub

  Private Sub Button13_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button13.Click
    'RO.SpeechRecognize("c:\rovio\rawwav.wav")
    RO.SpeechRecognize("c:\rovio\Scott_HelloThere.wav")
  End Sub

  Private Sub Button14_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button14.Click
    Dim TR As TrainedRovio = New TrainedRovio
    TR.Show()
  End Sub
End Class
