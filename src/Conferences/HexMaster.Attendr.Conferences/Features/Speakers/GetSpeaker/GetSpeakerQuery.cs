namespace HexMaster.Attendr.Conferences.Features.Speakers.GetSpeaker;

public sealed record GetSpeakerQuery(Guid ConferenceId, Guid SpeakerId);
