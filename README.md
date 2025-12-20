# .NET SCADA-Telegram bot

[![Nuget](https://img.shields.io/nuget/vpre/Telegram.Bot.svg?label=Telegram.Bot&style=flat-square&color=d8b541)](https://www.nuget.org/packages/Telegram.Bot) 
[![.NET](https://img.shields.io/badge/.NET-512BD4?logo=dotnet&logoColor=fff)](#)
[![C#](https://custom-icon-badges.demolab.com/badge/C%23-%23239120.svg?logo=cshrp&logoColor=white)](#)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/download)
[![License](https://img.shields.io/github/license/TelegramBots/telegram.bot.svg?style=flat-square&maxAge=2592000&label=License)](https://raw.githubusercontent.com/TelegramBots/telegram.bot/master/LICENSE)

## What is SCADA-Telegram bot

SCADA-Telegram bot is a Telegram bot written in C# 13 and .NET 10 that forwards messages (errors and failures) from the server to the Telegram bot.

## Feautures 

## Getting Started

### Installation

1. Clone the repository:
    ```bash
    git clone https://github.com/renx1eescada-telegram-bot
    ```
2. Restore dependencies:
    ```bash
    dotnet restore
    ```
3. Build the project:
    ```bash
    dotnet build
    ```

### Running the application

1. Navigate to project directory:
    ```bash
    cd scada-telegram-bot/SсadaTelegramBot
    ```

2. The run application: 
    ```bash
    dotnet run
    ```

## Development

### Project Structure

```bash
SсadaTelegramBot
├── BackgroundWorkers
├── Configurations 
├── DTOs
├── Helpers
├── logs
└── Services
```

## Configuration

The settings were places in the `appsettings.json` file.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Error",
      "Microsoft.AspNetCore": "Warning"
    },
    "PathFile": "logs/log-.txt"
  },
  "AllowedHosts": "*",
  "BotConfiguration": {
    "BotToken": "",
    "HostAddress": "opc.tcp://127.0.0.1:16550",
    "NodeIds": {
      "NotificationNodeId": "ns=1;i=39153",
      "NotificationResponseNodeId": "ns=1;i=42003",
      "ServerIsLifeNotificationNodeId": "ns=1;i=42003"
    },
    "IntervalOfUpdateServerLifeNotifier": 10,
    "IntervalOfUpdateOpcMotion": 5,
    "IntervalOfUpdateTelegramBot": 1,
    "BotMessages": {
      "StartMessage": "Добро пожаловать! \nОтправте /help что бы получить список доступных комманд!",
      "HelpMessage": "Список всех команд: \n/start\n/help\n/me",
      "MyInformationMessage": "Перешлите это сообщение администратору в Telegram",
      "UnknownCommand": "Команда не распознана! \nОтправте /help что бы получить список доступных комманд!"
    }
  }
}
```

* PathFile - the direction to `log-.txt` file
* BotToken - token from the `@BotFather` on Telegram
* HostAddress - server address
* NotificationNodeId 
* NotificationResponseNodeId
* ServerIsLifeNotificationNodeId
* IntervalOfUpdateServerLifeNotifier
* IntervalOfUpdateOpcMotion
* IntervalOfUpdateTelegramBot
  

## Contributing

Bug reports and/or pull requests are welcome!

## Licence

The code in this repo is licensed under the [MIT](LICENSE.TXT) license.
