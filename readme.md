# Digital Scoreboard

A football scoreboard that can be controlled through Bluetooth.  Great for little league games.

## Features
* Referee "client" screen that allows you to control the scoreboard remotely
* Customized Team Names
* Customized Game & Play Clock Timers
* Full Screen Clock for Half Time or Play Clock
* Screen Properties
    * Current quarter/period
    * Current Down
    * Play Clock
    * Game Clock
    * Timeouts Remaining
    * Ball Possession
    * Yards-To-Go

## The "Stack"

* .NET MAUI
* Prism
* ReactiveUI
* Shiny (BluetoothLE & BluetoothLE Hosting)
* Shiny.Framework (brings together Shiny, Prism, & ReactiveUI in a tidy little bundle)

## Scoreboard
This screen is meant for tablets in landscape view.  It sets up a bluetooth LE host using Shiny.BluetoothLE.Hosting that can allow connections to control the scoreboard clocks and scores

You can also control most settings, pause/resume of clocks, etc by tapping on the screen.  This is a good option if you don't have a tablet and a phone to remote control with or if bluetooth distance is a problem.

<img src="scoreboard.png" />

## Settings

The settings screen allows you to set all of the scoreboard constraints and others things like team names

<img src="settings.png" />

## TODO
* Auto font size for full screen/scoreboard
* Stop period timer when leaving scoreboard if hosted or self
* Referee Card
    * 1 press button to start play clock, 2nd press to start period clock, 3rd press to reset/stop
* Move possession indicator to be a mini football beside team name or score
* Load/Save Rulesets in settings
* Online Error Log - AppCenter, Firebase, Sentry, etc
* Setting to auto remember "scoreboard"?
* Client can tell remote scoreboard to go to fullscreen play clock or timer