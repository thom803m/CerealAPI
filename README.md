CerealAPI er en RESTful API skrevet i C# (.NET 7) til håndtering af morgenmadsprodukter. API’et bruger JWT til autentifikation, understøtter admin-roller og giver mulighed for filtrering og sortering af produkter via query-parametre. Koden er opdelt i flere klasser med hvert deres ansvar:

Cereal: Repræsenterer et morgenmadsprodukt med egenskaber som navn, producent, type, næringsindhold, vægt, kopper pr. portion, rating og en valgfri sti til billede (ImagePath).

User: Repræsenterer en bruger med brugernavn, hashed password og rolle (admin eller user). Admin-brugere kan oprette, opdatere og slette produkter.

SeedService: Loader produkter fra en CSV-fil (Cereal.csv) ved opstart, hvis databasen er tom, og mapper CSV-kolonner til Cereal properties via CerealMap.

CerealContext: DbContext der håndterer Cereals og Users. Indeholder unikke indexer på Name og Username.

AuthController: Håndterer login via POST /api/auth/login. Modtager LoginRequest med username og password, returnerer LoginResponse med JWT-token og udløbstid.

CerealsController: Håndterer CRUD-operationer for cereals:
- GET /api/cereals – Hent alle produkter med mulighed for filtrering på producent (mfr), kalorier (caloriesMin, caloriesMax), sukker (sugarsMin, sugarsMax) og sortering (sort).
- GET /api/cereals/{id} – Hent et enkelt produkt efter ID.
- POST /api/cereals [admin] – Opret nyt produkt.
- PUT /api/cereals/{id} [admin] – Opdater eksisterende produkt.
- DELETE /api/cereals/{id} [admin] – Slet produkt.

DTO’er:
- LoginRequest: indeholder Username og Password.
- LoginResponse: indeholder JWT Token og Expires tid.

Autentifikation & Roller:
- Admin-brugere kan udføre alle CRUD-operationer.
- Almindelige brugere kan kun læse produkter.
- JWT-token indsættes via Swagger UI eller som Authorization header.

Swagger: API dokumenteres automatisk via Swagger UI med operation filter, der viser hvilke endpoints der kræver autentifikation og hvilken rolle der kræves.

Program.cs: Konfigurerer DbContext, JWT-auth, Swagger, SeedService og middleware til HTTPS, autentifikation og autorisation.

API’et understøtter: filtrering, sortering, autentifikation, autorisation, og indlæsning af seed data ved opstart.
