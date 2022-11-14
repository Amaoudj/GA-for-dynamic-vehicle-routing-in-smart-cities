using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
namespace GA_VRP_CDTA
{
    public delegate double GAFunction(params int[] values);

    public class GA
    {
        // mes variables

        public double mutationRate;
        public double crossoverRate;
        public int    populationSize;
        public int    generationSize;
        public int    NumberOfBinsInCity;
        public double totalFitness;
        public string strFitness;
        public bool   elitism;
        public int    NumberOfAGV;
        public List<Chromosome> Elitisms;
        public List<Chromosome> thisGeneration;
        public List<Chromosome> nextGeneration;
        public List<Bin> Bins;
        public List<AGV> AGVs;
        public List<double> fitnessTable; // pour chaque chromosome on a une fitness 
        public int[,] DistanceMatrix;  // la matrice de distances

        static Random random = new Random();


        static private GAFunction getFitness;
      
        //  mutation rate to 5%, crossover to 80%, population to 100,
        // and generations to 2000.
       
        public GA(double crossoverRate_, double mutationRate_, int populationSize_, int generationSize_, List<Bin> bins , List<AGV> agv, int [,] DistMAtrix)
        {
            //initialiser  les variables
           // random = new Random();

            InitialValues();
            mutationRate   = mutationRate_;
            crossoverRate  = crossoverRate_;
            populationSize = populationSize_;
            generationSize = generationSize_;
            NumberOfBinsInCity = bins.Count;
            NumberOfAGV = agv.Count;
            Bins=new List<Bin>(); Bins.AddRange(bins);
            AGVs=new List<AGV>(); AGVs.AddRange(agv);
            Elitisms = new List<Chromosome>();

            DistanceMatrix = new int[NumberOfBinsInCity + 1, NumberOfBinsInCity + 1];
            for (int i=0;i< NumberOfBinsInCity + 1;i++)
            {
                for (int j = 0; j < NumberOfBinsInCity + 1; j++)
                      DistanceMatrix[i, j] = DistMAtrix[i, j];
            }

            strFitness = "";
        }
     
        public void InitialValues()
        {
            elitism = false;
        }

       public bool AllBinsAllocated(List<Bin> bins_)
        {
            bool Alocated = true;

            foreach (Bin b in bins_) if (! b.allocated) { Alocated = false;  break; }

           return Alocated;
        }

        public void AllocateBinExecutedByOnlyOneAGV()  //
        {    
                  
            int index = 0;
            int ag = 0;
            foreach (Bin b in Bins)
            {
                if (!b.allocated)
                {
                    for (int i = 0; i < NumberOfAGV; i++) // pour chaque robo je passe un test pour tous les bin et je prend jsq ce que ma free_capacity = 0
                    {
                        if (AGVs[i].free_capacity >= b.Capacity)// bin_id=6 located in Bine[5]..
                        {
                            index++;
                            ag = i;
                        }
                    }
                    if (index ==1 ) // un seule robot peut prendre cette bin 
                    {
                        AGVs[ag].free_capacity -= b.Capacity;
                        AGVs[ag].Local_AssignedBins.Add(b.id); //Add directly the value of the bin 
                        AGVs[ag].num_assigned_bind++;
                        b.Assingned_AGV = AGVs[ag].id;
                        b.allocated = true;
                        //Index_bin_allocated.Add(j); // the index of the bin allocated
                        index = 0; ag = 0;
                    }
                    
                }
            }
  
        }


