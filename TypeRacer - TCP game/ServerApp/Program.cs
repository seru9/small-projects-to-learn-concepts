using System.Net.Sockets;
using System.Net;
using System.Collections.Concurrent;

namespace Server;

// ==================================================================================
// Specyfikacja komunikacji między klientem a serwerem
// ==================================================================================
// The server communicates using a text-based protocol (ASCII/UTF8).
// Packets are delimited by newlines (\n).
//
// CLIENT -> SERVER:
// NAME|<Name>      - Sent immediately after connection to identify the player.
// WORD|<Word>      - Sent during the game when the user presses Space/Enter.
//
// SERVER -> CLIENT:
// LOBBY|<Msg>      - Info message about the lobby state (player count, names).
// MSG|<Msg>        - Generic system message (e.g., countdowns).
// TEXT|<Body>      - The full text to be typed. specific signal to start the UI.
// PROG|<Name>|<%>  - Progress update for a specific player (0-100).
// WIN|<Msg>        - Announcement of the winner and end of game signal.
// ==================================================================================

public class PlayerState
{
	public required StreamWriter Writer { get; set; }
	public required StreamReader Reader { get; set; }
	public string Name { get; set; } = "Unknown";

	public int WordIndex { get; set; } = 0;

	public int Id { get; set; }

	public bool IsDisconnected { get; set; } = false;
}

class Program
{
	private const int Port = 5000;

	// Game Data
	private static string _rawText = "The quick brown fox jumps over the lazy dog";
	private static string[] _targetWords = Array.Empty<string>();

	// List of player jest dostępna przez pare wątków, więc musi być chroniona - synchronizacja
	private static List<PlayerState> _players = new List<PlayerState>();
	private static bool _isGameRunning = false;
	private static bool _acceptingPlayers = true;

	static async Task Main(string[] args)
	{
		Console.Clear();
		Console.Title = "TYPERACER - Server";
		Console.WriteLine($"Wprowadź tekst, który gracze mają wprowadzać - Enter, żeby pominąć");
		string? inputText = Console.ReadLine();
		if(!string.IsNullOrEmpty(inputText))
		{
			_rawText = inputText;
		}
		_targetWords = _rawText.Split(' ');

		

		Console.WriteLine($"[SERVER] Listening on port {Port}.");
		Console.WriteLine("[INFO] Waiting for players to join the lobby.");
		Console.WriteLine("[COMMAND] Type 'start' to begin, or 'tekst <new text>' to change content.");
		var listener = new TcpListener(IPAddress.Loopback, Port);
		listener.Start();
		// W tle przyjmuje nowe połączenia dopóki gra się nie rozpocznie
		_ = Task.Run(() => AcceptClientsLoop(listener));
		while (true)
		{
			string? command = Console.ReadLine()?.Trim().ToLower();

			if (string.IsNullOrEmpty(command)) continue;

			if (command == "start")
			{
				if (ConnectedPlayersCount(_players) <= 1)
				{
					Console.WriteLine("[ERROR] Not enough players in the lobby (Minimum 2).");
				}
				else
				{
					_acceptingPlayers = false; 
					await StartGameAsync();
					break; 
				}
			}
		}

		Console.WriteLine("Press Enter to close the server...");
		Console.ReadLine();
	}
	

