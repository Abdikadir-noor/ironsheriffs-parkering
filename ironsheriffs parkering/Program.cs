using System;
using System.IO;
using System.Text.Json;
using Spectre.Console;

class MainProgram
{
    static void Main(string[] args)
    {
        // Ladda konfiguration och prissättning
        Configuration config = LoadConfiguration();
        if (config == null) return;

        Pricing pricing = LoadPricing();
        if (pricing == null) return;

        ParkingGarage parkingGarage = new ParkingGarage(config.NumberOfSpaces);

        bool isRunning = true;

        while (isRunning)
        {
            Console.Clear();
            DisplayTitle();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold yellow]Välj ett alternativ:[/]")
                    .PageSize(10)
                    .AddChoices(new[]
                    {
                        "1. Parkera ett fordon",
                        "2. Flytta ett fordon",
                        "3. Hämta ett fordon",
                        "4. Sök efter ett fordon",
                        "5. Visa parkeringsstatus",
                        "6. Avsluta"
                    })
                    .HighlightStyle(new Style(Color.DarkMagenta, decoration: Decoration.Bold)));

            switch (choice)
            {
                case "1. Parkera ett fordon":
                    ParkVehicle(parkingGarage);
                    break;
                case "2. Flytta ett fordon":
                    MoveVehicle(parkingGarage);
                    break;
                case "3. Hämta ett fordon":
                    RetrieveVehicle(parkingGarage);
                    break;
                case "4. Sök efter ett fordon":
                    SearchVehicle(parkingGarage);
                    break;
                case "5. Visa parkeringsstatus":
                    parkingGarage.ShowStatus();
                    break;
                case "6. Avsluta":
                    isRunning = false;
                    AnsiConsole.MarkupLine("[red]Programmet stängs ner...[/]");
                    break;
                default:
                    AnsiConsole.MarkupLine("[red]Ogiltigt val, vänligen försök igen.[/]");
                    break;
            }
            if (isRunning)
            {
                AnsiConsole.MarkupLine("[grey]Tryck på valfri tangent för att fortsätta...[/]");
                Console.ReadKey();
            }
        }
    }

    // Metod för att visa titeln
    private static void DisplayTitle()
    {
        AnsiConsole.Markup("[bold magenta]IRON SHERIFF'S PARKERING[/]\n");
        AnsiConsole.WriteLine();
    }

    // Metod för att ladda konfiguration från JSON-fil, skapa den om den inte finns
    private static Configuration LoadConfiguration()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Config.json");
        AnsiConsole.MarkupLine($"[grey]Letar efter konfigurationsfil på: {path}[/]");

        try
        {
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<Configuration>(json);
            }
            else
            {
                var defaultConfig = new Configuration
                {
                    NumberOfSpaces = 100 // Standardantal platser
                };

                string defaultJson = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, defaultJson);

                AnsiConsole.MarkupLine("[yellow]Konfigurationsfil saknades, en ny har skapats med standardvärden.[/]");

                return defaultConfig;
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Fel vid inläsning eller skapande av konfigurationsfilen: {ex.Message}[/]");
            return null;
        }
    }

    // Metod för att ladda prissättning från JSON-fil, skapa den om den inte finns
    private static Pricing LoadPricing()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "pricelist.json");
        AnsiConsole.MarkupLine($"[grey]Letar efter prislista på: {path}[/]");

        try
        {
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<Pricing>(json);
            }
            else
            {
                var defaultPricing = new Pricing
                {
                    HourlyRateCar = 20,  // Standardpris för bilar
                    HourlyRateMC = 10,   // Standardpris för motorcyklar
                    HourlyRateBus = 30,  // Standardpris för bussar
                    HourlyRateBike = 5    // Standardpris för cyklar
                };

                string defaultJson = JsonSerializer.Serialize(defaultPricing, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, defaultJson);

                AnsiConsole.MarkupLine("[yellow]Prislista saknades, en ny har skapats med standardvärden.[/]");
                return defaultPricing;
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]fel vid inläsning eller skapande av prislistan: {ex.Message}[/]");
            return null;
        }
    }

    // Metod för att parkera ett fordon
    static void ParkVehicle(ParkingGarage parkingGarage)
    {
        AnsiConsole.Markup("[bold cyan]Ange fordonstyp (CAR/MC/BUS/BIKE): [/]");
        string vehicleType = Console.ReadLine().ToUpper();

        Vehicle vehicle = null;

        AnsiConsole.Markup("[bold cyan]Ange registreringsnummer: [/]");
        string regNr = Console.ReadLine().ToUpper();

        switch (vehicleType)
        {
            case "CAR":
                vehicle = new Car(regNr);
                break;
            case "MC":
                vehicle = new MC(regNr);
                break;
            case "BUS":
                vehicle = new Bus(regNr);
                break;
            case "BIKE":
                vehicle = new Bike(regNr);
                break;
            default:
                AnsiConsole.MarkupLine("[red]Ogiltig fordonstyp.[/]");
                return;
        }

        if (parkingGarage.ParkVehicle(vehicle))
        {
            AnsiConsole.MarkupLine("[green]Fordon parkerat på plats.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Inga lediga platser tillgängliga.[/]");
        }
    }

    // Metod för att hämta ett fordon
    private static void RetrieveVehicle(ParkingGarage parkingGarage)
    {
        AnsiConsole.Markup("[bold cyan]Ange registreringsnummer på fordonet som ska hämtas: [/]");
        string regNr = Console.ReadLine().ToUpper();

        Vehicle vehicle = parkingGarage.RetrieveVehicle(regNr);
        if (vehicle != null)
        {
            AnsiConsole.MarkupLine($"[green]Fordon med registreringsnummer {regNr} har hämtats.[/]");
            AnsiConsole.MarkupLine($"[green]Parkeringsavgift: {vehicle.CalculateParkingFee()} SEK[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Fordonet hittades inte.[/]");
        }
    }

    // Metod för att söka efter ett fordon
    private static void SearchVehicle(ParkingGarage parkingGarage)
    {
        AnsiConsole.Markup("[bold cyan]Ange registreringsnummer för att söka: [/]");
        string regNr = Console.ReadLine().ToUpper();

        Vehicle vehicle = parkingGarage.SearchVehicle(regNr);
        if (vehicle != null)
        {
            AnsiConsole.MarkupLine($"[green]Fordon med registreringsnummer {regNr} finns i garaget.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Fordonet hittades inte.[/]");
        }
    }

    // Metod för att flytta ett fordon
    private static void MoveVehicle(ParkingGarage parkingGarage)
    {
        AnsiConsole.Markup("[bold cyan]Ange registreringsnummer på fordonet som ska flyttas: [/]");
        string regNr = Console.ReadLine().ToUpper();

        if (parkingGarage.MoveVehicle(regNr))
        {
            AnsiConsole.MarkupLine($"[green]Fordon med registreringsnummer {regNr} har flyttats.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Fordonet hittades inte eller kan inte flyttas.[/]");
        }
    }
}

