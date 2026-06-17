Project Management API

REST API stworzone w ramach projektu zaliczeniowego z przedmiotu Programowanie zaawansowane.

Aplikacja umożliwia zarządzanie projektami i zadaniami. Użytkownicy mogą tworzyć własne projekty, dodawać do nich zadania oraz przypisywać zadania innym zarejestrowanym użytkownikom.

Dostęp do endpointów zabezpieczony jest przy użyciu tokenów JWT.

Technologie

Projekt został wykonany z wykorzystaniem:

C#
.NET 8
ASP.NET Core Web API
Entity Framework Core
Microsoft ASP.NET Core Identity
JWT Bearer Authentication
SQL Server LocalDB
Swagger / OpenAPI
Główne funkcjonalności

Aplikacja umożliwia:

rejestrację nowych użytkowników,
logowanie i generowanie tokenu JWT,
tworzenie projektów,
wyświetlanie dostępnych projektów,
edytowanie i usuwanie własnych projektów,
tworzenie zadań w projektach,
przypisywanie zadań innym użytkownikom,
edytowanie statusu i danych zadania,
usuwanie zadań przez właściciela projektu,
kontrolowanie dostępu do zasobów na podstawie zalogowanego użytkownika.
Modele danych
ApplicationUser

Model użytkownika oparty jest na Microsoft Identity.

Użytkownik może:

posiadać wiele projektów,
mieć przypisanych wiele zadań.
Project

Projekt zawiera między innymi:

identyfikator,
nazwę,
opis,
datę utworzenia,
identyfikator właściciela,
listę zadań.

Każdy projekt należy do jednego użytkownika.

TaskItem

Zadanie zawiera między innymi:

identyfikator,
tytuł,
opis,
status wykonania,
termin realizacji,
datę utworzenia,
identyfikator projektu,
opcjonalnie identyfikator przypisanego użytkownika.

Każde zadanie należy do jednego projektu i może zostać przypisane do jednego użytkownika.

## Zasady dostępu

Zalogowany użytkownik widzi:

* projekty, których jest właścicielem,
* projekty, w których ma przypisane co najmniej jedno zadanie.

### Właściciel projektu może:

* przeglądać projekt,
* edytować projekt,
* usuwać projekt,
* tworzyć zadania,
* edytować zadania,
* przypisywać zadania użytkownikom,
* usuwać zadania.

### Użytkownik przypisany do zadania może:

* zobaczyć projekt, w którym znajduje się jego zadanie,
* zobaczyć przypisane zadanie,
* edytować dane i status swojego zadania.

### Użytkownik przypisany do zadania nie może:

* usuwać projektu,
* edytować projektu,
* tworzyć zadań w cudzym projekcie,
* usuwać zadań,
* zmieniać użytkownika przypisanego do zadania.

## Struktura projektu

```text
ProjectManagementApi
│
├── Controllers
│   ├── AuthController.cs
│   ├── ProjectsController.cs
│   └── TasksController.cs
│
├── Data
│   └── ApplicationDbContext.cs
│
├── DTOs
│   ├── Auth
│   ├── Projects
│   └── Tasks
│
├── Migrations
│
├── Models
│   ├── ApplicationUser.cs
│   ├── Project.cs
│   └── TaskItem.cs
│
├── Services
│   ├── ITokenService.cs
│   └── TokenService.cs
│
├── Program.cs
├── appsettings.json
└── ProjectManagementApi.csproj
```

## Endpointy uwierzytelniania

| Metoda | Endpoint             | Opis                            |
| ------ | -------------------- | ------------------------------- |
| POST   | `/api/Auth/register` | Rejestracja nowego użytkownika  |
| POST   | `/api/Auth/login`    | Logowanie i pobranie tokenu JWT |

## Endpointy projektów

Wszystkie endpointy projektów wymagają poprawnego tokenu JWT.

| Metoda | Endpoint             | Opis                                      |
| ------ | -------------------- | ----------------------------------------- |
| GET    | `/api/Projects`      | Pobiera projekty dostępne dla użytkownika |
| GET    | `/api/Projects/{id}` | Pobiera wybrany projekt                   |
| POST   | `/api/Projects`      | Tworzy nowy projekt                       |
| PUT    | `/api/Projects/{id}` | Aktualizuje projekt                       |
| DELETE | `/api/Projects/{id}` | Usuwa projekt                             |

## Endpointy zadań

Wszystkie endpointy zadań wymagają poprawnego tokenu JWT.

