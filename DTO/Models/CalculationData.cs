using System;
namespace SharedModels.Models
{
    public class CalculationData
    {
        public string Id { get; set; }
        public List<int> ListOfNumbers { get; set; }
        public string Operation { get; set; }
        public int Result { get; set; }
        public DateTime Time { get; set; }


    }

}

