Calculator (WPF, .NET 9, SQLite)
Prosty kalkulator w WPF (.NET 9) z historią obliczeń (SQLite + EF Core) oraz modułem Waluty (kursy NBP + wybór najlepszego dnia do przewalutowania).

Funkcje
Kalkulator: + - * / , C, CE, ⌫, ±, .
– działa klawiatura (0–9, + - * /, Enter, Backspace, Esc)
– wklej całe wyrażenie (Ctrl+V), policz po =

Historia:
-automatyczny zapis każdego działania (także łańcuchów, np. 12+3 +5…)
-okno Historia z filtrem (po wyrażeniu, wyniku i dacie yyyy-MM-dd / yyyy-MM), limit wyświetlania: 500

Filtr historii – wydajność/UX: filtrowanie wykonywane jest po stronie bazy (SQLite), 
zwracamy ostatnie 500 pasujących rekordów. W UI zastosowano debounce 250 ms i min. 2 znaki,
żeby ograniczyć liczbę zapytań. Puste pole = ostatnie 500 bez filtra.

Waluty (NBP, tabela A – kurs średni):
-pobranie kursów dla waluty i zakresu dat, zapis do DB
-wybór najlepszego dnia: MAX (sprzedaż) / MIN (zakup)
-cache w DB → po pobraniu działa offline

Jak uruchomić (użytkownik końcowy)
1. Pobierz ZIP z Releases i rozpakuj do folderu z prawem zapisu (np. Pulpit).
2. Uruchom Calculator.App.exe. Przy pierwszym starcie utworzy się calculator.db (SQLite) obok .exe.
2. Moduł Waluty → wybierz walutę, zakres dat, kwotę → Policz.
Nie trzeba instalować .NET – paczka jest self-contained (Windows x64).

Architektura
Calculator.sln
├─ Calculator.App      (WPF, UI + composition root/DI)
├─ Calculator.Domain   (logika + kontrakty: ICalculatorEngine, IHistoryService, IExchange*)
└─ Calculator.Data     (EF Core, DbContext, encje, implementacje serwisów)

-Domain zawiera interfejsy i proste DTO (np. CalculationDto, FxPoint) oraz ExchangeAdvisor.
-Data implementuje kontrakty Domain na EF Core/SQLite (EfHistoryService, EfExchangeRateStore).
-App składa wszystko przez Dependency Injection (Microsoft.Extensions.*), uruchamia auto-migracje na starcie (db.Database.Migrate()).

Technologie / pakiety (kluczowe)
WPF (.NET 9), EF Core 9, SQLite
Microsoft.EntityFrameworkCore.Sqlite, Microsoft.EntityFrameworkCore.Design
Microsoft.Extensions.DependencyInjection / Configuration / Logging
(publikacja self-contained, single file; bez trymowania)

Baza danych
-Plik: calculator.db (obok .exe) – tworzy się automatycznie.
-Tabele:
  Calculations(Id, CreatedAtUtc, Expression, Result)
  ExchangeRates(Id, Currency, EffectiveDate, Rate, Source) unikalny indeks (Currency, EffectiveDate)
-Auto-migracje: uruchamiane na starcie aplikacji.

Moduł „Waluty”
-Źródło: NBP API (tabela A, kurs średni).
-Flow:
  najpierw czytamy z DB (cache),
  jeśli brakuje dni → dociągamy z NBP i upsert do DB,
  wybór najlepszego dnia: MAX (sprzedaję) / MIN (kupuję),
  wynik: data, kurs, kwota po przewalutowaniu.
-Działa offline, jeśli dane były wcześniej pobrane i zapisane.

Skróty & UX
-Klawiatura: cyfry, + - * /, . / ,, Enter (=), Backspace (⌫), Esc (C)
-Wklejanie: Ctrl+V (wklejone wyrażenie liczone po =)
-Historia – filtr: wpisz fragment wyrażenia/wyniku lub datę yyyy-MM-dd / yyyy-MM
(limit wyniku po filtrze: 500 ostatnich pasujących)

Budowanie / Publikacja (skrót)
-Visual Studio 2022 → Publish na projekcie Calculator.App:
  Folder, runtime win-x64, Self-contained = true
  Single file = true, Trimming = false
-Output: publish\win-x64\Calculator.App.exe (+ po starcie calculator.db)

Testy (dev)
-Calculator.Domain.Tests – testy jednostkowe kalkulatora (parsowanie, działania, błędy)
-Calculator.Data.Tests – testy integracyjne EF + SQLite (Save/GetLast/Search, limity, filtr dat)
-Uruchom z Test Explorer w VS.

Znane ograniczenia
-NBP nie publikuje tabel w weekendy/święta – puste dni w siatce to normalne.
-Przy bardzo starych zakresach NBP może zwrócić 404 (komunikat w UI).
-DB w katalogu aplikacji – jeśli brak uprawnień do zapisu, uruchom z folderu użytkownika.

Źródła
Kursy: NBP API (tabela A).
