# GShade Preset Installer
**This application is a simple 64-bit C# .NET Framework 4.7.2 installer to aid GShade preset creators with distribution and end-user installation.**

Visual Studio 2019 or newer is required for building, and you will want to start by replacing the default icons in the `Resources` folder.

Afterwards, be sure to update the assembly information in `AssemblyInfo.cs`, as the `Company` field is used as the target installed folder name under `gshade-presets`.

Lastly, replace the existing `PresetPayload.zip` in `Resources` with your own. Please note the following with regard to compression support:
 * `zip` is the only supported format due to native .NET Framework limitations.
 * `Compression level` from 0 to 9 is supported.
 * The only supported `Compression method` is `Deflate`.
 * The valid range for `Word size` is `8` to `256`.
 * It is recommended that you use [7zip](https://www.7-zip.org/) to create your zip archive, as it offers a very straightforward GUI when adjusting these settings.

The contents of the zip archive may be structured in any way you like, but ensure that any additional textures are stored loose in the root of the file. For example:
 * `\My Studio Presets\MyStudioPreset1.ini`
 * `\My Studio Presets\MyStudioPreset2.ini`
 * `\My Outdoor Presets\MyOutdoorPreset1.ini`
 * `\OutdoorSnow.png`
 * `\StudioGlare.jpg`

Continuing with the above zip archive structure example and remembering that the `Company` value in `AssemblyInfo.cs` determines the `MyBrandName` folder, the final destination(s) post-install will look as below:
 * `?:\Path\To\Game\gshade-presets\MyBrandName\My Studio Presets\MyStudioPreset1.ini`
 * `?:\Path\To\Game\gshade-presets\MyBrandName\My Studio Presets\MyStudioPreset2.ini`
 * `?:\Path\To\Game\gshade-presets\MyBrandName\My Outdoor Presets\MyOutdoorPreset1.ini`
 * `?:\Path\To\Second\Game\gshade-presets\MyBrandName\My Studio Presets\MyStudioPreset1.ini`
 * `?:\Path\To\Second\Game\gshade-presets\MyBrandName\My Studio Presets\MyStudioPreset2.ini`
 * `?:\Path\To\Second\Game\gshade-presets\MyBrandName\My Outdoor Presets\MyOutdoorPreset1.ini`
 * `?:\Path\To\Etc\Game\gshade-presets\MyBrandName\My Studio Presets\MyStudioPreset1.ini`
 * `?:\Path\To\Etc\Game\gshade-presets\MyBrandName\My Studio Presets\MyStudioPreset2.ini`
 * `?:\Path\To\Etc\Game\gshade-presets\MyBrandName\My Outdoor Presets\MyOutdoorPreset1.ini`