	private static async Task AcceptClientsLoop(TcpListener listener)
	{
		while (_acceptingPlayers)
		{
			try
			{
				var client = await listener.AcceptTcpClientAsync();

				if (!_acceptingPlayers)
				{
					client.Close();
					continue;
				}

				// backgoround task to handle each client
				_ = HandleNewConnection(client);
			}
			catch (Exception ex)
			{
				if (_acceptingPlayers) Console.WriteLine($"[ACCEPT ERROR] {ex.Message}");
			}
		}
	}
	// Funkcja obsługująca nowe połączenie
	private static async Task HandleNewConnection(TcpClient client)
	{
		var stream = client.GetStream();
		var reader = new StreamReader(stream);
		var writer = new StreamWriter(stream) { AutoFlush = true };

		try
		{
			string? nameMsg = await reader.ReadLineAsync();
			string playerName = "Player";
			if (nameMsg != null && nameMsg.StartsWith("NAME|"))
			{
				playerName = nameMsg.Substring(5);
			}

			PlayerState player;
			// Lockujemy liste graczy, żeby max.1 wątek mógł ją modyfikować
			lock (_players)
			{
				var existingPlayer = _players.FirstOrDefault(p => p.Name == playerName);

				if (existingPlayer == null)
				{
					player = new PlayerState
					{
						Writer = writer,
						Reader = reader,
						Id = ConnectedPlayersCount(_players) + 1,
						Name = playerName,
						IsDisconnected = false
					};
					_players.Add(player);
					Console.WriteLine($"[LOBBY] New player joined: {player.Name}");
				}
				else
				{
					
					player = existingPlayer;
					player.IsDisconnected = false;
					player.Writer = writer;
					player.Reader = reader;
					Console.WriteLine($"[LOBBY] Player {player.Name} returned to the game!");
				}
			}

			// Updatuj lobby dla wszystkich graczy
			await BroadcastLobbyStatus();

			//input look dla graczy
			await ProcessPlayerMessages(player);
		}
		catch (IOException)
		{
			// Standard disconnection during handshake
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[ERROR] Handshake failed: {ex.Message}");
		}
	}
	//Funkcja rozsyłająca status lobby do wszystkich graczy
	private static async Task BroadcastLobbyStatus()
	{
		string names = string.Join(", ",
			_players.Where(p => !p.IsDisconnected)
					.Select(p => p.Name));
		string msg = $"LOBBY|Welcome! There are {ConnectedPlayersCount(_players)} players in lobby: {names}. Wait for start.";

		List<PlayerState> snapshot;
		lock (_players) snapshot = _players.ToList(); // Snapshot to avoid locking during I/O

		foreach (var p in snapshot)
		{
			try { await p.Writer.WriteLineAsync(msg); } catch { }
		}
	}
	//Funkcja rozpoczynająca grę
	private static async Task StartGameAsync()
	{
		_isGameRunning = true;
		Console.Clear();
		Console.WriteLine("=== GAME STARTED ===");
		Console.WriteLine($"Text: {_rawText}");
		Console.WriteLine("------------------------------------------------");

		DrawServerProgress(); // Initial draw

		// Countdown sequence
		await BroadcastAsync("MSG|Start in 3 seconds...");
		await Task.Delay(1000);
		await BroadcastAsync("MSG|Start in 2 seconds...");
		await Task.Delay(1000);
		await BroadcastAsync("MSG|Start in 1 second...");
		await Task.Delay(1000);

		// START COMMAND
		await BroadcastAsync($"TEXT|{_rawText}");

		while (_isGameRunning)
		{
			await Task.Delay(500); // UI Refresh Rate
			DrawServerProgress();

			lock (_players)
			{
				if (_players.All(p => p.IsDisconnected))
				{
					Console.WriteLine("\nAll players disconnected. Game over.");
					_isGameRunning = false;
				}
			}
		}
	}

	//Funkcja rysująca postęp graczy na serwerze
	private static void DrawServerProgress()
	{
		int top = 4;
		Console.SetCursorPosition(0, top);

		lock (_players)
		{
			foreach (var p in _players)
			{
				if (p.IsDisconnected)
				{
					Console.WriteLine($"[DISCONNECTED] {p.Name}".PadRight(80));
					continue;
				}

				int percent = (int)((float)p.WordIndex / _targetWords.Length * 100);
				if (_targetWords.Length == 0) percent = 100;

				string bar = new string('#', percent / 5).PadRight(20, '.');
				string status = p.WordIndex >= _targetWords.Length ? "FINISHED" : $"{percent}%";

				// Determine current word for debug view
				string currentWord = "";
				if (p.WordIndex < _targetWords.Length)
				{
					currentWord = _targetWords[p.WordIndex];
				}

				// Output formatted line: "Name       [#####.....] 25%   Now: word      "
				Console.WriteLine($"{p.Name.PadRight(15)} [{bar}] {status.PadRight(10)} Now: {currentWord}".PadRight(80));
			}
		}
	}
	//Funkcja przetwarzająca wiadomości od gracza
	private static async Task ProcessPlayerMessages(PlayerState player)
	{
		try
		{
			while (true)
			{
				var msg = await player.Reader.ReadLineAsync();

				if (msg == null) break;

				if (_isGameRunning)
				{
					if (msg.StartsWith("WORD|"))
					{
						string typedWord = msg.Substring(5).Trim();

						// Logika gry: sprawdź poprawność słowa
						if (player.WordIndex < _targetWords.Length && typedWord == _targetWords[player.WordIndex])
						{
							player.WordIndex++;

							int percent = (int)((float)player.WordIndex / _targetWords.Length * 100);

							// Globalny broadcast postępu
							await BroadcastAsync($"PROG|{player.Name}|{percent}");

							if (player.WordIndex >= _targetWords.Length)
							{
								await BroadcastAsync($"WIN|Player {player.Name} WON!");
								// Optional: _isGameRunning = false; to stop game immediately on first win
							}
						}
					}
				}
			}
		}
		catch (IOException)
		{
		
		}
		finally
		{
			player.IsDisconnected = true;
			Console.WriteLine($"[INFO] Player {player.Name} disconnected.");

			if (!_isGameRunning)
			{
				await BroadcastLobbyStatus();
			}
		}
	}

	private static async Task BroadcastAsync(string message)
	{
		List<PlayerState> snapshot;
		lock (_players) snapshot = _players.ToList();

		foreach (var p in snapshot)
		{
			if (!p.IsDisconnected)
			{
				try { await p.Writer.WriteLineAsync(message); } catch { }
			}
		}
	}

	private static int ConnectedPlayersCount(List<PlayerState> players)
	{
		lock (players)
		{
			return players.Count(p => !p.IsDisconnected);
		}
	}
}