# Unity-Android-Files-Opener
 Unity Android Files Opener allows your android application to open files on local drive with (including API Level 24 or higher).

## Requirements
 Unity Editor 5 or newer

## Installation
 Add Assets files to your Assets in Unity project.   

## Usage
The following example demonstrates how to open a file:
```csharp
public void Example()
{
    string documentUrl = "/storage/emulated/0/Test Folder/template.pdf";
    AndroidOpenUrl.OpenUrl(documentUrl);
}
```
## Known errors:
 * Plugins folder not found:
   The Editor Script `PackageNameChanger.cs` cannot find the Plugins folder in the root of the Assets folder.
   This error message can occur as a result of moving the Plugins folder to some other place that is different from the root of the Assets folder.
   To solve the problem, just re-import the asset.
 * File release.aar not found:
   The Editor Script `PackageNameChanger.cs`cannot find the release.aar archive in the Plugins folder. This error may occur if you deleted, moved or renamed the release.aar file.
   To solve the problem, just re-import the asset.

## Notes
 * The example project was built using Unity 2019.2.11f1
 * A minimum API level of 16 and target API level of 29 was used during testing.
 * If you use the API level below 4.4.4 KitKat and you need External access to files, then you need to set `Write Permission` to `Extarnal(SDCard)` in `Player Settings` of your project.
 * If your project needs to make any changes to your `AndroidManifest.xml`, then consider that the Editor Script `PackageNameChanger.cs` monitors the changes in the Package Name of your project and makes these changes in Plugins/realese.aar/AndroidManifest.xml and Plugins/realese.aar/res/xml/filepaths.xml files

## License
This project is licensed under the terms of the [MIT License](https://opensource.org/licenses/MIT).
