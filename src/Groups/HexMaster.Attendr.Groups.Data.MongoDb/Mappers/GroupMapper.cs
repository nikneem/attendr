using HexMaster.Attendr.Groups.Data.MongoDb.Models;
using HexMaster.Attendr.Groups.DomainModels;

namespace HexMaster.Attendr.Groups.Data.MongoDb.Mappers;

/// <summary>
/// Maps between Group domain model and GroupDocument.
/// </summary>
internal static class GroupMapper
{
    public static GroupDocument ToDocument(Group group)
    {
        return new GroupDocument
        {
            Id = group.Id,
            Name = group.Name,
            Settings = new GroupSettingsDocument
            {
                IsPublic = group.Settings.IsPublic,
                IsSearchable = group.Settings.IsSearchable
            },
            Members = group.Members.Select(m => new GroupMemberDocument
            {
                Id = m.Id,
                Name = m.Name,
                Role = (int)m.Role
            }).ToList(),
            Invitations = group.Invitations.Select(i => new GroupInvitationDocument
            {
                Id = i.Id,
                Name = i.Name,
                AcceptanceCode = i.AcceptanceCode,
                ExpirationDate = i.ExpirationDate
            }).ToList(),
            JoinRequests = group.JoinRequests.Select(jr => new GroupJoinRequestDocument
            {
                Id = jr.Id,
                Name = jr.Name,
                RequestedAt = jr.RequestedAt
            }).ToList(),
            FollowedConferences = group.FollowedConferences.Select(fc => new FollowedConferenceDocument
            {
                ConferenceId = fc.ConferenceId,
                Name = fc.Name,
                City = fc.City,
                Country = fc.Country,
                StartDate = fc.StartDate,
                EndDate = fc.EndDate
            }).ToList()
        };
    }

    public static Group ToDomain(GroupDocument document)
    {
        var settings = GroupSettings.Create(
            document.Settings.IsPublic,
            document.Settings.IsSearchable);

        // Find the owner member to initialize the group
        var ownerMember = document.Members.FirstOrDefault(m => m.Role == (int)GroupRole.Owner)
            ?? throw new InvalidOperationException("Group must have an owner.");

        var group = Group.FromPersisted(
            document.Id,
            document.Name,
            ownerMember.Id,
            ownerMember.Name,
            settings);

        // Add other members (excluding owner as it's already added)
        foreach (var memberDoc in document.Members.Where(m => m.Role != (int)GroupRole.Owner))
        {
            group.AddMember(memberDoc.Id, memberDoc.Name, (GroupRole)memberDoc.Role);
        }

        // Reconstitute invitations
        foreach (var invitationDoc in document.Invitations)
        {
            // Only add non-expired invitations
            if (invitationDoc.ExpirationDate > DateTimeOffset.UtcNow)
            {
                group.AddInvitation(
                    invitationDoc.Id,
                    invitationDoc.Name,
                    invitationDoc.ExpirationDate);
            }
        }

        // Reconstitute join requests
        foreach (var joinRequestDoc in document.JoinRequests)
        {
            group.AddJoinRequest(
                joinRequestDoc.Id,
                joinRequestDoc.Name);
        }

        // Reconstitute followed conferences
        foreach (var conferenceDoc in document.FollowedConferences)
        {
            group.FollowConference(
                conferenceDoc.ConferenceId,
                conferenceDoc.Name,
                conferenceDoc.City,
                conferenceDoc.Country,
                conferenceDoc.StartDate,
                conferenceDoc.EndDate);
        }

        return group;
    }
}
