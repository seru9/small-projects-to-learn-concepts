# 🚲 NYC City Bikes Analysis

Bardzo krótka aplikacja konsolowa w języku C# służąca do analizy danych z systemu rowerów miejskich **Citi Bike w Nowym Jorku**. Program przetwarza pliki CSV, wykonuje zaawansowane zapytania statystyczne oraz mapuje współrzędne GPS na konkretne dzielnice Nowego Jorku.


## 🛠 Technologia i Algorytmy

* **Język**: C# (.NET)
* **Przetwarzanie danych**: LINQ to Objects (efektywne filtrowanie i grupowanie danych).
* **Parsowanie**: Obsługa formatów `CultureInfo.InvariantCulture` dla poprawnego odczytu współrzędnych GPS i czasów trwania.

## 📁 Struktura Danych

Program oczekuje danych w następującej lokalizacji:
`{KatalogProgramu}/2014-citibike-tripdata/`

Wewnątrz powinny znajdować się foldery nazwane według wzoru `{numer_miesiąca}_`, np.:
* `1_January/`
* `2_February/`
    * `trip_data.csv`

---
