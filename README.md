# Pathfinding Algorithms In Dynamic Environments
This repository contains the code and resources used for my Master's thesis (found at https://urn.fi/URN:NBN:fi-fe2024052738693), which focuses on testing and comparing pathfinding algorithms in dynamic environments. The algorithms were evaluated based on their speed and found path lenghts in different scenarios.

## Algorithms Implemented

The following pathfinding algorithms are implemented in this project:

- **A\***: An advanced pathfinding algorithm that uses heuristics to improve performance.
- **D\* Lite**: An algorithm based on A\* that was created to efficiently update paths in an unknown or dynamic environments by using incremental searches.
- **Anytime D\* (AD\*)**: Anytime version of D* Lite that reduces path length the more time it is given to compute. 
- **Real-Time D\* (RTD\*)**: Combines incremental and real-time features to be able to execute fast searches in dynamic environments.


## Evaluation metrics

The following metrics are measured:

- Time to reach the goal node from the start.
- The average time to recompute the path after dynamics changes happen in the environment.
- The lenght of the path traveled.
- The number of nodes expanded by the algorithm.


## Project Structure

The project structure is organized as follows:

    Pathfinding-Algorithms/
    ├── Algorithms/
    ├── DataCollection/
    ├── DoorStates/
    ├── Grid/
    ├── Maps/
    ├── PriorityQueue/
    └── Program.cs


- **Algorithms/**: Contains the implementations of the different pathfinding algorithms.
- **DataCollection/**: Script for collecting and parsing data and where the results are stored as .csv files.
- **DoorStates/**: Contains .json files for door behaviors.
- **Grid/**: Contains classes for grid-related functionalities.
- **Maps/**: Contains maps used.
- **PriorityQueue/**: Implementation of priority queue data structure.
- **Program.cs**: Main program file for running the application.


## Program funtionality

As the program was created for a very specific funtionality (that is running each test once for each algorithm), it has no user interface nor takes command line inputs. To run tests you need to call the RunTests() method in the Main() method in Program.cs with corresponding arguments to the wanted test parameters. 
