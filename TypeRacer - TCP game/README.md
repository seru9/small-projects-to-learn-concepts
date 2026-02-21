# 🏎️ TypeRacer Console Edition

A multiplayer arcade-style "TypeRacer" game for the console, built using a **Client-Server** architecture with C# and TCP sockets. Players compete in real-time to type out a displayed text, with their progress synchronized across all participants by the server.

## 🌟 System Features

* **Network Communication**: Utilizes asynchronous TCP streams (`NetworkStream`) for lossless data transmission.
* **Real-Time Progress**: Dynamic updates of the progress bar for all players throughout the race.
* **Lobby**: A pre-game waiting room where the server administrator can input custom text for the competition.
* **Thread Safety**: Secure handling of multiple simultaneous connections using `lock` statements and thread-safe logic.
* **Disconnection Handling**: The server detects connection losses and informs remaining participants in real-time.

---

## 🛠️ Communication Protocol

The application uses a custom prefix-based text protocol, where every message is terminated with a newline character (`\n`).

| Direction | Prefix | Description |
| :--- | :--- | :--- |
| **C ➔ S** | `NAME\|<name>` | Player registration in the lobby upon connection. |
| **C ➔ S** | `WORD\|<word>` | Sending the typed word to the server (triggered by Space). |
| **S ➔ C** | `LOBBY\|<msg>` | Information about the number of players and their names in the lobby. |
| **S ➔ C** | `TEXT\|<body>` | The content to be typed (initializes the game view). |
| **S ➔ C** | `PROG\|<n>\|<%>` | Progress update for a specific player (0-100%). |
| **S ➔ C** | `WIN\|<msg>` | Announcement of the winner and end-of-session signal. |

---

## 🚀 Setup Instructions

### 1. Server
1. Launch the `Server` project.
2. (Optional) Enter custom text for the game or press **Enter** to use the default ("The quick brown fox...").
3. Wait for at least 2 players to connect.
4. Type `start` in the server console to begin the countdown.

### 2. Client
1. Launch the `Client` project.
2. Enter your username.
3. Once the server starts the game, type the words highlighted in yellow.
4. **Important**: After each correctly typed word, press **Space** to send it for verification.

---

## 🏗️ Technical Architecture

### Server (`Server`)
* Manages a list of `PlayerState` objects.
* Verifies the accuracy of words sent by clients against the target text.
* Broadcasts progress updates and game states to all connected users.

### Client (`Client`)
* Runs a background task (`Task.Run`) to constantly listen for incoming server messages.
* Captures keystrokes in real-time (`Console.ReadKey`), allowing for fluid typing without blocking the UI.
* Uses `Console.SetCursorPosition` to render the scoreboard and progress bars without screen flickering.



---