// Exempelklasser för Konfiguration och Prissättning
class Configuration
{
    public int NumberOfSpaces { get; set; }
}

class Pricing
{
    public int HourlyRateCar { get; set; }
    public int HourlyRateMC { get; set; }
    public int HourlyRateBus { get; set; }
    public int HourlyRateBike { get; set; }
}

// Placeholder-klasser för ParkeringsGarage, Fordon, Bil, Motorcykel, Buss och Cykel
class ParkingGarage
{
    public ParkingGarage(int spaces) { /* Initiera garaget med angivet antal platser */ }
    public bool ParkVehicle(Vehicle vehicle) => true;
    public Vehicle RetrieveVehicle(string regNr) => null;
    public Vehicle SearchVehicle(string regNr) => null;
    public bool MoveVehicle(string regNr) => true;
    public void ShowStatus() { /* Visa status för alla parkeringsplatser */ }
}

abstract class Vehicle
{
    public string RegistrationNumber { get; }
    public Vehicle(string regNr) => RegistrationNumber = regNr;
    public abstract int CalculateParkingFee();
}

class Car : Vehicle
{
    public Car(string regNr) : base(regNr) { }
    public override int CalculateParkingFee() => 100; // Exempelvärde
}

class MC : Vehicle
{
    public MC(string regNr) : base(regNr) { }
    public override int CalculateParkingFee() => 50; // Exempelvärde
}

class Bus : Vehicle
{
    public Bus(string regNr) : base(regNr) { }
    public override int CalculateParkingFee() => 30; // Exempelvärde
}

class Bike : Vehicle
{
    public Bike(string regNr) : base(regNr) { }
    public override int CalculateParkingFee() => 5; // Exempelvärde
}
