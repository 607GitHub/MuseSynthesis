# MuseSynthesis

## 1 Description
MuseSynthesis is a tool to create MuseScore scores that play back music based on rapidly played drums for frequency synthesis. It is very experimental and mostly developed to see what can be done with this concept, but it has progressed beyond a proof-of-concept.  
MuseSynthesis was made for and tested with MuseScore 3.6.2.

- [MuseSynthesis](#musesynthesis)
  * [1 Description](#1-description)
  * [2 How to install MuseSynthesis](#2-how-to-install-musesynthesis)
  * [3 How to contribute](#3-how-to-contribute)
  * [4 How to use MuseSynthesis](#4-how-to-use-musesynthesis)
  * [5 MuseSynthesis input format](#5-musesynthesis-input-format)
    + [5.1 Child elements of `<musesynthesis>`](#51-child-elements-of-musesynthesis)
      - [5.1.1 `<voices>`](#511-voices)
      - [5.1.2 `<score>`](#512-score)
      - [5.1.3 `<metatag>`](#513-metatag)
      - [5.1.4 `<preference>`](#514-preference)
        * [5.1.3.1 displaytempos](#5131-displaytempos)
    + [5.2 Child elements of `<score>`](#52-child-elements-of-score)
      - [5.2.1 `<tempo>`](#521-tempo)
      - [5.2.2 `<a4tuning>`](#522-a4tuning)
      - [5.2.3 `<leaddiv>`](#523-leaddiv)
      - [5.2.4 `<drum>`](#524-drum)
      - [5.2.5 `<velocity>`](#525-velocity)
      - [5.2.6 `<leadnote>`](#526-leadnote)
      - [5.2.7 `<rest>`](#527-rest)
    + [5.3 Child elements of `<leadnote>`](#53-child-elements-of-leadnote)
      - [5.3.1 `<note>`](#531-note)
      - [5.3.2 `<value>`](#532-value)
      - [5.3.3 `<harmony>`](#533-harmony)
        * [5.3.3.1 `<div>`](#5331-div)
        * [5.3.3.2 `<harmdiv>`](#5332-harmdiv)
      - [5.3.4 `<effects>`](#534-effects)
        * [5.3.4.1 `<portamento>`](#5341-portamento)
          + [5.3.4.1.1 `<goalnote>`](#53411-goalnote)
  * [6 Additional references](#6-additional-references)
    + [6.1 Useful drum sounds](#61-useful-drum-sounds)
    + [6.2 Velocities](#62-velocities)
    + [6.3 Intervals](#63-intervals)
  * [7 Known issues](#7-known-issues)
    + [7.1 Other pitch perceived than played](#71-other-pitch-perceived-than-played)
    + [7.2 Portamento timing issue](#72-portamento-timing-issue)

<small><i><a href='http://ecotrust-canada.github.io/markdown-toc/'>Table of contents generated with markdown-toc with fix by myself</a></i></small>



## 2 How to install MuseSynthesis
To install the latest official release of MuseSynthesis, click on the tag with 'Latest' in the 'Releases' container on GitHub, and download and extract the zip.
If you want the very latest version, you can also clone the `master` branch with git and compile the program yourself. I try to keep the program on the `master` branch in working order. This is only recommended for advanced users that might like to contribute to the program.

## 3 How to contribute
If you want to contribute to the development of MuseSynthesis, feel free to make pull requests or create issues. This is one of several hobby projects of mine, though, and I cannot guarantee that I will get to your work.
You can also help out by simply using the program and sharing your thoughts, or by testing the MuseScore synthesizer in combination with the program and keeping track of findings. To get into contact with me, email me at 607?607web.nl.

## 4 How to use MuseSynthesis
MuseSynthesis is a console program. When run without additional arguments (e.g. by double-clicking it), MuseSynthesis will process a file called `example.xml` as input and output an uncompressed MuseScore file (\*.mscx) with the date and time of creation as filename. This output file can be opened in MuseScore for playback, rendering, et cetera, and can also be saved as a compressed MuseScore file (\*.mscz).  
MuseSynthesis can be run with the following optional arguments: `musesynthesis [input path/filename] [output path/filename]`. You cannot specify only output path/filename. If no absolute/full path is specified for the filename, the input path (the location of the program if unspecified) will be used as the root.

## 5 MuseSynthesis input format
MuseSynthesis processes user input into a MuseScore score. The input must be in XML format. There are a few elements that must be in every MuseSynthesis input XML. The minimal input file that will not produce an error is as follows.
```
<?xml version="1.0" encoding="UTF-8"?>
<musesynthesis>
	<voices>1</voices>
	<score></score>
</musesynthesis>
```
The included file `example.xml` should provide some idea about how the format is structured. Detailed info can be found below.

### 5.1 Child elements of `<musesynthesis>`
The `<musesynthesis>` element contains all required data about the score you want to generate.
#### 5.1.1 `<voices>`
Required. Contains the number of voices that your score will use as an integer. Example:
```
<voices>3</voices>
```
Sets up three parts to be used in the score.
#### 5.1.2 `<score>`
Required. Contains all data concerning the musical part of your score. See [5.2](#52-child-elements-of-score).
#### 5.1.3 `<metatag>`
Optional; requires `name` attribute. Used to set the metatags seen near the top of default.xml. Example:
```
<metatag name="composer">Yours truly</metatag>
```
Sets the composer metatag to "Yours truly". None of the metatags are currently shown visually in the score.
#### 5.1.4 `<preference>`
Optional; requires `name` attribute. Used to set preferences for MuseSynthesis to use. All preferences currently implemented are shown below. Example:
```
<preference name="displaytempos">true</preference>
```
Sets the displaytempos preference to true.
##### 5.1.3.1 displaytempos
Default: false. If set to true, shows tempo on every tuplet.

### 5.2 Child elements of `<score>`
The `<score>` element contains all data about your score that can be changed during playback (eg. pitch and tempo). All encountered child elements are processed in the order that they occur in. All possible child elements are optional; if none are provided an empty score is generated.
#### 5.2.1 `<tempo>`
Default: 120. Sets the effective tempo of the track, specified in quarter notes per minute. Example:
```
<tempo>143</tempo>
```
Sets the current effective tempo to 143 quarter notes per minute.
#### 5.2.2 `<a4tuning>`
Default: 440. Sets the tuning of A4, specified in Hertz. Example:
```
<a4tuning>435</a4tuning>
```
Sets the current tuning of A4 to 435Hz. All other notes update accordingly.
#### 5.2.3 `<leaddiv>`
Default: 4. Sets the tuplet division that will be used for notes by default. Example:
```
<leaddiv>3</leaddiv>
```
Sets the default division to triplets.
#### 5.2.4 `<drum>`
Default: 41; requires `voice` attribute. Sets the drum used by this voice. For a list of drums that have been used to some success, see [6.1](#61-useful-drum-sounds). Example:
```
<drum voice="1">43</drum>
```
Sets the drum used for the second voice (voices are 0-indexed) to 43, corresponding to High Floor Tom.
#### 5.2.5 `<velocity>`
Default: 80; requires `voice` attribute. Sets the velocity used by this voice. Numbers from 1 to 127 have an effect (outside that there is no change). For an overview of elements from MuseScore's Dynamics palette and corresponding numerical velocities, see 5.1. Example:
```
<velocity voice="0">112</voice>
```
Sets the velocity for the first voice to 112, corresponding to ff.
#### 5.2.6 `<leadnote>`
Used to make sounds. See [5.3](#53-child-elements-of-leadnote).
#### 5.2.7 `<rest>`
Used to introduce rests (no voices playing). Must include a `<value>` element that specifies the desired rest value. Example:
```
<leadrest>
	<value>3/4</value>
</leadrest>
```
Adds a halve rest and a quarter rest.

### 5.3 Child elements of `<leadnote>`
A `<leadnote>` element contains information about one or more notes that you want to play. Two child elements are required. A minimal working example:
```
<leadnote>
	<note>C4</note>
	<value>1/4</value>
</leadnote>
```
This results in a quarter note with the pitch of C4.
#### 5.3.1 `<note>`
Required. Contains the main pitch that should play, in scientific pitch notation. Write # for sharp, x for double sharp, b for flat and bb for double flat. Example:
```
<note>F#3</note>
```
Sets the pitch F#3 to play.
#### 5.3.2 `<value>`
Required. Contains the note value, as a fraction (or whole). Example:
```
<value>1/12</value>
```
Sets the note value to that of an eighth note triplet.
#### 5.3.3 `<harmony>`
A `<harmony>` element is used to let multiple notes play at once. It includes two child elements, `<div>` and `<harmdiv>`. For a reference of interval ratios, see [6.3](#63-intervals). A minimal working example:
```
<harmony>
	<div>4</div>
	<harmdiv voice="1">3</harmdiv>
</harmony>
```
This results in the second voice playing the pitch a perfect fourth below the first voice. Using too high numbers might result in no sound being produced. You can specify a `harmdiv` for every available voice. You should use different drums for each voice if you want to perceive the intended pitches. Even then there might be issues, see [7.1](#71-other-pitch-perceived-than-played). See [6.3](#63-intervals) for useful harmony ratios.
##### 5.3.3.1 `<div>`
Required. Sets the tuplet division of the lead note. Using too high numbers might result in no sound being produced. When no `<harmony>` element is included, the default of 4 will be used.
##### 5.3.3.2 `<harmdiv>`
Optional; requires `voice` attribute. Sets the tuplet division of the specified voice. This element can be repeated within a single `<harmony>` element to let several voices play at the same time.
#### 5.3.4 `<effects>`
Optional. Sets effects to apply to the note played. All effects currently implemented are shown below.
##### 5.3.4.1 `<portamento>`
Makes the pitch slide from one note to another at a constant speed. It includes a child element, `<goalnote>`. The portamento effect has a timing issue, see [7.2](#72-portamento-timing-issue). An elaborate example:
```
<leadnote>
	<note>G3</note>
	<value>1/4</value>
</leadnote>
<leadnote>
	<note>G3</note>
	<value>1/2</value>
	<effects>
		<portamento>
			<goalnote>D4</goalnote>
		</portamento>
	</effects>
</leadnote>
<leadnote>
	<note>D4</note>
	<value>1/4</value>
</leadnote>
```
Makes the pitch start at G3 and hold for one quarter note, then slide to D4 over two quarter notes, then hold at D4 for another quarter note.
###### 5.3.4.1.1 `<goalnote>`
Required. Sets the note to slide to. Example:
```
<goalnote>C5</goalnote>
```
Sets the portamento to slide to C5.

## 6 Additional references
In this section you can find some overviews that might aid in using MuseSynthesis.

### 6.1 Useful drum sounds
Most drum sounds from MuseScore have not been found to work well with the way MuseSynthesis works. The following do. If you are able to use more to useful effect, please let me know.

| drum nbr. |      name          |
| --------- | ------------------ |
| 35        | Acoustic Bass Drum |
| 41        | Low Floor Tom      |
| 43        | High Floor Tom     |
| 49        | Crash Cymbal 1     |

### 6.2 Velocities
Given below are the default correspondencies between elements from MuseScore's Dynamics palette and numerical velocities. Mind that sounds created in this way are generally louder than other MuseScore sounds.
| symbol | number |
| ------ | ------ |
| pppppp | 1      |
| ppppp  | 5 	  |
| pppp   | 10     |
| ppp    | 16     |
| pp     | 33	  |
| p      | 49     |
| mp     | 64     |
| mf     | 80     |
| f      | 96     |
| ff     | 112    |
| fff    | 126    |
| ffff   | 127    |
| fffff  | 127    |

### 6.3 Intervals
Given below are harmonic ratios that you might use and what intervals or chords they represent. Only ratios that have been found to work are included.
Ratios are given in their suggested form; they can be divided or multiplied with no theoretic effect; in practice, different divisions give a different sound, and some of the below might give a different result than expected, see [7.1](#71-other-pitch-perceived-than-played).
Give the lower number to the voice that you want to play the lower pitch (eg. to have the second voice play a perfect fifth below the lead, do `<div>3</div>` and `<harmdiv voice="1">2</div>`).

| ratio |     interval      |
| ----- | ----------------- |
| 4:4   | Perfect unison    |
| 5:6   | Minor third       |
| 4:5   | Major third       |
| 3:4   | Perfect fourth    |
| 2:3   | Perfect fifth     |
| 3:5   | Major sixth       |
| 2:4   | Perfect octave    |

| ratio  |        chord           |
| ------ | ---------------------- |
| 4:3:5  | major second inversion |
| 4:5:6  | major root position    |
| 8:5:6  | major first inversion  |

## 7 Known issues
In developing this program, there are some things I haven't yet been able to figure out. Feel free to look into them if you'd like to help out. If not, it could still be nice to know about them.

### 7.1 Other pitch perceived than played
Sometimes the pitch perceived as played by MuseScore seems different than the one which is supposedly played. Whether this happens or not, and how, depends on (at least) the pitch played and the tuplet division used. The following table shows my perceptions of the pitch D3 with different tuplet divisions. Try it out for youself.

| div | perceived pitch |
| --- | --------------- |
| 1   | D3              |
| 2   | D2 + D3         |
| 3   | Db3 + D3        |
| 4   | Db3             |
| 5   | D3              |
| 6   | D2 + D3 + F#3   |
| 7   | D3 + D#3        |
| 8   | Db3 + D3        |
| 9   | G1 + D3 + B3    |
| 10  | D3 + A3         |

Try it with some other pitches as well, such as A3 and D4. Any idea what's going on here? Let me know! :)

### 7.2 Portamento timing issue
When you use portamento, notes don't last as long as they otherwise would. This can mess up the timing of your track (i.e. a beat won't last as long as it should). If you can find a mathematical solution to this problem, please let me know, or submit a pull request.