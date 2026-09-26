import json

leagues = {
    "ENG_PL": "e1000000-0000-0000-0000-000000000001",
    "ENG_CH": "e1000000-0000-0000-0000-000000000011",
    "ENG_L1": "e1000000-0000-0000-0000-000000000012",
    "ESP_LL": "e1000000-0000-0000-0000-000000000002",
    "ESP_SD": "e1000000-0000-0000-0000-000000000021",
    "DEU_BL": "e1000000-0000-0000-0000-000000000003",
    "DEU_2B": "e1000000-0000-0000-0000-000000000031",
    "ITA_SA": "e1000000-0000-0000-0000-000000000004",
    "ITA_SB": "e1000000-0000-0000-0000-000000000041",
    "FRA_L1": "e1000000-0000-0000-0000-000000000005",
    "FRA_L2": "e1000000-0000-0000-0000-000000000051",
}

clubs_data = [
    # ENG Premier League (Tier 1)
    ("c1000000-0000-0000-0000-000000000001", "Arsenal", "ARS", leagues["ENG_PL"], 88, 5, "Possession", 3500000, 85000000, "Emirates Stadium", 60704, 5, "#EF0107", "#FFFFFF", "#023474", 3500000, 85, 88, "Stan Kroenke", "Balanced", 4, 1886, 13, 14, 2),
    ("c1000000-0000-0000-0000-000000000002", "Manchester City", "MCI", leagues["ENG_PL"], 94, 5, "Possession", 4800000, 150000000, "Etihad Stadium", 53400, 5, "#6CABDD", "#FFFFFF", "#1C2C5B", 4200000, 80, 95, "City Football Group", "Patient", 5, 1894, 10, 7, 1),
    ("c1000000-0000-0000-0000-000000000008", "Liverpool", "LIV", leagues["ENG_PL"], 91, 5, "HighPress", 4200000, 110000000, "Anfield", 61276, 5, "#C8102E", "#FFFFFF", "#00B2A9", 4500000, 95, 92, "Fenway Sports Group", "Patient", 4, 1892, 19, 8, 6),
    ("c1000000-0000-0000-0000-000000000009", "Chelsea", "CHE", leagues["ENG_PL"], 85, 5, "Possession", 3800000, 95000000, "Stamford Bridge", 40341, 4, "#034694", "#FFFFFF", "#DBA111", 3100000, 78, 85, "BlueCo", "Impatient", 5, 1905, 6, 8, 2),
    ("c1000000-0000-0000-0000-000000000010", "Manchester United", "MUN", leagues["ENG_PL"], 86, 5, "Counter", 4400000, 120000000, "Old Trafford", 74310, 5, "#DA291C", "#FBE122", "#000000", 5000000, 90, 89, "INEOS & Glazers", "Balanced", 4, 1878, 20, 13, 3),
    ("c1000000-0000-0000-0000-000000000011", "Tottenham Hotspur", "TOT", leagues["ENG_PL"], 83, 5, "HighPress", 3100000, 70000000, "Tottenham Hotspur Stadium", 62850, 5, "#132257", "#FFFFFF", "#132257", 2600000, 82, 82, "ENIC Group", "Balanced", 3, 1882, 2, 8, 0),
    ("c1000000-0000-0000-0000-000000000012", "Newcastle United", "NEW", leagues["ENG_PL"], 81, 4, "Counter", 2600000, 65000000, "St James' Park", 52305, 4, "#000000", "#FFFFFF", "#41B6E6", 2100000, 92, 80, "PIF", "Patient", 4, 1892, 4, 6, 0),
    ("c1000000-0000-0000-0000-000000000013", "Aston Villa", "AVL", leagues["ENG_PL"], 82, 4, "HighPress", 2700000, 55000000, "Villa Park", 42682, 4, "#670E36", "#95BFE5", "#FEE12B", 1800000, 86, 81, "V Sports", "Balanced", 4, 1874, 7, 7, 1),
    ("c1000000-0000-0000-0000-000000000014", "Brighton & Hove Albion", "BHA", leagues["ENG_PL"], 78, 4, "Possession", 1800000, 45000000, "Amex Stadium", 31800, 4, "#0057B8", "#FFFFFF", "#FFCD00", 1200000, 84, 75, "Tony Bloom", "Patient", 3, 1901, 0, 0, 0),
    ("c1000000-0000-0000-0000-000000000015", "West Ham United", "WHU", leagues["ENG_PL"], 77, 4, "Counter", 2100000, 40000000, "London Stadium", 62500, 4, "#7A263A", "#1BB1E7", "#F3D446", 1600000, 88, 76, "David Sullivan", "Balanced", 3, 1895, 0, 3, 1),
    ("c1000000-0000-0000-0000-000000000016", "Everton", "EVE", leagues["ENG_PL"], 74, 3, "Direct", 1900000, 30000000, "Goodison Park", 39572, 3, "#003399", "#FFFFFF", "#F3D446", 1500000, 94, 72, "The Friedkin Group", "Balanced", 3, 1878, 9, 5, 1),
    ("c1000000-0000-0000-0000-000000000017", "Wolverhampton Wanderers", "WOL", leagues["ENG_PL"], 73, 3, "Counter", 1600000, 28000000, "Molineux", 32050, 3, "#FDB913", "#231F20", "#FFFFFF", 1100000, 85, 71, "Fosun International", "Patient", 3, 1877, 3, 4, 0),

    # ENG EFL Championship (Tier 2)
    ("c1000000-0000-0000-0000-000000000018", "Leeds United", "LEE", leagues["ENG_CH"], 72, 4, "HighPress", 1400000, 22000000, "Elland Road", 37890, 4, "#FFFFFF", "#0000FF", "#FFFF00", 1400000, 93, 76, "49ers Enterprises", "Balanced", 4, 1919, 3, 1, 0),
    ("c1000000-0000-0000-0000-000000000019", "Leicester City", "LEI", leagues["ENG_CH"], 71, 4, "Possession", 1350000, 20000000, "King Power Stadium", 32261, 4, "#003090", "#FFFFFF", "#FDBE11", 1250000, 89, 75, "King Power", "Patient", 3, 1884, 1, 1, 0),
    ("c1000000-0000-0000-0000-000000000020", "Southampton", "SOU", leagues["ENG_CH"], 69, 4, "Possession", 1100000, 16000000, "St Mary's Stadium", 32384, 4, "#D71920", "#FFFFFF", "#130C0E", 950000, 86, 72, "Sport Republic", "Patient", 3, 1885, 0, 1, 0),
    ("c1000000-0000-0000-0000-000000000021", "Sunderland", "SUN", leagues["ENG_CH"], 67, 3, "HighPress", 850000, 12000000, "Stadium of Light", 49000, 4, "#EB172B", "#FFFFFF", "#000000", 1100000, 96, 73, "Kyril Louis-Dreyfus", "Balanced", 3, 1879, 6, 2, 0),
    ("c1000000-0000-0000-0000-000000000022", "Norwich City", "NOR", leagues["ENG_CH"], 66, 3, "Possession", 800000, 10000000, "Carrow Road", 27244, 3, "#FFF200", "#00A651", "#FFFFFF", 800000, 88, 69, "Delia Smith & Joint", "Balanced", 2, 1902, 0, 0, 0),
    ("c1000000-0000-0000-0000-000000000023", "Middlesbrough", "MID", leagues["ENG_CH"], 65, 3, "Direct", 750000, 9000000, "Riverside Stadium", 34742, 3, "#E00000", "#FFFFFF", "#002060", 750000, 87, 68, "Steve Gibson", "Patient", 3, 1876, 0, 0, 0),
    ("c1000000-0000-0000-0000-000000000024", "West Bromwich Albion", "WBA", leagues["ENG_CH"], 65, 3, "Counter", 720000, 8500000, "The Hawthorns", 26850, 3, "#091442", "#FFFFFF", "#00A3E0", 720000, 85, 68, "Shilen Patel", "Balanced", 3, 1878, 1, 5, 0),
    ("c1000000-0000-0000-0000-000000000025", "Watford", "WAT", leagues["ENG_CH"], 64, 3, "Direct", 680000, 8000000, "Vicarage Road", 22200, 3, "#FBEE23", "#ED2127", "#111111", 620000, 82, 67, "Gino Pozzo", "Impatient", 3, 1881, 0, 0, 0),

    # ENG EFL League One (Tier 3)
    ("c1000000-0000-0000-0000-000000000026", "Bolton Wanderers", "BOL", leagues["ENG_L1"], 58, 3, "Direct", 320000, 3000000, "Toughsheet Community Stadium", 28723, 3, "#FFFFFF", "#1E2B37", "#EE1C25", 550000, 88, 62, "Football Ventures", "Patient", 2, 1874, 0, 4, 0),
    ("c1000000-0000-0000-0000-000000000027", "Portsmouth", "POR", leagues["ENG_L1"], 57, 2, "Counter", 310000, 2800000, "Fratton Park", 20688, 2, "#001489", "#FFFFFF", "#C8102E", 600000, 95, 63, "Tornante Company", "Patient", 2, 1898, 2, 2, 0),
    ("c1000000-0000-0000-0000-000000000028", "Derby County", "DER", leagues["ENG_L1"], 59, 3, "Possession", 350000, 3500000, "Pride Park", 33597, 3, "#FFFFFF", "#000000", "#1C242B", 650000, 92, 64, "David Clowes", "Balanced", 2, 1884, 2, 1, 0),
    ("c1000000-0000-0000-0000-000000000029", "Reading", "REA", leagues["ENG_L1"], 54, 2, "Counter", 260000, 2000000, "Select Car Leasing Stadium", 24161, 2, "#004494", "#FFFFFF", "#E30613", 450000, 84, 58, "Redwood Group", "Balanced", 2, 1871, 0, 0, 0),

    # ESP La Liga (Tier 1)
    ("c1000000-0000-0000-0000-000000000003", "Real Madrid", "RMA", leagues["ESP_LL"], 97, 5, "Direct", 5200000, 120000000, "Santiago Bernabeu", 81044, 5, "#FFFFFF", "#410099", "#FEBE10", 6000000, 90, 98, "Florentino Perez", "Impatient", 5, 1902, 36, 20, 15),
    ("c1000000-0000-0000-0000-000000000004", "Barcelona", "FCB", leagues["ESP_LL"], 93, 5, "Possession", 4600000, 75000000, "Camp Nou", 99354, 5, "#004D98", "#A50044", "#EDBB00", 5500000, 94, 94, "Joan Laporta", "Balanced", 4, 1899, 27, 31, 5),
    ("c1000000-0000-0000-0000-000000000030", "Atletico Madrid", "ATM", leagues["ESP_LL"], 89, 5, "Counter", 3600000, 80000000, "Metropolitano", 70460, 5, "#CB3524", "#FFFFFF", "#272E61", 3200000, 95, 88, "Enrique Cerezo", "Patient", 4, 1903, 11, 10, 3),
    ("c1000000-0000-0000-0000-000000000031", "Athletic Bilbao", "ATH", leagues["ESP_LL"], 82, 4, "HighPress", 2300000, 45000000, "San Mames", 53289, 5, "#EE2524", "#FFFFFF", "#000000", 2100000, 98, 83, "Jon Uriarte", "Patient", 3, 1898, 8, 24, 0),
    ("c1000000-0000-0000-0000-000000000032", "Real Sociedad", "RSO", leagues["ESP_LL"], 81, 4, "Possession", 2100000, 40000000, "Reale Arena", 39313, 4, "#0055A5", "#FFFFFF", "#000000", 1700000, 91, 80, "Jokin Aperribay", "Patient", 3, 1909, 2, 3, 0),
    ("c1000000-0000-0000-0000-000000000033", "Real Betis", "BET", leagues["ESP_LL"], 80, 4, "Possession", 2000000, 38000000, "Benito Villamarin", 60721, 4, "#0BB364", "#FFFFFF", "#000000", 2400000, 96, 79, "Angel Haro", "Balanced", 3, 1907, 1, 3, 0),
    ("c1000000-0000-0000-0000-000000000034", "Villarreal", "VIL", leagues["ESP_LL"], 79, 4, "Counter", 1900000, 35000000, "Estadio de la Ceramica", 23500, 4, "#FFF000", "#0055A5", "#FFFFFF", 950000, 89, 78, "Fernando Roig", "Patient", 3, 1923, 0, 0, 1),
    ("c1000000-0000-0000-0000-000000000035", "Sevilla", "SEV", leagues["ESP_LL"], 78, 4, "Direct", 1950000, 34000000, "Ramon Sanchez-Pizjuan", 43883, 4, "#FFFFFF", "#D40026", "#000000", 2200000, 92, 82, "Jose Maria del Nido", "Impatient", 3, 1890, 1, 5, 7),

    # ESP Segunda División (Tier 2)
    ("c1000000-0000-0000-0000-000000000036", "Espanyol", "ESP", leagues["ESP_SD"], 71, 4, "Direct", 950000, 14000000, "Stage Front Stadium", 40000, 4, "#007FC8", "#FFFFFF", "#ED1C24", 1000000, 87, 72, "Rastar Group", "Balanced", 3, 1900, 0, 4, 0),
    ("c1000000-0000-0000-0000-000000000037", "Real Zaragoza", "ZAR", leagues["ESP_SD"], 66, 3, "Counter", 650000, 8000000, "La Romareda", 33608, 3, "#FFFFFF", "#003399", "#FFCC00", 850000, 91, 68, "Jorge Mas", "Balanced", 3, 1932, 0, 6, 1),
    ("c1000000-0000-0000-0000-000000000038", "Sporting Gijón", "GIJ", leagues["ESP_SD"], 64, 3, "HighPress", 580000, 6500000, "El Molinon", 30000, 3, "#E20613", "#FFFFFF", "#003399", 750000, 93, 67, "Grupo Orlegi", "Patient", 2, 1905, 0, 0, 0),
    ("c1000000-0000-0000-0000-000000000039", "Levante", "LEV", leagues["ESP_SD"], 65, 3, "Counter", 620000, 7500000, "Ciutat de Valencia", 26354, 3, "#003366", "#990000", "#FFFFFF", 680000, 85, 66, "Danvila & Board", "Balanced", 2, 1909, 0, 0, 0),

    # DEU Bundesliga (Tier 1)
    ("c1000000-0000-0000-0000-000000000005", "Bayern Munich", "BAY", leagues["DEU_BL"], 95, 5, "Possession", 5000000, 130000000, "Allianz Arena", 75024, 5, "#DC052D", "#FFFFFF", "#0066B2", 5200000, 92, 97, "Herbert Hainer", "Impatient", 5, 1900, 33, 20, 6),
    ("c1000000-0000-0000-0000-000000000006", "Borussia Dortmund", "BVB", leagues["DEU_BL"], 87, 5, "HighPress", 3200000, 75000000, "Signal Iduna Park", 81365, 5, "#FDE100", "#000000", "#FFFFFF", 4800000, 98, 86, "Hans-Joachim Watzke", "Balanced", 4, 1909, 8, 5, 1),
    ("c1000000-0000-0000-0000-000000000040", "Bayer Leverkusen", "B04", leagues["DEU_BL"], 88, 5, "Possession", 3100000, 70000000, "BayArena", 30210, 5, "#E32219", "#000000", "#F6A800", 2200000, 89, 87, "Fernando Carro", "Patient", 4, 1904, 1, 2, 1),
    ("c1000000-0000-0000-0000-000000000041", "RB Leipzig", "RBL", leagues["DEU_BL"], 84, 5, "HighPress", 2900000, 60000000, "Red Bull Arena", 47069, 5, "#FFFFFF", "#E30613", "#FFCC00", 1800000, 75, 83, "Oliver Mintzlaff", "Balanced", 4, 2009, 0, 2, 0),
    ("c1000000-0000-0000-0000-000000000042", "Eintracht Frankfurt", "SGE", leagues["DEU_BL"], 81, 4, "Counter", 2100000, 42000000, "Deutsche Bank Park", 58000, 4, "#E1000F", "#000000", "#FFFFFF", 2500000, 97, 81, "Peter Fischer", "Balanced", 3, 1899, 1, 5, 2),
    ("c1000000-0000-0000-0000-000000000043", "VfB Stuttgart", "VFB", leagues["DEU_BL"], 80, 4, "HighPress", 1900000, 38000000, "MHP Arena", 60449, 4, "#FFFFFF", "#E32219", "#000000", 2300000, 94, 79, "Claus Vogt", "Balanced", 3, 1893, 5, 3, 0),

    # DEU 2. Bundesliga (Tier 2)
    ("c1000000-0000-0000-0000-000000000044", "Hamburger SV", "HSV", leagues["DEU_2B"], 71, 4, "Possession", 1100000, 16000000, "Volksparkstadion", 57000, 4, "#0000FF", "#FFFFFF", "#000000", 2800000, 96, 75, "Jonas Boldt", "Balanced", 3, 1887, 6, 3, 1),
    ("c1000000-0000-0000-0000-000000000045", "Schalke 04", "S04", leagues["DEU_2B"], 70, 4, "Direct", 1000000, 14000000, "Veltins-Arena", 62271, 4, "#004D9D", "#FFFFFF", "#000000", 3000000, 98, 76, "Matthias Tillmann", "Impatient", 3, 1904, 7, 5, 1),
    ("c1000000-0000-0000-0000-000000000046", "Hertha BSC", "BSC", leagues["DEU_2B"], 68, 4, "Counter", 850000, 11000000, "Olympiastadion Berlin", 74475, 4, "#005CA9", "#FFFFFF", "#E30613", 1900000, 86, 70, "Kay Bernstein Board", "Balanced", 3, 1892, 2, 0, 0),
    ("c1000000-0000-0000-0000-000000000047", "1. FC Nürnberg", "FCN", leagues["DEU_2B"], 65, 3, "Direct", 650000, 7500000, "Max-Morlock-Stadion", 50000, 3, "#8B0000", "#FFFFFF", "#000000", 1400000, 91, 67, "Peter Hammer", "Patient", 2, 1900, 9, 4, 0),

    # ITA Serie A (Tier 1)
    ("c1000000-0000-0000-0000-000000000048", "Inter Milan", "INT", leagues["ITA_SA"], 92, 5, "Possession", 4100000, 90000000, "San Siro", 75923, 5, "#001EA0", "#000000", "#F5A623", 4400000, 93, 93, "Oaktree Capital", "Balanced", 4, 1908, 20, 9, 3),
    ("c1000000-0000-0000-0000-000000000049", "AC Milan", "ACM", leagues["ITA_SA"], 88, 5, "HighPress", 3400000, 80000000, "San Siro", 75923, 5, "#FB090B", "#000000", "#FFFFFF", 4200000, 92, 89, "RedBird Capital", "Patient", 4, 1899, 19, 5, 7),
    ("c1000000-0000-0000-0000-000000000050", "Juventus", "JUV", leagues["ITA_SA"], 89, 5, "Counter", 3800000, 85000000, "Allianz Stadium", 41507, 5, "#000000", "#FFFFFF", "#D4AF37", 4600000, 90, 91, "Exor & Elkann", "Balanced", 4, 1897, 36, 15, 2),
    ("c1000000-0000-0000-0000-000000000051", "Napoli", "NAP", leagues["ITA_SA"], 85, 4, "HighPress", 2800000, 65000000, "Diego Armando Maradona", 54726, 4, "#0080FF", "#FFFFFF", "#000000", 3100000, 97, 86, "Aurelio De Laurentiis", "Impatient", 3, 1926, 3, 6, 1),
    ("c1000000-0000-0000-0000-000000000052", "AS Roma", "ASR", leagues["ITA_SA"], 83, 4, "Counter", 2600000, 50000000, "Stadio Olimpico", 70634, 4, "#8E1F2F", "#F1B434", "#FFFFFF", 2800000, 95, 84, "Friedkin Group", "Balanced", 3, 1927, 3, 9, 1),
    ("c1000000-0000-0000-0000-000000000053", "Lazio", "LAZ", leagues["ITA_SA"], 81, 4, "Possession", 2200000, 42000000, "Stadio Olimpico", 70634, 4, "#87CEEB", "#FFFFFF", "#000000", 2200000, 91, 82, "Claudio Lotito", "Balanced", 3, 1900, 2, 7, 1),

    # ITA Serie B (Tier 2)
    ("c1000000-0000-0000-0000-000000000054", "Sampdoria", "SAM", leagues["ITA_SB"], 69, 3, "Direct", 850000, 11000000, "Luigi Ferraris", 36599, 3, "#003399", "#FFFFFF", "#CC0000", 1600000, 95, 72, "Andrea Radrizzani", "Balanced", 3, 1946, 1, 4, 1),
    ("c1000000-0000-0000-0000-000000000055", "Palermo", "PAL", leagues["ITA_SB"], 68, 3, "Possession", 800000, 10000000, "Renzo Barbera", 36365, 3, "#F5A3B7", "#000000", "#D4AF37", 1400000, 93, 71, "City Football Group", "Patient", 3, 1900, 0, 0, 0),
    ("c1000000-0000-0000-0000-000000000056", "Bari", "BAR", leagues["ITA_SB"], 65, 3, "Counter", 600000, 7000000, "San Nicola", 58270, 3, "#FFFFFF", "#CC0000", "#000000", 1200000, 90, 67, "Filmauro Group", "Balanced", 2, 1908, 0, 0, 0),
    ("c1000000-0000-0000-0000-000000000057", "Parma", "PAR", leagues["ITA_SB"], 68, 3, "HighPress", 780000, 9500000, "Ennio Tardini", 22352, 3, "#FFF000", "#003399", "#FFFFFF", 900000, 88, 70, "Krause Group", "Patient", 3, 1913, 0, 3, 2),

    # FRA Ligue 1 (Tier 1)
    ("c1000000-0000-0000-0000-000000000007", "Paris Saint-Germain", "PSG", leagues["FRA_L1"], 95, 5, "Possession", 5500000, 160000000, "Parc des Princes", 47929, 5, "#004170", "#DA291C", "#FFFFFF", 4800000, 85, 96, "QSI & Nasser Al-Khelaifi", "Impatient", 5, 1970, 12, 15, 0),
    ("c1000000-0000-0000-0000-000000000058", "Olympique Marseille", "OM", leagues["FRA_L1"], 83, 4, "HighPress", 2700000, 52000000, "Orange Velodrome", 67394, 5, "#00A3E0", "#FFFFFF", "#DAA520", 3500000, 98, 87, "Frank McCourt", "Impatient", 4, 1899, 9, 10, 1),
    ("c1000000-0000-0000-0000-000000000059", "AS Monaco", "ASM", leagues["FRA_L1"], 82, 4, "Counter", 2400000, 48000000, "Stade Louis II", 18523, 4, "#ED1C24", "#FFFFFF", "#D4AF37", 1200000, 72, 80, "Dmitry Rybolovlev", "Balanced", 3, 1924, 8, 5, 0),
    ("c1000000-0000-0000-0000-000000000060", "Olympique Lyonnais", "OL", leagues["FRA_L1"], 80, 4, "Possession", 2200000, 40000000, "Groupama Stadium", 59186, 5, "#FFFFFF", "#002060", "#DA291C", 2600000, 90, 81, "Eagle Football", "Balanced", 3, 1950, 7, 5, 0),
    ("c1000000-0000-0000-0000-000000000061", "Lille OSC", "LOSC", leagues["FRA_L1"], 79, 4, "Counter", 1900000, 36000000, "Decathlon Arena", 50186, 4, "#E2001A", "#001D38", "#FFFFFF", 1600000, 88, 78, "Merlyn Partners", "Patient", 3, 1944, 4, 6, 0),
    ("c1000000-0000-0000-0000-000000000062", "OGC Nice", "NIC", leagues["FRA_L1"], 78, 4, "HighPress", 1800000, 34000000, "Allianz Riviera", 36178, 4, "#E2001A", "#000000", "#FFFFFF", 1400000, 86, 76, "INEOS", "Balanced", 3, 1904, 4, 3, 0),

    # FRA Ligue 2 (Tier 2)
    ("c1000000-0000-0000-0000-000000000063", "Girondins Bordeaux", "FCGB", leagues["FRA_L2"], 68, 3, "Direct", 800000, 10000000, "Matmut Atlantique", 42115, 4, "#001B44", "#FFFFFF", "#9E2A2B", 1800000, 92, 73, "Gerard Lopez", "Impatient", 3, 1881, 6, 4, 0),
    ("c1000000-0000-0000-0000-000000000064", "AS Saint-Etienne", "ASSE", leagues["FRA_L2"], 69, 4, "HighPress", 850000, 11000000, "Stade Geoffroy-Guichard", 41965, 4, "#00853F", "#FFFFFF", "#000000", 2200000, 97, 75, "Kilmer Sports", "Balanced", 3, 1919, 10, 6, 0),
    ("c1000000-0000-0000-0000-000000000065", "AJ Auxerre", "AJA", leagues["FRA_L2"], 66, 3, "Counter", 650000, 8000000, "Stade de l'Abbe-Deschamps", 18541, 3, "#FFFFFF", "#003399", "#000000", 900000, 88, 68, "James Zhou", "Patient", 2, 1905, 1, 4, 0),
    ("c1000000-0000-0000-0000-000000000066", "Angers SCO", "SCO", leagues["FRA_L2"], 64, 3, "Direct", 550000, 6500000, "Stade Raymond Kopa", 18752, 3, "#FFFFFF", "#000000", "#E30613", 700000, 84, 65, "Said Chabane", "Balanced", 2, 1919, 0, 0, 0),
]

