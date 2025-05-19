
namespace Api.DTOs.SecretTokenDTO
{
    public class SecretTokenResponseDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public string Service { get; set; } = string.Empty;

        public string Note { get; set; } = string.Empty;
        public SecretTokenResponseDTO(string id, string name, string service, string note)
        {
            Id = id;
            Name = name;
            Service = service;
            Note = note;
        }
    }
}