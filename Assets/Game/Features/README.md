# Zombie War Game Features

Game-specific code goes here. Do not move Zombie/Soldier/Weapon/Boss-specific rules into General Core or Gameplay Core.

Each feature may use the following structure when appropriate:

```text
Feature/
├── Model/
├── View/
├── Controller/
├── Domain/
├── Services/
├── Commands/
├── Events/
├── Config/
├── Factories/
├── Save/
├── Presentation/
└── Tests/
```

Use direct interface calls inside one feature, Commands for cross-feature requests, and Events for cross-feature facts/notifications.
