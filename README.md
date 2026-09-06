# Archipelago MW for The Battle of Polytopia

This is [Archipelago MW](https://archipelago.gg/) randomizer implementation for [The Battle Of Polytopia](https://store.steampowered.com/app/874390). Source code of `.apworld` can be found at [Eclassic32/Archipelago/worlds/polytopia](https://github.com/Eclassic32/Archipelago/tree/main/worlds/polytopia)  

## Goal of the randomizer:
You need to play several matches with unique tribes, and reach `{RequiredScoreForVictory}` score on `{RequiredUniqueTribesWins}` unique tribes.

## Items:
- Tribe Unlock - `{TRIBE_NAME}`
- Filler

## Locations:
- `{TRIBE_NAME}` - Score `{from 1, up to 100}`K
- `{TRIBE_NAME}` - Victory
> *// Note: Victory Location is sent only at the end of the match* 

## Options:
- **Playable Tribes** - Select which tribes are in your game. (default: first 12 tribes)
- **First Unlocked Tribe** - With which tribe you start the game, options: 16 tribes currently in game + 4 randomized options. (default: any playable tribe)
- **RequiredUniqueTribesWins** - How many *"`{TRIBE_NAME}` - Victory"* checks you need to goal your game. (default: 4)
- **RequiredScoreForVictory** - What score (in thousands) you need to check *"`{TRIBE_NAME}` - Victory"*. (default: 15)
- **ShouldSendScoreChecksImmediately** - Should Score Checks be sent immediately, or at the end of the match (default: true, immediately)
- **ScoreChecksMin** - Minimum score (in thousands) to create Score Checks. (default: 2)
- **ScoreChecksMax** - Maximum score (in thousands) to create Score Checks. (default: 20)
- **ScoreChecksStep** - Step size to create Score Checks. (default: 2)
> *// Note: Setting any of the Score Check options to 0 will disable them*

> Example: on default options next Score Checks are created: 2K, 4K, 6K, 8K, 10K, 12K, 14K, 16K, 18K, 20K

# Setting up The Battle of Polytopia Archipelago

## Requirements:
- [Archipelago Launcher](https://archipelago.gg)
- [Polytopia AP World](https://github.com/Eclassic32/Archipelago/releases?q=Polytopia&expanded=true)

- Copy of [The Battle of Polytopia on Steam](https://store.steampowered.com/app/874390)
- [Polymod](https://polymod.dev/)
- [Client-side Mod](https://github.com/Eclassic32/polytopia-AP/releases)

> Epic build of the game is not tested, but should work regardless

> Mobile builds are not supported, for a reason of not having Polymod

## Generation:
1. Install `polytopia-VERSION.apworld` by putting it into `Archipelago Launcher` window, or by double-clicking
2. Go To `Archipelago Launcher` > `Generate Template Options` > `The Battle of Polytopia.yaml` > Open and Change options > Move the YAML into parent `/Players` folder
3. Go To `Archipelago Launcher` > `Generate` > Open the `/output` folder
4. Host the new file either in https://archipelago.gg/uploads or `Archipelago Launcher` > `Host`

## Setting up the client-side mod:
1. Install The Battle of Polytopia from Steam and launch at least once
2. Open the folder where the game is (in Steam > Right Mouse Button on `The Battle of Polytopia` > `Properties...` > `Installed files` > `Browse...`)
3. Download Polymod Installer from https://polymod.dev/ and open it (Windows Defender might have issue with it, press `More Info...` > `Execute Anyway`)
4. Copy the folder address from Step 2 into `Game Path` > Press `Install`
5. Download client-side mod (`polytopia-AP_VERSION.polymod`) and place it in `...\The Battle of Polytopia\Mods`

## Connecting to the server:
1. In Main Menu, on top-left side of the screen, press `Archipelago Hub` button
2. Enter server address and port, slot name, password and press `CONNECT` 
3. On pressing `New Game` you will be sent to "Creative Mode" 
4. In Tribe Screen you can choose your tribe and start playing. Few notes:
  - Tribes NOT in your `Playable Tribes` **will not be displayed** at all, and can not be choosen neither by player or bot
  - Tribes you have YET TO RECEIVE have **red** background, and can not be choosen neither by player or bot
  - Tribes you received have default **blue** background, and can be choosen both by you and bot
  - Tribes you received, but DISABLED will have default **black** background, they can be re-enabled and can not be chosen by player or bot while disabled
  - (NOT IMPLEMENTED) Tribes you have checked Victory location will have **green** background
  - (NOT IMPLEMENTED) Tribes you have checked ALL location will have **gold** background
5. To Disconnect, in main menu press `Archipelago Hub` button and `DISCONNECT` button

## AI Disclosure:
No graphical assets were made by any generative AI.
Small portion of the code was written by AI Inline Suggestions (GitHub Copilot), with carefull inspection of its code.
AI Chat Bots were used to understand the basics of Unity modding and decypher few game methods decompiled by ghidra.
No part of the project was directly copy-pasted from AI Chat Bots.
