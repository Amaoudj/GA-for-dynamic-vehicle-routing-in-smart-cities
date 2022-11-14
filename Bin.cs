using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GA_VRP_CDTA
{
    public class Bin
    {
        //variables
        public int  id;
        public int  Capacity;
        public int  Assingned_AGV;
        public bool allocated;
        public Bin(int ID_Num, int capacity)
        {
            id       =ID_Num;
            Capacity = capacity;
            Assingned_AGV = 0;
            allocated = false;
        }
      
    }
}
