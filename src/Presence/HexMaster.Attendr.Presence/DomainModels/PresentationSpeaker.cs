namespace HexMaster.Attendr.Presence.DomainModels;

public sealed class PresentationSpeaker
{
    public Guid SpeakerId { get; private set; }
    public string Name { get; private set; }
    public string? ProfilePictureUrl { get; private set; }

    public PresentationSpeaker(Guid speakerId, string name, string? profilePictureUrl)
    {
        if (speakerId == Guid.Empty)
        {
            throw new ArgumentException("Speaker ID cannot be empty.", nameof(speakerId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        SpeakerId = speakerId;
        Name = name;
        ProfilePictureUrl = profilePictureUrl;
    }
}
