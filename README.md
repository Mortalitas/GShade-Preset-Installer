# GShade Preset Installer
**This application is a simple 64-bit C# installer to aid GShade preset creators with distribution and end-user installation.**

There are no special requirements or dependencies for building, but you will want to start by replacing the default icons in the `Resources` folder.

Afterwards, be sure to update the assembly information in `AssemblyInfo.cs`, as the `Company` field is used as the target installed folder name under `gshade-presets`.

Lastly, replace the existing `PresetPayload.zip` in `Resources` with your own. You can use any structure you like, but ensure that any additional textures are in the root of your new zip.

The included `PresetPayload.zip` is packaged with examples.