| Metoda | Endpoint                         | Opis                                     |
| ------ | -------------------------------- | ---------------------------------------- |
| GET    | `/api/Tasks`                     | Pobiera zadania dostępne dla użytkownika |
| GET    | `/api/Tasks/{id}`                | Pobiera wybrane zadanie                  |
| GET    | `/api/Tasks/project/{projectId}` | Pobiera zadania wskazanego projektu      |
| POST   | `/api/Tasks`                     | Tworzy nowe zadanie                      |
| PUT    | `/api/Tasks/{id}`                | Aktualizuje zadanie                      |
| DELETE | `/api/Tasks/{id}`                | Usuwa zadanie                            |


Wymagania

Do uruchomienia projektu potrzebne są:

Visual Studio 2022,
.NET 8 SDK,
SQL Server LocalDB,
dostęp do menedżera pakietów NuGet.
Konfiguracja JWT

Konfiguracja JWT znajduje się w pliku:

appsettings.json

Przykładowa konfiguracja:

"Jwt": {
  "Key": "CHANGE_THIS_TO_YOUR_OWN_SECRET_KEY_MINIMUM_32_CHARACTERS",
  "Issuer": "ProjectManagementApi",
  "Audience": "ProjectManagementApiUsers",
  "ExpiresInMinutes": 60
}

Przed uruchomieniem projektu należy ustawić własny klucz JWT o długości co najmniej 32 znaków.

W prawdziwej aplikacji klucz powinien być przechowywany poza repozytorium, na przykład w zmiennych środowiskowych lub User Secrets.

Uruchomienie projektu
Sklonuj repozytorium:
git clone https://github.com/michalkolodziejczak73/ProjectManagementApi.git
Otwórz projekt w Visual Studio.
Sprawdź konfigurację połączenia z bazą danych w pliku appsettings.json.

Domyślna konfiguracja:

"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ProjectManagementApiDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
Otwórz konsolę Menedżera pakietów NuGet:
Narzędzia → Menedżer pakietów NuGet → Konsola Menedżera pakietów
Utwórz lub zaktualizuj bazę danych:
Update-Database
Uruchom aplikację.

Po uruchomieniu powinna otworzyć się dokumentacja Swagger.

Przykładowy adres:

https://localhost:7044/swagger

Numer portu może być inny, zależnie od konfiguracji lokalnej.

Korzystanie z JWT w Swaggerze
Zarejestruj użytkownika przez endpoint:
POST /api/Auth/register
Zaloguj się przez:
POST /api/Auth/login
Skopiuj token zwrócony w polu token.
Kliknij przycisk Authorize na górze strony Swaggera.
Wklej sam token JWT, bez cudzysłowów i bez dopisywania słowa Bearer.
Zatwierdź przyciskiem Authorize.

Od tego momentu Swagger będzie automatycznie przesyłał token do zabezpieczonych endpointów.

Przykładowe konta testowe

Konta zostaną utworzone po wykonaniu rejestracji przez API.

Właściciel projektu
E-mail: michal@test.pl
Hasło: Test123
Użytkownik przypisany do zadania
E-mail: adam@test.pl
Hasło: Test123

Dane te służą wyłącznie do lokalnego testowania projektu.

Przykładowy scenariusz testowy
Zarejestruj użytkowników michal@test.pl i adam@test.pl.
Zaloguj się jako Michał.
Utwórz nowy projekt.
Utwórz zadanie i przypisz je do adam@test.pl.
Zaloguj się jako Adam.
Pobierz listę projektów.
Projekt Michała powinien być widoczny dla Adama, ponieważ Adam ma przypisane w nim zadanie.
Adam może edytować swoje zadanie, ale nie może usunąć projektu ani zadania.
Próba wykonania niedozwolonej operacji powinna zwrócić kod 403 Forbidden.
Obsługiwane kody HTTP

API wykorzystuje między innymi następujące kody odpowiedzi:

200 OK — operacja zakończona powodzeniem,
201 Created — zasób został utworzony,
204 No Content — zasób został zaktualizowany lub usunięty,
400 Bad Request — przesłane dane są nieprawidłowe,
401 Unauthorized — brak poprawnego tokenu JWT,
403 Forbidden — użytkownik nie ma uprawnień,
404 Not Found — zasób nie istnieje lub jest niedostępny,
409 Conflict — użytkownik o podanym adresie e-mail już istnieje,
500 Internal Server Error — nieoczekiwany błąd serwera.
Autor

Michał Kołodziejczak
