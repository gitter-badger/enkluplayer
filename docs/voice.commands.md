### Overview

Enklu has several persistent voice commands available for use throughout the runtime. 
Each command is currently hidden behind the word "debug". Commands marked as Admin require the additional word "admin" before the command.
For example `debug admin menu` brings up the experience menu.

| Command     | State | Admin  | Description                                                  |
| ----------- | ----- | ------ | ----------------------------------------------------------- |
| Menu        | Play  | Yes    | Brings up the experience menu, allowing access to other experiences or edit mode. |
| Edit        | Play  | Yes    | Brings up a prompt to enter edit mode. |
| Origin      | Play  | No     | For unanchored experiences, recenters the experience around the user's current view. |
| Reset       | Play  | No     | Closes the application. |
| Update      | Play  | No     | Attempts to update the current experience against Trellis. |
| Performance | Play  | No     | Brings up the performance dialog for testing. |
| Logging     | Play  | No     | Brings up the logging dialog for testing. |
| Trace       | Play  | No     | Enables the trace commands to work. |
| Start       | Play  | No     | [Trace] Starts a trace. |
| Stop        | Play  | No     | [Trace] Stops a trace. |
| Abort       | Play  | No     | [Trace] Aborts a trace. |
| Play        | Edit  | No     | Starts the experience in play mode. |
| New         | Edit  | No     | Opens the main menu in edit mode. |
| Back        | Edit  | No     | Closes the current menu and reopens the main menu in edit mode. |
| Crash       | Any   | Yes    | Emits a fatal log for testing. |
