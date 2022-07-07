Installation Steps

Please note that all of the following steps will be done automatically after package importing for Windows, Mac & Linux.
You can also force the auto installer to perform by going to menu "Tools => TerraUnity" and press "Reimport" so that
it tries to resolve missing steps.

Following is the steps needed for TerraWorld installation if you need to do it manually

After package importing, go to Player tab in project settings:
. Change the Scripting Runtime Version to .NET 4.x
. Change API Compatibility Level to .NET 4.x (Not needed in Unity 2019.x & later)
. Enable “Allow Unsafe Code”
. Change Color Space to Linear (Optional)
. In "Scripting Define Symbols" text field, add TERRAWORLD_PRO

Open up Package Manager from Unity's "Window" menu
. Install Post Processing from Package Manager

"IMPORTANT"
"IF YOU HAVE ANY PROBLEM WITH System.Drawing.GDIPlus or Lerc64"
----------------------
STEP 1 : Find the full path of "libgdiplus.dylib" file

.Check your mono.framework versions from here:
/Library/Frameworks/Mono.framework/Versions

.Check which version contains "libgdiplus.dylib" file in "lib" folder.

For example if you find the libgdiplus file under version "6.0.12" in "lib" folder, then the full path will be : 
/Library/Frameworks/Mono.framework/Versions/6.0.12/lib/libgdiplus.dylib

STEP 2 : Find Unity Config file

. Find Unity app file from here:
/Applications/Unity/Hub/Editor/"YOUR_UNITY_VERSION"/Unity.app

. Right-click on it and select "Show Package Contents"

. Continue to find Unity config file from here: 
/Contents/MonoBleedingEdge/etc/mono/config

STEP 3 : Map gdiplus.dll in confige file

.Open the config file in a text editor and search for the word "gdiplus".
If the word is found, replace the following 2 lines with the existing lines or if not found, add the following 2 lines after others.
And change "target" by yours in STEP 1 :

<dllmap dll="gdiplus" target="/Library/Frameworks/Mono.framework/Versions/YOUR_MONO_VERSION/lib/libgdiplus.dylib"/>
<dllmap dll="gdiplus.dll" target="/Library/Frameworks/Mono.framework/Versions/YOUR_MONO_VERSION/lib/libgdiplus.dylib"/>  

Example 
    <dllmap dll="gdiplus" target="/Library/Frameworks/Mono.framework/Versions/6.0.12/lib/libgdiplus.dylib" />
    <dllmap dll="gdiplus.dll" target="/Library/Frameworks/Mono.framework/Versions/6.0.12/lib/libgdiplus.dylib" />

STEP 4
.Also add the following line any where in the file and save:
<dllmap dll="Lerc64.dll" target="/Users/TerraWorld/Lerc64.dylib"/>

STEP 5
.Create a directory at "/Users/TerraWorld/" and copy-paste the provided "Lerc64.dylib" there.
You can find "Lerc64.dylib" in "plugin" folder of your project.


STEP 6
.Restart Unity.




