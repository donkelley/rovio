Imports System.Net
Imports System.IO
Imports System.Net.Sockets
'Imports Microsoft.DirectX.DirectSound

'************************************************************************************
'
' RovioWrap
'
' Encapsulates communication API from Wowwee Rovio "Roving Webcam" robot in a nice
' .NET wrapper.
'
' Tested at working with firmware 3.97 on Vista 64x
'
' Note: This is an ongoing opensource project.
'
' -----------------------------------------------------------------------------------
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
' Acknowledgements inline to: "RoboSapienPet", "mt", "Nocturnal"
'
' Visit Robosapienpet's cause for opensource Rovio code at:
' http://www.robocommunity.com/forum/thread/14178/Rovio-uses-eCos-WowWee-owes-us-some-open-source-code/;jsessionid=F885CB9D87BB3399D3E27CA1048EFA71
'
' -----------------------------------------------------------------------------------
'
' Wowwee Rovio Support page:
' http://www.wowweesupport.com/product_item.php?item=rovio&PHPSESSID=3132e7ed8a0235007faa51da1b9bd580
'
' Wowwee Rovio API v1.2: (That this object is currently based on)
' http://www.wowweesupport.com/pdf/Rovio_API_Specifications_v1.2.pdf?PHPSESSID=3132e7ed8a0235007faa51da1b9bd580
' 
' -----------------------------------------------------------------------------------
'
' I. Provides access to the Wowwee Rovio "Roving Webcam" in a .NET object.
'     a. Controls wheels, head position, headlight
'     b. Reads status of robot including IR obstacle sensor and other status
'     c. Requests image capture
'     d. Connects to RTSP stream from the microphone on robot
'     e. Outputs 8mhz - pcm - wav file to speaker on robot
'
'************************************************************************************
Public Class RovioWrap

  ' Rovio's address/port/and access to the last get status string requested
  Public RovioIP As String = "192.168.10.18"
    Public RovioHostName As String = "yourhostname"
  'Public RovioIP As String = "117.117.1.133"
  Public RovioPort As Integer = 80
  Public LastStatusString As String = ""
  Public Username As String = "admin"
  Public Password As String = ""

  ' Default movement speed for all movement commands
  ' (overridden in optional paramter list in some commands)
  Public DefaultMovementSpeed As Integer = 5

  Public Event RecognizedPhrase(ByVal ThePhrase As String)
  Public Event UnRecognizedPhrase()
  Public Event AudioFunctionComplete(ByVal TheCurrentAudioFunction As Integer)

  ' Rovio Head Elevation state and synonyms
  Public Enum RovioHeadElevationState
    Low = 0
    Down = 0
    Middle = 1
    Midway = 1
    High = 2
    Up = 2
  End Enum

  ' Rovio Movement parameters as per API
  ' Warning: If you insert additional synonyms, make sure you do not change order and that you
  '   add "=" number value or you will mess this up. (See VB enum documentation for understanding)
  Public Enum MovementParameter
    moveStop = 0
    moveForward = 1
    moveBackward
    slideLeft
    slideRight
    rotateLeftBySpeed
    rotateRightBySpeed
    moveDiagonalForwardLeft
    moveDiagonalForwardRight
    moveDiagonalBackwardLeft
    moveDiagonalBackwardRight = 10
    rotateLeft20Degrees = 17
    rotateRight20Degrees
  End Enum
  ' Rovio Image resolution ranges as per API
  Public Enum ImageResolutionIndex
    Lowest = 0 '{176, 144}
    Low = 1 '{352, 288}
    Midrange = 2 '{320, 240} (Default)
    ResolutionDefault = 2
    High = 3 '{640, 480}
  End Enum

#Region "Rovio Private variables"

  Private rovioAudioStreamSocket As Socket
  Private remoteAudioStreamEndPoint As IPEndPoint
  Private CurrentAudioSession As String = ""
  Private RequestCount As Integer = 1
  Private WithEvents T1 As Timers.Timer

  'Public WithEvents SRE As Speech.Recognition.SpeechRecognitionEngine
  Private ap1 As System.Media.SoundPlayer
  Private ms1 As System.IO.MemoryStream
  Private fs1 As System.IO.FileStream
  Dim enc As New System.Text.UTF8Encoding

  Private AlreadyPlayingStream As Boolean = False
  Public WithEvents SR As New System.Speech.Recognition.SpeechRecognitionEngine
  Private saf1 As System.Speech.AudioFormat.SpeechAudioFormatInfo
  Private G As Speech.Recognition.Grammar
  Private SpeechRecognitionStarted As Boolean = False

#End Region


#Region "Private Helper Functions"

  Private ReadOnly Property RovioHttpAddress() As String
    Get
      Dim foundIP = System.Net.Dns.GetHostByName(RovioHostName).AddressList(0).ToString()
      Return "http://" & foundIP
        'Return "http://" & RovioIP
    End Get
  End Property

  Private Function WowweeStringValue(ByVal TheField As String, ByVal TheStatusString As String) As Integer
    Dim i = InStr(TheStatusString, TheField)
    If i > 0 Then
      Dim j = InStr(i, TheStatusString, "|")
      If j > 0 Then
        Dim k = i + Len(TheField)
        Return Val(Mid(TheStatusString, k, j - k))
      End If
    End If
    Return -1
  End Function

  Private Function WowweeStringRealValue(ByVal TheField As String, ByVal TheStatusString As String) As Single
    Dim i = InStr(TheStatusString, TheField)
    If i > 0 Then
      Dim j = InStr(i, TheStatusString, "|")
      If j > 0 Then
        Dim k = i + Len(TheField)
        Return CSng(Mid(TheStatusString, k, j - k))
      End If
    End If
    Return 0
  End Function

  Private Shared Function StrToByteArray(ByVal str As String) As Byte()
    Dim encoding As New System.Text.ASCIIEncoding()
    Return encoding.GetBytes(str)
  End Function

  Private Shared Function ByteArrayToString(ByRef b() As Byte) As String
    Dim encoding As New System.Text.ASCIIEncoding()
    Return encoding.GetChars(b)
  End Function

  Private Sub SetRovioCredentials(ByVal Request As System.Net.WebRequest)
    If Username = "" Then
      ' If required by the server, set the credentials.
      Request.Credentials = CredentialCache.DefaultCredentials
    Else
      ' If you use a username password, try this
      Request.Credentials = New NetworkCredential(Username, Password)
    End If
  End Sub

  Private Sub WowweeRovioRequest(ByVal URLWithParameters As String, Optional ByRef ReturnResponse As String = "")
    ' Create a request for the URL. 		
    Dim Request As System.Net.WebRequest = WebRequest.Create(URLWithParameters)
    ' If required by the Rovio server, set the credentials.
    SetRovioCredentials(Request)
    ' Get the response.
    Dim Response As HttpWebResponse = CType(Request.GetResponse(), HttpWebResponse)
    ' Display the status.
    Debug.Print(Response.StatusDescription)
    ' Get the stream containing content returned by the server.
    Dim DataStream As Stream = Response.GetResponseStream()
    ' Open the stream using a StreamReader for easy access.
    Dim Reader As New StreamReader(DataStream)
    ' Read the content.
    Dim ResponseFromServer As String = Reader.ReadToEnd()
    ReturnResponse = ResponseFromServer
    ' Display the content.
    Debug.Print(ResponseFromServer)
    ' Cleanup the streams and the response.
    Reader.Close()
    DataStream.Close()
    Response.Close()
  End Sub

  Private Function ParseValueBetween(ByRef StringToParse As String, ByVal Prefix As Object, ByVal Postfix As Object) As String
    Dim BeginIndex, EndIndex As Integer

    If TypeOf Prefix Is String Then
      BeginIndex = InStr(StringToParse, Prefix)
      If BeginIndex = 0 Then Return ""
      BeginIndex += Len(Prefix)
    ElseIf TypeOf Prefix Is Integer Then
      BeginIndex = Prefix
    Else
      BeginIndex = 1
    End If
    If BeginIndex < 1 Or BeginIndex > StringToParse.Length Then Return ""

    If TypeOf Prefix Is String Then
      EndIndex = InStr(StringToParse, Postfix)
      EndIndex -= 1
      If EndIndex <= 0 Then Return ""
    ElseIf TypeOf Prefix Is Integer Then
      EndIndex = Prefix
    Else
      EndIndex = StringToParse.Length
    End If
    If EndIndex > StringToParse.Length Then Return ""

    If BeginIndex >= EndIndex Then Return ""

    Return Mid(StringToParse, BeginIndex, EndIndex - BeginIndex + 1)

  End Function

  Private Function KeepValueInRange(ByVal TheValue As Integer, ByVal LowerBound As Integer, ByVal UpperBound As Integer)
    If TheValue < LowerBound Then Return LowerBound
    If TheValue > UpperBound Then Return UpperBound
    Return TheValue
  End Function

