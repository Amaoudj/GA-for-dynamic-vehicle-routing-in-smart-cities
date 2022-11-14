using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;


namespace GA_VRP_CDTA
{
    public partial class Form1 : Form
    {
        
        public GA Ga;
        //public List<Bin> Bins ;
        //public List<AGV> AGVs ;
        public int[,] DistanceMatrix;
        public int Num_bins = 0;
        public int Num_AGV = 0;

        //OpenFileDialog openFileDialog;

        public Form1()
        {                
            InitializeComponent();       
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }


        private void label4_Click(object sender, EventArgs e)
        {

        }

        public float CPU_Time = 0;
        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox7.Text != null && int.Parse(textBox7.Text) >= 1)
            {
                for (int aa = 1; aa <= int.Parse(textBox7.Text); aa++)
                {
                    richTextBox1.AppendText("\nThe Instance " + Convert.ToString(aa) + "\n");

                    List<Bin> Bins = new List<Bin>();
                    List<AGV> AGVs = new List<AGV>();
                    int num_run = 1;
                    if ((textBox6.Text != null) && (textBox6.Text != "")) num_run = int.Parse(textBox6.Text);
                    else num_run = 1;
                    if (num_run == 0) num_run = 1;

                    //lire les benchmark et remplireliste des AGVs et Les Bins, puis Bins.distances_from_AGV et Bins.distances_from_bins
                    string AGV_CapacityFolde = "M:\\VRP_Final\\Benchmark\\MVRP1" + Convert.ToString(aa) + "_" + "AGVCAP" + ".txt";
                    int ii = 1;


                    string[] vecteur = new string[200];
                    using (StreamReader reader = new StreamReader(AGV_CapacityFolde))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            vecteur[ii] = line.Trim();
                            ii++;
                        }
                        reader.Close();
                    }
                    Num_AGV = int.Parse(vecteur[1]);
                    richTextBox1.AppendText("The AGV number = " + vecteur[1] + "\n");
                    int n = 1;
                    for (int d = 1; d <= int.Parse(vecteur[1]); d++)
                    {
                        AGVs.Add(new AGV(n, int.Parse(vecteur[d + 1])));
                        richTextBox1.AppendText("AGV " + Convert.ToString(AGVs[n - 1].id) + " has a capacity of =  " + Convert.ToString(AGVs[n - 1].Capacity) + "\n");
                        n++;
                    }
                    richTextBox1.AppendText("--------------------------------" + "\n");

                    // Lire les identifiant et les capacity of Bins
                    string BIN_CapacityFolde = "M:\\VRP_Final\\Benchmark\\MVRP" + Convert.ToString(aa) + "_" + "BINCAP" + ".txt";
                    int ii2 = 1;
                    string[] vecteur2 = new string[200];
                    using (StreamReader reader2 = new StreamReader(BIN_CapacityFolde))
                    {
                        string line2;
                        while ((line2 = reader2.ReadLine()) != null)
                        {
                            vecteur2[ii2] = line2.Trim();
                            ii2++;
                        }
                        reader2.Close();
                    }
                    Num_bins = int.Parse(vecteur2[1]);
                    richTextBox1.AppendText("The BIN number = " + vecteur2[1] + "\n");
                    int n2 = 1;
                    for (int d = 1; d <= int.Parse(vecteur2[1]); d++)
                    {
                        Bins.Add(new Bin(n2, int.Parse(vecteur2[d + 1])));
                        richTextBox1.AppendText("BIN " + Convert.ToString(Bins[n2 - 1].id) + " has a capacity of =  " + Convert.ToString(Bins[n2 - 1].Capacity) + "\n");
                        n2++;
                    }
                    // foreach(Bin b in Bins) 
                    richTextBox1.AppendText("-----------------------------------------" + "\n");

                    // lire les distance et les aajouter dans la matrice distance
                    DistanceMatrix = new int[Bins.Count + 1, Bins.Count + 1];

