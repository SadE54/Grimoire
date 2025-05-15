# 📔 Grimoire 🧙‍♂️

![Grimoire](pictures/grimoire.png)

**Grimoire** is a command-line and text-mode role-playing game assistant designed to help GMs run their sessions. A rich graphical interface can be distracting when quick access to information is needed. With just a few simple commands, **Grimoire** provides instant access to rules, random tables, dice rolls, and even the ability to generate NPCs or locations via the OpenAI API.

---

## ✨ Features

- 📚 Quick access to rules and custom content
- 🏷 Search by name or keywords
- 🎲 Dice rolling
- 🧠 NPC and location generation, ... using configurable prompts with the OpenAI API
- 🎲 Dice rolls on random tables
- 📃 Uses markdown files for content
- 🗂 Supports multiple games and versions via metadata

 🔥 In development:
 - 🕯️Torch simulation with configurable duration
 - Ask for new features !

---

## 📷 Screenshots 

You can see some screenshots in the Wiki section: 
[screenshots](https://github.com/SadE54/Grimoire/wiki/Screenshots)


## 📦 Binaries

Available directly from GitHub for Windows 10/11.
> You need to have .NET 9 runtime installed (https://dotnet.microsoft.com/fr-fr/download/dotnet/9.0/runtime)

---

## ⚙️ Build

1. Clone the repository:

```bash
git clone https://github.com/your-user/grimoire.git
cd grimoire
```

2. Build with .NET 9:

```bash
dotnet build
```

You can also use Visual Studio directly.

---

## ✨ Start

The configuration file is by default using the Shadowdark quickstart fr database. 
See in the **configuration **section how to edit the configuration file if needed. 

To start the application, you just have to launch `grimoire.exe` from the Grimoire directory.
I recommand to use the new Windows terminal application instead the old `cmd` tool.


## 🛠 Available Commands

Currently supported commands:

```bash
!rule                      # Display markdown content
!roll                      # Perform a dice roll
!event                     # Get a random entry from a table
!ai                        # Query the AI
!clear                     # Clear the screen
!help                      # List commands
!info                      # Display app and database info
```

### Rule

```bash
!rule tag_1 tag_2
```

Searches the rules database for content matching the tags. If multiple entries are found, the prompt lets you choose which one to display.

### Roll

Performs a dice roll with modifiers and advantage/disadvantage support.
Examples:

```bash
!roll 3d6           # Roll 3 six-sided dice and return the result
!roll 1d20 adv      # Roll a d20 twice and return the best roll
!roll 1d10 dis      # Roll a d10 twice and return the worst roll
!roll 2d6+1         # Roll 2d6 and add 1 to the final result
!roll 1d12+1 adv    # Roll a d12 twice, return the best and add 1
```
### Event

Get an event from a table from the rules database.
The found entry is analyzed for random table content and a random event is generated.
The event is then displayed in the console.

```bash
!event tag
!event tag1 tag2
```

### Ai

This feature requires an OpenAI account and API token. You can buy credits on OpenAI’s website. The cost is very reasonable since the model used is `chatgpt-3.5-turbo`. With $5, you can probably run close to a thousand requests.
Different prompts are defined in a separate database. You can configure a system prompt (your AI assistant's behavior) and associate it with a name used by the command.
If you define a prompt named 'npc' asking OpenAI to describe a character, you can use:

```bash
!ai npc troll
!ai npc nasty goblin
```

OpenAI will return a nice description of a troll! You can even include stats in the prompt if you like.
Another example:

```bash
!ai place tavern
```

With the corresponding prompt, it will describe a great tavern!

### Clear

Clears the screen:

```
!clear
```

### Help

Displays the help with a list of commands and usage instructions.

---

## 🔐 Configuration

At startup, Grimoire looks for a file named **config.toml** in the working directory.

```toml
[rules]
database_path = "/path_to/your_game_rules.json"

[openai]
api_token = "your-Open-API-key"
database_path = "Shadowdark/prompts.json"
```

The `rules` section sets the path to the JSON database for rules and tables.

The `openai` section defines parameters for OpenAI access:
- `api_token`: your OpenAI access token
- `database_path`: the path to the prompt database

---

## 🧾 JSON Rules Database Example

```json
{
  "game": "ShadowDark RPG",
  "version": "1.0",
  "author": "John Doe",
  "license": "MIT",
  "credits": "path_to_shadowdark_credits.md",
  "entries": [
    {
      "name": "combat",
      "tags": ["combat", "weapons", "initiative"],
      "path": "data/Shadowdark/combat.md"
    }
  ]
}
```

---

## 🧠 JSON Prompt Database Example

```json
{
  "version": "1.0",
  "author": "John Doe",
  "description": "System prompts for the OpenAI API",
  "prompts": [
    {
      "name": "npc",
      "title": "Describe an NPC character",
      "temperature": 1.0,
      "system": "You're a role-playing assistant who describes non-player characters in an immersive way. Use an evocative narrative tone, and detail appearance, personality, voice, and a striking detail."
    },
    {
      "command": "place",
      "title": "Describe a place",
      "temperature": 0.85,
      "system": "You're an assistant describing fantasy locations for a role-playing game. Use sensory description (sight, smell, sound) and set a strong mood. Add a unique element (object, character, strange noise, etc.)"
    }
  ]
}
```

---

## 📚 Provided Databases

- ⚔️ **Shadowdark** *(example database included)*
- 📜 Other games can be added using markdown files and tags.

---

## ✍️ Credits

### 🕯 *Shadowdark RPG*

- **Author**: Kelsey Dionne  
- **Publisher**: The Arcane Library  
- **French translation**: Sandy Julien for [Rabbit Hole](https://rabbit-hole.fr) (Arkhane Asylum Publishing)  
- **Official site**: [https://www.thearcanelibrary.com](https://www.thearcanelibrary.com)

> Shadowdark™ is a registered trademark of The Arcane Library. This project is not affiliated with, endorsed by, or sponsored by Kelsey Dionne or The Arcane Library.

**Note**: Some rules and content have been summarized or adapted for simplified use in sessions. Please refer to the official materials for the full rules.

---

## 📄 License

This project is under the MIT license.  
Content files may be subject to copyright depending on their origin.

---

## 🙏 Acknowledgements

Grimoire uses:

- [Spectre.Console](https://spectreconsole.net/) for the elegant console interface (MIT License)
- [ChatGPT.Net](https://github.com/Dasync/ChatGPT.Net) for simplified OpenAI access (MIT License)
- [Tommy](https://github.com/dezhidki/Tommy) for TOML file support

---

> "A good GM doesn’t have all the answers, but knows where to find them."
