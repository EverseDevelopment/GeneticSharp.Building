using Newtonsoft.Json;
using GeneticSharp;


namespace BuildingDesignGA
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Genetic Algorithm - Building Design with 3D Box Floors");

            var selection = new EliteSelection();
            var crossover = new UniformCrossover();
            var mutation = new TworsMutation();
            var population = new Population(50, 100, new BuildingChromosome(10));

            var fitness = new BuildingFitness(60, 100000); // 50 meters max height, $100,000 max cost
            var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);

            ga.Termination = new FitnessStagnationTermination(100);
            ga.Start();

            // Output the best solution
            var bestChromosome = ga.BestChromosome as BuildingChromosome;
            var bestBuilding = CreateBuildingFromChromosome(bestChromosome);

            foreach (var floor in bestBuilding.Floors)
            {
                Console.WriteLine($"Floor - Base Vertices: {string.Join(", ", floor.GetBaseVertices())}, Height: {floor.Height}");
            }

            // Save the building to a JSON file
            SaveBuildingToJson(bestBuilding, "C:\\Users\\User\\Desktop\\building_design.json");
        }

        // Method to save the building data as JSON to a file
        private static void SaveBuildingToJson(Building building, string filePath)
        {
            var json = JsonConvert.SerializeObject(building, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath, json);
            Console.WriteLine($"Building design saved to {filePath}");
        }

        private static Building CreateBuildingFromChromosome(BuildingChromosome chromosome)
        {
            var building = new Building();
            foreach (var gene in chromosome.GetGenes())
            {
                building.Floors.Add((Floor)gene.Value);
            }
            return building;
        }
    }

    // Floor class representing a 3D box with 4 base vertices and a height
    public class Floor
    {
        public double Height { get; set; }
        public List<(double x, double y)> BaseVertices { get; set; } // 4 points representing the rectangle's base
        public double Cost { get; set; }

        // Returns the base vertices as a list of coordinates
        public List<(double x, double y)> GetBaseVertices()
        {
            return BaseVertices;
        }

        // Generate a random floor with base vertices as a rectangle
        public static Floor GenerateRandomFloor()
        {
            var random = new Random();
            double width = random.NextDouble() * 10 + 5; // Random width between 5 and 15 meters
            double depth = random.NextDouble() * 10 + 5; // Random depth between 5 and 15 meters
            double height = random.NextDouble() * 4 + 2; // Random height between 2 and 6 meters

            // Base rectangle vertices (randomly positioned around origin)
            var baseVertices = new List<(double x, double y)>
            {
                (0, 0),
                (width, 0),
                (width, depth),
                (0, depth)
            };

            double cost = width * depth * height * 100; // Simple cost function based on volume

            return new Floor
            {
                Height = height,
                BaseVertices = baseVertices,
                Cost = cost
            };
        }
    }

    public class Building
    {
        public List<Floor> Floors { get; set; }

        public double TotalHeight => Floors.Sum(f => f.Height);
        public double TotalCost => Floors.Sum(f => f.Cost);

        public Building()
        {
            Floors = new List<Floor>();
        }
    }

    public class BuildingChromosome : ChromosomeBase
    {
        public BuildingChromosome(int numberOfFloors) : base(numberOfFloors)
        {
            for (int i = 0; i < numberOfFloors; i++)
            {
                ReplaceGene(i, GenerateRandomFloor());
            }
        }

        public override IChromosome CreateNew()
        {
            return new BuildingChromosome(Length);
        }

        public override Gene GenerateGene(int geneIndex)
        {
            return GenerateRandomFloor();
        }

        private Gene GenerateRandomFloor()
        {
            return new Gene(Floor.GenerateRandomFloor());
        }
    }

    public class BuildingFitness : IFitness
    {
        private readonly double _maxHeight;
        private readonly double _maxCost;

        public BuildingFitness(double maxHeight, double maxCost)
        {
            _maxHeight = maxHeight;
            _maxCost = maxCost;
        }

        public double Evaluate(IChromosome chromosome)
        {
            var buildingChromosome = chromosome as BuildingChromosome;
            var building = CreateBuildingFromChromosome(buildingChromosome);

            if (building.TotalHeight > _maxHeight || building.TotalCost > _maxCost)
            {
                return 0; // Invalid solution
            }

            // Favor solutions that maximize height and minimize cost
            return (building.TotalHeight / _maxHeight) - (building.TotalCost / _maxCost);
        }

        private Building CreateBuildingFromChromosome(BuildingChromosome chromosome)
        {
            var building = new Building();
            foreach (var gene in chromosome.GetGenes())
            {
                building.Floors.Add((Floor)gene.Value);
            }
            return building;
        }
    }
}