        public void Correct_Solution(Chromosome chrom)
        {
            int last_val = NumberOfBinsInCity - 1;

            int val = chrom.Bins_Visiting_Order[last_val];

            int index_ = 0;
            int cap = Bins[chrom.Bins_Visiting_Order[0] - 1].Capacity;
            for (int i = 1; i < NumberOfBinsInCity; i++)
            {
                if(Bins[chrom.Bins_Visiting_Order[i] - 1].Capacity < cap)
                {
                    cap = Bins[chrom.Bins_Visiting_Order[i] - 1].Capacity;
                     index_ = i;
                }
            }
            chrom.Bins_Visiting_Order[last_val] = chrom.Bins_Visiting_Order[index_]; //Chrom_random.Next(1, Num_AGV);
            chrom.Bins_Visiting_Order[index_] = val;

            // rank AGVs the beget Capa is the last
           for (int i = 0; i < NumberOfAGV; i++)
            {
                for (int j = i; j < NumberOfAGV; j++)
                {
                    if (AGVs[chrom.AGV_allocation_Order[i]-1].Capacity > AGVs[chrom.AGV_allocation_Order[j] - 1].Capacity)
                    {
                        int val_ = chrom.AGV_allocation_Order[i];
                        chrom.AGV_allocation_Order[i] = chrom.AGV_allocation_Order[j];
                        chrom.AGV_allocation_Order[j] = val_;
                    }
                }
            }

            foreach (AGV agv in AGVs) // initialise some parametres 
            {
                agv.Local_AssignedBins.Clear(); agv.Traveled_Distance = 0; agv.free_capacity = agv.Capacity; agv.num_assigned_bind = 0;
            }
            foreach (AGV agv in chrom.AGVs) // initialise some parametres 
            {
                agv.Local_AssignedBins.Clear(); agv.Traveled_Distance = 0; agv.free_capacity = agv.Capacity; agv.num_assigned_bind = 0;
            }

            chrom.Chrom_fitness = 0;
            chrom.AGVs.Clear();

            chrom.AGVs.AddRange(AGVs);

            foreach (Bin b in Bins) // initialise some parametres 
            {
                b.allocated = false;
                b.Assingned_AGV = 0;
            }

            int[] v_bins = new int[NumberOfBinsInCity];
            for (int i = 0; i < NumberOfBinsInCity; i++) v_bins[i] = chrom.Bins_Visiting_Order[i];

            List<int> Index_bin_allocated = new List<int>();

            // start by allocating the big_capacity_Bin
            for (int h = 0; h < NumberOfBinsInCity; h++)  //AllocateBinExecutedByOnlyOneAGV();
            {
                int index = 0;
                int ag = 0;
                foreach (Bin b in this.Bins)
                {
                    if (!b.allocated)
                    {
                        for (int i = 0; i < NumberOfAGV; i++) // pour chaque robo je passe un test pour tous les bin et je prend jsq ce que ma free_capacity = 0
                        {
                            if (chrom.AGVs[i].free_capacity >= b.Capacity)// bin_id=6 located in Bine[5]..
                            {
                                index++;
                                ag = i;
                            }
                        }
                        if (index == 1) // un seule robot peut prendre cette bin 
                        {
                            chrom.AGVs[ag].free_capacity -= b.Capacity;
                            chrom.AGVs[ag].Local_AssignedBins.Add(b.id); //Add directly the value of the bin 
                            chrom.AGVs[ag].num_assigned_bind++;
                            b.Assingned_AGV = chrom.AGVs[ag].id;
                            b.allocated = true;
                            index = 0; ag = 0;
                        }
                    }
                }
            }


            for (int i = 0; i < NumberOfAGV; i++) // pour chaque robot je passe un test pour tous les bin et je prend jsq ce que ma free_capacity = 0
            {
                //int ii = i;
                for (int j = 0; j < NumberOfBinsInCity; j++)
                {
                    if (!Bins[v_bins[j] - 1].allocated)
                    {
                        if (chrom.AGVs[chrom.AGV_allocation_Order[i] - 1].free_capacity > Bins[v_bins[j] - 1].Capacity)// bin_id=6 located in Bine[5]..
                        {
                            chrom.AGVs[chrom.AGV_allocation_Order[i] - 1].free_capacity -= Bins[v_bins[j] - 1].Capacity;
                            chrom.AGVs[chrom.AGV_allocation_Order[i] - 1].Local_AssignedBins.Add(v_bins[j]); //Add directly the value of the bin 
                            chrom.AGVs[chrom.AGV_allocation_Order[i] - 1].num_assigned_bind++;
                            Bins[v_bins[j] - 1].Assingned_AGV = chrom.AGV_allocation_Order[i];
                            Bins[v_bins[j] - 1].allocated = true;
                            Index_bin_allocated.Add(j); // the index of the bin allocated
                        }
                    }
                }
            }


        }

