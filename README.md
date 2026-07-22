# C# .NET Framework Snake Game - University Project

A complete, production-grade **Snake Game** application built using **C#**, **.NET Framework 4.8**, and **Windows Forms (WinForms)** in **Visual Studio**.

## 📌 Features Included
1. **Classic Game Loop**: Responsive timer-driven game engine with custom coordinate positioning.
2. **Double Buffered Drawing**: Eliminates screen flickering during high-speed rendering.
3. **Snake Controls**: Move using Arrow Keys or WASD keys. Prevented 180-degree instant self-collision turns.
4. **New Game & Pause/Resume**: Full menu & keyboard hotkey controls (`P` or `Space` to Pause, `N` for New Game).
5. **Save & Continue (Persistence)**: Save game state at any moment (`savegame.dat`) and resume anytime!
6. **High Score Tracker**: Persistent record keeping saved to disk (`highscore.txt`).
7. **Clean University-Level Architecture**: Object-Oriented Design separating Entities (`Snake`, `Food`), Models (`GameState`), Services (`SaveLoadManager`), and UI Controls (`FormMain`).

## 🛠️ Requirements & How to Run in Visual Studio
1. Open Visual Studio (2017, 2019, or 2022).
2. Go to **File -> Open -> Project/Solution** and select `SnakeGame.sln`.
3. Ensure **.NET Desktop Development** workload is installed.
4. Press **F5** or click **Start** to compile and run!

## 🎮 Controls
- **Arrow Keys / WASD**: Move Snake
- **Space / P**: Pause or Resume Game
- **N**: Start New Game
- **Save Button**: Save current snapshot to file
- **Continue Button**: Load and resume saved snapshot