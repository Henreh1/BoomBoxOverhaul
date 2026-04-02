## V80 Compatibility! - 2.1.0

##### Additions:

* Added Preset audio modes: Realistic, Pure Music and Balanced all configurable in the config file!
* Added config option to remove boombox weight (It is made of air!) Host decision is enforced to avoid exploting
* Added New Status on UI for the currently selected Audio mode!
* Added Regex cleaning on video titles so the displayed track should be less cluttered

##### Fixes:

* Fixed debug logs
* Fixed log spam that was casued by actually fixing the logs XD
* Fixed the "Whine" effect that audio would have when moving away from the boombox ->
* This was done by altering the doppler effect, I did this at 3am after hours of testing new features, I will add a config option soon to enable or disable!
* Fixed multiple typos around the CHANGELOG and README, I promise I can type! ( I spelt Audio "Aduio" in the Audio mode code lol)

##### Changes:

* Changed broadcasting synchronised server settings to only trigger when client list changes
* Changeed application of the prior mentioned settings to only trigger if values have changed
* Changed UI to be a "smidge" taller to accomodate for the new text!
* Changed README to reflect compatibility with V80 (So far testing has been conducted on the Steam Public beta branch,this means that comptibility status is subject to change when the full release is released!) 
* Changed CHANGELOG so new updates are at the top to be better to read! 
* Changed description in manifest to be more charismatic or something (Sorry it is the Austrian in me to be blunt :D!)
* Changed CHANGELOG to be formatted better

#### Issues:

* Refer to the Issues section of the README if you encounter any issues!

### Server-wide Volume - 2.0.2

##### Additons:

* Added Server wide volume changes!
* Add Config options for local or serverwide audio and checks so that host decision is final
* Added extra credits for Helpers and testers in README
* Added Known Incompatibilities section in README (Will update this when new data is found!)
* Added planned features in README
* Added playback run logs for my own sanity to hunt issues (When it is stable I will create a config option to disable this :D)

##### Changes:

* Changed cache clear on boot default to true, delete your config or manually edit it yourself (This just deletes the downloaded mp3s on boot!)

### Server-wide Audio - 2.0.1

##### Fixes:

* Fixed Audio not playing for everyone now waits for clients to be ready, this may cause a delay from pressing start and audio playing!

##### Changes:

* Changed UI interactions to suppress normal boombox audio
* Changed UI to stay open after pressing play

### V2! - 2.0.0
 
* New mod from the ground up!

##### Additions:

* Added YouTube video playback
* Added YouTube playlist playback
* Added automatic yt-dlp download
* Added automatic ffmpeg handling
* Added in-game URL input UI
* Added local volume controls
* Added scrolling track text in HUD (I am not happy with this yet so it may change in future updates)
* Added tooltip refresh improvements
* Added modernized boombox controller workflow

### Update README - 1.0.1

##### Changes:

* Changed Readme to fix spelling mistakes, I also made new ones :o

### Initial Release - 1.0.0

##### Additions:

* Added Volume controls
* Added Play while pocketed behaviour