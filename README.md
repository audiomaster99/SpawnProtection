![GitHub tag (with filter)](https://img.shields.io/github/v/tag/audiomaster99/SpawnProtection?style=for-the-badge&label=Version) ![GitHub Repo stars](https://img.shields.io/github/stars/audiomaster99/SpawnProtection?style=for-the-badge) ![GitHub all releases](https://img.shields.io/github/downloads/audiomaster99/SpawnProtection/total?style=for-the-badge)

### Features

- Configurable spawn protection
- Render player models transparent
- Timer with progress bar
- CT Only protection (can be enabled in config)
- 
![Protection Timer](https://media.discordapp.net/attachments/1172576974498177034/1236970784342282260/progressbar2.gif?ex=6639f250&is=6638a0d0&hm=1fdc2e68130fe8ee72d659986828194fc0b52b9c37fab9b3682165fa206aa404&= "Protection Timer")

### Config

```
  "spawn-protection-time": 10, // for how long will player be protected in seconds
  "spawn-prot-center-message": true, //enable center messages
  "spawn-prot-end-announce": true, // enablee announcement when spawn protection ended
  "attacker-center-message": true, // enable warning message for attacker
  "enable-center-html-message": true, // enable timer and progress bar
  "spawn-prot-transparent-model": true, // enable transparent models while protected
  "ct-protection-only": false, // enable protection only for Counter-Terrorists
  "ConfigVersion": 3 // dont change!
```

### Dependencies

[Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master "Metamod:Source")
[CounterStrike Sharp](https://github.com/roflmuffin/CounterStrikeSharp "CounterStrike Sharp")

### Installation

Place plugin contents to **addons/counterstrikesharp/plugins/SpawnProt**
