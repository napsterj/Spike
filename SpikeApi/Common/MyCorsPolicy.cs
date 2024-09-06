using Microsoft.AspNetCore.Cors;

namespace SpikeApi.Common
{
    public class MyCorsPolicy: EnableCorsAttribute
    {
        public MyCorsPolicy() 
        {
            PolicyName = "_mycorspolicy";
        }

        public string AllowedOrgin = "http://localhost:4200";        
    }
}