#End Region

  ' *********************************************************************************
  ' Sub: GetStatus
  '   Request the overall status of Rovio (sends an HTTP request)
  '   
  '   This should be called only when needed prior to READING the following properties:
  '     BatteryState, ObstacleFlag, HeadElevationState
  ' *********************************************************************************
  Public Sub GetStatus()
    WowweeRovioRequest(RovioHttpAddress & "/rev.cgi?Cmd=nav&action=1", ReturnResponse:=LastStatusString)
  End Sub

  ' *********************************************************************************
  ' ReadOnly Property: FirmwareVersion
  '   Returns the date and time of the version in string form as given by Rovio.
  ' *********************************************************************************
  Public ReadOnly Property FirmwareVersion() As String
    Get
      Static PreviouslyReadVersion As String = ""
      If PreviouslyReadVersion = "" Then
        Dim VersionString As String = ""
        WowweeRovioRequest(RovioHttpAddress & "/GetVer.cgi", ReturnResponse:=VersionString)
        'Version = Oct 20 2008 20:14:28 $Id: ChangeLog.txt 3051 2008-10-08 07:28:52Z xhchen $
        PreviouslyReadVersion = ParseValueBetween(VersionString, "Version = ", " $Id:")
      End If
      Return PreviouslyReadVersion
    End Get
  End Property

  ' *********************************************************************************
  ' ReadOnly Property: BatteryState
  '   Returns the value directly from Rovio (see API), no interpretation done.
  ' *********************************************************************************
  Public ReadOnly Property BatteryState() As Integer
    Get
      Return WowweeStringValue("battery=", LastStatusString)
    End Get
  End Property

  Public Structure PositionInfo
    Public X As Integer
    Public Y As Integer
    Public Theta As Single
  End Structure

  Public ReadOnly Property Position() As PositionInfo
    Get
      With Position
        .X = CInt(WowweeStringValue("x=", LastStatusString))
        .Y = CInt(WowweeStringValue("y=", LastStatusString))
        .Theta = CSng(WowweeStringRealValue("theta=", LastStatusString))
      End With
    End Get
  End Property

  ' *********************************************************************************
  ' ReadOnly Property: ObstacleFlag
  '   Returns True or False if Rovio is detecting an Obstacle.
  '
  '   There is a delay of .2-.4 seconds between the removal of an obstacle and Rovio
  '     deciding that there is no obstacle there.  In addition, Rovio returns
  '     "false positives", returning that there is an obstacle when there is none.
  '     This false reporting is probably an issue with the IR sensor (alignment?) or
  '     the specularity of the ground in front of it.  Running Rovio on a rug may
  '     issue more false positives than a flat wood floor.
  ' *********************************************************************************
  Public ReadOnly Property ObstacleFlag() As Boolean
    Get
      Return (WowweeStringValue("flags=", LastStatusString) >= 6)
    End Get
  End Property

  ' *********************************************************************************
  ' Property: HeadElevationState
  '   Set or Return the Head Position of Rovio.
  '
  ' Note: My function values differ from the API. The API has the following values:
  '   204 = position low
  '   135-140 = position mid-way
  '   65 = position high
  ' Note: You may have to tweak these values for your own Rovio, I had to deviate from API
  '   to get valid results.
  '
  ' ---Sidekick
  ' *********************************************************************************
  Public Property HeadElevationState() As RovioHeadElevationState
    Get
      Dim value = WowweeStringValue("head_position=", LastStatusString)
      Select Case value
        Case Is >= 200
          Return RovioHeadElevationState.Low
        Case Is <= 75
          Return RovioHeadElevationState.High
        Case Else
          Return RovioHeadElevationState.Midway
      End Select
    End Get
    Set(ByVal value As RovioHeadElevationState)
      Dim HeadState As Integer
      Select Case value
        Case RovioHeadElevationState.Up
          HeadState = 11
        Case RovioHeadElevationState.Midway
          HeadState = 13
        Case Else
          HeadState = 12
      End Select

      ' Issue the head movement request
      ' Uses the default speed property
      WowweeRovioRequest(RovioHttpAddress & "/rev.cgi?Cmd=nav&action=18&drive=" & HeadState & "&speed=" & DefaultMovementSpeed)

    End Set
  End Property

  ' *********************************************************************************
  ' Property: ImageResolution
  '   Set or Return the Image Resolution of the Rovio camera.
  '   0 = [176x144]
  '   1 = [320x240]
  '   2 = [352x240]
  '   3 = [640x480]
  ' *********************************************************************************
  Public Property ImageResolution() As ImageResolutionIndex
    Get
      Return WowweeStringValue("resolution=", LastStatusString)
    End Get
    Set(ByVal value As ImageResolutionIndex)
      WowweeRovioRequest(RovioHttpAddress & "/ChangeResolution.cgi?ResType=" & KeepValueInRange(value, 0, 3))
    End Set
  End Property

  ' *********************************************************************************
  ' Property: ImageBrightness
  '   Set or Return the Image Brightness of the Rovio camera.
  '   0 - 6 (The lower the value is, the dimmer the image is) [default 6]
  '
  ' Note: As you may already know, the camera is horrible in dark rooms.
  ' *********************************************************************************
  Public Property ImageBrightness() As Integer
    Get
      Return WowweeStringValue("brightness=", LastStatusString)
    End Get
    Set(ByVal value As Integer)
      WowweeRovioRequest(RovioHttpAddress & "/ChangeBrightness.cgi?Brightness=" & KeepValueInRange(value, 0, 6))
    End Set
  End Property

  ' *********************************************************************************
  ' Property: SpeakerVolume
  '   Set or Return the speaker volume ON the Rovio.
  '   0 - 31 (The lower the value is, the lower the speaker volume is) [default 15]
  ' *********************************************************************************
  Public Property SpeakerVolume() As Integer
    Get
      Return WowweeStringValue("speaker_volume=", LastStatusString)
    End Get
    Set(ByVal value As Integer)
      WowweeRovioRequest(RovioHttpAddress & "/ChangeSpeakerVolume.cgi?SpeakerVolume=" & KeepValueInRange(value, 0, 31))
    End Set
  End Property

  ' *********************************************************************************
  ' Property: AudioFormat
  '   Set or Return the Audio Format
  '   Audio setting as per API
  '     0 – AMR
  '     1 – PCM
  '     2 – IMAADPCM
  '     3 – ULAW
  '     4 – ALAW [default]
  ' Not making this an Enum since user shouldn't be really using it, but will make it public.
  '
  ' Note: Found that this call to SetMediaFormat takes a while to process on the Rovio
  '   even though it returns instantly.  Probably about a 2-3 second delay.
  ' *********************************************************************************
  Public Property AudioFormat() As Integer
    Get
      Dim ReturnValue As String = ""
      WowweeRovioRequest(RovioHttpAddress & "/GetMediaFormat.cgi", ReturnValue)
      Return CInt(ParseValueBetween(ReturnValue, "Audio = ", "Video ="))
    End Get
    Set(ByVal value As Integer)
      WowweeRovioRequest(RovioHttpAddress & "/SetMediaFormat.cgi?Audio=" & KeepValueInRange(value, 0, 4))
    End Set
  End Property

  ' *********************************************************************************
  ' Property: WiFiSignalStrength
  '   Set or Return the speaker volume ON the Rovio.
  '   0 - 254 (The lower the value is, the lower the signal is)
  ' *********************************************************************************
  Public ReadOnly Property WiFiSignalStrength() As Integer
    Get
      Return WowweeStringValue("wifi_ss=", LastStatusString)
    End Get
  End Property

  ' *********************************************************************************
  ' Property: CurrentlyCharging
  '   As per API, if charging value is <=79 then return True, else assume False is >=80
  '
  ' Note: Had to modify charging value to 80 for my Rovio.
  ' *********************************************************************************
  Public ReadOnly Property CurrentlyCharging() As Boolean
    Get
      Return (WowweeStringValue("charging=", LastStatusString) <= 80)
    End Get
  End Property

  ' *********************************************************************************
  ' Property: MicrophoneVolume
  '   Set or Return the speaker volume ON the Rovio.
  '   0 - 31 (The lower the value is, the lower the microphone volume is) [default 15]
  ' *********************************************************************************
  Public Property MicrophoneVolume() As Integer
    Get
      Return WowweeStringValue("mic_volume=", LastStatusString)
    End Get
    Set(ByVal value As Integer)
      WowweeRovioRequest(RovioHttpAddress & "/ChangeMicVolume.cgi?MicVolume=value" & KeepValueInRange(value, 0, 31))
    End Set
  End Property

  ' *********************************************************************************
  ' ReadOnly Property: CurrentImage
  '   Returns a System.Drawing.Image object from Rovio's current image buffer.
  '
  '   This command does not save the image to a file. To do so, after calling this
  ' property, call the Image.Save("Filename") command.
  '
  ' For example:
  '       PictureBox1.SizeMode = PictureBoxSizeMode.StretchImage
  '       PictureBox1.Image = RO.CurrentImage
  '       PictureBox1.Image.Save("c:\rovio\ImageIn.jpg")
  '
  ' This code snippet uses a PictureBox on a form (sets it to StretchImage since may
  '   not know the size of the returning picture), calls the Rovio Object (RO) CurrentImage
  '   property and sets it to the Image object of the PictureBox, then has the Image object
  '   save itself to a jpg file.
  ' Another way to save the image to a file would be just to call:
  '       RO.CurrentImage.Save("c:\rovio\ImageIn.jpg")
  '
  ' ---Sidekick
  ' *********************************************************************************
  Public ReadOnly Property CurrentImage() As System.Drawing.Image
    Get
      ' Create a request for the URL. 		
      Dim request As System.Net.WebRequest = WebRequest.Create(RovioHttpAddress & "/Jpeg/CamImg0001.jpg")
      ' If required by the server, set the credentials.
      'request.Credentials = CredentialCache.DefaultCredentials
      SetRovioCredentials(request)
      ' Get the response.
      Dim response As HttpWebResponse = CType(request.GetResponse(), HttpWebResponse)
      ' Display the status.
      Debug.Print(response.StatusDescription)
      ' Get the stream containing content returned by the server.
      Dim dataStream As Stream = response.GetResponseStream()
      ' Open the stream using a StreamReader for easy access.
      Dim reader As New StreamReader(dataStream)
      ' Read the content into an Image object
      Dim TheImage As Image = Image.FromStream(dataStream)
      ' Cleanup the streams and the response.
      reader.Close()
      dataStream.Close()
      response.Close()
      Return TheImage
    End Get
  End Property


  ' *********************************************************************************
  ' Sub: Move
  '   Moves Rovio in a specific direction for approximately .1-.2 seconds.  This movement
  ' command needs to be given every .1-.2 seconds for SMOOTH movement.
  '
  '   Direction - One of the MovementParamter enum type
  '   Speed - needs to be between 1 (for slowest) and 10 (for fastest), inclusive.
  '
  '   Wowwee designers were smart in that they made sure that the connection to Rovio
  ' still exists for movement to occur without having to require that the client be "pinged".
  ' For example, if you issue a move command foward and Rovio begins to move forward, but
  ' then your connection to Rovio drops, then it will still move forward. But since
  ' Wowwee only allows Rovio to move for about .1 seconds, this means that if the connection
  ' drops, movement will stop after about .2 seconds.
  '   However, this also means that to have smooth moment, you need to essentially spam
  ' Rovio with movement requests.  I have found that issuing a Move on a 100ms timer does
  ' the job quite nicely.
  ' *********************************************************************************
  Public Sub Move(ByVal Direction As MovementParameter, ByVal Speed As Integer)

    ' Keep speed within the range of the API request
    If Speed < 1 Then Speed = 1
    If Speed > 10 Then Speed = 10

    ' Send a movement request to Rovio
    WowweeRovioRequest(RovioHttpAddress & "/rev.cgi?Cmd=nav&action=18&drive=" & Direction & "&speed=" & Speed)

  End Sub

  ' *********************************************************************************
  ' Sub: Say
  '   This will use your default settings for the current TextToSpeech (TTS) voice installed
  ' on your system to output a string of text given to the TTS directly to the Rovio.
  '
  ' Cavets: Audio is low.  I will be updating this routine to have TTS output to a WAV stream, convert
  '   it appropriately, boost the amplitude, and use the PlayAudioFile function instead of having
  '   TTS directly output to the Rovio.  That is, unless someone can figure out how to make the
  '   TTS get the volume up, I've tried everything!
  '
  '   This code was adopted from the Rovio community message boards at:
  ' http://www.robocommunity.com/forum/thread/15055/Can-rovio-somehow-do-Text-gt-Speech/?highlight=text+to+speech
  ' Converted from C# to VB.NET by me, but originally written by user "mt".
  ' On behalf of all of us, thank you, "mt".
  '
  ' ---Sidekick
  ' *********************************************************************************
  Public Sub Say(ByVal TextToSpeak As String, _
                  Optional ByVal SpeakerVolume As Integer = 100, _
                  Optional ByVal SpeakerRate As Integer = -1)

    Try
      ' Set up the connection
      Dim rovioAudioSocket As Socket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
      Dim remoteEndPoint As IPEndPoint = New System.Net.IPEndPoint(System.Net.IPAddress.Parse(RovioHttpAddress()), RovioPort)
      rovioAudioSocket.Connect(remoteEndPoint)
      Debug.Print(Now & " : Audio socket connected")

      ' Prepare the audio port for sending audio from the PC back to the Rovio
      Dim temp1 As String = "POST /GetAudio.cgi HTTP/1.1" & vbCrLf & "User-Agent: AudioAgent" & vbCrLf & "Host: " & RovioHttpAddress() & vbCrLf & "Content-Length: 2147483647" & vbCrLf & "Cache-Control: no-cache" & vbCrLf & vbCrLf
      Dim szSend() As Byte
      szSend = StrToByteArray(temp1)
      '//REVIEW: skipping Credentials ???
      If (rovioAudioSocket.Send(szSend) = -1) Then Debug.Print(Now & " : Error sending initial packet")

      ' Create a memory stream and set up the TTS SpeechSynthesizer
      ' Audio format: 16bit PCM, 8000Hz
      Dim stream1 As Stream = New MemoryStream()
      Dim speaker As New System.Speech.Synthesis.SpeechSynthesizer
      speaker.SetOutputToAudioStream(stream1, New System.Speech.AudioFormat.SpeechAudioFormatInfo(8000, System.Speech.AudioFormat.AudioBitsPerSample.Sixteen, System.Speech.AudioFormat.AudioChannel.Mono))
      speaker.Volume = SpeakerVolume
      speaker.Rate = SpeakerRate

      ' TTS now speaks the text given
      speaker.Speak(TextToSpeak)

      ' Convert the TTS output to something Rovio can understand. e.g. stream to byte array
      stream1.Seek(0, SeekOrigin.Begin)
      Dim output1(stream1.Length) As Byte
      stream1.Read(output1, 0, stream1.Length)
      rovioAudioSocket.Send(output1)
      rovioAudioSocket.Close()
      Debug.Print(Now & " : Audio socket closed")

    Catch ex As Exception
      ' Doesn't play? Check your debug window for why, but program won't crash
      Debug.Print(Now + " : " + ex.Message.ToString())

    End Try

  End Sub

  ' *********************************************************************************
  ' Sub: PlayAudioFile
  '   This command will play an audio file through to the Rovio.
  '   Currently WAV File must be PCM 8mhz 16-bit Mono
  '
  ' Note: To convert your WAV files you can use various methods, one I use is the open source:
  '   http://mediacoder.sourceforge.net/index.htm
  '   It is fast, and does many, many things. Amazing opensource project.
  '
  ' Note: I am leaving in some older code traces for future development.  I want to
  '   automatically convert any wave file into a valid stream that Rovio can play as
  '   well as add some "effects" to the stream.  For example, it appears in the secondary
  '   buffer I can add some reverb to a playing stream to simulate a bump, I have to
  '   experiment with this and see what is available.
  '
  ' Here are a few URL's that have additional WAV file information I am using:
  '   http://www.sonicspot.com/guide/wavefiles.html
  '   http://ccrma.stanford.edu/courses/422/projects/WaveFormat/
  '   http://www.thisisnotalabel.com/wavio/WavIO.vb
  '
  '   This code was adapted from the Rovio community message boards at:
  ' http://www.robocommunity.com/forum/thread/15055/Can-rovio-somehow-do-Text-gt-Speech/?highlight=text+to+speech
  ' Converted from C# to VB.NET by me, but originally written by user "mt".
  ' On behalf of all of us, thank you, "mt".
  '
  ' ---Sidekick
  '   
  ' *********************************************************************************
  Public Sub PlayAudioFile(ByVal AudioFile As String)

    'Dim sms = New System.Media.SoundPlayer

    'Dim b2 As Microsoft.DirectX.DirectSound.SecondaryBuffer
    'Dim d2 As Microsoft.DirectX.DirectSound.Device
    'd2 = New Device
    'd2.SetCooperativeLevel(RovioController.Handle, CooperativeLevel.Priority)
    'b2 = New SecondaryBuffer(AudioFile, d2)
    ''b2.Play(0, BufferPlayFlags.Default)

    'Exit Sub

    'Dim ap1 As New System.Media.SoundPlayer
    'ap1.SoundLocation = AudioFile
    'ap1.Play()

    'Exit Sub
    Try
      ' Create the connections
      Dim rovioAudioSocket As Socket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
      Dim remoteEndPoint As IPEndPoint = New System.Net.IPEndPoint(System.Net.IPAddress.Parse(RovioHttpAddress()), RovioPort)
      rovioAudioSocket.Connect(remoteEndPoint)
      Debug.Print(Now & " : Audio socket connected")
      ' Prepare the audio port for sending audio from the PC back to the Rovio
      Dim temp1 As String = "POST /GetAudio.cgi HTTP/1.1" & vbCrLf & "User-Agent: AudioAgent" & vbCrLf & "Host: " & RovioHttpAddress() & vbCrLf & "Content-Length: 2147483647" & vbCrLf & "Cache-Control: no-cache" & vbCrLf & vbCrLf
      Dim szSend() As Byte
      szSend = StrToByteArray(temp1)
      '//REVIEW: skipping Credentials ???
      If (rovioAudioSocket.Send(szSend) = -1) Then Debug.Print(Now & " : Error sending initial packet")

      ' Open the WAV file as a stream
      ' WAV File must be PCM 8mhz 16-bit Mono
      ' so you may need to convert it at the moment, see comments above
      Dim stream1 As Stream = New FileStream(AudioFile, FileMode.Open)

      ' Stream to byte array
      stream1.Seek(44, SeekOrigin.Begin)
      Dim output1 As Byte()
      ReDim output1(stream1.Length)
      stream1.Read(output1, 0, stream1.Length)

      ' Send the byte array (from the WAV file data) to Rovio to play
      rovioAudioSocket.Send(output1)
      rovioAudioSocket.Close()
      stream1.Close()
      Debug.Print(DateTime.Now.ToString() & " : Audio socket closed")

    Catch ex As Exception
      ' Doesn't play? Check your debug window for why, but program won't crash
      Debug.Print(Now + " : " + ex.Message)

    End Try

  End Sub


  ' *********************************************************************************
  ' Property (WriteOnly) : Headlight
  '   Sets the headlight on or off.
  '
  ' Hardwork done for this tip by Robosapienpet!
  ' Thank you, Robosapienpet!
  '
  ' Visit Robosapienpet's cause for opensource Rovio code at:
  '   http://www.robocommunity.com/forum/thread/14178/Rovio-uses-eCos-WowWee-owes-us-some-open-source-code/;jsessionid=F885CB9D87BB3399D3E27CA1048EFA71
  '
  '---Sidekick
  ' *********************************************************************************
  Public WriteOnly Property Headlight() As Boolean
    Set(ByVal value As Boolean)
      WowweeRovioRequest(RovioHttpAddress & "/rev.cgi?Cmd=nav&action=19&LIGHT=" & IIf(value, 1, 0))
    End Set
  End Property

  '' I must be doing something wrong, this still doesn't work
  '' Must have firmware version 5.00b10
  '' *********************************************************************************
  '' Property BlueLight
  ''   Turns off and on the BlueLights.
  ''   Supply LightFlags as desired.  63 represents all 6 lights.
  ''
  ''   Depending on if the hack has been used already, the first call to Bluelight may
  '' not give desired results.  Commenting it out until further testing.
  '' *********************************************************************************
  'Public WriteOnly Property BlueLight(Optional ByVal LightFlags As Integer = 63) As Boolean
  '  Set(ByVal value As Boolean)
  '    LightFlags = KeepValueInRange(LightFlags, 0, 63)
  '    'Invert the bits if turning off
  '    If Not value Then LightFlags = 64 - LightFlags
  '    LightFlags = 16

  '    'http://www.robocommunity.com/forum/thread/14374/Stupid-URL-tricks-turn-off-the-annoying-blue-LEDs/
  '    '
  '    'http://{IP_ADDRESS}/debug.cgi?action=write_mem&address=0xB4E1C&size=0x1&value=0x1A
  '    'http://{IP_ADDRESS}/rev.cgi?Cmd=nav&action=19&LIGHT=0
  '    'http://{IP_ADDRESS}/debug.cgi?action=write_mem&address=0xB4E1C&size=0x1&value=0x18 
  '    '      For firmware 409 (access these 3 URLs in order, no spaces)
  '    'http://{IP_ADDRESS} /debug.cgi?action=write_mem&address=0xB6B0C&size=0x1&value=0x1A
  '    'http://{IP_ADDRESS} /rev.cgi?Cmd=nav&action=19&LIGHT=0
  '    'http://{IP_ADDRESS} /debug.cgi?action=write_mem&address=0xB6B0C&size=0x1&value=0x18
  '    '
  '    'What(it Is doing)
  '    'The first URL patches the firmware to let you control the body LEDs using the "LIGHT" command
  '    'The second URL controls the body LEDs. You can use other numbers for the LIGHT parameter. 63 will turn on all 6 blue LEDs. Other numbers will turn on selective lights
  '    '(lights are numbered from the front left counterclockwise: 1,2,4,8,16,32)
  '    'eg: 1 turns on the front left light, 5 (1+4) turns on the front left and rear left.
  '    'The last URL reverses the firmware patch so you can use the LIGHT command as usual (it normally controls the headlight)
  '    'NOTE: Changes are not permanent. Power off and on to undo any patches. 
  '    'DISCLAIMER: Firmware 4.01 or 4.02 (may not work in future releases). USE AT YOUR OWN RISK.
  '    'Hardwork done for this tip by Robosapienpet!
  '    'Thank you, Robosapienpet!
  '    '
  '    'Visit Robosapienpet's cause for opensource Rovio code at:
  '    ' http://www.robocommunity.com/forum/thread/14178/Rovio-uses-eCos-WowWee-owes-us-some-open-source-code/;jsessionid=F885CB9D87BB3399D3E27CA1048EFA71
  '    '
  '    ' New 5.00b10 location is at 0xC57E4
  '    ' Thanks to Nocturnal with this update.

  '    ' Original thread is at: 
  '    ' http://www.robocommunity.com/forum/thread/14374/Stupid-URL-tricks-turn-off-the-annoying-blue-LEDs/?page=1
  '    '
  '    '---Sidekick


  '    'Select Case FirmwareVersion
  '    '  Case "5.00b10'"
  '    ' Updated location from Nocturnal on robocommunity forums
  '    ' 5.00b10 can get gotten from http://www.robocommunity.com/download/list
  '    WowweeRovioRequest(RovioHttpAddress & "/debug.cgi?action=read_mem&address=0xC57E4&size=0x1&value=0x1A")
  '    WowweeRovioRequest(RovioHttpAddress & "/rev.cgi?Cmd=nav&action=19&LIGHT=" & LightFlags)
  '    WowweeRovioRequest(RovioHttpAddress & "/debug.cgi?action=read_mem&address=0xC57E4&size=0x1&value=0x18")
  '    'Case "Oct 20 2008 20:14:28"
  '    '  ' This is my Rovio version number out of the box, works for this
  '    '  WowweeRovioRequest(RovioHttpAddress & "/debug.cgi?action=read_mem&address=0xB6B0C&size=0x1&value=0x1A")
  '    '  WowweeRovioRequest(RovioHttpAddress & "/rev.cgi?Cmd=nav&action=19&LIGHT=" & LightFlags)
  '    '  WowweeRovioRequest(RovioHttpAddress & "/debug.cgi?action=read_mem&address=0xB6B0C&size=0x1&value=0x18")
  '    'Case "409"
  '    '  'Not sure what firmware version 409 date is, so should never get here
  '    '  WowweeRovioRequest(RovioHttpAddress & "/debug.cgi?action=read_mem&address=0xB4E1C&size=0x1&value=0x1A")
  '    '  WowweeRovioRequest(RovioHttpAddress & "/rev.cgi?Cmd=nav&action=19&LIGHT=" & LightFlags)
  '    '  WowweeRovioRequest(RovioHttpAddress & "/debug.cgi?action=read_mem&address=0xB4E1C&size=0x1&value=0x18")
  '    'Case Else
  '    'I don't know the other dates or how to get a real version number
  '    'Let me know if you do!
  '    'End Select

  '  End Set
  'End Property




  ' *********************************************************************************
  ' IN DEVELOPMENT
  '   The following code region is in development.  I have gotten it to do many things
  ' and the RTSP protocol is working perfectly, as well as the streaming into a WAV file,
  ' but I seem to be unable to direct that 8-bit A-LAW 8mhz stream into the Speech Recognition
  ' system.  I am, however, able to get the Speech Recognition to read from a WAV file AND
  ' a WAV stream, just not this A-LAW compressed stream.  A very inner-exception of object is
  ' null always comes up as I activate the async recognition.  I'll keep trying, but any of you
  ' out there that have a clue about this stuff, your comments would be appreciated.
  '
  ' I coverted a lot of C++ code from robosapienpet@aibohack.com from the forum message he posted:
  ' http://www.robocommunity.com/forum/thread/14158/Rovio-WebServer-URLs-JavaScript-and-tech-stuff/?highlight=video+stream
  ' Thank you, "robosapienpet"!
  '
  ' ---Sidekick
  ' *********************************************************************************

