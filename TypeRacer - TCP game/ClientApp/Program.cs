using System.Net;
using System.Net.Sockets;

namespace Client;

public static class Program
{
	const int Port = 5000;
	static string _currentWordBuffer = "";
	static Dictionary<string, int> _playersProgress = new Dictionary<string, int>();
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

		// Wysyłamy nazwę gracza do serwera
		await writer.WriteLineAsync($"NAME|{playerName}");

		// Background task do odbierania wiadomości od serwera
		_ = Task.Run(async () =>
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
			catch
			{
				Environment.Exit(0);
			}
		});

		ProcessInputLoop(writer);
	}
	// Pętla do odczytu wpisywanego tekstu i wysyłania słów do serwera
	private static void ProcessInputLoop(StreamWriter writer)
	{
		while (true)
		{
			// wCZYTUJEMY KLAWISZ
			ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

			lock (Console.Out)
			{
				if (keyInfo.Key == ConsoleKey.Spacebar)
				{
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

				RedrawInputLine();
			}
		}
	}

	// Hadnluje wiadomości od serwera, czyli lobby dla graczy, tekst do wpisania, postęp graczy i ekran zwycięzcy
	private static void HandleServerMessage(string msg)
	{
		lock (Console.Out)
		{
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

	// Rysuje tabelę postępu graczy poniżej tekstu do wpisania
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