        public int Get_bin_notAlolocated()
        {
            int bb = 0;

            foreach (Bin b in Bins) if (!b.allocated) { bb=b.id ; break; }

            return bb;
        }
        public List<int> Get_AGV_List(int b)
        {
            List<int> list = new List<int>();

            for (int i = 0; i < NumberOfAGV; i++) // pour chaque robo je passe un test pour tous les bin et je prend jsq ce que ma free_capacity = 0
            {
                if (AGVs[i].Capacity >= Bins[b].Capacity)// bin_id=6 located in Bine[5]..
                {
                    list.Add(AGVs[i].id);
                }
            } 
 
            return list;
        }
        public List<int> Get_AGV_Vide()
        {
            List<int> list = new List<int>();

            for (int i = 0; i < NumberOfAGV; i++) // pour chaque robo je passe un test pour tous les bin et je prend jsq ce que ma free_capacity = 0
            {
                if (AGVs[i].num_assigned_bind==0)// bin_id=6 located in Bine[5]..
                {
                    list.Add(AGVs[i].id);
                }
            }

            return list;
        }
        public int get_Agv_to_Vide(List<int> AGV_List)
        {
            int numRobot = AGV_List[0];
            int videcap = AGVs[AGV_List[0] - 1].free_capacity;
            
            for (int i =1; i< AGV_List.Count; i++)
            {
                if (AGVs[AGV_List[i] - 1].free_capacity > videcap) {
                    videcap = AGVs[AGV_List[i] - 1].free_capacity;
                    numRobot = AGV_List[i];
                }
            }
 
            return numRobot;

        }
       

