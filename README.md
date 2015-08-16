# Rainmeter Ovh Plugin
A rainmeter plugin using OVH and Kimsufi API in order to get few servers/vps informations.

## Disclaimer
This plugin is still in eavy developpment and not stable.
In order to test it, you need to put Newtonsoft.Json, Ovh.RestLib and RestSharp DLLs inside your rainmeter installation folder.

After the first load, a web page linked to OVH or Kimsufi depending on the chosed provider should be opened in your default browser.
You should logged in with your account in order to make the generated credential valid.
Once you're logged in, you should reload the skin in order to make it collects data from the API.

## Available measures :
- RAMLoad : It will return the used memory in percent
- RAMSize : It will return the maximum available memory
- CPULoad : It will return the used cpu in percent
- ReverseName : It will return the server reverse name
- FirstIp : It will return the first IP linked to the server (basically the IPv4)
- SecondIp : It will return the second IP linked to the server (basically the IPv6)
- Download : It will return the last download byte value
- Upload : It will return the last upload byte value

## Available providers :
KimsufiEU : Kimsufi servers based in europe
KimsufiCA : Kimsufi servers based in canada
OvhEU : Ovh servers and VPS based in europe
OvhCA : Ovh servers and VPS based in canada

## Available service :
Dedicated : informations related to dedicated server
VPS : informations related to VPS server

## Skin sample
This skin is based on illustro default skin.

