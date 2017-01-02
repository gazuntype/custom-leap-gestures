# custom-leap-gestures
Open source project to detect various hand gestures using Leap Motion's detection utilities.

## Motivation
The gestures that Leap Motion's detection utilities capture are limited without a lot of scripting coming to play. Hence, this repo contains scripts that can be important and used to detect slightly more complex gestures.

## How it works
These scripts extend Leap Motion's basic Detector scripts and can just be added as components to detect various gestures. Hence when a gesture is detected, the event Activate() is called.

## Prerequisites
To use these scripts, Leap Motion's SDKs must be installed and their core assets and detection utility modules, imported. These can be found [here](https://developer.leapmotion.com/get-started) and [here](https://developer.leapmotion.com/unity#100) respectively. 

## How to use
Simply import the unitypackage into your project and add any of the gestures you want as components to your Leap Motion hands. Fill in the necessary variables.

## Contribution
Comments and bugs should please be mentioned. Also, if you have any custom detection scripts, don't hesitate to commit it to the folder.
