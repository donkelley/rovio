RovioWrap
By: Scott Settembre
Latest code: Feb 2, 2009

This is just my working code, some of it is not commented.
This is for your amusement, do with it what you will.

The startup form is currently 'RovioController'.
Buttons do actions, some code is commented out and you can reactivate it.  This code
shows how to interact with the RovioWrap object.

If you change the startup form to 'TrainedRovio', make sure a connection
can be established to your Rovio.  TrainedRovio will immediately make a connection
and then start listening for a command.  It will record 2 seconds of audio (while the
headlight is on) then immediately process it for a recognized phrase.

It can recognize the following phrases:
"Rovio", "Bark", "Hello", "Run forward", "Good dog"
"Move left", "Move right", "Go back", "Go backwards", "Move back"
"Go forward", "Move forward", "Slide left", "Slide right"
"Turn around", "Turn left", "Turn right", "Spin", "Spin around"
"Wander", "Speak", "Move backwards", "Move up"
"Good bye", "Exit", "Sit up", "Down boy", "Down girl"
"Attack of the show"

Have fun!

Check out the YouTube video when I put it up in the next few days.
