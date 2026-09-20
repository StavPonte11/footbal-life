using System;
using System.Collections.Generic;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Generates culturally authentic player names across common footballing nationalities.
    /// </summary>
    public static class RandomNameGenerator
    {
        public static readonly IReadOnlyList<string> SupportedNationalities = new[]
        {
            "England", "France", "Germany", "Spain", "Italy",
            "Brazil", "Argentina", "Netherlands", "Portugal", "USA"
        };

        private static readonly Dictionary<string, (string[] FirstNames, string[] LastNames)> NamePools = new(StringComparer.OrdinalIgnoreCase)
        {
            ["England"] = (
                new[] { "Jack", "Harry", "Oliver", "George", "Leo", "Charlie", "Freddie", "Alfie", "Marcus", "Mason", "Declan", "Cole", "Jude", "Kieran", "Callum" },
                new[] { "Sterling", "Walker", "Vance", "Bennett", "Cole", "Kane", "Shaw", "Palmer", "Rice", "Pickford", "Saka", "Mount", "Maddison", "Foden", "Bellingham" }
            ),
            ["France"] = (
                new[] { "Antoine", "Kylian", "Théo", "Lucas", "Adrien", "Aurélien", "Eduardo", "Hugo", "Kingsley", "Dayot", "William", "Randal", "Jules", "Brice", "Maxime" },
                new[] { "Mbappé", "Griezmann", "Hernandez", "Rabiot", "Tchouaméni", "Camavinga", "Lloris", "Coman", "Upamecano", "Saliba", "Kolo Muani", "Koundé", "Dembele", "Pavard", "Giroud" }
            ),
            ["Germany"] = (
                new[] { "Florian", "Jamal", "Joshua", "Kai", "Leon", "Leroy", "Thomas", "Toni", "Ilkay", "Maximilian", "Nico", "Lukas", "Felix", "David", "Julian" },
                new[] { "Wirtz", "Musiala", "Kimmich", "Havertz", "Goretzka", "Sané", "Müller", "Kroos", "Gündogan", "Mittelstädt", "Schlotterbeck", "Rüdiger", "Brandt", "Füllkrug", "Tah" }
            ),
            ["Spain"] = (
                new[] { "Lamine", "Pedri", "Gavi", "Ferran", "Rodri", "Nico", "Dani", "Álvaro", "Mikel", "Unai", "Alejandro", "Pau", "Aymeric", "Fabian", "Marco" },
                new[] { "Yamal", "Gonzalez", "Torres", "Hernandez", "Williams", "Olmo", "Morata", "Merino", "Simon", "Balde", "Cubarsí", "Laporte", "Ruiz", "Asensio", "Carvajal" }
            ),
            ["Italy"] = (
                new[] { "Federico", "Nicolo", "Gianluigi", "Alessandro", "Lorenzo", "Giacomo", "Mateo", "Davide", "Bryan", "Riccardo", "Gianluca", "Mattia", "Manuel", "Andrea", "Marco" },
                new[] { "Chiesa", "Barella", "Donnarumma", "Bastoni", "Pellegrini", "Raspadori", "Retegui", "Frattesi", "Cristante", "Calafiori", "Mancini", "Zaccagni", "Locatelli", "Dimarco", "Scamacca" }
            ),
            ["Brazil"] = (
                new[] { "Vinicius", "Rodrygo", "Endrick", "Gabriel", "Bruno", "Lucas", "Raphinha", "Richarlison", "Casemiro", "Douglas", "Ederson", "Alisson", "Marquinhos", "Danilo", "Bremer" },
                new[] { "Junior", "Goes", "Felipe", "Magalhães", "Guimarães", "Paquetá", "Dias", "Andrade", "Silva", "Luiz", "Santana", "Becker", "Correa", "Costa", "Ribeiro" }
            ),
            ["Argentina"] = (
                new[] { "Lionel", "Julian", "Lautaro", "Alexis", "Enzo", "Rodrigo", "Emiliano", "Cristian", "Nahuel", "Nicolas", "Alejandro", "Giovani", "Lisandro", "Leandro", "Exequiel" },
                new[] { "Messi", "Alvarez", "Martinez", "Mac Allister", "Fernandez", "De Paul", "Romero", "Molina", "Otamendi", "Garnacho", "Lo Celso", "Paredes", "Palacios", "Acuna", "Montiel" }
            ),
            ["Netherlands"] = (
                new[] { "Virgil", "Frenkie", "Memphis", "Cody", "Xavi", "Tijjani", "Nathan", "Denzel", "Bart", "Jeremie", "Stefan", "Marten", "Brian", "Teun", "Joey" },
                new[] { "van Dijk", "de Jong", "Depay", "Gakpo", "Simons", "Reijnders", "Aké", "Dumfries", "Verbruggen", "Frimpong", "de Vrij", "de Roon", "Brobbey", "Koopmeiners", "Veerman" }
            ),
            ["Portugal"] = (
                new[] { "Cristiano", "Bruno", "Bernardo", "Ruben", "Rafael", "Diogo", "Joao", "Goncalo", "Vitinha", "Nuno", "Pedro", "Antonio", "Francisco", "Danilo", "Nelson" },
                new[] { "Ronaldo", "Fernandes", "Silva", "Dias", "Leão", "Jota", "Felix", "Ramos", "Ferreira", "Mendes", "Neto", "Silva", "Conceicao", "Pereira", "Semedo" }
            ),
            ["USA"] = (
                new[] { "Christian", "Weston", "Tyler", "Timothy", "Folarin", "Gio", "Antonee", "Sergiño", "Chris", "Malik", "Yunus", "Matt", "Miles", "Ricardo", "Brenden" },
                new[] { "Pulisic", "McKennie", "Adams", "Weah", "Balogun", "Reyna", "Robinson", "Dest", "Richards", "Tillman", "Musah", "Turner", "Robinson", "Pepi", "Aaronson" }
            )
        };

        public static string GenerateFirstName(string nationality, SimulationRandom? random = null)
        {
            var rng = random ?? new SimulationRandom((int)DateTime.UtcNow.Ticks);
            if (!NamePools.TryGetValue(nationality, out var pool))
            {
                pool = NamePools["England"];
            }

            int index = rng.NextInt(0, pool.FirstNames.Length);
            return pool.FirstNames[index];
        }

        public static string GenerateLastName(string nationality, SimulationRandom? random = null)
        {
            var rng = random ?? new SimulationRandom((int)DateTime.UtcNow.Ticks + 1);
            if (!NamePools.TryGetValue(nationality, out var pool))
            {
                pool = NamePools["England"];
            }

            int index = rng.NextInt(0, pool.LastNames.Length);
            return pool.LastNames[index];
        }

        public static (string FirstName, string LastName) GenerateFullName(string nationality, SimulationRandom? random = null)
        {
            var rng = random ?? new SimulationRandom((int)DateTime.UtcNow.Ticks);
            if (!NamePools.TryGetValue(nationality, out var pool))
            {
                pool = NamePools["England"];
            }

            int fIdx = rng.NextInt(0, pool.FirstNames.Length);
            int lIdx = rng.NextInt(0, pool.LastNames.Length);
            return (pool.FirstNames[fIdx], pool.LastNames[lIdx]);
        }
    }
}
