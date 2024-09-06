namespace SpikeApi.Common
{
    public interface IJwtTokenFactory
    {
        public string GenerateToken(string email, string roleName);

        public string ValidateToken(string token);
    }
}
