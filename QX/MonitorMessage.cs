namespace QueueExchange
{
    public class MonitorMessage
    {
        public string uuid;
        public string id;
        public string name;
        public string id1;
        public string name1;
        public string v;
        public string v1;

        public MonitorMessage(string uuid, string id, string name, string id1, string name1, string v, string v1)
        {
            this.uuid = uuid;
            this.id = id;
            this.name = name;
            this.id1 = id1;
            this.name1 = name1;
            this.v = v;
            this.v1 = v1;
        }
    }
}