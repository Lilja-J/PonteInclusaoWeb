
namespace PonteInclusaoWeb.Models
{
    public class Place
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Coordinates Coordinates { get; set; } = new(0, 0); 
        public double Rating { get; set; }
        public bool IsPermanentlyClosed { get; set; }
    }
}