clubs_list = []
for c in clubs_data:
    cid, name, short, lid, rep, fac, tac, wage, transfer, s_name, s_cap, s_pitch, kit_p, kit_s, kit_t, fan_count, fan_loy, fan_exp, owner, patience, ambition, f_year, l_titles, d_cups, c_trophies = c
    club_obj = {
        "id": cid,
        "name": name,
        "shortName": short,
        "leagueId": lid,
        "reputationRating": rep,
        "facilityRating": fac,
        "tacticalStyle": tac,
        "finances": {
            "weeklyWageBudget": wage,
            "transferBudget": transfer
        },
        "stadium": {
            "name": s_name,
            "capacity": s_cap,
            "pitchQuality": s_pitch
        },
        "visuals": {
            "logoAssetKey": f"badges/{short.lower()}",
            "homeKit": { "primaryHex": kit_p, "secondaryHex": kit_s, "trimHex": kit_t },
            "awayKit": { "primaryHex": kit_s, "secondaryHex": kit_p, "trimHex": kit_t }
        },
        "fanbase": {
            "supporterCount": fan_count,
            "loyaltyRating": fan_loy,
            "expectationRating": fan_exp
        },
        "board": {
            "ownerName": owner,
            "patience": patience,
            "financialAmbition": ambition
        },
        "history": {
            "foundedYear": f_year,
            "leagueTitles": l_titles,
            "domesticCups": d_cups,
            "continentalTrophies": c_trophies
        }
    }
    clubs_list.append(club_obj)

output = {
    "$schema": "./schema/clubs.schema.json",
    "clubs": clubs_list
}

with open("content/data/clubs.json", "w") as f:
    json.dump(output, f, indent=2)

print(f"Generated {len(clubs_list)} clubs in content/data/clubs.json successfully!")
