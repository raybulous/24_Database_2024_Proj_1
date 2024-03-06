using _24_Database_2024_Proj_1.tests;
namespace _24_Database_2024_Proj_1;
using System;
class Program
{
    static void Main(string[] args)
    {
        Experiment experiment = new Experiment(); // Create an instance of the Experiment class
        experiment.RunExp1();
        Console.WriteLine();
        experiment.RunExp2();
        Console.WriteLine();
        experiment.RunExp3();
        Console.WriteLine();
        experiment.RunExp4();
        Console.WriteLine();
        experiment.RunExp5();
    }
}


