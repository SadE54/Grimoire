## 🛠 Available Commands

Currently supported commands:

```bash
!rule                      # Display markdown content
!roll                      # Perform a dice roll
!event                     # Get an event from table
!ai                        # Query the AI
!clear                     # Clear the screen
!help                      # List commands
!info                      # Display app and database info
```

### Show

```bash
!show tag_1 tag_2
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

Get an event from a table from the rules database. The found entry is analyzed for random table content and a random event is generated. The event is then displayed in the console.

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