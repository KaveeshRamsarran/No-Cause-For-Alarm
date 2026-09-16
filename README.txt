NO CAUSE FOR ALARM

First-person psychological horror in a locked college wing.
Talk, investigate, test and decide who boards the 18:00 evacuation bus.

WINDOWS PLAY
Launch NO CAUSE FOR ALARM.exe. Keep all adjacent folders and DLLs together.
In the source repository the executable is under Builds/Windows after building.
Start BEGIN A NEW DAY for the classroom introduction.

CONTROLS
WASD: move. Mouse: look. E: interact.
Shift: sprint. Ctrl: crouch.
F: ignite/extinguish. R: raise/lower lighter.
Tab: notebook. M: campus directory. Esc: pause/close.

Four consequential actions advance the hour. Movement and brief questions are free.
The notebook has a WAIT UNTIL NEXT HOUR option. At 18:00, use the transport doors
at the north end of the main corridor to choose passengers and finish the day.

Settings include volumes, brightness, sensitivity, FOV, graphics, subtitles,
analog effects and camera movement. Reduce the last two to zero for less motion.

BUILD
Unity 6000.4.7f1 with Windows Build Support; URP 17.4.0.
Open Assets/Scenes/EastWing.unity. The bootstrap builds the campus at runtime.
Menu: NO CAUSE FOR ALARM > Configure and build Windows.
Command: powershell -ExecutionPolicy Bypass -File Tools/build.ps1

ASSETS AND CREDITS
Styloo / School Classrooms Asset Pack, Kenney / Furniture Kit (CC0).
Original code, story, procedural art and synthetic sound.
Original AI-generated face atlas, mapped onto an original Blender head mesh.
Full provenance: ASSET_CREDITS.txt. Development notes: DEVELOPMENT_LOG.md.

Save data: Windows LocalLow/Bellwether Games/NO CAUSE FOR ALARM.
Duration and subjective horror pacing require further human playtesting.
