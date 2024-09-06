using System.Data;

namespace Spike.Services
{
    public static class CommonConstants
    {
        //Authentication
        public const string INVALID_LOGIN = "Invalid login details";
        public const string SUCCESSFUL_LOGIN = "Successful Login";
        public const string NOT_AUTHORIZED_VIEW = "Access Denied !! You are not authorized to access this page.";
        
        //Registration
        public const string USER_ALREADY_REGISTERED = "Sorry !! The user already exists in our db. Please login.";
        public const string USER_CREATED_SUCCESSFULLY = "Thanks. The user is registered successfully.";
        
        //Token
        public const string TOKEN_CREATION = "TOKEN_CREATION";
        public const string TOKEN_VALIDATION = "TOKEN_VALIDATION";
        public const string INVALID_TOKEN = "Invalid Token!! Token cannot be empty";
        public const string INVALID_JWT_TOKEN = "Invalid Jwt Token!! Missing the prefix for the valid jwt token.";
        public const string UNAUTHORIZED_USER = "Access Denied!! User does not belong to any role.";
        public const string SOME_ERROR_OCCURRED = "Some error has occured...";
        public const string SUCCESSFUL = "Successful";

        public const string EMPTY = "cannot be empty";

        //Roles
        public const string SUPERADMIN = "SuperAdmin";
        public const string ADMIN = "Admin";
        public const string USER = "User";
    }
}
