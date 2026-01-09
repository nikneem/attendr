using HexMaster.Attendr.Groups.Data.Postgress.Entities;
using HexMaster.Attendr.Groups.DomainModels;

namespace HexMaster.Attendr.Groups.Data.Postgress.Mappers;

/// <summary>
/// Maps between Group domain model and GroupEntity.
/// </summary>
internal static class GroupMapper
{
    public static GroupEntity ToEntity(Group group)
    {
        return new GroupEntity(
            Id: group.Id,
            Name: group.Name,
            Settings: new GroupSettingsEntity(
                IsPublic: group.Settings.IsPublic,
                IsSearchable: group.Settings.IsSearchable
            ),
            Members: group.Members.Select(m => new GroupMemberEntity(
                Id: m.Id,
                Name: m.Name,
                Role: (int)m.Role
            )).ToList(),
            Invitations: group.Invitations.Select(i => new GroupInvitationEntity(
                Id: i.Id,
                Name: i.Name,
                AcceptanceCode: i.AcceptanceCode,
                ExpirationDate: i.ExpirationDate
            )).ToList(),
            JoinRequests: group.JoinRequests.Select(jr => new GroupJoinRequestEntity(
                Id: jr.Id,
                Name: jr.Name,
                RequestedAt: jr.RequestedAt
            )).ToList(),
            FollowedConferences: group.FollowedConferences.Select(fc => new FollowedConferenceEntity(
                ConferenceId: fc.ConferenceId,
                Name: fc.Name,
                City: fc.City,
                Country: fc.Country,
                ImageUrl: fc.ImageUrl,
                SpeakersCount: fc.SpeakersCount,
                SessionsCount: fc.SessionsCount,
                StartDate: fc.StartDate,
                EndDate: fc.EndDate
            )).ToList(),
            Activities: group.Activities.Select(a => new GroupActivityEntity(
                Id: a.Id,
                ProfileId: a.ProfileId,
                CreatedAt: a.CreatedAt,
                Description: a.Description,
                ActivityTypeId: a.ActivityType.ActivityTypeId
            )).ToList()
        );
    }

    public static Group ToDomain(GroupEntity entity)
    {
        var settings = GroupSettings.Create(
            entity.Settings.IsPublic,
            entity.Settings.IsSearchable);

        // Find the owner member to initialize the group
        var ownerMember = entity.Members.FirstOrDefault(m => m.Role == (int)GroupRole.Owner)
            ?? throw new InvalidOperationException("Group must have an owner.");

        // Map activities from entity
        var activities = entity.Activities
            .Select(a => new GroupActivity(
                a.Id,
                a.ProfileId,
                a.CreatedAt,
                a.Description,
                GroupActivityType.FromId(a.ActivityTypeId)))
            .ToList();

        var group = Group.FromPersisted(
            entity.Id,
            entity.Name,
            ownerMember.Id,
            ownerMember.Name,
            settings,
            activities);

        // Add other members (excluding owner as it's already added)
        foreach (var memberEntity in entity.Members.Where(m => m.Role != (int)GroupRole.Owner))
        {
            group.AddMember(memberEntity.Id, memberEntity.Name, (GroupRole)memberEntity.Role);
        }

        // Reconstitute invitations
        foreach (var invitationEntity in entity.Invitations)
        {
            // Only add non-expired invitations
            if (invitationEntity.ExpirationDate > DateTimeOffset.UtcNow)
            {
                group.AddInvitation(
                    invitationEntity.Id,
                    invitationEntity.Name,
                    invitationEntity.ExpirationDate);
            }
        }

        // Reconstitute join requests
        foreach (var joinRequestEntity in entity.JoinRequests)
        {
            group.AddJoinRequest(
                joinRequestEntity.Id,
                joinRequestEntity.Name);
        }

        // Reconstitute followed conferences
        foreach (var conferenceEntity in entity.FollowedConferences)
        {
            group.FollowConference(
                conferenceEntity.ConferenceId,
                conferenceEntity.Name,
                conferenceEntity.City,
                conferenceEntity.Country,
                conferenceEntity.ImageUrl,
                conferenceEntity.SpeakersCount,
                conferenceEntity.SessionsCount,
                conferenceEntity.StartDate,
                conferenceEntity.EndDate);
        }

        return group;
    }
}
