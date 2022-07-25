using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TelegramBot_WPF
{
    struct MessageLog
    {
        [Newtonsoft.Json.JsonProperty("time")]
        public string Time { get; set; }
        [Newtonsoft.Json.JsonProperty("id")]
        public long Id { get; set; }
        [Newtonsoft.Json.JsonProperty("msg")]
        public string Msg { get; set; }
        [Newtonsoft.Json.JsonProperty("firstName")]
        public string FirstName { get; set; }

        public MessageLog(string Time, string Msg, string FirstName, long Id)
        {
            this.Time = Time;
            this.Msg = Msg;
            this.FirstName = FirstName;
            this.Id = Id;
        }

        public override string ToString()
        {
            return $"{Time} {Msg} {FirstName}";
        }
    }
}
