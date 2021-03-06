using System.Text.Json.Serialization;

namespace HandleMyQueue.Models.DTOs
{
    public class CredentialsDto
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
    }
}