                    String input = File.ReadAllText(@"M:\\VRP_Final\\Benchmark\\MVRP" + Convert.ToString(aa) + "_" + "BINDIST" + ".txt");
                    int i = 0, j = 0;
                    foreach (var row in input.Split('\n'))
                    {
                        j = 0;
                        foreach (var col in row.Trim().Split(' '))
                        {
                            DistanceMatrix[i, j] = int.Parse(col.Trim());
                            j++;
                        }
                        i++;
                    }
                    richTextBox1.AppendText("DistancesMatrix Loaded: OK " + "\n");
                    richTextBox1.AppendText("----------------------------------------" + "\n");


                    // Progress = double.Parse((produced * 100d / 10000000).ToString("0.00"));

                    //=============================>< Genetic Alhgorithm>==================
                    int Best_Solution = 20000000; // Over 30 run
                    double average_Solution = 0.0;
                    double time_bestSolution = 0.0;
                    double average_Time = 0.0;

                    List<AGV> AGVs_Best = new List<AGV>();
                    List<Chromosome> solutions_manyrun = new List<Chromosome>();
                    int somme_solution = 0;
                    double some_time = 0;

                    int bins_capacity = 0;
                    int Agv_Capacity = 0;
                    bool ok = true;

                    foreach (Bin b in Bins) bins_capacity += b.Capacity;
                    foreach (AGV a in AGVs) Agv_Capacity += a.Capacity;


                    foreach (Bin b in Bins) bins_capacity += b.Capacity;
                    foreach (AGV a in AGVs) Agv_Capacity += a.Capacity;

                    if (bins_capacity > Agv_Capacity)
                    {
                        ok = false;
                    }