       public void Evaluate_Chromosome(Chromosome chrom) // interprète et évaluer un chromosome
       {
           
            // je commence par le premier AGV dans la list, et je le remplir utilisant BinList[] puis le deusième et .....
            foreach (AGV agv in AGVs) // initialise some parametres 
            {
                agv.Local_AssignedBins.Clear(); agv.Traveled_Distance = 0; agv.free_capacity = agv.Capacity; agv.num_assigned_bind=0; 
            }
            foreach (AGV agv in chrom.AGVs) // initialise some parametres 
            {
                agv.Local_AssignedBins.Clear(); agv.Traveled_Distance = 0; agv.free_capacity = agv.Capacity; agv.num_assigned_bind = 0;
            }

            chrom.Chrom_fitness = 0;
            chrom.AGVs.Clear();
            
            chrom.AGVs.AddRange(AGVs);

            foreach (Bin b in Bins) // initialise some parametres 
            {
                b.allocated=false;
                b.Assingned_AGV = 0;
            }
                    
            int[] v_bins = new int[NumberOfBinsInCity];
            for (int i = 0; i < NumberOfBinsInCity; i++) v_bins[i] = chrom.Bins_Visiting_Order[i];
                
            List<int> Index_bin_allocated = new List<int>();

            // start by allocating the big_capacity_Bin

            int changeval = 0;
            for (int h=0;h< NumberOfBinsInCity; h++)  //AllocateBinExecutedByOnlyOneAGV();
            {
                int index = 0;
                int ag = 0;
                foreach (Bin b in this.Bins)
                {
                    if (!b.allocated)
                    {
                        for (int i = 0; i < NumberOfAGV; i++) // pour chaque robo je passe un test pour tous les bin et je prend jsq ce que ma free_capacity = 0
                        {
                            if (chrom.AGVs[i].free_capacity >= b.Capacity)// bin_id=6 located in Bine[5]..
                            {
                                index++;
                                ag = i;
                            }
                        }
                        if (index == 1) // un seule robot peut prendre cette bin 
                        {
                            chrom.AGVs[ag].free_capacity -= b.Capacity;
                            chrom.AGVs[ag].Local_AssignedBins.Add(b.id); //Add directly the value of the bin 
                            chrom.AGVs[ag].num_assigned_bind++;
                            b.Assingned_AGV = chrom.AGVs[ag].id;
                            b.allocated = true;


                            // cet agv sera le premier à remplir si en rest de capacity
                            int ind = get_index_ofValue(ag, chrom.AGV_allocation_Order);
                            int x = chrom.AGV_allocation_Order[changeval];
                            chrom.AGV_allocation_Order[changeval]= chrom.AGV_allocation_Order[ind];
                            chrom.AGV_allocation_Order[ind] = x;
                            changeval++;
                            index = 0; ag = 0;
                        }
                    }
                }
            }


            for (int i = 0; i < NumberOfAGV; i++) // pour chaque robo je passe un test pour tous les bin et je prend jsq ce que ma free_capacity = 0
            {
                //int ii = i;
                for (int j = 0; j < NumberOfBinsInCity; j++)
                {
                    if (!Bins[v_bins[j] - 1].allocated)
                    {
                        if (chrom.AGVs[chrom.AGV_allocation_Order[i] - 1].free_capacity > Bins[v_bins[j] - 1].Capacity)// bin_id=6 located in Bine[5]..
                        {
                            chrom.AGVs[chrom.AGV_allocation_Order[i] - 1].free_capacity -= Bins[v_bins[j] - 1].Capacity;
                            chrom.AGVs[chrom.AGV_allocation_Order[i] - 1].Local_AssignedBins.Add(v_bins[j]); //Add directly the value of the bin 
                            chrom.AGVs[chrom.AGV_allocation_Order[i] - 1].num_assigned_bind++;
                            Bins[v_bins[j] - 1].Assingned_AGV = chrom.AGV_allocation_Order[i];
                            Bins[v_bins[j] - 1].allocated = true;
                            Index_bin_allocated.Add(j); // the index of the bin allocated
                        }
                    }
                }
            }

             //int xb = DistanceMatrix[01111, 100000];
            //corriger le chromosome s'il n'est pas correct

          if (!Solution_correct(chrom))   Correct_Solution(chrom);

           //int bb = DistanceMatrix[01111, 100000];

            chrom.Chrom_fitness = 0;
            foreach (AGV agv in chrom.AGVs) // calculate the Fitness of the chromosome
            {
                if (agv.num_assigned_bind != 0)
                {
                    for (int i = 0; i < agv.num_assigned_bind - 1; i++)
                    {                                                        
                       agv.Traveled_Distance += DistanceMatrix[agv.Local_AssignedBins[i], agv.Local_AssignedBins[i + 1]]; // from bin (i-1) to bin (i)
                    }
                    agv.Traveled_Distance += DistanceMatrix[0, agv.Local_AssignedBins[0]];
                    agv.Traveled_Distance += DistanceMatrix[agv.Local_AssignedBins[agv.num_assigned_bind - 1], 0];

                }

                chrom.Chrom_fitness = chrom.Chrom_fitness +agv.Traveled_Distance;
            }

          /*  foreach (AGV agv in AGVs) // initialise some parametres 
            {
                agv.Local_AssignedBins.Clear(); agv.Traveled_Distance = 0; agv.free_capacity = agv.Capacity; agv.num_assigned_bind = 0;
            }
            foreach (Bin b in Bins) // initialise some parametres 
            {
                b.allocated = false;
                b.Assingned_AGV = 0;
            }
          */

        }
        private void CreateChromosomes()
        {
            for (int i = 0; i < populationSize; i++) //
            {
               Chromosome chro = new Chromosome(NumberOfAGV, NumberOfBinsInCity);
               Evaluate_Chromosome(chro);
               thisGeneration.Add(chro);             
            }

          CheckRepetedChromosome();

        }

        public void CheckRepetedChromosome()
        {
            for (int i = 0; i < populationSize; i++) //
            {
                for (int j = i; j < populationSize; j++) //
                {

                   if(CompareTwoChromosomes(thisGeneration[i], thisGeneration[j]))
                    {

                        int posBin = random.Next(1, NumberOfBinsInCity - 1);
                        //int posagv = random.Next(1, NumberOfAGV - 1);

                        int val = thisGeneration[j].Bins_Visiting_Order[posBin - 1];
                        thisGeneration[j].Bins_Visiting_Order[posBin - 1] = thisGeneration[j].Bins_Visiting_Order[posBin]; 
                        thisGeneration[j].Bins_Visiting_Order[posBin] = val;

                        Evaluate_Chromosome(thisGeneration[j]);
                    }
                }
            }
        }

