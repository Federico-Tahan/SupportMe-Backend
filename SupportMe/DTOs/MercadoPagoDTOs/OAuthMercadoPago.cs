namespace SupportMe.DTOs.MercadoPagoDTOs
{
    public class OAuthMercadoPago
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
        public int user_id { get; set; }
        public string refresh_token { get; set; }
        public string public_key { get; set; }
        public bool live_mode { get; set; }
    }

}
