Imports System.Speech.Recognition

Public Class TrainedRovio

  Public State As Integer = 1
  Public LastRecognizedPhrase As String

  Private WithEvents RO As New RovioWrap
  'Private RG As Speech.Recognition.Grammar

  Public WithEvents SR As New System.Speech.Recognition.SpeechRecognitionEngine
  Private RG As Speech.Recognition.Grammar

  Private Sub TrainedRovio_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
    CreateGrammar()
    SR.LoadGrammar(RG)
    RO.SpeakerVolume = 31
    State = 1
    Timer1.Enabled = True
  End Sub

  Private Sub CreateGrammar()
    'Dim gb As GrammarBuilder = New GrammarBuilder(New Choices("Rovio", "Bark", "Good boy", "Move left", "Move right", "Move back", "Move forward", "Turn around", "Turn left", "Turn right", "Spin", "Spin around", "Wander", "Speak", "Move backwards", "Move up"))
    Dim CommType As Choices = New Choices()
    CommType.Add("Rovio", "Bark", "Hello", "Run forward", "Good dog", "Move left", "Move right", "Go back", "Go backwards", "Move back", "Go forward", "Move forward", "Slide left", "Slide right", "Turn around", "Turn left", "Turn right", "Spin", "Spin around", "Wander", "Speak", "Move backwards", "Move up", "Good bye", "Exit", "Sit up", "Down boy", "Down girl", "Attack of the show")
    Dim gb As GrammarBuilder = New GrammarBuilder()
    gb.Append(CommType)
    RG = New Grammar(gb)
  End Sub

  Private Sub ChangeStatus(ByVal StatusLine As String)
    CheckForIllegalCrossThreadCalls = False
    lblStatus.Text = StatusLine
    txtHistory.AppendText(Now & " : " & StatusLine & vbCrLf)
  End Sub

  Private WaitNTicks As Integer
  Private NextStateAfterWait As Integer
  Private WanderCount As Integer
  Private tempfilenumber As Integer
  Private LastMoveParam As RovioWrap.MovementParameter

  Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
    Select Case State
      Case 0
        ' Wait state
        WaitNTicks -= 1
        If WaitNTicks < 0 Then
          State = NextStateAfterWait
        End If

      Case 1
        ' Just starting out, play greetings
        ChangeStatus("Saying hello")
        RO.PlayAudioFile("c:\rovio\ruff-ruff.pcm")
        State = 2

      Case 2
        ' Listen for command
        RO.Headlight = True
        ChangeStatus("Listening for 2 seconds")
        'RO.SpeechRecognize(2)
        tempfilenumber += 1 : If tempfilenumber > 10 Then tempfilenumber = 0
        Dim Filename As String = "c:\rovio\recwav" & tempfilenumber & ".wav"
        Try
          My.Computer.FileSystem.DeleteFile(Filename)
        Catch ex As Exception
          'Do nothing
        End Try
        RO.RecordWAVFile(2, Filename)
        'Application.DoEvents()
        State = 3

      Case 21
        Dim Filename = "c:\rovio\recwav" & tempfilenumber & ".wav"
        SR.SetInputToWaveFile(Filename)
        'SR.RecognizeAsync()
        Dim rr As System.Speech.Recognition.RecognitionResult
        rr = SR.Recognize()
        If rr Is Nothing Then
          ChangeStatus("No recognition")
          Debug.Print("Sync recognized : Nothing")
          LastRecognizedPhrase = ""
          State = 4
        Else
          ChangeStatus("Recognized : '" & rr.Text & "'")
          Debug.Print("Sync recognized : '" & rr.Text & "'")
          LastRecognizedPhrase = rr.Text
          State = 5
        End If


        'Case 21
        '  ' Command recording done
        '  LastRecognizedPhrase = ""
        '  Debug.Print(Now & " : Starting speech recognition on " & tempfilenumber)
        '  Dim Filename = "c:\rovio\recwav" & tempfilenumber & ".wav"
        '  Dim f1 As System.IO.FileInfo = My.Computer.FileSystem.GetFileInfo(Filename)
        '  
        '  Debug.Print("Info on '" & Filename & "' : " & f1.Length)
        '  RO.SpeechRecognize(Filename, RG)
        '  If LastRecognizedPhrase = "" Then
        '    State = 4
        '  Else
        '    State = 5
        '  End If


      Case 3
        ' Wait for response or no response
        ' do nothing

      Case 4
        ' Not recognized, try again
        Debug.Print(Now & " : Not Recognized")
        State = 2

      Case 6
        ' Prompt for command
        RO.PlayAudioFile("c:\rovio\roof-question.pcm")
        State = 0
        WaitNTicks = 6
        NextStateAfterWait = 2

      Case 5
        Debug.Print(Now & " : Matching recognized phrase '" & LastRecognizedPhrase & "'")

        Select Case LastRecognizedPhrase
          Case "Rovio"
            State = 6

          Case "Speak", "Hello"

            RO.PlayAudioFile("c:\rovio\Scott_HelloThere.pcm")
            State = 0
            WaitNTicks = 12
            NextStateAfterWait = 2

          Case "Bark", "Good dog"
            RO.PlayAudioFile("c:\rovio\ruff-ruff.pcm")
            State = 0
            WaitNTicks = 12
            NextStateAfterWait = 2


          Case "Sit up"
            RO.HeadElevationState = RovioWrap.RovioHeadElevationState.Midway
            RO.PlayAudioFile("c:\rovio\whine.pcm")
            State = 0
            WaitNTicks = 12
            NextStateAfterWait = 2

          Case "Down boy", "Down girl"
            RO.HeadElevationState = RovioWrap.RovioHeadElevationState.Down
            State = 0
            WaitNTicks = 12
            NextStateAfterWait = 6


          Case "Attack of the show"
            WanderCount = 50
            State = 40

          Case "Wander"
            RO.PlayAudioFile("c:\rovio\ru-roh.pcm")
            State = 0
            WaitNTicks = 6
            WanderCount = 100
            NextStateAfterWait = 30


          Case "Spin around", "Spin"
            LastMoveParam = RovioWrap.MovementParameter.rotateRight20Degrees
            WaitNTicks = 15
            State = 10

          Case "Turn left"
            LastMoveParam = RovioWrap.MovementParameter.rotateLeft20Degrees
            WaitNTicks = 4
            State = 10
          Case "Turn right"
            LastMoveParam = RovioWrap.MovementParameter.rotateRight20Degrees
            WaitNTicks = 4
            State = 10
          Case "Move left", "Slide left"
            LastMoveParam = RovioWrap.MovementParameter.slideLeft
            WaitNTicks = 4
            State = 10
          Case "Move right", "Slide right"
            LastMoveParam = RovioWrap.MovementParameter.slideRight
            WaitNTicks = 4
            State = 10
          Case "Move back", "Move backwards", "Go back", "Go backwards"
            LastMoveParam = RovioWrap.MovementParameter.moveBackward
            WaitNTicks = 4
            State = 10
          Case "Move forward", "Move up", "Go forward"
            LastMoveParam = RovioWrap.MovementParameter.moveForward
            WaitNTicks = 4
            State = 10

          Case "Run forward"
            LastMoveParam = RovioWrap.MovementParameter.moveForward
            WaitNTicks = 16
            State = 10

          Case "Turn around"
            LastMoveParam = RovioWrap.MovementParameter.rotateRight20Degrees
            WaitNTicks = 7
            State = 10

          Case "Good bye", "Exit"
            RO.PlayAudioFile("c:\rovio\bye-bye.pcm")
            State = 0
            WaitNTicks = 12
            NextStateAfterWait = 99

          Case Else
            State = 6

        End Select


      Case 10
        If WaitNTicks >= 0 Then
          WaitNTicks -= 1
          RO.Move(LastMoveParam, 1)
        Else
          State = 0
          WaitNTicks = 2
          NextStateAfterWait = 6
        End If

      Case 30
        WanderCount -= 1
        If WanderCount < 0 Then
          State = 6
        Else
          MoveRovio()
        End If

      Case 40
        WanderCount -= 1
        If WanderCount < 0 Then
          State = 6
        Else
          CrazyRovio()
        End If

      Case 99
        State = -1
        Timer1.Enabled = False
        Application.Exit()

    End Select
  End Sub



  Public Sub MoveRovio()
    ' Rovio, Come here!

    ' Get status needs to be called each time you wish to update the object
    ' But I am requiring that the user does it at the moment.
    RO.GetStatus()
    If RO.ObstacleFlag Then
      'Full stop
      'RO.Move(RovioWrap.MovementParameter.moveStop, 1)
      'Play ruh-roh
      If WaitNTicks > 0 Then
        ' don't play anything
        WaitNTicks -= 1
      Else
        WaitNTicks = 6
        RO.PlayAudioFile("c:\rovio\ru-roh.pcm")
      End If
      'Turn 20 degrees
      RO.Move(RovioWrap.MovementParameter.rotateRight20Degrees, 1)
    Else
      'Move forward
      RO.Move(RovioWrap.MovementParameter.moveForward, 1)
    End If
  End Sub


  Public Sub CrazyRovio()
    ' Rovio, Go nuts!

    If WaitNTicks > 0 Then
      ' don't play anything
      WaitNTicks -= 1
    Else
      WaitNTicks = 6
      Select Case Rnd()
        Case Is < 0.3
          RO.PlayAudioFile("c:\rovio\ru-roh.pcm")
        Case Is < 0.6
          RO.PlayAudioFile("c:\rovio\whine.pcm")
        Case Else
          RO.PlayAudioFile("c:\rovio\ruff-ruff.pcm")
      End Select
    End If
    'Turn 20 degrees
    RO.Move(RovioWrap.MovementParameter.rotateLeft20Degrees, 1)

  End Sub


  Private Sub RO_AudioFunctionComplete(ByVal TheCurrentAudioFunction As Integer) Handles RO.AudioFunctionComplete
    ' Do nothing atm
    Select Case TheCurrentAudioFunction
      Case 1
        RO.Headlight = False
        State = 21
    End Select
  End Sub

  Private Sub RO_RecognizedPhrase(ByVal ThePhrase As String) Handles RO.RecognizedPhrase
    LastRecognizedPhrase = ThePhrase
    ChangeStatus("Recognized: '" & ThePhrase & "'")
  End Sub

  Private Sub RO_UnRecognizedPhrase() Handles RO.UnRecognizedPhrase
    LastRecognizedPhrase = ""
    ChangeStatus("UnRecognized phrase")
  End Sub

  Private Sub SR_RecognizeCompleted(ByVal sender As Object, ByVal e As System.Speech.Recognition.RecognizeCompletedEventArgs) Handles SR.RecognizeCompleted
    Debug.Print("SR_RecognizeCompleted")
  End Sub

  Private Sub SR_SpeechDetected(ByVal sender As Object, ByVal e As System.Speech.Recognition.SpeechDetectedEventArgs) Handles SR.SpeechDetected
    Debug.Print("SR_SpeechDetected")
  End Sub

  Private Sub SR_SpeechRecognitionRejected(ByVal sender As Object, ByVal e As System.Speech.Recognition.SpeechRecognitionRejectedEventArgs) Handles SR.SpeechRecognitionRejected
    Debug.Print("SR_SpeechRecognitionRejected")
  End Sub

  Private Sub SR_SpeechRecognized(ByVal sender As Object, ByVal e As System.Speech.Recognition.SpeechRecognizedEventArgs) Handles SR.SpeechRecognized
    Debug.Print("SR_SpeechRecognized")
    Debug.Print("Recognized : '" & e.Result.Text & "'")
  End Sub

End Class