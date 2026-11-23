namespace HarmonyPlugins.Api.Domain.Entities
{
    public class WebhookEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? PaymentId { get; set; }
        public Payment? Payment { get; set; }
        public string Gateway { get; set; }
        public string ExternalEventId { get; set; }
        public string Topic { get; set; }
        public string ResourceId { get; set; }
        public string Payload { get; set; }
        public int Attempts { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    }
}
