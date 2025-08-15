# Garden Station

<p align="center"><img src="https://github.com/Gardenstation/Gardenstation/blob/master/Resources/Textures/Logo/splashlogo.png?raw=true" width="512px" /></p>

---

Garden Station is a fork of [Einstein Engines](https://github.com/Simple-Station/Einstein-Engines).

Space Station 14 is inspired heavily by Space Station 13 and runs on [Robust Toolbox](https://github.com/space-wizards/RobustToolbox), a homegrown engine written in C#.

## Links

 [Discord](https://discord.gg/CTuEXeb4dA) | [Steam(WizDen Launcher)](https://store.steampowered.com/app/1255460/Space_Station_14/)

## Contributing

We are happy to accept contributions from anybody, come join our Discord if you want to help.
We've got a [list of issues](https://github.com/Gardenstation/Gardenstation/issues) that need to be done and anybody can pick them up. Don't be afraid to ask for help in Discord either!

We are currently accepting translations of the game on our main repository.
If you would like to translate the game into another language check the #contributor-general channel in our Discord.

## Building

Refer to [the Space Wizards' guide](https://docs.spacestation14.com/en/general-development/setup/setting-up-a-development-environment.html) on setting up a development environment for general information, but keep in mind that Einstein Engines is not the same and many things may not apply.
We provide some scripts shown below to make the job easier.

### Build dependencies

> - Git
> - .NET SDK 9.0.101


### Windows

> 1. Clone this repository
> 2. Run `git submodule update --init --recursive` in a terminal to download the engine
> 3. Run `Scripts/bat/buildAllDebug.bat` after making any changes to the source
> 4. Run `Scripts/bat/runQuickAll.bat` to launch the client and the server
> 5. Connect to localhost in the client and play

### Linux

> 1. Clone this repository
> 2. Run `git submodule update --init --recursive` in a terminal to download the engine
> 3. Run `Scripts/sh/buildAllDebug.sh` after making any changes to the source
> 4. Run `Scripts/sh/runQuickAll.sh` to launch the client and the server
> 5. Connect to localhost in the client and play

### MacOS

> I don't know anybody using MacOS to test this, but it's probably roughly the same steps as Linux

## License

Please read the [LEGAL.md](./LEGAL.md) file for information on the licenses of the code and assets in this repository.