```
[Rainmeter]
; This section contains general settings that can be used to change how Rainmeter behaves.
Update=1000
Background=#@#Background.png
; #@# is equal to Rainmeter\Skins\illustro\@Resources
BackgroundMode=3
BackgroundMargins=0,34,0,14

[Metadata]
; Contains basic information of the skin.
Name=Server
Author=Elfaus
Information=Displays server RAM Load
License=Creative Commons BY-NC-SA 3.0
Version=1.0.0

[Variables]
; Variables declared here can be used later on between two # characters (e.g. #MyVariable#).
fontName=Trebuchet MS
textSize=8
colorBar=24,207,252,255
colorText=255,255,255,205
ApplicationKey=tNHdSh5YGvbu2dRV
ApplicationSecret=mGT9YDYi17nsLGvQOAq5EcZceDhbZhZT
ServerName=somename
RefreshTime=20

; ----------------------------------
; MEASURES return some kind of value
; ----------------------------------

[ServerRamMeasure]
Measure=PLUGIN
Plugin=OVH.dll
Type=Number
ApplicationKey=#ApplicationKey#
ApplicationSecret=#ApplicationSecret#
ServerName=#ServerName#
Provider=KimsufiEU
MeasureSource=RAMLoad
Service=Dedicated
UpdateDivider=#RefreshTime#

[ServerCpuMeasure]
Measure=PLUGIN
Plugin=OVH.dll
Type=Number
ApplicationKey=#ApplicationKey#
ApplicationSecret=#ApplicationSecret#
ServerName=#ServerName#
Provider=KimsufiEU
MeasureSource=CPULoad
Service=Dedicated
UpdateDivider=#RefreshTime#

[ServerRamSizeMeasure]
Measure=PLUGIN
Plugin=OVH.dll
Type=Number
ApplicationKey=#ApplicationKey#
ApplicationSecret=#ApplicationSecret#
ServerName=#ServerName#
Provider=KimsufiEU
MeasureSource=RAMSize
Service=Dedicated
UpdateDivider=#RefreshTime#

[ServerDownloadMeasure]
Measure=PLUGIN
Plugin=OVH.dll
Type=Number
ApplicationKey=#ApplicationKey#
ApplicationSecret=#ApplicationSecret#
ServerName=#ServerName#
Provider=KimsufiEU
MeasureSource=Download
Service=Dedicated
UpdateDivider=#RefreshTime#

[ServerUploadMeasure]
Measure=PLUGIN
Plugin=OVH.dll
Type=Number
ApplicationKey=#ApplicationKey#
ApplicationSecret=#ApplicationSecret#
ServerName=#ServerName#
Provider=KimsufiEU
MeasureSource=Upload
Service=Dedicated
UpdateDivider=#RefreshTime#

[ServerReverseName]
Measure=PLUGIN
Plugin=OVH.dll
;Type=String
ApplicationKey=#ApplicationKey#
ApplicationSecret=#ApplicationSecret#
ServerName=#ServerName#
Provider=KimsufiEU
MeasureSource=ReverseName
Service=Dedicated

[ServerFirstIp]
Measure=PLUGIN
Plugin=OVH.dll
;Type=String
ApplicationKey=#ApplicationKey#
ApplicationSecret=#ApplicationSecret#
ServerName=#ServerName#
Provider=KimsufiEU
MeasureSource=FirstIp
Service=Dedicated

[ServerSecondIp]
Measure=PLUGIN
Plugin=OVH.dll
;Type=String
ApplicationKey=#ApplicationKey#
ApplicationSecret=#ApplicationSecret#
ServerName=#ServerName#
Provider=KimsufiEU
MeasureSource=SecondIp
Service=Dedicated

; ----------------------------------
; STYLES are used to "centralize" options
; ----------------------------------

[styleTitle]
StringAlign=Center
StringCase=Upper
StringStyle=Bold
StringEffect=Shadow
FontEffectColor=0,0,0,50
FontColor=#colorText#
FontFace=#fontName#
FontSize=10
AntiAlias=1
ClipString=1

[styleSectionTitle]
StringAlign=Left
; Meters using styleLeftText will be left-aligned.
StringCase=None
StringStyle=Bold
StringEffect=Shadow
FontEffectColor=0,0,0,20
FontColor=#colorText#
FontFace=#fontName#
FontSize=#textSize#
;SolidColor=255,255,255,255
AntiAlias=1
ClipString=1

[styleLeftText]
StringAlign=Left
; Meters using styleLeftText will be left-aligned.
StringCase=None
StringStyle=Bold
StringEffect=Shadow
FontEffectColor=0,0,0,20
FontColor=#colorText#
FontFace=#fontName#
FontSize=#textSize#
AntiAlias=1
ClipString=1

[styleRightText]
StringAlign=Right
StringCase=None
StringStyle=Bold
StringEffect=Shadow
FontEffectColor=0,0,0,20
FontColor=#colorText#
FontFace=#fontName#
FontSize=#textSize#
AntiAlias=1
ClipString=1

[styleBar]
BarColor=#colorBar#
BarOrientation=HORIZONTAL
SolidColor=255,255,255,15

; ----------------------------------
; METERS display images, text, bars, etc.
; ----------------------------------

[meterServerTitle]
Meter=String
MeterStyle=styleTitle
X=100
Y=12
W=188
H=18
Text=Kuro

; ---------------------------------------------
; Basic informations
; ---------------------------------------------

[meterServerBasicTitle]
Meter=String
MeterStyle=styleSectionTitle
X=10
Y=40
W=188
H=14
Text=Basic informations

[meterServerBasicSeparator]
Meter=String
MeterStyle=styleSectionTitle
X=10
Y=3r
W=188
H=14
Text=_____________________________

; ---------------------------------------------
; Server CPU Load
; ---------------------------------------------

[meterServerLabelCpu]
Meter=String
MeterStyle=styleLeftText
X=10
Y=17r
W=188
H=14
Text=CPU Usage

[meterServerValueCpu]
Meter=String
MeterStyle=styleRightText
MeasureName=ServerCpuMeasure
X=200
Y=0r
W=188
H=14
Text=%1%
Percentual=1
AutoScale=1

[meterServerBarCpu]
Meter=Bar
MeterStyle=styleBar
MeasureName=ServerCpuMeasure
X=10
Y=15r
W=188
H=1

; ---------------------------------------------
; Server RAM Load
; ---------------------------------------------

[meterServerLabelRam]
Meter=String
MeterStyle=styleLeftText
X=10
Y=5r
W=188
H=14
Text=RAM Usage

[meterServerValueRam]
Meter=String
MeterStyle=styleRightText
MeasureName=ServerRamMeasure
X=200
Y=0r
W=188
H=14
Text=%1%
Percentual=1
AutoScale=1

[meterServerBarRam]
Meter=Bar
MeterStyle=styleBar
MeasureName=ServerRamMeasure
X=10
Y=15r
W=188
H=1

; ---------------------------------------------
; Informations section
; ---------------------------------------------

[meterServerDetailsTitle]
Meter=String
MeterStyle=styleSectionTitle
X=10
Y=15r
W=188
H=14
Text=Detailed informations

[meterServerDetailsSeparator]
Meter=String
MeterStyle=styleSectionTitle
X=10
Y=3r
W=188
H=14
Text=_____________________________

; ---------------------------------------------
; Reverse informations
; ---------------------------------------------

[meterServerDetailsReverseLabel]
Meter=String
MeterStyle=styleLeftText
X=10
Y=20r
W=188
H=14
Text=Reverse

[meterServerDetailsReverseValue]
Meter=String
MeterStyle=styleRightText
MeasureName=ServerReverseName
X=200
Y=0r
W=188
H=14
Text=%1

; ---------------------------------------------
; Memory informations
; ---------------------------------------------

[meterServerDetailsRamLabel]
Meter=String
MeterStyle=styleLeftText
X=10
Y=15r
W=188
H=14
Text=Memory

[meterServerDetailsRamValue]
Meter=String
MeterStyle=styleRightText
MeasureName=ServerRamSizeMeasure
X=200
Y=0r
W=188
H=14
Text=%1 MB

; ---------------------------------------------
; IPs informations
; ---------------------------------------------

[meterServerDetailsFirstIpLabel]
Meter=String
MeterStyle=styleLeftText
X=10
Y=15r
W=188
H=14
Text=IP1

[meterServerDetailsFirstIpValue]
Meter=String
MeterStyle=styleRightText
MeasureName=ServerFirstIp
X=200
Y=0r
W=188
H=14
Text=%1

[meterServerDetailsSecondIpLabel]
Meter=String
MeterStyle=styleLeftText
X=10
Y=15r
W=188
H=14
Text=IP2

[meterServerDetailsSecondIpValue]
Meter=String
MeterStyle=styleRightText
MeasureName=ServerSecondIp
X=200
Y=0r
W=188
H=14
Text=%1

; ---------------------------------------------
; Network history
; ---------------------------------------------

[meterServerNetworkTitle]
Meter=String
MeterStyle=styleSectionTitle
X=10
Y=25r
W=188
H=14
Text=Network chart

[meterServerNetworkSeparator]
Meter=String
MeterStyle=styleSectionTitle
X=10
Y=3r
W=188
H=14
Text=_____________________________


[meterServerLineNetwork]
Meter=Line
MeasureName=ServerUploadMeasure
MeasureName2=ServerDownloadMeasure
X=10
Y=20r
W=188
H=40
LineColor=#colorBar#
LineColor2=255,255,255,255
LineCount=2
SolidColor=0,0,0,100
AntiAlias=1
AutoScale=1
UpdateDivider=#RefreshTime#

[meterServerDownloadLabel]
Meter=String
MeterStyle=styleLeftText
X=10
Y=45r
W=100
H=14
Text=___/| Download

[meterServerUploadLabel]
Meter=String
MeterStyle=styleLeftText
X=111
Y=0r
W=78
H=14
Text=___/| Upload
FontColor=#colorBar#
```

## Dependencies
- Newtonsoft.Json
- Ovh.RestLib (thanks to @acesyde for its library : https://github.com/acesyde/Ovh.RestLib)
- RestSharp
