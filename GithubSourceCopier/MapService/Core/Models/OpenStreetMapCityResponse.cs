using System.Text.Json.Serialization;

namespace GithubSourceCopier.MapService.Core.Models
{
    public class OpenStreetMapCityResponse
    {
        /// <summary>
        /// Yerin unikal ID-si (place_id).
        /// </summary>
        [JsonPropertyName("place_id")]
        public long Place_id { get; set; }

        /// <summary>
        /// OpenStreetMap lisenziya məlumatları (licence).
        /// </summary>
        [JsonPropertyName("licence")]
        public string Licence { get; set; }

        /// <summary>
        /// OpenStreetMap obyekt növü (osm_type).
        /// Məsələn: "relation", "way" və s.
        /// </summary>
        [JsonPropertyName("osm_type")]
        public string Osm_type { get; set; }

        /// <summary>
        /// OpenStreetMap obyektinin ID-si (osm_id).
        /// </summary>
        [JsonPropertyName("osm_id")]
        public long Osm_id { get; set; }

        /// <summary>
        /// Latitude koordinatı (lat).
        /// </summary>
        [JsonPropertyName("lat")]
        public string Lat { get; set; }

        /// <summary>
        /// Longitude koordinatı (lon).
        /// </summary>
        [JsonPropertyName("lon")]
        public string Lon { get; set; }

        /// <summary>
        /// Yer obyektinin sinfi (class).
        /// Məsələn: "boundary", "place".
        /// </summary>
        [JsonPropertyName("class")]
        public string Class { get; set; }

        /// <summary>
        /// Yer obyektinin tipi (type).
        /// Məsələn: "administrative".
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// Yer obyektinin rütbəsi (place_rank).
        /// </summary>
        [JsonPropertyName("place_rank")]
        public int Place_rank { get; set; }

        /// <summary>
        /// Yer obyektinin əhəmiyyət dərəcəsi (importance).
        /// 0 ilə 1 arasında bir dəyər.
        /// </summary>
        [JsonPropertyName("importance")]
        public double Importance { get; set; }

        /// <summary>
        /// Ünvan növü (addresstype).
        /// Məsələn: "city".
        /// </summary>
        [JsonPropertyName("addresstype")]
        public string Addresstype { get; set; }

        /// <summary>
        /// Obyektin adı (name).
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Obyektin tam adı (display_name).
        /// Məsələn: "Baku City, Azerbaijan".
        /// </summary>
        [JsonPropertyName("display_name")]
        public string Display_name { get; set; }

        /// <summary>
        /// Yer obyektinin bounding box koordinatları (boundingbox).
        /// 4 fərqli dəyərdən ibarətdir: [cənub, şimal, qərb, şərq].
        /// </summary>
        [JsonPropertyName("boundingbox")]
        public string[] Boundingbox { get; set; }
    }

}
