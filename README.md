# # 🎾 TennisApp – Aplikacja do zarządzania zawodnikami tenisa

## 📋 Opis aplikacji

**TennisApp** to prosta aplikacja okienkowa stworzona w języku **C# (.NET Windows Forms)**, umożliwiająca:

- dodawanie zawodników do bazy danych PostgreSQL,
- przeglądanie listy zawodników,
- edytowanie i usuwanie zawodników z listy,
- Funkcje walidacyjne: (Imię i nazwisko nie mogą zawierać cyfr, Ranking musi być unikalny, Wymagane pola: imię, nazwisko, data urodzenia, kraj, ranking i wiele innych)
- Zmiany Trybu na ciemny
- Dodanie usunięcie i edytowanie meczu
- Dodawanie Państw i usuwanie ich
- Sortowanie zawodników i meczów wedlug kategori




Aplikacja wspiera podstawową obsługę GUI i łączy się z bazą danych PostgreSQL przy użyciu biblioteki **Npgsql**.

---

## 🗃️ Struktura bazy danych

# tennisdb

## Opis bazy danych

Baza danych służy do przechowywania informacji o krajach, zawodnikach, turniejach oraz meczach tenisowych 1 na 1.

---

## Struktura bazy danych

### Tabela `countries` — kraje

| Kolumna | Typ           | Opis                       |
|---------|---------------|----------------------------|
| id      | SERIAL PK     | Unikalny identyfikator kraju |
| name    | VARCHAR(100)  | Nazwa kraju (unikalna)     |

**Przykładowe dane:**

| id | name      |
|----|-----------|
| 1  | Polska    |
| 2  | Hiszpania |
| 3  | Francja   |
| 4  | USA       |
| 5  | Australia |

---

### Tabela `players` — zawodnicy

| Kolumna    | Typ           | Opis                                                |
|------------|---------------|----------------------------------------------------|
| id         | SERIAL PK     | Unikalny identyfikator zawodnika                    |
| first_name | VARCHAR(100)  | Imię zawodnika                                      |
| last_name  | VARCHAR(100)  | Nazwisko zawodnika                                  |
| birth_date | DATE          | Data urodzenia                                     |
| country_id | INTEGER       | Id kraju (FK do `countries.id`)                    |
| ranking    | INTEGER       | Ranking zawodnika (od 1 do 1000)                   |
| is_active  | BOOLEAN       | Status aktywności zawodnika (TRUE = aktywny)       |

**Przykładowy rekord:**

| id | first_name | last_name | birth_date | country_id | ranking | is_active |
|----|------------|-----------|------------|------------|---------|-----------|
| 1  | Iga        | Świątek   | 2001-05-31 | 1          | 1       | TRUE      |
| 2  | Olaf       | Slowik    | 2003-09-30 | 1          | 2       | TRUE      |

---

### Tabela `tournaments` — turnieje

| Kolumna   | Typ          | Opis                             |
|-----------|--------------|---------------------------------|
| id        | SERIAL PK    | Unikalny identyfikator turnieju |
| name      | VARCHAR(100) | Nazwa turnieju                  |
| location  | VARCHAR(100) | Lokalizacja turnieju            |
| start_date| DATE         | Data rozpoczęcia                |
| end_date  | DATE         | Data zakończenia                |

**Przykładowe rekordy:**

| id | name            | location | start_date | end_date   |
|----|-----------------|----------|------------|------------|
| 1  | Roland Garros   | Paryż    | 2025-05-20 | 2025-06-10 |
| 2  | turniejowy turniej | Paryż  | 2025-05-20 | 2025-06-10 |

---

### Tabela `matches` — mecze 1v1

| Kolumna       | Typ          | Opis                                               |
|---------------|--------------|---------------------------------------------------|
| id            | SERIAL PK    | Unikalny identyfikator meczu                       |
| tournament_id | INTEGER      | Id turnieju (FK do `tournaments.id`)              |
| player1_id    | INTEGER      | Id pierwszego zawodnika (FK do `players.id`)      |
| player2_id    | INTEGER      | Id drugiego zawodnika (FK do `players.id`)        |
| match_date    | DATE         | Data rozegrania meczu                              |
| winner_id     | INTEGER      | Id zwycięzcy meczu (FK do `players.id`, nullable)|
| score         | TEXT         | Wynik meczu (np. '6:3, 6:2')                      |

**Przykładowy rekord:**

| id | tournament_id | player1_id | player2_id | match_date | winner_id | score     |
|----|---------------|------------|------------|------------|-----------|-----------|
| 1  | 1             | 1          | 2          | 2025-05-21 | 1         | 6:3, 6:2  | 


## ⚙️ Konfiguracja i uruchomienie

### 1. Wymagania

-.NET Framework lub .NET 6+
- Visual Studio lub inny edytor C#
- PostgreSQL
- Biblioteka Npgsql (do komunikacji z bazą PostgreSQL)

### 2. Tworzenie bazy danych
- zaloguj się do PostreSQL i utwórz baze danych przy użyciu zalączonego pliku

### 3. Zainstaluj NpgSql
- W visual studio przejdz do Tools > NuGet Package Manager > Package Manager Console i zainstaluj NpgSql

