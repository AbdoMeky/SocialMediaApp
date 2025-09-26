using System.Text.Json.Serialization;

namespace SocialMediaApp.Core.DTO.AuthDTO
{
    public class GooglePeopleApiResponseDTO
    {
        [JsonPropertyName("names")]
        public List<NameData> Names { get; set; }

        [JsonPropertyName("photos")]
        public List<PhotoData> Photos { get; set; }

        [JsonPropertyName("birthdays")]
        public List<BirthdayData> Birthdays { get; set; }

        [JsonPropertyName("emailAddresses")]
        public List<EmailData> EmailAddresses { get; set; }
    }

    public class NameData
    {
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }
    }

    public class PhotoData
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }

    public class BirthdayData
    {
        [JsonPropertyName("date")]
        public DateData Date { get; set; }
    }

    public class DateData
    {
        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("month")]
        public int Month { get; set; }

        [JsonPropertyName("day")]
        public int Day { get; set; }
    }

    public class EmailData
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

}
