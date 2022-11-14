using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GA_VRP_CDTA
{
    public class AGV
    {
        // variables
        public int       id;
        public int       Capacity;
        public int       free_capacity;
        public int       num_assigned_bind;
        public List<int> Local_AssignedBins;
        public int       Traveled_Distance;
        //public List<int> distance_To_Bins; 
        //==================================
        public AGV(int ID_Num, int capacity)
        {
            Local_AssignedBins = new List<int>();
            id                 = ID_Num;
            Capacity           = capacity;            
            Traveled_Distance   = 0;
            num_assigned_bind  = 0;
            free_capacity      = Capacity;
        }
       
    }
}
