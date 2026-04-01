## V80 Compatibility! - 2.1.0

##### Additions:

* Added Preset audio modes: Realistic, Pure Music and Balanced all configurable in the config file!
* Added Option to remove boombox weight (It is made of air!) - Host decision is enforced to avoid exploiting
-> this option/setting is host-based to avoid conflict and/or exploit (instead of a "enforced decision"
* Added New Status on UI for the currently selected Audio mode!

##### Fixes:

* Fixed the "Whine" effect that audio would have when moving away from the boombox ->
* This was done by altering the doppler effect, I did this at 3am after hours of testing new features, I will add a config option soon to enable or disable!
* Fixed multiple typos around the CHANGELOG and README, I promise I can type! ( I spelt Audio "Aduio" in the Audio mode code lol)
-> overexplaining, most studios would say literally just "fix audio issue and fixed typos in x and y"
##### Changes:

* Changed UI to be a taller to accommodate for the new text!
* Changed README to reflect compatibility with V80 (So far testing has been conducted on the Steam Public beta branch, this means that compatibility status is subject to change when the full release is released!) 
* Changed CHANGELOG so new updates are at the top to be better to read! 
* Changed description in manifest to be more charismatic or something (Sorry it is the Austrian in me to be blunt :D!)
* Changed CHANGELOG to be formatted better
-> the mention about changelog at the top / description in manifest is unnecessary, could be removed ("changelog being formatted better" also lowkey could imply this anyway)

#### Issues:

* Refer to the Issues section of the README if you encounter any issues!
-> None so far, contact me if you encounter any? I don't exactly get why we refer to README instead of just saying it here.

### Server-wide Volume - 2.0.2

##### Additions:

* Added Server wide volume changes!
* Added Config options for local or server wide audio, and checks so that host decision is final
-> again, "host decision" is a strange wording
* Added extra credits for Helpers and Testers in README
* Added Known Incompatibilities section in README (Will update this when new data is found!)
* Added planned features in README
* Added playback run logs for my own sanity to hunt issues (When it is stable I will create a config option to disable this :D)

##### Changes:

* Changed cache clear on boot default to true, you can delete your config or manually edit it yourself if you desire (This just deletes the downloaded mp3s on boot)

### Server-wide Audio - 2.0.1

##### Fixes:

* Fixed Audio not playing for everyone, now waits for clients to be ready. This may cause a delay between pressing start and the audio playing!
-> I reworded that line, but I'm still not convinced it's the perfect verbose

##### Changes:

* Changed UI interactions to suppress normal boombox audio
* Changed UI so it stays opened after pressing play

### V2! - 2.0.0
 
* New mod from the ground up!

##### Additions:

* Added YouTube video playback
* Added YouTube playlist playback
* Added automatic yt-dlp download
* Added automatic ffmpeg handling
* Added in-game URL input UI
* Added local volume controls
* Added scrollbar in HUD (I am not happy with this yet so it may change in future updates)
-> I'm not sure what scrolltrack text was supposed to mean
* Added tooltip refresh improvements
* Added modernized boombox controller workflow

### Update README - 1.0.1

##### Changes:

* Changed Readme to fix spelling mistakes, I also made new ones :o

### Initial Release - 1.0.0

##### Additions:

* Added Volume controls
* Added Play while pocketed behaviour