                    if (ok) // pas de pb avec les instances de Mr. Ahmed
                    {
                        for (int ih = 0; ih < num_run; ih++)  // pour 30 run 
                        {

                            foreach (AGV agv in AGVs) // initialise some parametres 
                            {
                                agv.Local_AssignedBins.Clear(); agv.Traveled_Distance = 0; agv.free_capacity = agv.Capacity; agv.num_assigned_bind = 0;
                            }
                            foreach (Bin b in Bins) // initialise some parametres 
                            {
                                b.allocated = false;
                                b.Assingned_AGV = 0;
                            }

                            Ga = new GA(Double.Parse(textBox2.Text), Double.Parse(textBox1.Text), int.Parse(textBox3.Text), int.Parse(textBox4.Text), Bins, AGVs, DistanceMatrix); // à modifier par les les benchmark
                            Ga.Elitism = true;

                            float startTime = Environment.TickCount;
                            Ga.Run();
                            CPU_Time = (Environment.TickCount - startTime) / 1000;

                            //======================================================================
                            //get the best solution
                            // List<AGV> ag;
                            //Chromosome chro = new Chromosome(Num_AGV, Num_bins, false); 

                            Chromosome chro = Ga.GetBest();
                            //  AGV c = chro.AGVs[10000];
                            Ga.Evaluate_Chromosome(chro);
                            //  AGV cc = chro.AGVs[10000];

                            int fitness = chro.Chrom_fitness;
                            somme_solution += fitness;
                            some_time += CPU_Time;
                            solutions_manyrun.Add(chro);

                            /*  if (chro.Chrom_fitness < Best_Solution)
                              {
                                  Best_Solution = chro.Chrom_fitness; // fitness;                     
                                  time_bestSolution = CPU_Time;
                              }                    
                           */

                            average_Solution = somme_solution / num_run;
                            average_Time = some_time / num_run;

                            // je rank the list et la mieure solution est à la pos 0
                            //  solutions_manyrun.Sort(delegate (Chromosome x, Chromosome y)
                            //  { return Comparer<int>.Default.Compare(x.Fitness, y.Fitness); });
                            //Chromosome chrom = solutions_manyrun[0];

                            //Chromosome c = solution_manyrun[10000];

                            //Afichage of the best solution over 30 run
                            //foreach (Chromosome cho in Ga.Elitisms)  Afficher_solution(cho);
                            richTextBox1.AppendText("------< The best solution>------" + "\n");
                            richTextBox1.AppendText("---------------------------------" + "\n");

                            Afficher_solution(chro);// chrom);//chro            
                            Afficher_solution(chro.AGVs);// chrom.AGVs);// chro.Chrom_fitness);

                            richTextBox1.AppendText(" Execution Time = " + Convert.ToString(CPU_Time) + "\n");//CPU_Time
                            if (Solution_correct(chro)) richTextBox1.AppendText(" Solution Correct: OK" + "\n");
                            else richTextBox1.AppendText(" Solution Not Correct : NO" + "\n");
                            richTextBox1.AppendText("==============================================================\n");
                            richTextBox1.AppendText("==============================================================\n");


                        } // end bocle run

                        solutions_manyrun.Sort(delegate (Chromosome x, Chromosome y)
                        {
                            return Comparer<int>.Default.Compare(x.Fitness, y.Fitness);
                        });

                        Chromosome chrom = solutions_manyrun[0];
                        // write the path positions in Path.txt Forlder
                        string benchmark = "Solution_MVRP" + Convert.ToString(aa);
                        string fileName = @"M:\\VRP_Final\\Benchmark\\" + benchmark + ".txt";
                        try
                        {
                            // Check if file already exists. If yes, delete it.     
                            if (File.Exists(fileName))
                            {
                                File.Delete(fileName);
                            }
                            // Create a new file     
                            using (FileStream fs = File.Create(fileName))
                            {

                            }
                            using (StreamWriter writer = new StreamWriter(fileName))
                            {
                                foreach (AGV agv in chrom.AGVs)
                                {
                                    string schedule = "AGV" + Convert.ToString(agv.id) + ": [Depot -> ";
                                    for (int h = 0; h < agv.num_assigned_bind; h++) schedule += "b" + Convert.ToString(agv.Local_AssignedBins[h]) + " -->  ";
                                    schedule += "Depot]";
                                    writer.WriteLine(schedule);
                                }
                                string ss = "\n";
                                writer.WriteLine(ss);
                                string Solution_dist = "Distance Traveled = " + Convert.ToString(chrom.Chrom_fitness);
                                writer.WriteLine(Solution_dist);

                                string cpuTime = "CPU Time = " + Convert.ToString(CPU_Time);
                                writer.WriteLine(cpuTime);


                                writer.WriteLine(ss);
                                writer.WriteLine(ss);
                                string s = "=================< Statistics >========================";
                                writer.WriteLine(s);

                                string ave_solu = "Average Solution over many runs = " + Convert.ToString(average_Solution);
                                writer.WriteLine(ave_solu);
                                string ave_cpuTime = "Average CPU Time over many runs = " + Convert.ToString(time_bestSolution);
                                writer.WriteLine(ave_cpuTime);
                            }
                        }
                        catch (Exception Ex)
                        {
                            MessageBox.Show(Ex.ToString());
                        }
                    }
                }
            }

           else richTextBox1.AppendText("      Veuillez corriger votre instance !!!  \n");
        }

        public void Afficher_solution(Chromosome chro)
        {
            
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("Allocation : [");
            for (int i =0; i < chro.AGV_allocation_Order.Length;i++)
            {
              richTextBox1.AppendText(" " + Convert.ToString(chro.AGV_allocation_Order[i]) );
            }
            richTextBox1.AppendText(" ]\n");

            richTextBox1.AppendText("Bins_Visiting_Order [");
            for (int h = 0; h < chro.Bins_Visiting_Order.Length; h++)
            {
                richTextBox1.AppendText(" " + Convert.ToString(chro.Bins_Visiting_Order[h]));
            }

            richTextBox1.AppendText(" ]\n");
  
            richTextBox1.AppendText("\nSolution =  " + Convert.ToString(chro.Chrom_fitness) + "\n");
         
            richTextBox1.AppendText("========================================\n");         
        }
        public void Afficher_solution(List<AGV> agvs)
        {
            int fit = 0;
            foreach (AGV agv in agvs) // affichafe
            {
                fit += agv.Traveled_Distance;
                richTextBox1.AppendText("AGV" + Convert.ToString(agv.id) + ": [Depot -> ");
                for (int h = 0; h < agv.num_assigned_bind; h++) richTextBox1.AppendText("b" + Convert.ToString(agv.Local_AssignedBins[h]) + " -->  ");
                richTextBox1.AppendText("Depot], traveled distance( " + Convert.ToString(agv.Traveled_Distance) + " )\n");
                richTextBox1.AppendText("----------------------------------------------------------------\n");
            }

            richTextBox1.AppendText(" Final Distance =  " + Convert.ToString(fit)+"\n");
            richTextBox1.AppendText("---------------------------------------------------------------\n");
        }
        private void button3_Click(object sender, EventArgs e)
        {
            using (var openfildialg = new OpenFileDialog())// { Filter = "PNG|*.png|*.JPEG*.jpeg" })
            { //openfildialg.Filter = "Image Files|*.JPEG; *.png";
                if (openfildialg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                       // textBox5.Text = openFileDialog.FileName;
                }
            }
        }

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

            for (int i = 0; i < v.Length; i++) if (v[i] == val) { index = i; }

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
            int[] V_index= new int[v.Length];            
            V_index = get_table_Indexs(v, V_glob);
            

            for (int i = 0; i < v.Length; i++)
            {
                int min = V_index[i];
                for (int j = i+1; j < v.Length; j++)
                {
                    if (V_index[j] < min)
                    {                      
                        int val=  V_index[j];                      
                        V_index[i] = val;
                        V_index[j] = min;
                        int val2= v[i];                      
                        v[i] = v[j];
                        v[j] = val2;
                        min = V_index[i];
                    }
                }
            }

            return v;
        }
        public void correct_Chromosome(int[] chrom)
        {

        }

        private void button__Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }
        bool cheked_ = false;
        public bool onlyoneInstance = true;
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            cheked_ = !cheked_;
            if (cheked_) { groupBox3.Enabled = false; textBox7.Enabled = true; onlyoneInstance = false; }
            else {
                onlyoneInstance = true;
                groupBox3.Enabled = true; textBox7.Enabled = false;
              }
        }

        private void button1_Click(object sender, EventArgs e)
        {          
            this.Close();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
           
            List<Bin>  Bins = new List<Bin>();
            List<AGV> AGVs = new List<AGV>();
            int num_run = 1;
            if ((textBox6.Text != null) && (textBox6.Text != "")) num_run = int.Parse(textBox6.Text);
            else num_run = 1;
            if (num_run == 0) num_run = 1;

            //lire les benchmarks et remplire liste des AGVs et des Bins, puis Bins.distances_from_AGV et Bins.distances_from_bins
            string AGV_CapacityFolde = "M:\\VRP_Final\\Benchmark\\" + textBox5.Text + "_" + "AGVCAP" + ".txt";
            int ii = 1;

            string[] vecteur = new string[200];
            using (StreamReader reader = new StreamReader(AGV_CapacityFolde))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    vecteur[ii] = line.Trim();
                    ii++;
                }
                reader.Close();
            }
            Num_AGV = int.Parse(vecteur[1]);
            richTextBox1.AppendText("The AGV number = " + vecteur[1] + "\n");
            int n = 1;
            for (int d = 1; d <= int.Parse(vecteur[1]); d++)
            {
                AGVs.Add(new AGV(n, int.Parse(vecteur[d + 1])));
                richTextBox1.AppendText("AGV " + Convert.ToString(AGVs[n - 1].id) + " has a capacity of =  " + Convert.ToString(AGVs[n - 1].Capacity) + "\n");
                n++;
            }
            richTextBox1.AppendText("--------------------------------" + "\n");

            // Lire les identifiant et les capacity of Bins
            string BIN_CapacityFolde = "M:\\VRP_Final\\Benchmark\\" + textBox5.Text + "_" + "BINCAP" + ".txt";
            int ii2 = 1;
            string[] vecteur2 = new string[200];
            using (StreamReader reader2 = new StreamReader(BIN_CapacityFolde))
            {
                string line2;
                while ((line2 = reader2.ReadLine()) != null)
                {
                    vecteur2[ii2] = line2.Trim();
                    ii2++;
                }
                reader2.Close();
            }
            Num_bins = int.Parse(vecteur2[1]);
            richTextBox1.AppendText("The BIN number = " + vecteur2[1] + "\n");
            int n2 = 1;
            for (int d = 1; d <= int.Parse(vecteur2[1]); d++)
            {
                Bins.Add(new Bin(n2, int.Parse(vecteur2[d + 1])));
                richTextBox1.AppendText("BIN " + Convert.ToString(Bins[n2 - 1].id) + " has a capacity of =  " + Convert.ToString(Bins[n2 - 1].Capacity) + "\n");
                n2++;
            }
            // foreach(Bin b in Bins) 
            richTextBox1.AppendText("-----------------------------------------" + "\n");

            // lire les distance et les aajouter dans la matrice distance
            DistanceMatrix = new int[Bins.Count + 1, Bins.Count + 1];

            String input = File.ReadAllText(@"M:\\VRP_Final\\Benchmark\\" + textBox5.Text + "_" + "BINDIST" + ".txt");
            int i = 0, j = 0;
            foreach (var row in input.Split('\n'))
            {
                j = 0;
                foreach (var col in row.Trim().Split(' '))
                {
                    DistanceMatrix[i, j] = int.Parse(col.Trim());
                    j++;
                }
                i++;
            }
            richTextBox1.AppendText("DistancesMatrix Loaded: OK " + "\n");
            richTextBox1.AppendText("----------------------------------------" + "\n");


            // Progress = double.Parse((produced * 100d / 10000000).ToString("0.00"));

            //=============================>< Genetic Alhgorithm>==================
            int Best_Solution = 20000000; // Over 30 run
            double Ovrage_Solution = 0.0;
            double time_bestSolution = 0.0;
            double overage_Time = 0.0;
            List<AGV> AGVs_Best = new List<AGV>();
            List<Chromosome> solutions_manyrun = new List<Chromosome>();
            int somme_solution = 0;
            double some_time = 0;

            int bins_capacity = 0;
            int Agv_Capacity = 0;
            bool ok = true;

            foreach (Bin b in Bins) bins_capacity += b.Capacity;
            foreach (AGV a in AGVs) Agv_Capacity += a.Capacity;

            if (bins_capacity > Agv_Capacity)
            {
                ok = false;
            }
            int solution = 0;
            if (ok) // pas de pb avec les instances de Mr. Ahmed
            {
              for (int ih = 0; ih < num_run; ih++)  // pour 30 run 
               {

                    foreach (AGV agv in AGVs) // initialise some parametres 
                    {
                        agv.Local_AssignedBins.Clear(); agv.Traveled_Distance = 0; agv.free_capacity = agv.Capacity; agv.num_assigned_bind = 0;
                    }
                    foreach (Bin b in Bins) // initialise some parametres 
                    {
                        b.allocated = false;
                        b.Assingned_AGV = 0;
                    }

                    Ga = new GA(Double.Parse(textBox2.Text), Double.Parse(textBox1.Text), int.Parse(textBox3.Text), int.Parse(textBox4.Text), Bins, AGVs, DistanceMatrix); // à modifier par les les benchmark
                    Ga.Elitism = true;

                    float startTime = Environment.TickCount;
                    Ga.Run();
                    CPU_Time = (Environment.TickCount - startTime) / 1000;

                    //======================================================================
                    //get the best solution
                   // List<AGV> ag;
                    //Chromosome chro = new Chromosome(Num_AGV, Num_bins, false); 
                    
                    Chromosome chro=  Ga.GetBest();
                  //  AGV c = chro.AGVs[10000];
                    Ga.Evaluate_Chromosome(chro);
                  //  AGV cc = chro.AGVs[10000];

                    int fitness= chro.Chrom_fitness;
                    somme_solution += fitness;
                    some_time += CPU_Time;
                    solutions_manyrun.Add(chro);

                  /*  if (chro.Chrom_fitness < Best_Solution)
                    {
                        Best_Solution = chro.Chrom_fitness; // fitness;                     
                        time_bestSolution = CPU_Time;
                    }                    
                 */

                 Ovrage_Solution = somme_solution / num_run;
                 overage_Time = some_time / num_run;
              
                // je rank the list et la mieure solution est à la pos 0
                //  solutions_manyrun.Sort(delegate (Chromosome x, Chromosome y)
                //  { return Comparer<int>.Default.Compare(x.Fitness, y.Fitness); });
                //Chromosome chrom = solutions_manyrun[0];

               //Chromosome c = solution_manyrun[10000];
                
                //Afichage of the best solution over 30 run
                //foreach (Chromosome cho in Ga.Elitisms)  Afficher_solution(cho);
                richTextBox1.AppendText("------< The best solution>------" + "\n");
                richTextBox1.AppendText("---------------------------------" + "\n");

                Afficher_solution(chro);// chrom);//chro            
                Afficher_solution(chro.AGVs);// chrom.AGVs);// chro.Chrom_fitness);
               
                richTextBox1.AppendText(" Execution Time = " + Convert.ToString(CPU_Time) + "\n");//CPU_Time
                if (Solution_correct(chro)) richTextBox1.AppendText(" Solution Correct: OK" + "\n");
                else richTextBox1.AppendText(" Solution Not Correct : NO" + "\n");
                richTextBox1.AppendText("==============================================================");
                richTextBox1.AppendText("==============================================================");


             } // end bocle run

                solutions_manyrun.Sort(delegate (Chromosome x, Chromosome y)
                { return Comparer<int>.Default.Compare(x.Fitness, y.Fitness); });

                Chromosome chrom = solutions_manyrun[0];
                // write the path positions in Path.txt Forlder
                string benchmark = "Solution_" + textBox5.Text;
                string fileName = @"M:\\VRP_Final\\Benchmark\\" + benchmark + ".txt";
                try
                {
                    // Check if file already exists. If yes, delete it.     
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                    // Create a new file     
                    using (FileStream fs = File.Create(fileName))
                    {

                    }
                    using (StreamWriter writer = new StreamWriter(fileName))
                    {
                        foreach (AGV agv in chrom.AGVs)
                        {
                            string schedule = "AGV" + Convert.ToString(agv.id) + ": [Depot -> ";
                            for (int h = 0; h < agv.num_assigned_bind; h++) schedule += "b" + Convert.ToString(agv.Local_AssignedBins[h]) + " -->  ";
                            schedule += "Depot]";
                            writer.WriteLine(schedule);
                        }
                        string ss = "\n";
                        writer.WriteLine(ss);
                        string Solution_dist = "Distance traveled = " + Convert.ToString(chrom.Chrom_fitness);
                        writer.WriteLine(Solution_dist);

                        string cpuTime = "CPU Time = " + Convert.ToString(CPU_Time);
                        writer.WriteLine(cpuTime);


                        writer.WriteLine(ss);
                        writer.WriteLine(ss);
                        string s = "=================< Statistics >========================";
                        writer.WriteLine(s);

                        string ove_solu = "Average Solution over many runs = " + Convert.ToString(Ovrage_Solution);
                        writer.WriteLine(ove_solu);
                        string ove_cpuTime = "Average CPU Time over many runs = " + Convert.ToString(time_bestSolution);
                        writer.WriteLine(ove_cpuTime);
                    }
                }
                catch (Exception Ex)
                {
                    MessageBox.Show(Ex.ToString());
                }
            }
            else MessageBox.Show("Barka matmahbel bina Ahmed !!!, Vérifies ton instance ? ");
        }

        public bool Solution_correct(Chromosome ch)
        {
            int num = 0;
            bool ok = true;
            for (int b = 1; b <= Num_bins; b++)
            {
                foreach (AGV agv in ch.AGVs)
                {
                    foreach (int h in agv.Local_AssignedBins)
                    {
                        if (h == b) num++;
                    }
                }
            }

            if (num == Num_bins) ok = true;
            else ok = false;


            return ok;
        }

    }
}
