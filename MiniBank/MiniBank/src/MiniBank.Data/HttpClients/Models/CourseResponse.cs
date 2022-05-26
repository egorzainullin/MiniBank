namespace MiniBank.Data.HttpClients.Models
{
    public class CourseResponse
    {
        public DateTime Date { get; set; }
        
        public Dictionary<string, ValueItem> Valute { get; set; }
    }

    public class ValueItem
    {
        public string ID { get; set; }
        
        public string NumCode { get; set; }
        public double Value { get; set; }
    }
}