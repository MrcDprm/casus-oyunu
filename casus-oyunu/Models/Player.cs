namespace casus_oyunu.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsSpy { get; set; }
        public string AssignedWord { get; set; }
    }
} 