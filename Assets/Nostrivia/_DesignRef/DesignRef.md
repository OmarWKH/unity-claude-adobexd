# Overall description

See Overall.png for the full list of screens.

The question and answer screens are basically one gameplay screen.

# Layout requirements

the target ratio is same ratio as the XD prototype (360x640).

it should look good a wider/longer phone sizes (no need to go as far as tablets).
e.g. buttons that are in far corners or edges should stay there, otherwise can be centred or relative.

# Home

See Home.png

The main menu has two buttons:
- Settings: Opens the Settings pop up
- Play: Opens the gameplay screen

# Settings

See Settings.png (note that the pop up has a transparent background, not shown here).

Settings has audio sliders which control audio mixer groups.

Settings have notification toggles (no functionality).

Settings have a difficulty selector (no functionality).

Settings have a close button.

# Main Menu -> Settings

The settings pop up slides in from the bottom.

# Settings -> Main Menu

The settings pop up slides out towards the bottom.

# Gameplay (Question and Asnwer)

See Gameplay.png

Gameplay only has one hardcoded question, although the UI shows 1/10 but it's not functional.

The gameplay screen has a timer. Once the timer reaches 10 seconds, the timer background will blink between yellow and red.

The player selects an option, then clicks Submit (see Gameplay_Selection.png for how an option changes once selected and how the submit button changes once a selection is made).

The player cannot click Submit before selecting (see Gameplay.png).

Once you click submit, the Submit button switches to the result button, and you see a color change on the selected option (see Gameplay_Wrong.png and Gameplay_Correct.png).

If you click Results, the Results pop up appears (all its content is hardcoded, no win or lose diffrentiation).

If you click the exit button, you see the Confirmation pop up.

# Results (Game Results)

See Results.png

Results pop up is hardcoded (no win/lose).

Clicking Play Again sends you back to gameplay.

Clicking Back to Home sends you back to the main menu.

# Gameplay -> Results

The results pop up fades in from below.

# Results -> Gameplay

This is an interesting transition. It is probably an artifact of how Adobe XD interactive protoype works, but still it would be interesting to think of how this could be done.

A new gameplay window slides down from above, at the same time the old gameplay window and the results pop up slide down towards the bottom.

See ResultsToGameplay.mp4

# Results -> Main Menu

The main menu slides down from above, at the same time the old gameplay window and results pop up slide down towards the bottom (like Results -> Gameplay).

# Confirmation (Leave pop-up)

See Leave.png

This window allows the player to confirm if they want to leave gameplay or stay.

If you click Cancel, you resume gameplay.

If you click Leave, you transition to the main menu.

# Confirmation -> Gameplay

Confirmation pop up slides down towards the bottom.

# Confirmation -> Main Menu

Similar to Results -> Main Menu