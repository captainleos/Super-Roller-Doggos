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

"IF YOU HAVE ANY PROBLEM WITH gdiplus or Lerc64"
----------------------
. Go to /home folder

. Find Unity config file from here:
/Unity/Hub/Editor/"YOUR_UNITY_VERSION"/Editor/Data/MonoBleedingEdge/etc/mono/config

. Open the config file in a text editor and search for the word "gdiplus"

You may find some lines like this:
    <dllmap dll="gdiplus" target="libgdiplus.so.0" os="!windows"/>
    <dllmap dll="gdiplus.dll" target="libgdiplus.so.0"  os="!windows"/>
    <dllmap dll="gdi32" target="libgdiplus.so.0" os="!windows"/>
    <dllmap dll="gdi32.dll" target="libgdiplus.so.0" os="!windows"/>


. Remove any lines with "gdiplus" in it.

. Restart Unity.

