# Digital Scoreboard

A football scoreboard that can be controlled through Bluetooth.  Great for little league games.

## Features
* Customizable settings
    * Team Names
    * Game & Play Clock Times
    * Number of Downs (little leagues often have extra downs)
    * Timeouts
    * Default Yards-To-Go (once it is 1st down again)
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
* Pretty Up
* Referee Card
    * 1 press button to start play clock, 2nd press to start period clock, 3rd press to reset/stop
    * Need to be able sync & tell what state each timer is in
* Load/Save Rulesets in settings
* Online Error Log - AppCenter, Firebase, Sentry, etc
* Client can tell remote scoreboard to go to fullscreen play clock or timer

## ISSUES
* Period time remaining is not sync'ing over
* iOS
    * Lock orientation on scoreboard
* Stop period timer when leaving scoreboard if hosted or self
* Time isn't perfectly in-sync