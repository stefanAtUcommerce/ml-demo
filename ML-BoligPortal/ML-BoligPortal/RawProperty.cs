namespace ML_BoligPortal
{
    public class RawProperty
    {
        public string District { get; set; }

        public string PropertyType { get; set; }
        public int Rooms { get; set; }
        public bool Furnished { get; set; }
        public bool Shareable { get; set; }
        public bool PetsAllowed { get; set; }
        public bool Elevator { get; set; }
        public bool SeniorFriendly { get; set; }
        public bool StudentOnly { get; set; }
        public bool Balcony { get; set; }
        public bool Parking { get; set; }
        public int Size { get; set; }
        public int Floor { get; set; }

        public string RentalPeriod { get; set; }
        public string AvailableFrom { get; set; }
        public int MonthlyNetRent { get; set; }
        public int Utilities { get; set; }
        public int Deposit { get; set; }
        public int PrepaidRent { get; set; }
        public int MoveInPrice { get; set; }
    }
}
