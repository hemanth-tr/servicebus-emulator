namespace Consumer
{
    public class ConsumerConfiguration
    {
        public string ConnectionString { get; set; }
        public string Topic {  get; set; }
        public string Queue {  get; set; }
        public string Subscription { get; set; }
        public string Endpoint { get; set; }
    }
}
