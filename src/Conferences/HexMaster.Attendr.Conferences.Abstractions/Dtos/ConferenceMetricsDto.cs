namespace HexMaster.Attendr.Conferences.Abstractions.Dtos;

public sealed record ConferenceMetricsDto(
    int VisibleConferences,
    int UpcomingConferences,
    int TotalPresentations,
    int TotalSpeakers,
    int TotalRooms,
    int TotalTopics);
