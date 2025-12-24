namespace HexMaster.Attendr.Presence.DomainModels;

public sealed class PresentationSpeaker
{
    public Guid SpeakerId { get; private set; }
    public string Name { get; private set; }
    public string ProfilePictureUrl { get; private set; }

    private PresentationSpeaker()
    {
        SpeakerId = Guid.Empty;
        Name = string.Empty;
        ProfilePictureUrl = string.Empty;
    }

    public PresentationSpeaker(Guid speakerId, string name, string profilePictureUrl)
    {
        if (speakerId == Guid.Empty)
        {
            throw new ArgumentException("Speaker ID cannot be empty.", nameof(speakerId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentException.ThrowIfNullOrWhiteSpace(profilePictureUrl, nameof(profilePictureUrl));

        SpeakerId = speakerId;
        Name = name;
        ProfilePictureUrl = profilePictureUrl;
    }
}