        public bool CompareTwoChromosomes(Chromosome ch1, Chromosome ch2)
        {
            bool thesame = true;

            for (int i=0;i< NumberOfBinsInCity; i++)
            {
                if (ch1.Bins_Visiting_Order[i] != ch2.Bins_Visiting_Order[i])
                {
                    thesame = false;
                    break;
                }
            }
            for (int i = 0; i < NumberOfAGV; i++)
            {
                if (ch1.AGV_allocation_Order[i] != ch2.AGV_allocation_Order[i])
                {
                    thesame = false;
                    break;
                }
            }
            return thesame;
        }

        public void CreateNextGeneration()
        {
            nextGeneration.Clear();
            Chromosome chrom = thisGeneration[0];   //save the best chrom
          
            int h = 0;
            for (int i = 0; i < populationSize; i += 2) // puisque je vais prendre deux chromosome
            {
                int pidx1 = RouletteSelection(); 
                int pidx2 = RouletteSelection();
             
                Chromosome parent1, parent2, child1, child2;
                parent1 = thisGeneration[h];// pidx1 // i  // j
                h++;
                parent2 = thisGeneration[h];//pidx2  // i+1 //h
                //h++;
                if(random.NextDouble() < crossoverRate) //crosover operation
                {
                    parent1.Crossover2(ref parent2, out child1, out child2);
                    child1.Mutate_One_gene_BinePartes();
                    child2.Mutate_One_gene_BinePartes();
                }
                else
                {
                    child1 = parent1;
                    child2 = parent2;
                }

                // mutation: on a deux types
                if (random.NextDouble() < 0.5) //crosover operation
                {
                    child1.Mutate_One_gene_twoPartes();
                    child2.Mutate_One_gene_twoPartes();
                }
                else
                {
                    child1.MutateALLGenes_tow_parts();
                    child2.MutateALLGenes_tow_parts();
                } 
                

                Evaluate_Chromosome(child1);
                Evaluate_Chromosome(child2);

                nextGeneration.Add(child1);
                nextGeneration.Add(child2);
                //chrom = thisGeneration[10000]; 
            }
            // if (elitism && chrom != null)  

            nextGeneration.Sort(delegate (Chromosome x, Chromosome y)
            { return Comparer<int>.Default.Compare(x.Fitness, y.Fitness); });

           // nextGeneration[populationSize-1] =  chrom; // toujour le mieur individu remplace le mouvais ;//           
           
            thisGeneration.Clear();
            thisGeneration.AddRange(nextGeneration);

           // CheckRepetedChromosome();
        }

        public void CreateNextGeneration2()
        {
            nextGeneration.Clear();
            if (Elitisms.Count > 0)
            {
                Chromosome chrom = Elitisms[Elitisms.Count - 1]; //save the best chrom
                nextGeneration.Add(chrom);
            }

            int j = populationSize;
            int h = 0;
            for (int i = 0; i < populationSize-1; i += 1) // une nouvelle génération contient 2xpop init
            {
                j--;
                Chromosome parent1, parent2, child1, child2;
                parent1 = thisGeneration[h];//  j
                h++;
                parent2 = thisGeneration[h];// i+1 //h

                // parent1.Crossover2(ref parent2, out child1, out child2);
                if (random.NextDouble() < crossoverRate) //crosover operation
                {
                    parent1.Crossover2(ref parent2, out child1, out child2);
                }
                else
                {
                    child1 = parent1;
                    child2 = parent2;
                }

                Evaluate_Chromosome(child1);
                Evaluate_Chromosome(child2);

                nextGeneration.Add(child1);
                nextGeneration.Add(child2);

                // la mutation
                if (h < (int) populationSize/3) // les best chrom (je fait une muation simple et légé pour ne pas perturbé la solution) 
                {
                    child1.Mutate_One_gene_AGVPartes();
                    child2.Mutate_One_gene_BinePartes();
                }
                else if (h>=(int) populationSize / 3 && h < (int) populationSize *(2/3)) // les best chrom (je fait une muation simple et légé pour ne pas perturbé la solution) 
                {
                    child1.Mutate_One_gene_twoPartes();
                    child2.Mutate_One_gene_twoPartes();
                }
                else // le rest (the bed solutions)
                {
                    child1.MutateALLGenes_tow_parts();
                    child2.MutateALLGenes_tow_parts();
                }

                Evaluate_Chromosome(child1);
                Evaluate_Chromosome(child2);

                nextGeneration.Add(child1);
                nextGeneration.Add(child2);
                //chrom = thisGeneration[10000]; 
            }

            nextGeneration.Sort(delegate (Chromosome x, Chromosome y)
            { return Comparer<int>.Default.Compare(x.Fitness, y.Fitness); });

            thisGeneration.Clear();
            for (int f = 0 ; f < populationSize ; f++) thisGeneration.Add(nextGeneration[f]);


            // nextGeneration[populationSize] = chrom;
        }

