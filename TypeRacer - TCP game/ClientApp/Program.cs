using System.Net;
using System.Net.Sockets;

namespace Client;

/// <summary>
/// The main client application for the Typeracer game.
/// Handles TCP connection, user input, and rendering the game UI.
/// </summary>
public static class Program
{
	const int Port = 5000;

	/// <summary>
	/// buffer to store the current word the user is typing (before pressing Space).
	/// </summary>
	static string _currentWordBuffer = "";

	/// <summary>
	/// Stores the progress of other players to render the leaderboard.
	/// Key: Player Name, Value: Completion percentage (0-100).
	/// </summary>
	static Dictionary<string, int> _playersProgress = new Dictionary<string, int>();

	/// <summary>
	/// The entry point of the client application.
	/// </summary>
	/// <remarks>
	/// 1. Prompts for the player's name.
	/// 2. Connects to the server via TCP.
	/// 3. Starts a background task to listen for server messages.
	/// 4. enters the main input blocking loop.
	/// </remarks>
	public static async Task Main()
	{
		Console.Title = "GAME CLIENT";
		Console.Write("Enter your name: ");
		string playerName = Console.ReadLine() ?? "Nameless";

		var client = new TcpClient();
		try
		{
			Console.WriteLine("Connecting to server...");
			await client.ConnectAsync(IPAddress.Loopback, Port);
		}
		catch (Exception)
		{
			Console.WriteLine("Failed to connect. Is the server running?");
			return;
		}

		var stream = client.GetStream();
		var reader = new StreamReader(stream);
		var writer = new StreamWriter(stream) { AutoFlush = true };

		// 1. Send initial handshake with name
		await writer.WriteLineAsync($"NAME|{playerName}");

		// Start a background thread for receiving data to avoid blocking the input loop
		_= Task.Run(async () => 
		{
			try
			{
				while (true)
				{
					var msg = await reader.ReadLineAsync();
					if (msg == null) return; // Connection closed
					HandleServerMessage(msg);
				}
			}
			catch { 
				Environment.Exit(0); 
			} 
		});

		ProcessInputLoop(writer);
	}

	/// <summary>
	/// Continuously reads keystrokes from the user to construct words.
	/// </summary>
	/// <param name="writer">The stream writer used to send completed words to the server.</param>
	/// <remarks>
	/// - Spacebar: Sends the current buffer as a word command (<c>WORD|...</c>).
	/// - Backspace: Removes the last character from the buffer.
	/// - Other keys: Appends to the buffer.
	/// Uses <c>lock (Console.Out)</c> to prevent UI collisions with the listener thread.
	/// </remarks>
	private static void ProcessInputLoop(StreamWriter writer)
	{
		while (true)
		{
			// Intercept: true hides the key from appearing automatically
			ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

			lock (Console.Out)
			{
				if (keyInfo.Key == ConsoleKey.Spacebar)
				{
					// Send word on Space
					if (!string.IsNullOrWhiteSpace(_currentWordBuffer))
					{
						writer.WriteLine($"WORD|{_currentWordBuffer}");
						_currentWordBuffer = "";
					}
				}
				else if (keyInfo.Key == ConsoleKey.Backspace)
				{
					if (_currentWordBuffer.Length > 0)
						_currentWordBuffer = _currentWordBuffer[..^1];
				}
				else if (!char.IsControl(keyInfo.KeyChar))
				{
					_currentWordBuffer += keyInfo.KeyChar;
				}

				// Refresh only the typing line
				RedrawInputLine();
			}
		}
	}

	/// <summary>
	/// Parses and acts upon raw text protocol messages received from the server.
	/// </summary>
	/// <param name="msg">The raw message string (e.g., "PROG|Player1|50").</param>
	/// <remarks>
	/// Handles cursor management (<c>CursorLeft</c>, <c>CursorTop</c>) to update the UI parts
	/// (Lobby, Progress, Text) without disrupting the user's typing position at the bottom.
	/// </remarks>
	private static void HandleServerMessage(string msg)
	{
		lock (Console.Out)
		{
			// Save current cursor position to restore it after UI updates
			int oldLeft = Console.CursorLeft;
			int oldTop = Console.CursorTop;

			if (msg.StartsWith("LOBBY|"))
			{
				Console.Clear();
				Console.WriteLine("=== LOBBY ===");
				Console.WriteLine(msg.Substring(6));
				Console.WriteLine("Wait for the 'start' command from the server admin...");
			}
			else if (msg.StartsWith("MSG|"))
			{
				// System messages (e.g., Countdown)
				Console.SetCursorPosition(0, 3);
				Console.WriteLine(msg.Substring(4).PadRight(50));
			}
			else if (msg.StartsWith("TEXT|"))
			{
				// Game start - display the text to type
				Console.Clear();
				Console.WriteLine("=== TEXT TO TYPE ===");
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine(msg.Substring(5));
				Console.ResetColor();
				Console.WriteLine("============================");
				Console.WriteLine("\n\n"); // Space for scoreboard
				Console.WriteLine("\n");   // Space for input
				_currentWordBuffer = "";
			}
			else if (msg.StartsWith("PROG|"))
			{
				// Format: PROG|Name|Percent
				var parts = msg.Split('|');
				if (parts.Length == 3)
				{
					string name = parts[1];
					int percent = int.Parse(parts[2]);
					_playersProgress[name] = percent;
					RedrawProgressTable();
				}
			}
			else if (msg.StartsWith("WIN|"))
			{
				// Game Over screen
				Console.Clear();
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("\n\n" + msg.Substring(4));
				Console.ResetColor();
				Console.WriteLine("Press any key to exit...");
				Environment.Exit(0);
			}

			// Restore cursor to the typing line
			RedrawInputLine();
		}
	}

	/// <summary>
	/// Redraws the progress/leaderboard section based on <c>_playersProgress</c>.
	/// </summary>
	/// <remarks>
	/// Draws at a fixed screen position (lines 5-8 typically) so it doesn't scroll the text.
	/// </remarks>
	private static void RedrawProgressTable()
	{
		int startLine = 5;
		int currentLine = startLine;

		Console.SetCursorPosition(0, currentLine);
		Console.WriteLine("--- PROGRESS ---".PadRight(40));
		currentLine++;

		foreach (var kvp in _playersProgress)
		{
			Console.SetCursorPosition(0, currentLine);
			string bar = new string('#', kvp.Value / 10).PadRight(10, '.');
			Console.WriteLine($"{kvp.Key}: [{bar}] {kvp.Value}%".PadRight(40));
			currentLine++;
		}
	}

	/// <summary>
	/// Redraws the user's input line at the bottom of the screen.
	/// </summary>
	private static void RedrawInputLine()
	{
		// Input line is always at the bottom, e.g., line 12
		int inputLine = 12;
		Console.SetCursorPosition(0, inputLine);
		Console.Write("Your word: ".PadRight(50)); // Clear previous line
		Console.SetCursorPosition(0, inputLine);
		Console.ForegroundColor = ConsoleColor.Cyan;
		Console.Write($"Your word: {_currentWordBuffer}");
		Console.ResetColor();
	}
}