#Region "RTSP Streaming Microphone Audio to Speech Recognition or to WAV file"

  Private Function SendRTSPRequest(ByVal TheSocket As Socket, ByVal TheSend As String, ByRef TheResponse As String) As Boolean
    Dim szSend() As Byte = StrToByteArray(TheSend)
    'Debug.Print("Sent: """ & TheSend & """")
    TheSocket.Send(szSend)
    Dim szRecieve(512) As Byte
    Dim r As Integer = TheSocket.Receive(szRecieve, szRecieve.Length, SocketFlags.None)
    '//socket must be blocking, with nothing before or after it
    If r = -1 Then Return False
    TheResponse = ByteArrayToString(szRecieve)
    'Debug.Print("Recieved: """ & TheResponse & """")
    If Not (Left(TheResponse, 15) = "RTSP/1.0 200 OK") Then
      Return False
    End If
    Return True
  End Function

  Dim CurrentAudioFunction As Integer = 0
  Dim CurrentFilename As String

  Public Sub RecordWAVFile(ByVal SecondsToRecord As Integer, ByVal Filename As String)
    If Not SR Is Nothing Then
      SR.SetInputToNull()
    End If
    CurrentAudioFunction = 1
    CurrentFilename = Filename
    If Not fs1 Is Nothing Then
      fs1.Close()
      fs1 = Nothing
    End If
    If fs1 Is Nothing Then fs1 = CreateWAVFile(SecondsToRecord, Filename)
    StartAudioStream()
  End Sub

  Public Sub SpeechRecognize(ByVal SecondsToRecognize As Integer)
    MsgBox("Does not work because Microsoft Speech Recognition object does not accept a stream for some reason.  Probably I am not formatting it appropraitely.")
    ' This is not working yet due to issue with SR Engine
    ' apparently this is an improper WAV file stream for the SR engine
    'CurrentAudioFunction = 2
    'ms1 = CreateWAVStream(SecondsToRecognize)
    'StartAudioStream()

    'commented out all CurrentAudioFunction = 2 code below
  End Sub

  ' Use the Speech Recognition engine
  Public Sub SpeechRecognize(ByVal Filename As String, Optional ByVal G As Speech.Recognition.Grammar = Nothing)
    MsgBox("Does not work because of threading issues.  I'll get to this, just not now.  Use the 'TrainedRovio.vb' as an example how to do some simple speech recognition.")
    If SR Is Nothing Then
      SR = New System.Speech.Recognition.SpeechRecognitionEngine
      If G Is Nothing Then G = New Speech.Recognition.DictationGrammar
      SR.LoadGrammar(G)
    End If
    'SR.SetInputToNull()
    'Dim ms3 As System.IO.FileStream = New System.IO.FileStream(Filename, FileMode.Open, FileAccess.Read)
    'SR.SetInputToWaveStream(ms3)
    SR.SetInputToWaveFile(Filename)
    Dim rr As Speech.Recognition.RecognitionResult = SR.Recognize()
    'SR.SetInputToNull()
    'SR.SetInputToWaveStream(Nothing)
    'ms3.Close()
    'ms3 = Nothing
    Debug.Print("Done recognizing")

    If Not rr Is Nothing Then
      Debug.Print("Recognized '" & rr.Text & "'")
      RaiseEvent RecognizedPhrase(rr.Text)
    Else
      Debug.Print("Nothing recognized")
      RaiseEvent UnRecognizedPhrase()
    End If

  End Sub

  Public Function StartAudioStream() As System.IO.Stream

    CurrentAudioSession = ""
    rovioAudioStreamSocket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    remoteAudioStreamEndPoint = New System.Net.IPEndPoint(System.Net.IPAddress.Parse(RovioHttpAddress()), 554)
    rovioAudioStreamSocket.Connect(remoteAudioStreamEndPoint)
    Debug.Print(Now & " : Audio STREAM Input socket connected")
    '// Prepare the audio port for sending audio from the PC back to the Rovio

    Dim Send01 As String = "DESCRIBE rtsp://" & RovioHttpAddress() & "/webcam RTSP/1.0" & vbCrLf & "CSeq : " & RequestCount & vbCrLf & _
                          "Accept: application/sdp" & vbCrLf & "User-Agent: fakeagent" & vbCrLf & vbCrLf
    RequestCount += 1
    Dim Response01 As String = ""
    If SendRTSPRequest(rovioAudioStreamSocket, Send01, Response01) Then
      'Debug.Print(Now & " : DESCRIBE complete ('" & Response01 & "')")
    Else
      Debug.Print(Now & " : DESCRIBE failed ('" & Response01 & "')")
      rovioAudioStreamSocket.Disconnect(False) : Return Nothing
    End If

    Dim Send02 As String = "SETUP rtsp://" & RovioHttpAddress() & "/webcam/track1 RTSP/1.0" & vbCrLf & "CSeq : " & RequestCount & vbCrLf & _
                          "Transport: RTP/AVP/TCP;unicast" & vbCrLf & vbCrLf
    RequestCount += 1
    Dim Response02 As String = ""
    If SendRTSPRequest(rovioAudioStreamSocket, Send02, Response02) Then
      'Debug.Print(Now & " : SETUP complete ('" & Response02 & "')")
      ' Parse out the current 30 byte session string
      Dim n = InStr(Response02, "Session: ")
      If n = 0 Then
        Debug.Print(Now & " : SETUP failed - No Session ID parsable in return from SETUP call")
        rovioAudioStreamSocket.Disconnect(False) : Return Nothing
      Else
        CurrentAudioSession = Mid(Response02, n + 9, 30)
      End If
    Else
      Debug.Print(Now & " : SETUP failed ('" & Response02 & "')")
      rovioAudioStreamSocket.Disconnect(False) : Return Nothing
    End If

    Dim Send03 As String = "PLAY rtsp://" & RovioHttpAddress() & "/webcam RTSP/1.0" & vbCrLf & "CSeq : " & RequestCount & vbCrLf & _
                      "Session: " & CurrentAudioSession & vbCrLf & "Range: npt=0-" & vbCrLf & "User-Agent : fakeagent" & vbCrLf & vbCrLf
    RequestCount += 1
    Dim Response03 As String = ""
    If SendRTSPRequest(rovioAudioStreamSocket, Send03, Response03) Then
      'Debug.Print(Now & " : PLAY complete ('" & Response03 & "')")
    Else
      Debug.Print(Now & " : PLAY failed ('" & Response03 & "')")
      rovioAudioStreamSocket.Disconnect(False) : Return Nothing
    End If


    '--------------------------------
    ' Output to wav stream and play

    'ap1 = New System.Media.SoundPlayer
    'ms1 = New System.IO.MemoryStream
    'AlreadyPlayingStream = False
    'ap1.Stream = ms1

    ' write wav header to stream
    'http://www.thisisnotalabel.com/wavio/WavIO.vb
    'PutWavHeaderOnStream(ms1)
    'http://www.eggheadcafe.com/community/aspnet/14/52234/waveform-byte-stream.aspx




    '--------------------------------

    ''For future use : speech recognition
    ''http://msdn.microsoft.com/en-us/library/system.speech.recognition.speechrecognitionengine.setinputtoaudiostream.aspx
    'SpeechRecognitionStarted = False
    'SR = New System.Speech.Recognition.SpeechRecognitionEngine
    'G = New Speech.Recognition.DictationGrammar
    'SR.LoadGrammar(G)

    ''From RTSP audio stream (DOESN'T WORK FOR ALAW?)
    'ms1 = New System.IO.MemoryStream
    'saf1 = New System.Speech.AudioFormat.SpeechAudioFormatInfo(Speech.AudioFormat.EncodingFormat.ALaw, 8000, Speech.AudioFormat.AudioBitsPerSample.Eight, Speech.AudioFormat.AudioChannel.Mono, 8000, 1, Nothing)
    'SR.SetInputToAudioStream(ms1, saf1)

    'From Wav file in Stream
    'Dim ms3 As System.IO.FileStream = New System.IO.FileStream("c:\rovio\Scott_HelloThere.wav", FileMode.Open, FileAccess.Read)
    'SR.SetInputToWaveStream(ms3)

    'From Wav file
    'SR.SetInputToWaveFile("c:\rovio\Scott_HelloThere.wav")

    'Default input microphone
    'SR.SetInputToDefaultAudioDevice()

    '' From RTSP audio stream converted from ALAW 8-8000 or PCM 8000 to WAV 16-8000
    'ms1 = New System.IO.MemoryStream
    'ms1 = CreateWavHeaderStream()
    'SR.SetInputToWaveStream(ms1)


    Select Case CurrentAudioFunction
      Case 1
        ' Record audio stream to wav file
        ' From RTSP audio stream converted from ALAW 8-8000 or PCM 8000 to WAV 16-8000
        'fs1 = CreateWAVFile(TargetRecordSeconds, TargetAudioFilename)
      Case 2
        ' Recognize audio stream
        'SpeechRecognitionStarted = False
        'SR = New System.Speech.Recognition.SpeechRecognitionEngine
        'G = New Speech.Recognition.DictationGrammar
        'SR.LoadGrammar(G)

    End Select


    'Continue RTSP transmission and place data on stream, start recognition
    T1 = New Timers.Timer(50)
    T1.Start()

    Return Nothing

  End Function

  Private Sub SR_RecognizeCompleted(ByVal sender As Object, ByVal e As System.Speech.Recognition.RecognizeCompletedEventArgs) Handles SR.RecognizeCompleted

    RaiseEvent RecognizedPhrase(e.Result.Text)
    'Debug.Print(Now & " : Recognized """ & e.Result.Text & """")

    ' Save to file what was recognized
    Static w As Integer = 0
    Dim ms2 As System.IO.FileStream = New System.IO.FileStream("c:\rovio\recoutput" & w & ".wav", FileMode.Create)
    e.Result.Audio.WriteToWaveStream(ms2)
    ms2.Close()
    ms2 = Nothing
    w += 1

  End Sub

  Dim MaximumBytesToWrite As Integer = 0

  Public Function CreateWAVFile(ByVal SampleSeconds As Integer, ByVal Filename As String) As System.IO.FileStream
    'http://www.thisisnotalabel.com/wavio/WavIO.vb
    'Dim header(43) As Byte
    Dim ms As New System.IO.FileStream(Filename, FileMode.Create) '(header, 0, 44, True)

    Dim myFormat As Integer = 6 '1 for PCM, 6 for a-law
    Dim myChannels As Integer = 1
    Dim myBitsPerSample As Integer = 8
    Dim mySampleRate As Integer = 8000
    Dim myByteRate As Integer = mySampleRate * myChannels * myBitsPerSample / 8
    Dim myBlockAlign As Integer = myChannels * myBitsPerSample / 8

    Dim SampleDataSize As Integer = myByteRate * SampleSeconds

    ' CHUNK DESCRIPTOR
    ms.Write(enc.GetBytes("RIFF"), 0, 4) ' 0: RIFF
    ms.Write(System.BitConverter.GetBytes(36 + SampleDataSize), 0, 4) ' 4: entire filesize - 8
    ms.Write(enc.GetBytes("WAVE"), 0, 4) ' 8: WAVE

    ' FORMAT SUBCHUNK
    ms.Write(enc.GetBytes("fmt "), 0, 4) ' 12: fmt
    ms.Write(System.BitConverter.GetBytes(16), 0, 4) ' 16: 16 for subchunk size
    ms.Write(System.BitConverter.GetBytes(myFormat), 0, 2) ' 18: 1 for PCM
    ms.Write(System.BitConverter.GetBytes(myChannels), 0, 2) ' 20: 1 for mono, 2 for stereo
    ms.Write(System.BitConverter.GetBytes(mySampleRate), 0, 4) ' 24: 44100 for CD quality
    ms.Write(System.BitConverter.GetBytes(myByteRate), 0, 4) ' 28: 
    ms.Write(System.BitConverter.GetBytes(myBlockAlign), 0, 2) ' 32:
    ms.Write(System.BitConverter.GetBytes(myBitsPerSample), 0, 2) ' 34:

    ' DATA SUBCHUNK
    ms.Write(enc.GetBytes("data"), 0, 4)
    ms.Write(System.BitConverter.GetBytes(SampleDataSize), 0, 4)
    MaximumBytesToWrite = SampleDataSize


    Return ms

  End Function

  Public Function CreateWAVStream(ByVal SampleSeconds As Integer) As System.IO.MemoryStream
    'http://www.thisisnotalabel.com/wavio/WavIO.vb
    Dim ms As New System.IO.MemoryStream  '(header, 0, 44, True)

    Dim myFormat As Integer = 6 '1 for PCM, 6 for a-law
    Dim myChannels As Integer = 1
    Dim myBitsPerSample As Integer = 8
    Dim mySampleRate As Integer = 8000
    Dim myByteRate As Integer = mySampleRate * myChannels * myBitsPerSample / 8
    Dim myBlockAlign As Integer = myChannels * myBitsPerSample / 8

    Dim SampleDataSize As Integer = myByteRate * SampleSeconds

    ' CHUNK DESCRIPTOR
    ms.Write(enc.GetBytes("RIFF"), 0, 4) ' 0: RIFF
    ms.Write(System.BitConverter.GetBytes(36 + SampleDataSize), 0, 4) ' 4: entire filesize - 8
    ms.Write(enc.GetBytes("WAVE"), 0, 4) ' 8: WAVE

    ' FORMAT SUBCHUNK
    ms.Write(enc.GetBytes("fmt "), 0, 4) ' 12: fmt
    ms.Write(System.BitConverter.GetBytes(16), 0, 4) ' 16: 16 for subchunk size
    ms.Write(System.BitConverter.GetBytes(myFormat), 0, 2) ' 18: 1 for PCM
    ms.Write(System.BitConverter.GetBytes(myChannels), 0, 2) ' 20: 1 for mono, 2 for stereo
    ms.Write(System.BitConverter.GetBytes(mySampleRate), 0, 4) ' 24: 44100 for CD quality
    ms.Write(System.BitConverter.GetBytes(myByteRate), 0, 4) ' 28: 
    ms.Write(System.BitConverter.GetBytes(myBlockAlign), 0, 2) ' 32:
    ms.Write(System.BitConverter.GetBytes(myBitsPerSample), 0, 2) ' 34:

    ' DATA SUBCHUNK
    ms.Write(enc.GetBytes("data"), 0, 4)
    ms.Write(System.BitConverter.GetBytes(SampleDataSize), 0, 4)
    MaximumBytesToWrite = SampleDataSize

    Return ms

  End Function

  'Private Function CreateWavHeaderStream() As System.IO.FileStream
  '  'http://www.thisisnotalabel.com/wavio/WavIO.vb
  '  'Dim header(43) As Byte
  '  Dim ms As New System.IO.FileStream("c:\rovio\rawwav.wav", FileMode.Create) '(header, 0, 44, True)

  '  Dim myDataSize As Integer = 20000 '(172 - 12) + 8
  '  Dim myFormat As Integer = 6 '1 for PCM, 6 for a-law
  '  Dim myChannels As Integer = 1
  '  Dim mySampleRate As Integer = 8000
  '  Dim myByteRate As Integer = 8000
  '  Dim myBlockAlign As Integer = 1
  '  Dim myBitsPerSample As Integer = 8

  '  ms.Write(enc.GetBytes("RIFF"), 0, 4) ' 0: RIFF
  '  'ms.Write(System.BitConverter.GetBytes(myDataSize + 36), 0, 4) ' 4: entire filesize - 8
  '  ms.Write(System.BitConverter.GetBytes(myDataSize + 36), 0, 4) ' 4: entire filesize - 8
  '  ms.Write(enc.GetBytes("WAVE"), 0, 4) ' 8: WAVE
  '  ms.Write(enc.GetBytes("fmt "), 0, 4) ' 12: fmt
  '  ms.Write(System.BitConverter.GetBytes(16), 0, 4) ' 16: 16 for PCM
  '  ms.Write(System.BitConverter.GetBytes(myFormat), 0, 2) ' 18: 1 for PCM
  '  ms.Write(System.BitConverter.GetBytes(myChannels), 0, 2) ' 20: 1 for mono, 2 for stereo
  '  ms.Write(System.BitConverter.GetBytes(mySampleRate), 0, 4) ' 24: 44100 for CD quality
  '  ms.Write(System.BitConverter.GetBytes(myByteRate), 0, 4) ' 28: 
  '  ms.Write(System.BitConverter.GetBytes(myBlockAlign), 0, 2) ' 32:
  '  ms.Write(System.BitConverter.GetBytes(myBitsPerSample), 0, 2) ' 34:
  '  'ms.Write(enc.GetBytes("data"), 0, 4)
  '  'ms.Write(System.BitConverter.GetBytes(172 * 2), 0, 4)
  '  Return ms

  '  ' Create a file and write the byte data to a file.
  '  'Dim oFileStream As System.IO.FileStream
  '  'oFileStream = New System.IO.FileStream(myPath, System.IO.FileMode.Create)
  '  'oFileStream.Write(header, 0, header.Length)
  '  'oFileStream.Write(myData, 0, myData.Length)
  '  'oFileStream.Close()
  'End Function

  Public Sub StopAudioStream()

    '--------------------------------
    ' Output to wav stream and play

    'SR.RecognizeAsyncStop()
    'SR = Nothing
    'saf1 = Nothing
    'ap1.Stop()
    'ap1 = Nothing
    'ms1.Flush()
    'ms1.Close()
    'ms1 = Nothing
    Select Case CurrentAudioFunction
      Case 1
        fs1.Close()
        fs1 = Nothing

      Case 2
        'If Not SpeechRecognitionStarted And ms1.Length > 8000 Then
        '  Debug.Print(Now & " : Starting speech recognition")

        '  'saf1 = New System.Speech.AudioFormat.SpeechAudioFormatInfo(Speech.AudioFormat.EncodingFormat.ALaw, 8000, Speech.AudioFormat.AudioBitsPerSample.Eight, Speech.AudioFormat.AudioChannel.Mono, 8000, 1, Nothing)
        '  'saf1 = New System.Speech.AudioFormat.SpeechAudioFormatInfo(Speech.AudioFormat.EncodingFormat.ALaw, 8000, Speech.AudioFormat.AudioBitsPerSample.Eight, Speech.AudioFormat.AudioChannel.Mono, 8000, 1, Nothing)
        '  'SR.SetInputToAudioStream(ms1, saf1)

        '  SR.SetInputToWaveStream(ms1)
        '  SR.RecognizeAsync()
        '  SpeechRecognitionStarted = True
        'End If
        'SR.RecognizeAsyncStop()
        'SR = Nothing
        'saf1 = Nothing
        'ms1.Close()
        'ms1 = Nothing
        'SpeechRecognitionStarted = False

    End Select

    '--------------------------------

    T1.Stop()
    T1 = Nothing

    Dim Send04 As String = "TEARDOWN rtsp://" & RovioHttpAddress() & "/webcam RTSP/1.0" & vbCrLf & "CSeq : " & RequestCount & vbCrLf & _
                      "Session: " & CurrentAudioSession & vbCrLf & "User-Agent: fakeagent" & vbCrLf & vbCrLf
    RequestCount += 1
    Dim Response04 As String = ""
    If SendRTSPRequest(rovioAudioStreamSocket, Send04, Response04) Then
      'Debug.Print(Now & " : TEARDOWN complete ('" & Response04 & "')")
    Else
      'ignoring teardown response
      'Debug.Print(Now & " : TEARDOWN complete ('" & Response04 & "')")
    End If
    rovioAudioStreamSocket.Disconnect(False)
    CurrentAudioSession = ""
    rovioAudioStreamSocket = Nothing
    remoteAudioStreamEndPoint = Nothing
    Debug.Print(Now & " : Audio STREAM Input socket TERMINATED")

    RaiseEvent AudioFunctionComplete(CurrentAudioFunction)
    CurrentAudioFunction = 0


  End Sub

  Private Sub T1_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles T1.Elapsed

    Dim LoopedAlready As Integer = 0
    Do While (rovioAudioStreamSocket.Available > 172 And LoopedAlready < 50 And MaximumBytesToWrite > 0)

      ' Process 4 byte header
      Dim HeaderPrelude(4) As Byte
      ' ''If rovioAudioStreamSocket.Available < 4 Then
      ' ''  Debug.Print(Now & " : Nothing to read/header not available: Available = " & rovioAudioStreamSocket.Available)
      ' ''  Exit Sub
      ' ''End If
      Dim r As Integer = rovioAudioStreamSocket.Receive(HeaderPrelude, 4, SocketFlags.None)
      Dim StreamByteCount As Integer = HeaderPrelude(3)
      If HeaderPrelude(0) <> 36 Or HeaderPrelude(1) <> 0 Or HeaderPrelude(2) <> 0 Or StreamByteCount <= 12 Then
        Debug.Print(Now & " : Bad header, try again. {" & HeaderPrelude(0) & ", " & HeaderPrelude(1) & ", " & HeaderPrelude(2) & ", " & HeaderPrelude(3) & "}")
        Exit Sub
      End If

      ' Process remaining stream that the header governs (currently 172 byte packets it seems)
      ' 8 bit 8000hz A-Law encoded
      ' 12 bytes are ignored
      ' ''Do While rovioAudioStreamSocket.Available < StreamByteCount
      ' ''  Debug.Print(Now & " : Need to read " & StreamByteCount & " bytes but only available = " & rovioAudioStreamSocket.Available)
      ' ''  Application.DoEvents()
      ' ''Loop
      Dim DataToGet(256) As Byte
      r = rovioAudioStreamSocket.Receive(DataToGet, StreamByteCount, SocketFlags.None)
      'Debug.Print(Now & " : recieved (" & r & ") bytes of data")


      '---------------------------
      'Convert to PCM 16bit (temp ALAW 8 bit)
      'Dim ToConvertCount As Integer = StreamByteCount - 12 - 1
      'Dim DataToWrite(ToConvertCount) As Short
      'Dim i As Integer
      'For i = 0 To ToConvertCount
      '  DataToWrite(i) = 0
      'Next i


      '---------------------------
      ' What to do with data?
      'ms1.Write(enc.GetBytes("data"), 0, 4)
      'ms1.Write(System.BitConverter.GetBytes(172 - 12), 0, 4)
      'ms1.Write(DataToGet, 12, StreamByteCount - 12)

      'ms1.Write(DataToGet, 0, StreamByteCount)
      ''''ms1.Write(DataToGet, 12, StreamByteCount - 12)
      ''''Debug.Print(ms1.Length)
      'If Not AlreadyPlayingStream Then
      '  ap1.Play()
      '  AlreadyPlayingStream = True
      'End If

      Select Case CurrentAudioFunction
        Case 1
          Dim ByteCount As Integer = StreamByteCount - 12
          If ByteCount > MaximumBytesToWrite Then ByteCount = MaximumBytesToWrite
          fs1.Write(DataToGet, 12, ByteCount)
          MaximumBytesToWrite -= ByteCount
          If MaximumBytesToWrite <= 0 Then
            StopAudioStream()
            Exit Sub
          End If

        Case 2
          ' ''Dim ByteCount As Integer = StreamByteCount - 12
          ' ''If ByteCount > MaximumBytesToWrite Then ByteCount = MaximumBytesToWrite
          ' ''ms1.Write(DataToGet, 12, ByteCount)
          ' ''MaximumBytesToWrite -= ByteCount
          ' ''If MaximumBytesToWrite <= 0 Then
          ' ''  StopAudioStream()
          ' ''  Exit Sub
          ' ''End If

          'If Not SpeechRecognitionStarted And ms1.Length > 8000 Then
          '  Debug.Print(Now & " : Starting speech recognition")

          '  'saf1 = New System.Speech.AudioFormat.SpeechAudioFormatInfo(Speech.AudioFormat.EncodingFormat.ALaw, 8000, Speech.AudioFormat.AudioBitsPerSample.Eight, Speech.AudioFormat.AudioChannel.Mono, 8000, 1, Nothing)
          '  'saf1 = New System.Speech.AudioFormat.SpeechAudioFormatInfo(Speech.AudioFormat.EncodingFormat.ALaw, 8000, Speech.AudioFormat.AudioBitsPerSample.Eight, Speech.AudioFormat.AudioChannel.Mono, 8000, 1, Nothing)
          '  'SR.SetInputToAudioStream(ms1, saf1)

          '  SR.SetInputToWaveStream(ms1)
          '  SR.RecognizeAsync()
          '  SpeechRecognitionStarted = True
          'End If

      End Select

      '---------------------------
      LoopedAlready += 1
    Loop
  End Sub

#End Region

End Class

