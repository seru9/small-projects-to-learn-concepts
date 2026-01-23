using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace NYC_citybikes
{
	class Program
	{
		static void Main(string[] args)
		{
			DisplayBike();
			string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "2014-citibike-tripdata");

			Console.WriteLine($"Szukam folderów w: {rootPath}");

			if (!Directory.Exists(rootPath))
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("BŁĄD: Nie widzę żadnych folderów w miejscu uruchomienia programu!");
				Console.WriteLine("Upewnij się, że w Visual Studio ustawiłeś dla plików CSV opcję 'Kopiuj do katalogu wyjściowego' na 'Kopiuj zawsze'.");
				Console.ResetColor();
				return;
			}

			Console.Write("\nWybierz miesiące (np. 1,2,3 lub 4 7 12): ");
			string input = Console.ReadLine() ?? string.Empty;

			string[] tokens = input
				.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

			int[] months = tokens
				.Select(t => int.TryParse(t, out var m) ? m : -1)
				.Where(m => m is >= 1 and <= 12)
				.Distinct() // jeżeli się powtórzą miesiące
				.ToArray();

			if (months.Length == 0)
			{
				Console.WriteLine("Błąd: Podaj co najmniej jeden miesiąc w zakresie 1–12 (oddzielając przecinkiem lub spacją).");
				return;
			}

			string[] monthDirectoryPaths = months
				.Select(m =>
				{
					string searchPattern = $"{m}_";
					return Directory
						.GetDirectories(rootPath)
						.FirstOrDefault(d => Path.GetFileName(d).StartsWith(searchPattern));
				})
				.Where(p => !string.IsNullOrWhiteSpace(p))
				.Select(p => p!)
				.ToArray();

			if (monthDirectoryPaths.Length == 0)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Nie znaleziono żadnych folderów dla wybranych miesięcy w lokalizacji: {rootPath}");
				Console.ResetColor();
				return;
			}

			Console.WriteLine("Znalezione foldery:");
			foreach (var p in monthDirectoryPaths)
			{
				Console.WriteLine($" - {Path.GetFileName(p)}");
			}

			foreach (var monthDirectoryPath in monthDirectoryPaths)
			{
				ProcessMonth(monthDirectoryPath);
			}

		
		}

		private static Trip? MapCsvLineToTrip(string csvLine)
		{
			try
			{
				var parts = csvLine.Split(',');

				return new Trip
				{
					TripDuration = int.Parse(parts[0]),
					StartTime = DateTime.Parse(parts[1], CultureInfo.InvariantCulture),
					StopTime = DateTime.Parse(parts[2], CultureInfo.InvariantCulture),
					StartStationId = parts[3],
					StartStationName = parts[4],
					StartStationLatitude = double.Parse(parts[5], CultureInfo.InvariantCulture),
					StartStationLongitude = double.Parse(parts[6], CultureInfo.InvariantCulture),
					EndStationId = parts[7],
					EndStationName = parts[8],
					EndStationLatitude = double.Parse(parts[9], CultureInfo.InvariantCulture),
					EndStationLongitude = double.Parse(parts[10], CultureInfo.InvariantCulture),
					BikeId = parts[11],
					UserType = parts[12],
					BirthYear = int.TryParse(parts[13], out int year) ? year : (int?)null,
					Gender = parts[14]
				};
			}
			catch
			{
				return null;
			}
		}

		public static void DisplayBike()
		{
			Console.WriteLine("	Witam w aplikacji NYC City Bikes Analysis!");
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine(@"
                                                      *Bike*
              d$$$$$$$P""                  $    J
                  ^$.                     4r  ""
                  d""b                    .db
                 P   $                  e"" $
        ..ec.. .""     *.              zP   $.zec..
    .^        3*b.     *.           .P"" .@""4F      ""4
  .""         d""  ^b.    *c														        .$""  d""   $         %
 /          P      $.    ""c      d""   @     3r         3
4        .eE........$r===e$$$$eeP    J       *..        b
$       $$$$$       $   4$$$$$$$     F       d$$$.      4
$       $$$$$       $   4$$$$$$$     L       *$$$""      4
4         ""      """"3P ===$$$$$$""     3                  P
 *                 $       """"""        b                J
  "".             .P                    %.             @
    %.         z*""                      ^%.        .r""
       ""*==*""""                             ^""*==*""""   Gilo94'
    ");

			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(@"
		 _   _  __     __   _____ 
		| \ | | \ \   / /  / ____|
		|  \| |  \ \_/ /  | |     
		| . ` |   \   /   | |     
		| |\  |    | |    | |____ 
		|_| \_|    |_|     \_____|
			");

			Console.ResetColor();
		}
		public static void ProcessMonth(string monthDirectoryPath)
		{
			Console.WriteLine($"Wczytuję dane z: {Path.GetFileName(monthDirectoryPath)}...");

			List<Trip> trips = Directory
				.GetFiles(monthDirectoryPath, "*.csv")
				.SelectMany(file => File.ReadLines(file).Skip(1))
				.Select(MapCsvLineToTrip)
				.Where(t => t != null)
				.Select(t => t!)
				.ToList();

			if (trips.Count == 0)
			{
				Console.WriteLine("Folder istnieje, ale nie znaleziono w nim poprawnych danych CSV.");
				return;
			}

			Console.WriteLine("Wybierz informacje (1–5):");
			Console.WriteLine("1. Najbardziej obciążona stacja w godzinach szczytu");
			Console.WriteLine("2. Najdłuższe wycieczki");
			Console.WriteLine("3. Średni wiek ludzi w poszczególnych dzielnicach");
			Console.WriteLine("4. Stacje, do których najchętniej się wraca");
			Console.WriteLine("5. Najbardziej eksploatowane rowery (intensywność)");

			string input = Console.ReadLine() ?? string.Empty;

			string[] tokens = input
				.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string token in tokens)
			{
				switch (token)
				{
					case "1":
						QueryBusiestRushHourStation(trips);
						break;
					case "2":
						QueryLongestAvgRoutes(trips);
						break;
					case "3":
						QueryDistrictMostTraveledAge(trips);
						break;
					case "4":
						QueryLoopStations(trips);
						break;
					case "5":
						QueryMostUsedBikes(trips);
						break;
					default:
						Console.WriteLine("Nieprawidłowy wybór.");
						break;
				}
			}
		}
		public static void QueryBusiestRushHourStation(List<Trip> trips)
		{
			var result = trips
				.Where(t =>
					(t.StartTime.Hour >= 7 && t.StartTime.Hour <= 9) ||
					(t.StartTime.Hour >= 16 && t.StartTime.Hour <= 18))
				.GroupBy(t => new { t.StartStationId, t.StartStationName })
				.OrderByDescending(g => g.Count())
				.Select(g => new
				{
					g.Key.StartStationName,
					Count = g.Count()
				})
				.FirstOrDefault();

			if (result == null)
			{
				Console.WriteLine("[1] Brak danych dla godzin szczytu.");
				return;
			}

			Console.WriteLine($"[1] Najbardziej obciążona stacja (szczyt): {result.StartStationName} ({result.Count})");
		}

		public static void QueryLongestAvgRoutes(List<Trip> trips)
		{
			var routes = trips
				.Where(t => t.TripDuration > 300)
				.GroupBy(t => new { t.StartStationName, t.EndStationName })
				.Select(g => new
				{
					Route = $"{g.Key.StartStationName} → {g.Key.EndStationName}",
					AvgDuration = g.Average(t => t.TripDuration),
					Count = g.Count()
				})
				.Where(x => x.Count > 50)
				.OrderByDescending(x => x.AvgDuration)
				.Take(5)
				.ToList();

			if (routes.Count == 0)
			{
				Console.WriteLine("[2] Brak danych do wyświetlenia.");
				return;
			}

			Console.WriteLine("[2] Najdłuższe relacje (średni czas):");
			foreach (var r in routes)
			{
				Console.WriteLine($"   {r.Route}: {r.AvgDuration:F0}s ({r.Count} przejazdów)");
			}
		}

		public static void QueryDistrictMostTraveledAge(List<Trip> trips)
		{
			int currentYear = DateTime.Now.Year;

			var result = trips
				// odsiewamy błędne dane (brak roku urodzenia lub błędne koordynaty)
				.Where(t => t.BirthYear.HasValue && t.BirthYear > 1900
							&& t.StartStationLatitude != 0 && t.StartStationLongitude != 0)

				// mapowanie GPS na Nazwę Dzielnicy.
				.Select(t => new
				{
					Trip = t,
					FoundNeighborhood = neighborhoods.FirstOrDefault(n =>
						GeoHelper.IsPointInPolygon(n.Vertices, t.StartStationLatitude, t.StartStationLongitude))
				})

				// Odrzucamy przejazdy, które nie wpadły do żadnej z zdefiniowanych dzielnic
				.Where(x => x.FoundNeighborhood != null)

				// Grupujemy po nazwie znalezionej dzielnicy
				.GroupBy(x => x.FoundNeighborhood.Name)

				.Select(g => new
				{
					NeighborhoodName = g.Key,
					TopAgeStat = g.Select(x => x.Trip) // Wracamy do obiektu Trip
								  .GroupBy(t => t.BirthYear) // Grupujemy rocznikami
								  .Select(ageGroup => new
								  {
									  Age = currentYear - ageGroup.Key, 
									  TotalDuration = ageGroup.Sum(t => t.TripDuration) // Suma czasu dla tego wieku
								  })
								  .OrderByDescending(stat => stat.TotalDuration) 
								  .FirstOrDefault()
				})
				.ToList();

			if (result.Count == 0)
			{
				Console.WriteLine("3. Brak danych pasujących do zdefiniowanych dzielnic.");
				return;
			}

			Console.WriteLine("[3] Wiek użytkowników z największym czasem przejazdu w dzielnicach:");
			foreach (var r in result)
			{
				if (r.TopAgeStat != null)
				{
					Console.WriteLine($"   {r.NeighborhoodName}: Wiek {r.TopAgeStat.Age} lat (Łącznie: {r.TopAgeStat.TotalDuration:F0}s)");
				}
			}
		}

		public static void QueryLoopStations(List<Trip> trips)
		{
			var loops = trips
				.Where(t => t.StartStationId == t.EndStationId)
				.GroupBy(t => t.StartStationName)
				.Select(g => new
				{
					Station = g.Key,
					Count = g.Count(),
					AvgDuration = g.Average(t => t.TripDuration)
				})
				.OrderByDescending(x => x.Count)
				.Take(5)
				.ToList();

			if (loops.Count == 0)
			{
				Console.WriteLine("[4] Brak stacji do których klienci wracają.");
				return;
			}

			Console.WriteLine("[4] Stacje, do których najchętniej się wraca:");
			foreach (var l in loops)
			{
				Console.WriteLine($"   {l.Station}: {l.Count} pętli, avg {l.AvgDuration:F0}s");
			}
		}

		public static void QueryMostUsedBikes(List<Trip> trips)
		{
			var bikes = trips
				.GroupBy(t => t.BikeId)
				.Select(g => new
				{
					BikeId = g.Key,
					Trips = g.Count(),
					TotalHours = g.Sum(t => t.TripDuration) / 3600.0
				})
				.Where(x => x.Trips > 100)
				.OrderByDescending(x => x.TotalHours)
				.Take(5)
				.ToList();

			if (bikes.Count == 0)
			{
				Console.WriteLine("[5] Brak danych o rowerach.");
				return;
			}

			Console.WriteLine("[5] Najbardziej eksploatowane rowery:");
			foreach (var b in bikes)
			{
				Console.WriteLine($"   Rower {b.BikeId}: {b.Trips} przejazdów, {b.TotalHours:F1} h");
			}
		}
		public static List<Neighborhood> neighborhoods = new List<Neighborhood>
		{
			// --- MANHATTAN ---

			// 1. Financial District (Dolny Manhattan - poniżej Chambers St)
			new Neighborhood
			{
				Name = "Financial District",
				Borough = "Manhattan",
				Vertices = new List<(double, double)>
				{
					(40.7005, -74.0130), // Battery Park (Południe)
					(40.7130, -74.0160), // Okolice WTC (Zachód)
					(40.7130, -74.0000), // Pod Mostem Brooklińskim (Wschód)
					(40.7005, -74.0130)  // Zamknięcie pętli
				}
			},

			// 2. SoHo / Tribeca (Pomiędzy Canal St a Houston St)
			new Neighborhood
			{
				Name = "SoHo/Tribeca",
				Borough = "Manhattan",
				Vertices = new List<(double, double)>
				{
					(40.7130, -74.0160), // Canal St (Zachód)
					(40.7250, -74.0050), // Houston St (Zachód)
					(40.7220, -73.9940), // Houston St (Wschód)
					(40.7130, -74.0000)  // Canal St (Wschód)
				}
			},

			// 3. Midtown (Centrum biurowe - od 34th St do 59th St)
			new Neighborhood
			{
				Name = "Midtown",
				Borough = "Manhattan",
				Vertices = new List<(double, double)>
				{
					(40.7480, -74.0050), // 34th St / Hudson Yards (Zachód)
					(40.7680, -73.9900), // 59th St / Columbus Circle (Północny Zachód)
					(40.7600, -73.9650), // 59th St / Queensboro Bridge (Północny Wschód)
					(40.7420, -73.9700)  // 34th St / FDR Drive (Wschód)
				}
			},

			// 4. Central Park (Park i bezpośrednie okolice)
			new Neighborhood
			{
				Name = "Central Park Area",
				Borough = "Manhattan",
				Vertices = new List<(double, double)>
				{
					(40.7680, -73.9820), // Columbus Circle (Południowy Zachód)
					(40.8000, -73.9580), // Górny lewy róg parku (Północny Zachód)
					(40.7950, -73.9490), // Górny prawy róg parku (Północny Wschód)
					(40.7640, -73.9730)  // Plaza Hotel (Południowy Wschód)
				}
			},

			// --- BROOKLYN ---

			// 5. Williamsburg (Hipsterska dzielnica przy rzece)
			new Neighborhood
			{
				Name = "Williamsburg",
				Borough = "Brooklyn",
				Vertices = new List<(double, double)>
				{
					(40.7200, -73.9650), // Williamsburg Bridge (Południe)
					(40.7200, -73.9350), // W głąb Brooklynu (Wschód)
					(40.7050, -73.9350), // Południowa granica
					(40.7050, -73.9700)  // Nabrzeże (Zachód)
				}
			},

			// 6. DUMBO / Downtown Brooklyn (Przy mostach)
			new Neighborhood
			{
				Name = "DUMBO/Downtown BK",
				Borough = "Brooklyn",
				Vertices = new List<(double, double)>
				{
					(40.7050, -73.9950), // Brooklyn Bridge Park
					(40.7050, -73.9750), // Navy Yard
					(40.6850, -73.9750), // Atlantic Terminal
					(40.6850, -73.9950)  // Cobble Hill
				}
			}
		};
	}
	public class Neighborhood
	{
		public required string Name { get; set; }
		public required string Borough { get; set; }
		public required List<(double Lat, double Lon)> Vertices { get; set; } // Punkty graniczne
	}

	public static class GeoHelper
	{
		public static bool IsPointInPolygon(List<(double Lat, double Lon)> polygon, double testLat, double testLon)
		{
			bool result = false;
			int j = polygon.Count - 1;
			for (int i = 0; i < polygon.Count; i++)
			{
				if (polygon[i].Lat < testLat && polygon[j].Lat >= testLat || polygon[j].Lat < testLat && polygon[i].Lat >= testLat)
				{
					if (polygon[i].Lon + (testLat - polygon[i].Lat) / (polygon[j].Lat - polygon[i].Lat) * (polygon[j].Lon - polygon[i].Lon) < testLon)
					{
						result = !result;
					}
				}
				j = i;
			}
			return result;
		}
	}
	public class Trip
	{
		public int TripDuration { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime StopTime { get; set; }
		public required string StartStationId { get; set; }
		public required string StartStationName { get; set; }
		public double StartStationLatitude { get; set; }
		public double StartStationLongitude { get; set; }
		public required string EndStationId { get; set; }
		public required string EndStationName { get; set; }
		public double EndStationLatitude { get; set; }
		public double EndStationLongitude { get; set; }
		public required string BikeId { get; set; }
		public required string UserType { get; set; }
		public int? BirthYear { get; set; }
		public required string Gender { get; set; }
	}
}