        public bool Solution_correct(Chromosome ch)
        {
            int num = 0;
            bool ok = true;
            
            foreach (AGV agv in ch.AGVs) num += agv.num_assigned_bind;
                   
            if (num == NumberOfBinsInCity) ok = true;
            else ok = false;


            return ok;
        }

        public void Run()  // Method that starts the GA execution
        {
            //if (NumberOfBinsInCity == 0) throw new IndexOutOfRangeException("Please set the number of bins in the city !!! ");
            //if (NumberOfAGV == 0)        throw new IndexOutOfRangeException("Please set the number of AGV !!! ");
           
            //Create the fitness table.
            fitnessTable   = new List<double>();
            thisGeneration = new List<Chromosome>(generationSize);
            nextGeneration = new List<Chromosome>(generationSize);
            Chromosome.MutationRate = mutationRate;

            CreateChromosomes();
            RankPopulation();//sort the popolation in order of fitness

            for (int i = 0; i < generationSize; i++)
            {
                CreateNextGeneration();
                RankPopulation();  // ici je garde toujour la mieur solution dans une liste des Elitisms[]
            }       
        }
        // After ranking all the genomes by fitness, use a 'roulette wheel' selection method
        //This allocates a large probability of selection to those with the highest fitness.
        
        public int RouletteSelection()
        {
            double randomFitness = random.NextDouble() * totalFitness;
           
            int idx = -1;
            int mid;
            int first = 0;
            int last = populationSize - 1;
            mid = (last - first) / 2;

            //  ArrayList's BinarySearch is for exact values only
            //  so do this by hand.
            while (idx == -1 && first <= last)
            {
                if (randomFitness < fitnessTable[mid])
                {
                    last = mid;
                }
                else if (randomFitness > fitnessTable[mid])
                {
                    first = mid;
                }
                mid = (first + last) / 2;
                //  lies between i and i+1
                if ((last - first) == 1)
                    idx = last;
            }

            return idx;
        }

        /// Rank population and sort in order of fitness.
        public void RankPopulation()
        {
            // je cherche les chromo equaux pour les éliminé puis ordone
            //CheckRepetedChromosome();

            totalFitness = 0.0;
            foreach (Chromosome g in thisGeneration)
            {    
                totalFitness += g.Fitness;
            }

            thisGeneration.Sort(delegate(Chromosome x, Chromosome y)
            { return Comparer<int>.Default.Compare(x.Fitness, y.Fitness); });

            //Now sorted in order of fitness. the smolest is the one
         /*  for (int i = 0; i < thisGeneration.Count; i++)
            {
                if (Solution_correct(thisGeneration[i])) {
                    
                    break;
                }
            }*/

          Elitisms.Add(thisGeneration[0]);

          //  Elitisms.Add(thisGeneration[0]);
            int fitness = 0;
            fitnessTable.Clear();
            foreach (Chromosome t in thisGeneration)
            {
               fitness += t.Fitness;
               fitnessTable.Add(t.Fitness);
            }

           // Elitisms.Add(thisGeneration[1000]);
        }

        // Create a correcte chromosomes  and calculate its finess

