namespace nauth_asp.Helpers
{
    public static class KeyGenerators
    {
        public static string GetUserAvatarKey(long userId)
        {
            return $"userAvatars/{userId}/avatar.png";
        }
    }
}
