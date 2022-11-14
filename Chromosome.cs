using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GA_VRP_CDTA
{
    public class Chromosome
    {
        // output of the chromosome
        public int[]   AGV_allocation_Order;
        public int[]   Bins_Visiting_Order;

        public int     Num_Bins_city;// number of bins in the city        
        public int     Num_AGV;
        public int     Chrom_fitness;
        static Random  Chrom_random = new Random();
        public static double Chrom_mutationRate;
        
        public List<AGV> AGVs;

        //public List<Bin> Bins;
        //public int[,] DistanceMatrix;
       

        public Chromosome(int num_agv, int num_bin)// List<AGV> agvs, List<Bin> bins, int[,] sidtMatrix)
        {
            //Chrom_random = new Random();
            Num_Bins_city = num_bin; //bins.Count;
            Num_AGV = num_agv; //agvs.Count;
            AGV_allocation_Order = new int[Num_AGV];
            Bins_Visiting_Order = new int[Num_Bins_city];
            AGVs = new List<AGV>();
            Chrom_fitness = 0;
            CreateChromosome();
            //Used_AGV_chromo = new List<int>();

        }
   
        public Chromosome(int num_agv, int num_bin, bool createGenes) //List<AGV> agvs, List<Bin> bins, bool createGenes) // on va l'utiliser dans l'operations Crossover et Mutation
        {
            //Chrom_random = new Random();
            Num_Bins_city = num_bin;// bins.Count;
            Num_AGV = num_agv; //agvs.Count;
            AGV_allocation_Order = new int[Num_AGV];
            Bins_Visiting_Order = new int[Num_Bins_city];
            AGVs = new List<AGV>();
            Chrom_fitness = 0;
            //Used_AGV_chromo = new List<int>();

            if (createGenes) CreateChromosome();
        }
  
        public Chromosome ChromosomeCopy()
        {
            Chromosome g = new Chromosome(this.Num_AGV, this.Num_Bins_city, false);

            Array.Copy(AGV_allocation_Order, g.AGV_allocation_Order, Num_AGV);
            Array.Copy(Bins_Visiting_Order,  g.Bins_Visiting_Order, Num_Bins_city); // copy from 'Bins_Visiting_Order' to  'g.Bins_Visiting_Order'
            g.AGVs.AddRange(AGVs);
            g.Chrom_fitness = Chrom_fitness;
           
            return g;
        }

        public void CreateChromosome() // à revoir ... dénérer un vecteur contenant pour chaque cellule le AGV qui va prendre le bins 
        {
            List<int> randomBins_Order = new List<int>();
            List<int> AGVNumbers = new List<int>();          
            //1- Generate AGV allocation order          
            AGVNumbers.AddRange(Enumerable.Range(1, Num_AGV).OrderBy(i => Chrom_random.Next(1, Num_AGV)).Take(Num_AGV));
            for (int j = 0; j < Num_AGV; j++) AGV_allocation_Order[j] = AGVNumbers[j];      
            
            //2-Generated an order of execution "Schedule"
            randomBins_Order.AddRange(Enumerable.Range(1, Num_Bins_city).OrderBy(i => Chrom_random.Next(1, Num_Bins_city)).Take(Num_Bins_city));
            for (int j = 0; j < Num_Bins_city; j++)  Bins_Visiting_Order[j] = randomBins_Order[j];
                                
        }
      
        public void Crossover(ref Chromosome chrom2, out Chromosome child1, out Chromosome child2) // à revoire en introduisant genome1 
        {
            //Random Chrom_random_ = new Random();

            int pos1 = (int)(Chrom_random.NextDouble() * (double)Num_AGV);
            if (pos1 == 1) pos1 = 2;
            if (pos1 == Num_AGV || pos1 == Num_AGV-1) pos1  = Num_AGV-3 ;

            int pos2 = (int)(Chrom_random.NextDouble() * (double)Num_Bins_city);
            if (pos2 == 1) pos2 = 2;
            if (pos2 == Num_Bins_city || pos2 == Num_Bins_city-1) pos2 = Num_Bins_city-3;
            child1 = new Chromosome(Num_AGV, Num_Bins_city, false);
            child2 = new Chromosome(Num_AGV, Num_Bins_city, false);


            int h = Num_AGV;            
            
            // je change la partie 1 et je garde la parie 2 du chromosome
            h = pos1;
            for (int i = 0; i < pos1; i++) // 
            {
                h--;
                child1.AGV_allocation_Order[i] = AGV_allocation_Order[h];
            }
            for (int i = pos1; i < Num_AGV; i++) child1.AGV_allocation_Order[i] = AGV_allocation_Order[i];
            
            for (int ji = 0; ji < pos2; ji++) child1.Bins_Visiting_Order[ji] = Bins_Visiting_Order[ji];  //= randomBins_Order[ji];
            // changer la partie 2
            h = Num_Bins_city;
            for (int ji = pos2; ji < Num_Bins_city; ji++) { h--; child1.Bins_Visiting_Order[ji] = Bins_Visiting_Order[h]; } 


            //================= child 02=====================
            // je fix la partie 1         
            for (int i = 0; i < pos1; i++) child2.AGV_allocation_Order[i] = chrom2.AGV_allocation_Order[i];
             h = Num_AGV;
            for (int ii = pos1; ii < Num_AGV; ii++)
            {
                h--;
                child2.AGV_allocation_Order[ii] = chrom2.AGV_allocation_Order[h];
            }

            int j = pos2; // change la première partie
            for (int ji = 0; ji < pos2; ji++)
            {
                j--;
                child2.Bins_Visiting_Order[ji] = chrom2.Bins_Visiting_Order[j];
            }  
                 
            for (int ji = pos2; ji < Num_Bins_city; ji++) child2.Bins_Visiting_Order[ji] = chrom2.Bins_Visiting_Order[ji];              

            // pour tester ... add an exepltion ...
           
        }

        public void Crossover2(ref Chromosome chrom2, out Chromosome child1, out Chromosome child2) // à revoire en introduisant genome1 
        {
           // swap : 
            child1 = new Chromosome(Num_AGV, Num_Bins_city, false);
            child2 = new Chromosome(Num_AGV, Num_Bins_city, false);
            int pos1 = (int)(Chrom_random.NextDouble() * (double)Num_Bins_city);
            if (pos1 == 1) pos1 = 2;
            if (pos1 == Num_Bins_city || pos1 == Num_Bins_city - 1) pos1 = Num_Bins_city - 3;
            int pos = (int)(Chrom_random.NextDouble() * (double)Num_Bins_city);


            for (int i = 0; i < Num_AGV; i++)           child1.AGV_allocation_Order[i] = AGV_allocation_Order[i];
            for (int i = 0; i < Num_AGV; i++)           child2.AGV_allocation_Order[i] = chrom2.AGV_allocation_Order[i];
            
            for (int ji = 0; ji < Num_Bins_city; ji++)  child1.Bins_Visiting_Order[ji] = chrom2.Bins_Visiting_Order[ji];
            for (int ji = 0; ji < Num_Bins_city; ji++)  child2.Bins_Visiting_Order[ji] = Bins_Visiting_Order[ji];
            

       /*     for (int i = 0; i < pos1; i++)             child1.Bins_Visiting_Order[i] = Bins_Visiting_Order[i];
            for (int i = pos1; i < Num_Bins_city; i++) child1.Bins_Visiting_Order[i] = chrom2.Bins_Visiting_Order[i];

            for (int i = 0; i < pos1; i++)             child2.Bins_Visiting_Order[i] = chrom2.Bins_Visiting_Order[i];
            for (int i = pos1; i < Num_Bins_city; i++) child2.Bins_Visiting_Order[i] = Bins_Visiting_Order[i];
           
            // child1.Bins_Visiting_Order[100000] = chrom2.Bins_Visiting_Order[100000000000];
            Repair();
       */
        }

        public void Repair()  // 
        {
            //int[] repeted = new int[Num_Bins_city];
            List<int> notUsed = new List<int>();
            List<int> repeted = new List<int>();
            List<int> num_repetition = new List<int>();
            int repet = 0;
            bool trouve = false;

            for (int j = 1; j <= Num_Bins_city; j++)
            {

                for (int i = 0; i < Num_Bins_city; i++)
                {
                    if (Bins_Visiting_Order[i] == j)
                    {
                        repet++;
                        trouve = true;
                    }
                }
                if (trouve && repet > 1) { repeted.Add(j); num_repetition.Add(repet); }
                else if (!trouve) notUsed.Add(j);

                repet = 0;
                trouve = false;
            }
            //===========================================================================
            //S'il y a des répétitions ...
            int jj = 0;
            //int numre = 0;
            if (notUsed.Count != 0)
            {
                for (int i = 0; i < repeted.Count; i++)//notUsed
                {//je cherche la valeur repétée et la remplacé par NotUsed[]                                    
                    for (int f = 1; f < num_repetition[i]; f++) // il y a des repetition, donc des chifres not repeted yet
                    {
                        for (int h = 0; h < Num_Bins_city; h++)
                        {
                            if (Bins_Visiting_Order[h] == repeted[i]) // la valeur répétée
                            {
                                Bins_Visiting_Order[h] = notUsed[jj];
                                //notUsed.RemoveAt(0);
                                jj++;
                                break;
                            }
                        }
                    }
                }
            }
        }

        public void MutateALLGenes_tow_parts() // à revoir  (la premier proposition est de généré un num randomly et faire           
        {
            //Random Chrom_random = new Random();
           for (int pos = 0; pos < Num_AGV; pos++) // pour tous les gènes dans 'AGV_allocation_Order'
           {
                int posAvg= Chrom_random.Next(1, Num_AGV-1);
                if (Chrom_random.NextDouble() < Chrom_mutationRate)
                {
                    int val = this.AGV_allocation_Order[posAvg - 1];

                    this.AGV_allocation_Order[posAvg - 1] = this.AGV_allocation_Order[posAvg]; //Chrom_random.Next(1, Num_AGV);
                    this.AGV_allocation_Order[posAvg]= val;
                }
            }
        
            for (int pos = 0; pos < Num_Bins_city; pos++)  // pour chaque bin in 'Bins_Visiting_Order'
            {
                int posBin = Chrom_random.Next(1, Num_Bins_city - 1);

                if (Chrom_random.NextDouble() < Chrom_mutationRate)
                {
                    int val = this.Bins_Visiting_Order[posBin - 1];

                    this.Bins_Visiting_Order[posBin - 1] = this.Bins_Visiting_Order[posBin]; //Chrom_random.Next(1, Num_AGV);
                    this.Bins_Visiting_Order[posBin]     = val;
                }
            }                
        }
        public void Mutate_One_gene_twoPartes() // à revoir  (la premier proposition est de généré un num randomly et faire           
        {
     
               int posAvg = Chrom_random.Next(1, Num_AGV - 1);
               int posAvg2 = Chrom_random.Next(1, Num_AGV - 1);
              
               if (Chrom_random.NextDouble() < Chrom_mutationRate)
                {
                    int val1 = this.AGV_allocation_Order[posAvg];

                    this.AGV_allocation_Order[posAvg] = this.AGV_allocation_Order[posAvg2]; //Chrom_random.Next(1, Num_AGV);
                    this.AGV_allocation_Order[posAvg2] = val1;
                }
            
                //je fait la muation obligatoire d'une valeur dans le chromosome BINs[]
                int posBin = Chrom_random.Next(1, Num_Bins_city - 1);
                int posBin2 = Chrom_random.Next(1, Num_Bins_city - 1);

                int val = this.Bins_Visiting_Order[posBin];

                if (Chrom_random.NextDouble() < Chrom_mutationRate)
                 {
                   this.Bins_Visiting_Order[posBin] = this.Bins_Visiting_Order[posBin2]; //Chrom_random.Next(1, Num_AGV);
                   this.Bins_Visiting_Order[posBin2] = val;
                 }
        }
        public void Mutate_One_gene_BinePartes() // à revoir  (la premier proposition est de généré un num randomly et faire           
        {
            // je fait la muation obligatoire d'une valeur dans le chromosome BINs[]
            /* int posBin = Chrom_random.Next(1, Num_Bins_city - 1);
             int val = this.Bins_Visiting_Order[posBin - 1];
             this.Bins_Visiting_Order[posBin - 1] = this.Bins_Visiting_Order[posBin]; //Chrom_random.Next(1, Num_AGV);
             this.Bins_Visiting_Order[posBin] = val;*/
          //  for (int pos = 0; pos < Num_Bins_city; pos++)  // pour chaque bin in 'Bins_Visiting_Order'
          //  {
                int posBin = Chrom_random.Next(1, Num_Bins_city - 1);

                if (Chrom_random.NextDouble() < Chrom_mutationRate)
                {
                    int val = Bins_Visiting_Order[posBin - 1];

                    Bins_Visiting_Order[posBin - 1] = Bins_Visiting_Order[posBin]; //Chrom_random.Next(1, Num_AGV);
                    Bins_Visiting_Order[posBin] = val;
                }
          //  }
        }

        public void Mutate_One_gene_AGVPartes() // à revoir  (la premier proposition est de généré un num randomly et faire           
        {
             int posagv = Chrom_random.Next(1, Num_AGV - 1);
             int val = this.AGV_allocation_Order[posagv - 1];
             this.AGV_allocation_Order[posagv - 1] = this.AGV_allocation_Order[posagv]; 
             this.AGV_allocation_Order[posagv] = val;
            
        }

        public int[] Output_Chrom_AGV_order()
        {
            return this.AGV_allocation_Order;
        }
        public int[] Output_Chrom_Bin_order()
        {
            return this.Bins_Visiting_Order;
        }
       
        public int Fitness
        {
            get
            {
                return Chrom_fitness;
            }
            set
            {
                Chrom_fitness = value;
            }
        }

        public static double MutationRate
        {
            get
            {
                return Chrom_mutationRate;
            }
            set
            {
                Chrom_mutationRate = value;
            }
        }

        public int Length_AGVs
        {
            get
            {
                return Num_AGV;
            }
        }
        public int Length_Bins
        {
            get
            {
                return Num_Bins_city;
            }
        }

    }

}
