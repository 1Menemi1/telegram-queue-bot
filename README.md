# telegram-queue-bot

## [Link to the Bot](https://t.me/M33041Queue_bot)

## TODO:

- [x] Add a comand 4 admin that print secret commands
- [x] Clear useless text 4 admin commands
- [x] Add `/reset` command 4 admin
- [x] How 2 use this bot
- [ ] Deploy bot on Heroku / Kubernetes / smth else
- [ ] Add PlantUml diagram

## How to use bot

Common user has thier own commands as:
- `/start` - start the bot and registrate a new user
- `/commands` - gives a list of all unlocked commands for common users
- `/queue` - gives you a place in queue
- `/list` - gives you a list of users in queue
- `/stop` - removes you from queue

Admin has all commands from common user and:
- `/admin` - gives a list of all unlocked commands for admin
- `/reset` - reset the queue by removing all the users from it
- `/secret` - gives a list of all secret commands
- `/pop username` - removes *username* from queue