        public Chromosome getbestChromo(List<Chromosome> ch)
        {
            Chromosome best_chro = ch[0];

           foreach (Chromosome c in ch) if (c.Chrom_fitness < best_chro.Chrom_fitness) best_chro = c;          
            return best_chro;
        }
        public void GetBest(out List<AGV> AGVss, out Chromosome chrom, out int fitness) // à virifier
        {
            AGVss = new List<AGV>();
            Chromosome g = thisGeneration[0]; //populationSize - 1];
            Elitisms.Add(g);

            //sort Elitisms 
            Elitisms.Sort(delegate (Chromosome x, Chromosome y)
            { return Comparer<int>.Default.Compare(x.Fitness, y.Fitness); });

            chrom = Elitisms[0];
            AGVss.AddRange(chrom.AGVs);
            
            fitness = chrom.Chrom_fitness;

        }
        public Chromosome GetBest() // à virifier
        {
            if (Elitisms.Count == 0)
            {
                Chromosome g = thisGeneration[0];
                Elitisms.Add(g);
            }

            Elitisms.Sort(delegate (Chromosome x, Chromosome y)
            { return Comparer<int>.Default.Compare(x.Fitness, y.Fitness); });
            //Evaluate_Chromosome(Elitisms[0]);
           // foreach (Chromosome s in Elitisms) if (!Solution_correct(s)) Elitisms.Remove(s);

            return (Elitisms[0]);           
        }

        // Des fonctions utils pour mon code hhhhh 
        public int get_Index_Min(int[] v)
        {
            int h = 0;
            int minVal = v[0];

            for (int i = 1; i < v.Length; i++) if (v[i] < minVal) { minVal = v[i]; h = i; }

            return h;
        }
        public int get_index_ofValue(int val, int[] v)
        {
            int index = 0;

            for (int i = 0; i < v.Length; i++) if (v[i] == val) { index = i; break; }

            return index;
        }
        public int[] get_table_Indexs(int[] v, int[] V_index)
        {
            int[] inex_table = new int[v.Length];
            int j = 0;
            for (int i = 0; i < v.Length; i++)
            {
                inex_table[i] = get_index_ofValue(v[i], V_index);
                j++;
            }

            return inex_table;
        }
        public int[] Ordre_table_by_Indexs(int[] v, int[] V_glob)
        {
            int[] ordred_table = new int[v.Length];
            int[] V_index = new int[v.Length];
            V_index = get_table_Indexs(v, V_glob);


            for (int i = 0; i < v.Length; i++)
            {
                int min = V_index[i];
                for (int j = i + 1; j < v.Length; j++)
                {
                    if (V_index[j] < min)
                    {
                        int val = V_index[j];
                        V_index[i] = val;
                        V_index[j] = min;
                        int val2 = v[i];
                        v[i] = v[j];
                        v[j] = val2;
                        min = V_index[i];
                    }
                }
            }

            return v;
        }
        public bool numberGeneratedBefor(int num, int[] v)
        {
            bool yes = false;
            for (int i = 0; i < v.Length; i++)
            {
                if (v[i] == num) { yes = true; break; }
            }

            return yes;
        }

        //  Properties
        public int PopulationSize
        {
            get
            {
                return populationSize;
            }
            set
            {
                populationSize = value;
            }
        }
        public int Generations
        {
            get
            {
                return generationSize;
            }
            set
            {
                generationSize = value;
            }
        }
        public int ChromosomeSize
        {
            get
            {
                return NumberOfBinsInCity;
            }
            set
            {
                NumberOfBinsInCity = value;
            }
        }
        public double CrossoverRate
        {
            get
            {
                return crossoverRate;
            }
            set
            {
                crossoverRate = value;
            }
        }
        public double MutationRate
        {
            get
            {
                return mutationRate;
            }
            set
            {
                mutationRate = value;
            }
        }
        public string FitnessFile
        {
            get
            {
                return strFitness;
            }
            set
            {
                strFitness = value;
            }
        }
        // Keep previous generation's fittest individual in place of worst in current
        public bool Elitism
        {
            get
            {
               return elitism;
            }
            set
            {
                elitism = value;
            }
        }

        
    }

}
