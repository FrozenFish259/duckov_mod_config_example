# ModConfigExample
This is an example project showing how to use [ModConfig](https://github.com/FrozenFish259/duckov_mod_config)  
![ScreenShot](./1.JPG)

---

## Index

1. [Installation](#installation)
2. [Project Configuration](#project-configuration-modifying-for-your-own-project)
3. [Build Configurations](#build-configurations)
4. [Usage](#usage)

---

## Installation
1. Clone project repository from GitHub
    ```shell
    git clone https://github.com/FrozenFish259/duckov_mod_config_example
    ```
2. Create `ModConfigExample.csproj.user` at project root with following content:
   > Tip: Also possible to set `DuckovPath` environment variable instead of creating `ModConfigExample.csproj.user`
    ```xml
    <Project>
        <PropertyGroup>
            <DuckovPath>D:\SteamLibrary\steamapps\common\Escape from Duckov</DuckovPath>
        </PropertyGroup>
    </Project>
    ```
3. Change `DuckovPath` to your Escape from Duckov installation path
4. Open `ModConfigExample.sln` with Visual Studio or Rider
5. Build project
6. Copy `ModConfigExample.dll` to `Duckov_Data\Managed`

---

## Project Configuration (Modifying for Your Own Project)

To create your own mod based on this example project, you'll need to modify several properties in the main `<PropertyGroup>` section of the `ModConfigExample.csproj` file.

These properties are used to automatically generate the `info.ini` file during the build process.

* `<Name>`: The unique ID for your mod and the resulting `.dll` filename. This becomes the `name` entry in `info.ini`.
* `<DisplayName>`: The name displayed in the in-game mod list. This becomes the `displayName` entry in `info.ini`.
* `<Version>`: The version of your mod. This becomes the `version` entry in `info.ini`.
* `<Description>`: A short description of your mod. This becomes the `description` entry in `info.ini`.
* `<Authors>`: The name of the mod author. This becomes the `author` entry in `info.ini`.
* `<PreviewImagePath>`: The path to your mod's preview image. The default is `preview.png` in the project root.

> **Important Note**: The `Version`, `Authors` properties is optional. Not affecting the info.ini generation if they are missing.

## Build Configurations

This solution includes three main build configurations, defined in the `.sln` and `.csproj` files:

### 1. Release

Building in **Release** mode will create a `.zip` file in the project's `dist/` folder.

* This zip file is packaged in a distribution-ready format, suitable for uploading to the Steam Workshop or other platforms.
* The package includes the mod `.dll`, the `info.ini` file, and the preview image (if it exists).

### 2. Build And Copy

The **Build And Copy** configuration builds the mod and then automatically copies the necessary files directly into your game's `Mods` folder. This is based on the `DuckovPath` set in your `.csproj.user` file or environment variable. This is extremely useful for rapid testing.

The files copied include:
* The built `.dll` file
* The `.pdb` debug symbols (if they exist)
* The auto-generated `info.ini` file
* The preview image (if it exists)

### 3. Debug

This is the standard **Debug** build configuration. It does not perform any automatic file copying or packaging.

---

## Usage

### References
- [Duckov Notice about Mod](https://steamcommunity.com/games/3167020/announcements/detail/617681009992796273) 
  - [Duckov Official Modding Development Guide(GitHub)](https://github.com/xvrsl/duckov_modding)

### Tips
- ModManager.OnModActivated += OnModActivated;
  - Subscribe to this event to know when your mod is activated
- ModConfigAPI.IsAvailable()
  - ModConfigAPI is available only after your mod is activated
- ModConfigAPI.SafeAddOnOptionsChangedDelegate(OnOptionsChanged);
  - Subscribe to this event to know when your mod's options are changed