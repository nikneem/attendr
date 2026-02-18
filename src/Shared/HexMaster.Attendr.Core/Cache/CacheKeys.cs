namespace HexMaster.Attendr.Core.Cache;

public static class CacheKeys
{
    public static class Conferences
    {
        public const string Metrics = "conferences:metrics";
    }

    public static class Profiles
    {
        public static string Subject(string subjectId)
        {
            if (string.IsNullOrWhiteSpace(subjectId))
            {
                throw new ArgumentException("SubjectId cannot be null or whitespace.", nameof(subjectId));
            }
            return $"profiles:subject:{subjectId}";
        }

        public static string Details(string profileId)
        {
            if (string.IsNullOrWhiteSpace(profileId))
            {
                throw new ArgumentException("ProfileId cannot be null or whitespace.", nameof(profileId));
            }
            return $"profiles:details:{profileId}";
        }
    }
}
