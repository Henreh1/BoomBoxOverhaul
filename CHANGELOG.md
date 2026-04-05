# 2.1.0 - V80 Compatibility!

#### **Additions:**

* Added Preset audio modes: Realistic, Pure Music and Balanced all configurable in the config file! More info on this can be found [Here](wiki link when dylans lazy ass finishes it!)
* Added config option to remove boombox weight (It is made of air!) Host decision is enforced to avoid exploiting
* Added New Status on UI for the currently selected Audio mode!
* Added Regex cleaning on video titles so the displayed track should be less cluttered
* Added Debug log mode for most network related logs and gameplay run logs
* Added config option for debug logs, enable if you are having trouble with the mod (Disabled by default :D)
* Added clip rebuilding for noticeable gains in increased volume figures (100 - 200 should be more noticeable) More info on how this works can be found [Here](wiki link when I make it)
* Added distanced based volume to make the music distanced be based off of the current volume option
* Added modded checks so that a client cant use the "Advantageous" features if the host does not have the mod

#### **Fixes:**

* Fixed ```Audio source failed to initialize audio spatializer.``` Log spam
* Fixed ```PlayOneShot was called with a null AudioClip.``` Error when Equipping or unequipping the boombox
* Fixed logs not working as intended
* Fixed log spam that was caused by actually fixing the logs XD
* Fixed the "Whine" effect that audio would have when moving away from the boombox ->
* This was done by altering the doppler effect, I did this at 3am after hours of testing new features, If people miss this follow the issue/feature request guide in the README
* Fixed multiple typos around the CHANGELOG and README, I promise I can type!

#### **Changes:**

* Changed broadcasting synchronized server settings to only trigger when client list changes
* Changed application of the prior mentioned settings to only trigger if values have changed
* Changed UI to be a "smidge" taller to accommodate for the new text!
* Changed README to reflect compatibility with V80 (So far testing has been conducted on the Steam Public beta branch, this means that compatibility status is subject to change when the full release is released!) 
* Changed CHANGELOG so new updates are at the top to be better to read! 
* Changed description in manifest to be more charismatic or something (Sorry it is the Austrian in me to be blunt :D!)
* Changed Packaged README to be better (Thanks to my friends who helped with this!)
* Changed CHANGELOG to be formatted better

#### **Issues:**

* Refer to the Issues section of the README if you encounter any issues!

## 2.0.2 - Server-wide Volume!

#### **Additions:**

* Added Server wide volume changes!
* Add Config options for local or server wide audio and checks so that host decision is final
* Added extra credits for Helpers and testers in README
* Added Known Incompatibilities section in README (Will update this when new data is found!)
* Added planned features in README
* Added playback run logs for my own sanity to hunt issues (When it is stable I will create a config option to disable this :D)

#### **Changes:**

* Changed cache clear on boot default to true, delete your config or manually edit it yourself (This just deletes the downloaded mp3s on boot!)

## 2.0.1 - Server-wide Audio!

#### **Fixes:**

* Fixed Audio not playing for everyone now waits for clients to be ready, this may cause a delay from pressing start and audio playing!

#### **Changes:**

* Changed UI interactions to suppress normal boombox audio
* Changed UI to stay open after pressing play

## 2.0.0 - V2!
 
* New mod from the ground up!

#### **Additions:**

* Added YouTube video playback
* Added YouTube playlist playback
* Added automatic yt-dlp download
* Added automatic ffmpeg handling
* Added in-game URL input UI
* Added local volume controls
* Added scrolling track text in HUD (I am not happy with this yet so it may change in future updates)
* Added tooltip refresh improvements
* Added modernized boombox controller workflow

## 1.0.1 - Update README

#### **Changes:**

* Changed Readme to fix spelling mistakes, I also made new ones :o

## 1.0.0 - Initial Release!

#### **Additions:**

* Added Volume controls
* Added Play while pocketed behaviour